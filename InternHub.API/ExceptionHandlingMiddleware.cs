using System.Net;

namespace InternHub.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            int statusCode = (int)HttpStatusCode.InternalServerError;
            string message = "An unexpected error occurred.";

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                (statusCode, message) = ex switch
                {
                    KeyNotFoundException => ((int)HttpStatusCode.NotFound, ex.Message),
                    UnauthorizedAccessException => ((int)HttpStatusCode.Forbidden, ex.Message), 
                    InvalidOperationException => ((int)HttpStatusCode.BadRequest, ex.Message), 
                    
                    _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.")
                };

                context.Response.StatusCode = statusCode;

                if (statusCode == (int)HttpStatusCode.InternalServerError)
                {
                    Console.WriteLine($"Unhandled exception: {ex.GetType()} - {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
                
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = message });
            }
        }
    }
}