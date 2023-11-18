using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace Poker.com.endy.poker.application.online {
    public class API {
        private static Uri BASE_URI = new("ws://localhost:9000");

        private static ClientWebSocket WEBSOCKET = new();
        private static ConcurrentQueue<string> WEBSOCKET_MESSAGES = new();

        public static AutoResetEvent INCOMING_MESSAGE_EVENT = new AutoResetEvent(false);

        public static async void Connect() {
            Debug.WriteLine("Connecting to the websocket at address: " + BASE_URI.ToString());
            await WEBSOCKET.ConnectAsync(BASE_URI, CancellationToken.None);
            Debug.WriteLine("WebSocket connected");


            Thread websocketHandler = new(async () => {
                try {
                    while (true) {
                        byte[] buffer = new byte[1024 * 4];
                        var result = await WEBSOCKET.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close) {
                            Debug.WriteLine("WebSocket Closed from other part");
                            break;
                        }

                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        WEBSOCKET_MESSAGES.Enqueue(message);
                        INCOMING_MESSAGE_EVENT.Set();

                        Debug.WriteLine("Received: " + message);
                    }
                } catch (WebSocketException ex) {
                    Debug.WriteLine(ex.Message);
                }
            });

            websocketHandler.Name = "WebSocketHandler-Thread";
            websocketHandler.Start();
        }

        public static void Bet() {

        }

        public static void EndTurn() {

        }

        public static void Fold() {

        }
    }
}
