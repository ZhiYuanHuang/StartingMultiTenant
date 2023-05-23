using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using IdentityServer.MultiTenant.Dto;

namespace IdentityServer.MultiTenant.Filter
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        readonly ILogger<GlobalExceptionFilter> _logger;
        readonly IWebHostEnvironment _env;
        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger
            , IWebHostEnvironment env) {
            _logger = logger;
            _env = env;
        }
        public void OnException(ExceptionContext filterContext) {
            _logger.LogError(new EventId(filterContext.Exception.HResult), filterContext.Exception, filterContext.Exception.Message);

            AppResponseDto appResponseDto = new AppResponseDto(false) { ErrorMsg = "未知错误" };

            Exception exception = filterContext.Exception;
            HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError;
            if (exception is ArgumentException) {
                httpStatusCode = HttpStatusCode.BadRequest;
            } else if (exception is ArgumentNullException) {
                httpStatusCode = HttpStatusCode.BadRequest;
            } else {
                httpStatusCode = HttpStatusCode.InternalServerError;
            }

            filterContext.Result = new JsonResult(appResponseDto);
            filterContext.HttpContext.Response.StatusCode = (int)httpStatusCode;
            filterContext.ExceptionHandled = true;
        }
    }
}
