namespace Pet_Center.Security
{
    public interface IJwtService
    {
        string GenerateToken(string email, List<string> roles);
    }
}
