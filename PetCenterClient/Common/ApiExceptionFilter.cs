using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PetCenterClient.Common
{
    public class ApiExceptionFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            var exception = context.Exception;

            if (exception is HttpRequestException)
            {
                context.Result = new RedirectToActionResult(
                    "ServiceUnavailable",
                    "Error",
                    new
                    {
                        message = exception.Message
                    });

                context.ExceptionHandled = true;
            }

            return Task.CompletedTask;
        }
    }
}
