using Poker.application;
using Poker.application.online;
using System.Diagnostics;

namespace Poker {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();

            Card card = new(5, Card.CardType.Heart);
            Debug.WriteLine(card);
            Debug.WriteLine(card.ToFilepath(null));
            Debug.WriteLine(card.ToFilepath("G:/Downloads"));
        }
    }
}