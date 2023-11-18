﻿using Newtonsoft.Json;

namespace Poker.com.endy.poker.application.online {
    public class WebsocketMessage {
        public String Type { get; }
        public Dictionary<string, string> Data = new();

        public WebsocketMessage(String type) {
            this.Type = type;
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }
}