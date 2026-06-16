using PetCenterAPI.Models;
using System.Linq.Expressions;

namespace PetCenterAPI.Common
{
        public class BrandSpecification
        {
            public string? Search { get; set; }
            public Status? Status { get; set; }

            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;

            public Expression<Func<Brand, bool>> ToExpression()
            {
                return b =>
                    (string.IsNullOrEmpty(Search) ||
                     b.BrandName.Contains(Search)) &&

                    (!Status.HasValue ||
                     b.Status == Status);
            }
        }

        public class CategorySpecification
        {
            public string? Search { get; set; }
            public Status? Status { get; set; }

            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;

            public Expression<Func<Category, bool>> ToExpression()
            {
                return c =>
                    (string.IsNullOrEmpty(Search) ||
                     c.CategoryName.Contains(Search)) &&

                    (!Status.HasValue ||
                     c.Status == Status);
            }
        }

        public class ProductSpecification
        {
            public string? Search { get; set; }

            public Status? Status { get; set; }

            public decimal? MinPrice { get; set; }
            public decimal? MaxPrice { get; set; }

            public Guid? BrandId { get; set; }
            public Guid? CategoryId { get; set; }

            public string? SortBy { get; set; }
            public string SortOrder { get; set; } = "asc";

            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }

            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;

            public Expression<Func<Product, bool>> ToExpression()
            {
                return p =>
                    (string.IsNullOrEmpty(Search) ||
                     p.ProductName.Contains(Search)) &&

                    (!Status.HasValue ||
                     p.Status == Status) &&

                    (!MinPrice.HasValue ||
                     p.ProductPrice >= MinPrice) &&

                    (!MaxPrice.HasValue ||
                     p.ProductPrice <= MaxPrice) &&

                    (!BrandId.HasValue ||
                     p.BrandId == BrandId) &&

                    (!CategoryId.HasValue ||
                     p.CategoryId == CategoryId) &&

                    (!FromDate.HasValue ||
                     p.AddedAt >= FromDate) &&

                    (!ToDate.HasValue ||
                     p.AddedAt <= ToDate);
            }
        }

    public class ServiceSpecification
    {
        public string? Search { get; set; }

        public Status? Status { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int? MinDuration { get; set; }
        public int? MaxDuration { get; set; }

        public int? ServiceType { get; set; }

        public string? SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public Expression<Func<Models.Service, bool>> ToExpression()
        {
            return s =>
                (string.IsNullOrEmpty(Search) ||
                 s.ServiceName.Contains(Search)) &&

                (!Status.HasValue ||
                 s.Status == Status) &&

                (!MinPrice.HasValue ||
                 s.Price >= MinPrice.Value) &&

                (!MaxPrice.HasValue ||
                 s.Price <= MaxPrice.Value) &&

                (!MinDuration.HasValue ||
                 s.Duration >= MinDuration.Value) &&

                (!MaxDuration.HasValue ||
                 s.Duration <= MaxDuration.Value) &&

                (!ServiceType.HasValue ||
                 s.ServiceType == ServiceType.Value);
        }
    }
}