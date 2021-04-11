using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GVCServer.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
        private readonly ILogger logger;

        public ErrorController(ILogger logger)
        {
            this.logger = logger;
        }

        [Route("/error-local-development")]
        public IActionResult ErrorLocalDevelopment(
        [FromServices] IWebHostEnvironment webHostEnvironment)
        {
            if (webHostEnvironment.EnvironmentName != "Development")
            {
                throw new InvalidOperationException(
                    "This shouldn't be invoked in non-development environments.");
            }
            
            var context = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            logger.LogError(context.Error.Message);
            return Problem(
                detail: context.Error.StackTrace,
                title: context.Error.Message);
        }

        [Route("error")]
        public IActionResult Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            logger.LogError(context.Error.Message);
            return  Problem(); 
        }

    }
}
