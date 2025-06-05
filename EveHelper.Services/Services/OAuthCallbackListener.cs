using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace EveHelper.Services.Services
{
    /// <summary>
    /// Simple HTTP listener for handling OAuth callbacks
    /// </summary>
    public class OAuthCallbackListener : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly string _callbackUrl;
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// Event raised when an OAuth callback is received
        /// </summary>
        public event EventHandler<OAuthCallbackEventArgs>? CallbackReceived;

        /// <summary>
        /// Initializes a new instance of the OAuth callback listener
        /// </summary>
        /// <param name="callbackUrl">The callback URL to listen on</param>
        public OAuthCallbackListener(string callbackUrl)
        {
            _callbackUrl = callbackUrl ?? throw new ArgumentNullException(nameof(callbackUrl));
            _listener = new HttpListener();
            
            // Add the callback URL as a prefix
            _listener.Prefixes.Add(callbackUrl.EndsWith("/") ? callbackUrl : callbackUrl + "/");
        }

        /// <summary>
        /// Starts listening for OAuth callbacks
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task representing the listening operation</returns>
        public async Task StartListeningAsync(CancellationToken cancellationToken = default)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            try
            {
                _listener.Start();
                
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        var context = await _listener.GetContextAsync();
                        _ = Task.Run(() => HandleCallback(context), _cancellationTokenSource.Token);
                    }
                    catch (HttpListenerException)
                    {
                        // Listener was stopped
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        // Listener was disposed
                        break;
                    }
                }
            }
            catch (Exception)
            {
                // Handle any startup errors
                throw;
            }
        }

        /// <summary>
        /// Stops listening for OAuth callbacks
        /// </summary>
        public void StopListening()
        {
            _cancellationTokenSource?.Cancel();
            
            if (_listener.IsListening)
            {
                _listener.Stop();
            }
        }

        /// <summary>
        /// Handles an incoming OAuth callback
        /// </summary>
        /// <param name="context">HTTP context</param>
        private async Task HandleCallback(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                // Extract query parameters
                var query = request.Url?.Query;
                if (string.IsNullOrEmpty(query))
                {
                    await SendErrorResponse(response, "No query parameters received");
                    return;
                }

                var queryParams = ParseQueryString(query);
                var code = queryParams["code"];
                var state = queryParams["state"];
                var error = queryParams["error"];
                var errorDescription = queryParams["error_description"];

                // Check for OAuth errors
                if (!string.IsNullOrEmpty(error))
                {
                    var errorMessage = $"OAuth error: {error}";
                    if (!string.IsNullOrEmpty(errorDescription))
                    {
                        errorMessage += $" - {errorDescription}";
                    }

                    await SendErrorResponse(response, errorMessage);
                    CallbackReceived?.Invoke(this, new OAuthCallbackEventArgs
                    {
                        IsSuccess = false,
                        ErrorMessage = errorMessage
                    });
                    return;
                }

                // Check for authorization code
                if (string.IsNullOrEmpty(code))
                {
                    await SendErrorResponse(response, "No authorization code received");
                    CallbackReceived?.Invoke(this, new OAuthCallbackEventArgs
                    {
                        IsSuccess = false,
                        ErrorMessage = "No authorization code received"
                    });
                    return;
                }

                // Send success response to browser
                await SendSuccessResponse(response);

                // Raise callback event
                CallbackReceived?.Invoke(this, new OAuthCallbackEventArgs
                {
                    IsSuccess = true,
                    AuthorizationCode = code,
                    State = state
                });
            }
            catch (Exception ex)
            {
                try
                {
                    await SendErrorResponse(context.Response, $"Internal error: {ex.Message}");
                }
                catch
                {
                    // Ignore errors when sending error response
                }

                CallbackReceived?.Invoke(this, new OAuthCallbackEventArgs
                {
                    IsSuccess = false,
                    ErrorMessage = $"Internal error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Sends a success response to the browser
        /// </summary>
        /// <param name="response">HTTP response</param>
        private static async Task SendSuccessResponse(HttpListenerResponse response)
        {
            const string html = @"
<!DOCTYPE html>
<html>
<head>
    <title>EVE Helper - Authentication Success</title>
    <style>
        body { font-family: Arial, sans-serif; text-align: center; padding: 50px; background: #0a0a0a; color: #ffffff; }
        .container { max-width: 500px; margin: 0 auto; }
        .success { color: #4CAF50; font-size: 24px; margin-bottom: 20px; }
        .message { font-size: 16px; line-height: 1.5; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='success'>✓ Authentication Successful</div>
        <div class='message'>
            You have successfully authenticated with EVE Online.<br/>
            You can now close this window and return to EVE Helper.
        </div>
    </div>
    <script>
        // Auto-close after 3 seconds
        setTimeout(function() { window.close(); }, 3000);
    </script>
</body>
</html>";

            response.ContentType = "text/html";
            response.StatusCode = 200;
            
            var buffer = System.Text.Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;
            
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        /// <summary>
        /// Sends an error response to the browser
        /// </summary>
        /// <param name="response">HTTP response</param>
        /// <param name="errorMessage">Error message to display</param>
        private static async Task SendErrorResponse(HttpListenerResponse response, string errorMessage)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>EVE Helper - Authentication Error</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding: 50px; background: #0a0a0a; color: #ffffff; }}
        .container {{ max-width: 500px; margin: 0 auto; }}
        .error {{ color: #f44336; font-size: 24px; margin-bottom: 20px; }}
        .message {{ font-size: 16px; line-height: 1.5; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='error'>✗ Authentication Failed</div>
        <div class='message'>
                         {System.Net.WebUtility.HtmlEncode(errorMessage)}<br/>
            Please close this window and try again.
        </div>
    </div>
</body>
</html>";

            response.ContentType = "text/html";
            response.StatusCode = 400;
            
            var buffer = System.Text.Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;
            
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        /// <summary>
        /// Parses a query string into a NameValueCollection
        /// </summary>
        /// <param name="query">Query string to parse</param>
        /// <returns>Parsed query parameters</returns>
        private static NameValueCollection ParseQueryString(string query)
        {
            var result = new NameValueCollection();
            
            if (string.IsNullOrEmpty(query))
                return result;
                
            // Remove leading '?' if present
            if (query.StartsWith("?"))
                query = query.Substring(1);
                
            var pairs = query.Split('&');
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = Uri.UnescapeDataString(keyValue[0]);
                    var value = Uri.UnescapeDataString(keyValue[1]);
                    result[key] = value;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Disposes the HTTP listener
        /// </summary>
        public void Dispose()
        {
            StopListening();
            _cancellationTokenSource?.Dispose();
            _listener?.Close();
        }
    }

    /// <summary>
    /// Event arguments for OAuth callback events
    /// </summary>
    public class OAuthCallbackEventArgs : EventArgs
    {
        /// <summary>
        /// Whether the callback was successful
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Authorization code if successful
        /// </summary>
        public string? AuthorizationCode { get; set; }

        /// <summary>
        /// State parameter if successful
        /// </summary>
        public string? State { get; set; }

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
} 