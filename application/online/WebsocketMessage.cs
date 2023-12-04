using Newtonsoft.Json;

namespace Poker.application.online {
    public class WebsocketMessage {
        public String Type { get; }
        public Dictionary<string, object> Data = new();

        public WebsocketMessage(String type) {
            this.Type = type;
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }
}
