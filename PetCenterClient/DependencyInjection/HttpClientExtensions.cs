using PetCenterClient.Services;
using PetCenterClient.Services.Interface;
using System.Reflection;

namespace PetCenterClient.DependencyInjection
{
    /// <summary>
    /// Các phương thức mở rộng (Extension Methods) giúp đăng ký tự động và quản lý các Http API Client của Client App.
    /// </summary>
    public static class HttpClientExtensions
    {
        /// <summary>
        /// Tự động đăng ký toàn bộ các API Client sử dụng HttpClient và HttpClientFactory.
        /// Quét tất cả các class có tên kết thúc bằng Client, Service, ApiService, APIClient...
        /// </summary>
        public static IServiceCollection AddApplicationHttpClients(this IServiceCollection services, string apiUrl)
        {
            // Lấy assembly hiện tại chứa code Client
            var assembly = typeof(HttpClientExtensions).Assembly;
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
                .ToList();

            // Các từ khóa kết thúc của tên class được coi là các client gọi API
            var clientSuffixes = new[] { "APIClient", "ServiceClient", "ApiService", "ApiClient", "Client" };

            // Lấy các MethodInfo cho AddTypedClient bằng Reflection
            var methods = typeof(HttpClientBuilderExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static);
            
            // Overload 1: AddTypedClient<TClient, TImplementation>(this IHttpClientBuilder builder)
            var addTypedClientTwoArgs = methods.FirstOrDefault(m => 
                m.Name == "AddTypedClient" && 
                m.IsGenericMethod && 
                m.GetGenericArguments().Length == 2 && 
                m.GetParameters().Length == 1);

            // Overload 2: AddTypedClient<TClient>(this IHttpClientBuilder builder)
            var addTypedClientOneArg = methods.FirstOrDefault(m => 
                m.Name == "AddTypedClient" && 
                m.IsGenericMethod && 
                m.GetGenericArguments().Length == 1 && 
                m.GetParameters().Length == 1);

            foreach (var type in types)
            {
                // Bỏ qua các service logic thông thường, không gọi qua mạng (Excel, Google API)
                // và bỏ qua InventoryApiService vì nó tự cấu hình BaseAddress trong Constructor
                if (type.Name == "ExcelService" || type.Name == "GoogleAPIClient" || type.Name == "InventoryApiService")
                {
                    continue;
                }

                // Kiểm tra xem class có khớp với hậu tố HttpClient hoặc có tên dịch vụ cụ thể
                bool isHttpClient = clientSuffixes.Any(suffix => type.Name.EndsWith(suffix))
                                    || type.Name == "StaffService" 
                                    || type.Name == "CartService" 
                                    || type.Name == "ImportStockService" 
                                    || type.Name == "CheckoutService";

                if (isHttpClient)
                {
                    // Quy ước: Tên interface bắt đầu bằng chữ "I" + tên Class (ví dụ: IBrandAPIClient cho BrandAPIClient)
                    var interfaceName = "I" + type.Name;
                    var matchingInterface = type.GetInterfaces()
                        .FirstOrDefault(i => i.Name == interfaceName);

                    var httpClientBuilder = services.AddHttpClient(matchingInterface != null ? matchingInterface.Name : type.Name, client =>
                    {
                        client.BaseAddress = new Uri(apiUrl);
                    });

                    if (matchingInterface != null && addTypedClientTwoArgs != null)
                    {
                        // Đăng ký HttpClient qua Interface (TInterface, TImplementation)
                        var genericMethod = addTypedClientTwoArgs.MakeGenericMethod(matchingInterface, type);
                        genericMethod.Invoke(null, new object[] { httpClientBuilder });
                    }
                    else if (addTypedClientOneArg != null)
                    {
                        // Đăng ký HttpClient trực tiếp Class (TImplementation)
                        var genericMethod = addTypedClientOneArg.MakeGenericMethod(type);
                        genericMethod.Invoke(null, new object[] { httpClientBuilder });
                    }
                }
            }

            // ĐĂNG KÝ CÁC DỊCH VỤ NGOẠI LỆ (Không sử dụng HttpClient)
            
            // ExcelService xử lý logic file Excel
            services.AddScoped<ExcelService>();
            
            // GoogleAPIClient đăng ký dạng Scoped thông thường
            services.AddScoped<IGoogleAPIClient, GoogleAPIClient>();

            // InventoryApiService đăng ký HttpClient thủ công không set BaseAddress ở đây
            // vì class này tự set BaseAddress trong Constructor của nó.
            services.AddHttpClient<InventoryApiService>();

            return services;
        }
    }
}
