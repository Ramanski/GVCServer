using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Models
{
    public class HttpResponseException : Exception
    {
        public int Status { get; set; } = 500;
        public string Value { get; set; }
    }

    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order { get; set; } = int.MaxValue - 10;
        public ILogger<HttpResponseExceptionFilter> Logger { get; }

        public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger)
        {
            Logger = logger;
        }

        public HttpResponseExceptionFilter()
        {
        }

        public void OnActionExecuting(ActionExecutingContext context) 
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if(context.Exception != null)
            {
                if (context.Exception is HttpResponseException exception)
                {
                    context.Result = new ObjectResult(exception.Value)
                    {
                        StatusCode = exception.Status
                    };
                    context.ExceptionHandled = true;
                    Logger.LogWarning(context.Exception, "");
                }
                else
                {
                    Logger.LogError(context.Exception, "Unexpected error on action exexuted");
                }
            }
        }
    }
}
