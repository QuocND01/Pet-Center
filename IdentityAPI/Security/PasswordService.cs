namespace IdentityAPI.Security
{
    public class PasswordService
    {
        public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

        public bool Verify(string password, string hash)
            => BCrypt.Net.BCrypt.Verify(password, hash);

        public string GenerateTemporaryPassword()
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "@#$%";

            var rng = new Random();

            // Đảm bảo đủ điều kiện: 1 hoa + 1 thường + 1 số + 1 ký tự đặc biệt
            var password = new[]
            {
                upper[rng.Next(upper.Length)],
                lower[rng.Next(lower.Length)],
                digits[rng.Next(digits.Length)],
                special[rng.Next(special.Length)]
            }.ToList();

            // Thêm 4 ký tự random nữa cho đủ 8 ký tự
            const string all = upper + lower + digits + special;
            for (int i = 0; i < 4; i++)
                password.Add(all[rng.Next(all.Length)]);

            // Shuffle để không bị đoán pattern
            return new string(password.OrderBy(_ => rng.Next()).ToArray());
        }
    }
}
