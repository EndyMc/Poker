using Poker.com.endy.poker.application.online;
using System.Diagnostics;

namespace Poker {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            API.Connect();
        }
    }
}