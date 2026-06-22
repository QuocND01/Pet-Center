using PetCenterAPI.Models;
using System.Text.RegularExpressions;

namespace PetCenterAPI.Helpers
{
    public static class SkuGenerator
    {
        public static string Generate(Product product)
        {
            string brandCode = GetCode(product.Brand?.BrandName);
            string nameCode = product.ProductName.Contains(' ')
                ? GetShort(product.ProductName)
                : GetCode(product.ProductName);

            string attributeCode = "GEN";

            if (product.ProductAttributes.Any())
            {
                attributeCode = string.Concat(
                    product.ProductAttributes
                        .Where(x => !string.IsNullOrWhiteSpace(x.AttributeValue))
                        .Select(x => GetCode(x.AttributeValue))
                );

                if (string.IsNullOrWhiteSpace(attributeCode))
                {
                    attributeCode = "GEN";
                }
            }

            //string dateCode = DateTime.UtcNow.ToString("yyMMdd");

            string randomCode = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 4)
                .ToUpper();

            return $"{brandCode}-{nameCode}-{attributeCode}-{randomCode}";
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
        private static string GetShort(string text)
        {
            return string.Concat(
                text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(word => char.ToUpper(word[0]))
            );
        }
    }
}
