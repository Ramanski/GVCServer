using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GVCServer.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {
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
            return Problem(
                detail: context.Error.StackTrace,
                title: context.Error.Message);
        }

        [Route("error")]
        public IActionResult Error() => Problem();
/*        public MyErrorResponse Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context?.Error; // Your exception
            var code = 500; // Internal Server Error by default

            if (exception is MyNotFoundException) code = 404; // Not Found
            else if (exception is MyUnauthorizedException) code = 401; // Unauthorized
            else if (exception is MyException) code = 400; // Bad Request

            Response.StatusCode = code; // You can use HttpStatusCode enum instead

            return new MyErrorResponse(exception); // Your error model
        }*/
    }
}
