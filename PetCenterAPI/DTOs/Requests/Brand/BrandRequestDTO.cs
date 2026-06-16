using PetCenterAPI.Common;

namespace PetCenterAPI.DTOs.Requests.Brand
{
    public class BrandRequestDTO
    {
        public class ReadBrandDTO
        {
            public Guid BrandId { get; set; }
            public string BrandName { get; set; } = null!;
            public string? BrandDescription { get; set; }

            public string? BrandLogo { get; set; }

            public Status Status { get; set; }
        }


        public class ReadBrandDTOForCustomer
        {
            public Guid BrandId { get; set; }
            public string BrandName { get; set; } = null!;
            public string? BrandDescription { get; set; }

            public string? BrandLogo { get; set; }
        }

    }
}
