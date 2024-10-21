using openbrow.Models;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace openbrow
{
    internal class ChromeUtilities
    {
        internal static async Task FocusOrCreateTab(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                HttpClient client = new();
                string apiUrl = "http://127.0.0.1:9222/json";

                try
                {
                    // Send the GET request
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Check if the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    var chromeTabs = await response.Content.ReadFromJsonAsync<ChromeTab[]>();

                    if (chromeTabs != null && chromeTabs.Length > 0)
                    {
                        var chromeTab = chromeTabs.FirstOrDefault(ct => ct.Url?.Contains(uri.AbsoluteUri) ?? false);

                        if (chromeTab != null)
                        {
                            await SendWebSocketCommand(chromeTab.WebSocketDebuggerUrl, BringToFrontCommand());
                        }
                        else
                        {
                            await SendWebSocketCommand(chromeTabs.First().WebSocketDebuggerUrl, OpenNewTabCommand(uri.AbsoluteUri));
                        }
                    }
                }
                catch (HttpRequestException e)
                {
                    // Handle any errors that occur during the request
                    Console.WriteLine($"Request error: {e.Message}");
                }
            }
            else
            {
                Console.WriteLine("The url parameter is not a valid url");
                return;
            }
        }

        private static async Task SendWebSocketCommand(string? websocketUri, string command)
        {
            if (websocketUri == null)
            {
                return;
            }

            using ClientWebSocket client = new();
            // Connect to the WebSocket server
            try
            {
                await client.ConnectAsync(new Uri(websocketUri), CancellationToken.None);

                byte[] messageBuffer = Encoding.UTF8.GetBytes(command);
                await client.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);

                // Receive a message from the WebSocket server
                byte[] receiveBuffer = new byte[1024];
                WebSocketReceiveResult result = await client.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);

                // Close the WebSocket connection
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
        }

        private static string BringToFrontCommand()
        {
            // Create the command to focus on the tab
            var command = new
            {
                id = 1,
                method = "Page.bringToFront"
            };

            // Serialize the command to JSON
            return JsonSerializer.Serialize(command);
        }

        private static string OpenNewTabCommand(string url)
        {
            // Create the command to focus on the tab

            var parameters = new
            {
                url
            };

            var command = new
            {
                id = 1,
                method = "Target.createTarget",
                @params = parameters
            };

            // Serialize the command to JSON
            return JsonSerializer.Serialize(command);
        }
    }
}
