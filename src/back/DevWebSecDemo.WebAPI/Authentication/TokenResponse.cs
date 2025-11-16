namespace DevWebSecDemo.WebAPI.Authentication
{
    public class TokenResponse
    {
        public required string Token { get; set; }
        
        public required DateTime Expires { get; set; }
    }    
}
