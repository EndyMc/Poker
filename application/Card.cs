using Newtonsoft.Json;

namespace Poker.application {
    public class Card {
        public enum CardType {
            Diamond,
            Heart,
            Spade,
            Clover
        }

        public int Value { get; }
        public CardType Suite { get; }

        public Card(int value, CardType suite) {
            // The value of the card (e.g. Ace)
            this.Value = value;

            // The suite of the card (e.g. Heart)
            this.Suite = suite;
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }
}
