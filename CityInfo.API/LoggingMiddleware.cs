using System.Text;

namespace CityInfo.API
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            // Log the request details
            LogRequest(context.Request);

            // Capture the response body
            var originalBodyStream = context.Response.Body;
            using (var responseBody = new MemoryStream())
            {
                context.Response.Body = responseBody;

                // Call the next middleware in the pipeline
                await _next(context);
                Console.WriteLine(context);


                // Log the response details
                LogResponse(context.Response);

                // Copy the captured response body to the original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private void LogRequest(HttpRequest request)
        {
            // Log request details such as method, path, headers, etc.
            var logMessage = new StringBuilder();
            logMessage.AppendLine($"Request Method: {request.Method}");
            logMessage.AppendLine($"Request Path: {request.Path}");

            foreach (var header in request.Headers)
            {
                logMessage.AppendLine($"{header.Key}: {header.Value}");
            }

            _logger.LogInformation(logMessage.ToString());
        }

        private void LogResponse(HttpResponse response)
        {
            // Log response details such as status code, headers, etc.
            var logMessage = new StringBuilder();
            logMessage.AppendLine($"Response Status Code: {response.StatusCode}");

            foreach (var header in response.Headers)
            {
                logMessage.AppendLine($"{header.Key}: {header.Value}");
            }

            _logger.LogInformation(logMessage.ToString());
        }
    }

    public static class LoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}
