namespace CustomerAPI.Security
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string email, List<string> roles, string fullName = "");
    }
}
