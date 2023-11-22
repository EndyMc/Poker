using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace Poker.application.online {
    public class API {
        private static Uri BASE_URI = new("ws://localhost:9000");

        private static ClientWebSocket WEBSOCKET = new();
        private static ConcurrentQueue<WebsocketMessage> WEBSOCKET_MESSAGES = new();

        /// <summary>
        /// If this is activated, that means that there is a message in the <c>WEBSOCKET_MESSAGES</c> queue which needs to be handled.
        /// </summary>
        public static AutoResetEvent INCOMING_MESSAGE_EVENT = new AutoResetEvent(false);

        // Needs to be fetched from the server when logging in
        private static string PlayerID = "Endy-" + (new Random().NextInt64().ToString("X"));

        /// <summary>
        /// Connect to the websocket server. This also sets up the eventhandler which is in charge of handling messages from the server and distributing them to the rest of the application.
        /// </summary>
        public static async Task Connect() {
            try {
                Debug.WriteLine("Connecting to the websocket at address: " + BASE_URI.ToString());
                WEBSOCKET.Options.SetRequestHeader("origin", "PokerClient (" + PlayerID + ")");
                WEBSOCKET.Options.SetRequestHeader("player", PlayerID);
                await WEBSOCKET.ConnectAsync(BASE_URI, CancellationToken.None);
                Debug.WriteLine("WebSocket connected");
            } catch(Exception ex) {
                Debug.WriteLine(ex.Message);
            }

            Thread websocketHandler = new(async () => {
                try {
                    while (true) {
                        byte[] buffer = new byte[1024 * 4];
                        var result = await WEBSOCKET.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close) {
                            Debug.WriteLine("WebSocket closed from other part");
                            break;
                        }

                        string str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        WebsocketMessage? message = JsonConvert.DeserializeObject<WebsocketMessage>(str);

                        // If the WebsocketMessage is null of some reason or the message was sent by this player, then ignore it.
                        if (message == null || message.Data.GetValueOrDefault<string, string>("Player", "") == PlayerID) continue;
                        Debug.WriteLine("Received: " + message);

                        if (message.Type == "close") {
                            OnWebsocketClose(message);
                            break;
                        }

                        WEBSOCKET_MESSAGES.Enqueue(message);
                        INCOMING_MESSAGE_EVENT.Set();

                    }
                } catch (WebSocketException ex) {
                    Debug.WriteLine(ex.Message);
                }
            });

            websocketHandler.Name = "WebSocketHandler-Thread";
            websocketHandler.Start();
        }

        /// <summary>
        /// Make a bet as the player. 
        ///
        /// <para>The player can either:</para>
        /// <para>* Make a forced bet, because of blinds</para>
        /// <para>* Raise the value of the highest bet.</para>
        /// <para>* Bet the same as the highest bet.</para>
        /// <para>* Not bet at all if their bet is equal to the highest bet.</para>
        /// <para>* Go all in. Which has some special rules, such as they not being able to win more than what they bet and that they aren't forced to bet again, since they don't have the money for it. If all players are all in (or have bet the same as the player which is all in and cannot raise it any further), the cards are all face up with no more bets being made.</para>
        /// </summary>
        /// <param name="amount">The amount of money the player has bet</param>
        public static async Task Bet(int amount) {
            // The player can either:
            // * Make a forced bet, because of blinds
            // * Raise the value of the highest bet.
            // * Bet the same as the highest bet.
            // * Not bet at all if their bet is equal to the highest bet.
            // * Go all in. Which has some special rules, such as they not being able to win more than what they bet and that they aren't forced to bet again, since they don't have the money for it. If all players are all in (or have bet the same as the player which is all in and cannot raise it any further), the cards are all face up with no more bets being made.

            WebsocketMessage message = new("bet");
            message.Data.Add("Player", PlayerID);
            message.Data.Add("BetAmount", "" + amount);

            byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            await WEBSOCKET.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// Make the player fold.
        /// 
        /// <para>If there's only one other player they win.</para>
        /// </summary>
        public static async Task Fold() {
            WebsocketMessage message = new("fold");
            message.Data.Add("Player", PlayerID);

            byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            await WEBSOCKET.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private static void OnWebsocketClose(WebsocketMessage message) {
            // Was disconnected from server, this can be either because:
            // * Duplicate name,
            // * Other unknown error

            // That should be returned to the user.
            MessageBox.Show(message.Data.GetValueOrDefault<string, string>("Reason", "Unknown network-error encountered"));
            Environment.Exit(0);
        }
    }
}
