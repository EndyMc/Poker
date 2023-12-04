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

        public string GetSuiteAsString() {
            switch(this.Suite) {
                case CardType.Diamond:
                    return "D";
                case CardType.Heart:
                    return "H";
                case CardType.Spade:
                    return "S";
                case CardType.Clover:
                    return "C";
                default:
                    return "Err";
            }
        }

        public string ToFilepath(string? basePath) {
            return (basePath != null ? Path.Combine(basePath, GetSuiteAsString() + this.Value) : (GetSuiteAsString() + this.Value)) + ".png";
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }
}
