namespace StaffAPI.Security
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string email, string fullName, List<string> roles);
    }
}

