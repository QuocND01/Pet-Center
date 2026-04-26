namespace StaffAPI.Security
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string email, List<string> roles);
    }
}
