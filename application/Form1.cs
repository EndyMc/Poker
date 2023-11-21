using Poker.application.online;

namespace Poker {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            API.Connect();
        }
    }
}