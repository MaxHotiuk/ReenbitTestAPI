using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ReenbitTest.Core.DTOs;
using ReenbitTest.Core.Entities;
using ReenbitTest.Core.Interfaces;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ReenbitTest.API.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISentimentAnalysisService _sentimentAnalysisService;
        private readonly ILogger<ChatHub> _logger;

        private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections 
            = new ConcurrentDictionary<string, HashSet<string>>();
        
        private static readonly ConcurrentDictionary<string, HashSet<string>> _connectionGroups 
            = new ConcurrentDictionary<string, HashSet<string>>();

        public ChatHub(
            IChatRepository chatRepository,
            IUserRepository userRepository,
            ISentimentAnalysisService sentimentAnalysisService,
            ILogger<ChatHub> logger)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _sentimentAnalysisService = sentimentAnalysisService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var connectionId = Context.ConnectionId;
            
            _logger.LogInformation($"User {userId} connected with connection ID: {connectionId}");
            
            _userConnections.AddOrUpdate(
                userId,
                new HashSet<string> { connectionId },
                (_, connections) =>
                {
                    connections.Add(connectionId);
                    return connections;
                });
            
            _connectionGroups.TryAdd(connectionId, new HashSet<string>());
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var connectionId = Context.ConnectionId;
            
            _logger.LogInformation($"User {userId} disconnected. Connection ID: {connectionId}. Exception: {exception?.Message}");
            
            if (_connectionGroups.TryRemove(connectionId, out var groups))
            {
                foreach (var group in groups)
                {
                    try
                    {
                        await Groups.RemoveFromGroupAsync(connectionId, group);
                        _logger.LogInformation($"Removed connection {connectionId} from group {group} during disconnect");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error removing from group {group} during disconnect: {ex.Message}");
                    }
                }
            }
            
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                
                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChatRoom(int chatRoomId)
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var connectionId = Context.ConnectionId;
            
            _logger.LogInformation($"User {userId} attempting to join chat room {chatRoomId} with connection {connectionId}");
            
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(chatRoomId);

            if (chatRoom == null)
            {
                _logger.LogWarning($"Chat room {chatRoomId} not found");
                await Clients.Caller.SendAsync("Error", $"Chat room {chatRoomId} not found");
                return;
            }

            if (!chatRoom.Users.Any(u => u.UserId == userId))
            {
                _logger.LogWarning($"User {userId} is not a member of chat room {chatRoomId}");
                await Clients.Caller.SendAsync("Error", "You are not a member of this chat room");
                return;
            }

            try
            {
                string groupName = $"chatroom_{chatRoomId}";
                
                _connectionGroups.AddOrUpdate(
                    connectionId,
                    new HashSet<string> { groupName },
                    (_, groups) =>
                    {
                        groups.Add(groupName);
                        return groups;
                    });
                
                await Groups.AddToGroupAsync(connectionId, groupName);
                
                await Clients.Caller.SendAsync("JoinedChatRoom", chatRoomId);
                _logger.LogInformation($"User {userId} joined chat room {chatRoomId}");
                
                var recentMessages = await _chatRepository.GetMessagesForChatRoomAsync(chatRoomId, 1, 20);
                
                if (recentMessages != null && recentMessages.Any())
                {
                    var messageDtos = new List<MessageDto>();
                    
                    foreach (var message in recentMessages)
                    {
                        var sender = await _userRepository.GetUserByIdAsync(message.SenderId);
                        
                        messageDtos.Add(new MessageDto
                        {
                            Id = message.Id,
                            Content = message.Content,
                            SentAt = message.SentAt,
                            SenderUserName = sender.UserName!,
                            SenderFullName = $"{sender.FirstName} {sender.LastName}",
                            ChatRoomId = message.ChatRoomId!.Value,
                            SentimentLabel = message.SentimentLabel
                        });
                    }
                    
                    _logger.LogInformation($"Sending {messageDtos.Count} messages to user {userId}");
                    await Clients.Caller.SendAsync("LoadRecentMessages", messageDtos);
                }
                
                var user = await _userRepository.GetUserByIdAsync(userId);
                
                await Clients.OthersInGroup(groupName)
                    .SendAsync("UserJoined", new { 
                        chatRoomId, 
                        userId, 
                        userName = user.UserName 
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in JoinChatRoom: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Failed to join chat room: {ex.Message}");
            }
        }

        public async Task LeaveChatRoom(int chatRoomId)
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var connectionId = Context.ConnectionId;
            
            _logger.LogInformation($"User {userId} leaving chat room {chatRoomId} with connection {connectionId}");

            try
            {
                string groupName = $"chatroom_{chatRoomId}";
                
                var user = await _userRepository.GetUserByIdAsync(userId);
                await Clients.OthersInGroup(groupName)
                    .SendAsync("UserLeft", new { 
                        chatRoomId, 
                        userId, 
                        userName = user.UserName 
                    });
                
                if (_connectionGroups.TryGetValue(connectionId, out var groups))
                {
                    groups.Remove(groupName);
                }
                
                await Groups.RemoveFromGroupAsync(connectionId, groupName);
                await Clients.Caller.SendAsync("LeftChatRoom", chatRoomId);
                
                _logger.LogInformation($"User {userId} left chat room {chatRoomId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in LeaveChatRoom: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Failed to leave chat room: {ex.Message}");
            }
        }

        public async Task SendMessage(CreateMessageDto messageDto)
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var connectionId = Context.ConnectionId;
            
            _logger.LogInformation($"User {userId} sending message to chat room {messageDto.ChatRoomId} with connection {connectionId}");
            
            var user = await _userRepository.GetUserByIdAsync(userId);
            var chatRoom = await _chatRepository.GetChatRoomByIdAsync(messageDto.ChatRoomId);

            if (chatRoom == null)
            {
                _logger.LogWarning($"Chat room {messageDto.ChatRoomId} not found");
                await Clients.Caller.SendAsync("Error", $"Chat room {messageDto.ChatRoomId} not found");
                return;
            }

            if (!chatRoom.Users.Any(u => u.UserId == userId))
            {
                _logger.LogWarning($"User {userId} is not a member of chat room {messageDto.ChatRoomId}");
                await Clients.Caller.SendAsync("Error", "You are not a member of this chat room");
                return;
            }

            try
            {
                string groupName = $"chatroom_{messageDto.ChatRoomId}";
                
                bool isInGroup = false;
                if (_connectionGroups.TryGetValue(connectionId, out var groups))
                {
                    isInGroup = groups.Contains(groupName);
                }
                
                if (!isInGroup)
                {
                    _logger.LogWarning($"User {userId} trying to send message but not in group {groupName}. Re-joining.");
                    
                    _connectionGroups.AddOrUpdate(
                        connectionId,
                        new HashSet<string> { groupName },
                        (_, existingGroups) =>
                        {
                            existingGroups.Add(groupName);
                            return existingGroups;
                        });
                    
                    await Groups.AddToGroupAsync(connectionId, groupName);
                }

                var (sentimentScore, sentimentLabel) = await _sentimentAnalysisService.AnalyzeSentimentAsync(messageDto.Content);

                var message = new Message
                {
                    Content = messageDto.Content,
                    SentAt = DateTime.UtcNow,
                    SenderId = userId,
                    ChatRoomId = messageDto.ChatRoomId,
                    SentimentScore = sentimentScore,
                    SentimentLabel = sentimentLabel
                };

                await _chatRepository.AddMessageAsync(message);

                var messageToReturn = new MessageDto
                {
                    Id = message.Id,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    SenderUserName = user.UserName!,
                    SenderFullName = $"{user.FirstName} {user.LastName}",
                    ChatRoomId = message.ChatRoomId.Value,
                    SentimentLabel = message.SentimentLabel
                };

                _logger.LogInformation($"Broadcasting message to group {groupName}");
                
                await Clients.Group(groupName).SendAsync("ReceiveMessage", messageToReturn);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendMessage: {ex.Message}");
                await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
            }
        }

        public async Task UserIsTyping(int chatRoomId)
        {
            try
            {
                var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                var user = await _userRepository.GetUserByIdAsync(userId);
                
                string groupName = $"chatroom_{chatRoomId}";
                
                await Clients.OthersInGroup(groupName).SendAsync("UserTyping", new {
                    userId,
                    userName = user.UserName,
                    chatRoomId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserIsTyping: {ex.Message}");
            }
        }

        public async Task UserStoppedTyping(int chatRoomId)
        {
            try
            {
                var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                var user = await _userRepository.GetUserByIdAsync(userId);
                
                string groupName = $"chatroom_{chatRoomId}";
                
                await Clients.OthersInGroup(groupName).SendAsync("UserStoppedTyping", new {
                    userId,
                    userName = user.UserName,
                    chatRoomId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UserStoppedTyping: {ex.Message}");
            }
        }

        public async Task GetConnectionInfo()
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var connectionId = Context.ConnectionId;
            var chatRooms = await _chatRepository.GetChatRoomsAsync(userId);
            
            var activeGroups = new List<string>();
            if (_connectionGroups.TryGetValue(connectionId, out var groups))
            {
                activeGroups = groups.ToList();
            }
            
            await Clients.Caller.SendAsync("ConnectionInfo", new { 
                connectionId = connectionId,
                userId = userId,
                availableChatRooms = chatRooms.Select(cr => cr.Id).ToList(),
                activeGroups = activeGroups
            });
        }
        
        public async Task Heartbeat()
        {
            var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var connectionId = Context.ConnectionId;
            
            _logger.LogDebug($"Heartbeat received from user {userId} with connection {connectionId}");
            
            try
            {
                if (_connectionGroups.TryGetValue(connectionId, out var groups))
                {
                    foreach (var group in groups)
                    {
                        try 
                        {
                            await Groups.AddToGroupAsync(connectionId, group);
                            _logger.LogDebug($"Re-added connection {connectionId} to group {group} during heartbeat");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error re-adding to group {group} during heartbeat: {ex.Message}");
                        }
                    }
                }
                
                await Clients.Caller.SendAsync("HeartbeatResponse", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Heartbeat: {ex.Message}");
            }
        }
    }
}