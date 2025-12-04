using System.Net;

namespace InternHub.API.Middleware
{
    // Оскільки код Middleware був повторений, я надаю лише один виправлений примірник
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
                    // 🛑 ВИПРАВЛЕННЯ: UnauthorizedAccessException тепер мапиться на 403 Forbidden.
                    // Це виправляє помилки тесту, де очікується 403, але приходить 401.
                    UnauthorizedAccessException => ((int)HttpStatusCode.Forbidden, ex.Message), 
                    
                    // 🛑 ВИПРАВЛЕННЯ: InvalidOperationException тепер мапиться на 400 BadRequest.
                    // Це виправляє помилки тесту, де очікується 400, але приходить 403 
                    // (наприклад, при спробі видалити власника).
                    InvalidOperationException => ((int)HttpStatusCode.BadRequest, ex.Message), 
                    
                    _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.")
                };

                context.Response.StatusCode = statusCode;

                if (statusCode == (int)HttpStatusCode.InternalServerError)
                {
                    // Логування тут
                    Console.WriteLine($"Unhandled exception: {ex.GetType()} - {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                }
                
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = message });
            }
        }
    }
}