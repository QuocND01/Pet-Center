namespace IdentityAPI.Security
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId,string email, List<string> roles);
    }
}
