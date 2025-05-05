namespace ReenbitTest.Core.DTOs
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Token { get; set; } = null!;
        public UserDto User { get; set; }  = null!;
        public List<string>? Errors { get; set; }
    }
}