using PetCenterAPI.Common;

namespace PetCenterAPI.DTOs.Requests.Service
{
    public class ServiceRequestDTO
    {
        public class ReadServiceDTO
        {
            public Guid ServiceId { get; set; }

            public string ServiceName { get; set; } = null!;

            public decimal Price { get; set; }

            public Status Status { get; set; }

            public string? ServiceDescription { get; set; }

            public int Duration { get; set; }

            public int ServiceType { get; set; }

            public List<string> ImageFiles { get; set; } = new();
        }

        public class ReadServiceDTOForCustomer
        {
            public Guid ServiceId { get; set; }

            public string ServiceName { get; set; } = null!;

            public decimal Price { get; set; }


            public string? ServiceDescription { get; set; }

            public int Duration { get; set; }

            public int ServiceType { get; set; }

            public List<string> ImageFiles { get; set; } = new();
        }

    }
}
