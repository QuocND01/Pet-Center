using PetCenterAPI.Models;
using System.Text.RegularExpressions;

namespace PetCenterAPI.Helpers
{
    public static class SkuGenerator
    {
        public static string Generate(Product product)
        {
            string brandCode = GetCode(product.Brand?.BrandName);
            string categoryCode = GetCode(product.Category?.CategoryName);

            string attributeCode = "GEN";

            if (product.ProductAttributes.Any())
            {
                attributeCode = GetCode(
                    product.ProductAttributes.First().AttributeValue
                );
            }

            string dateCode = DateTime.UtcNow.ToString("yyMMdd");

            string randomCode = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 4)
                .ToUpper();

            return $"{brandCode}-{categoryCode}-{attributeCode}-{dateCode}-{randomCode}";
        }

        private static string GetCode(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "UNK";

            value = Regex.Replace(value.ToUpper(), @"[^A-Z0-9]", "");

            return value.Length <= 3
                ? value
                : value.Substring(0, 3);
        }
    }
}
