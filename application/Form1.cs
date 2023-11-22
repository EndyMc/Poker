using Poker.application;
using Poker.application.online;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace Poker {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            API.Connect();
            int x = this.Width / 16;
            int y = this.Height / 9;

            if (x >= y) {
                DrawScale = Convert.ToDouble(this.Width) / 1366;
            } else {
                DrawScale = Convert.ToDouble(this.Height) / 768;
            }
            Invalidate();
            Test(8, 1);
        }

        List<List<Card>> Players = new List<List<Card>>();

        double DrawScale = 1.0;
        Point CardSize = new Point(100, 140);
        int PlayerIndex;

        private void Form1_Resize(object sender, EventArgs e) {
            double x = this.Width / 16;
            double y = this.Height / 9;

            if (x >= y) {
                DrawScale = Convert.ToDouble(this.Width) / 1366;
            } else {
                DrawScale = Convert.ToDouble(this.Height) / 768;
            }
            Invalidate();
        }

        private void Test(int PlayerCount, int PlayerIndex) {
            Random rnd = new Random();;
            for (int i = 0; i <= PlayerCount; i++) {
                List<Card> player = new List<Card>();
                for (int x = 0; x <= 2; x++) {
                    player.Add(new Card(rnd.Next(1, 13), Card.CardType.Heart));
                }
                Players.Add(player);
            }
            foreach(List<Card> Player in Players) {
                Debug.WriteLine(Player[0] + " " + Player[1] + "\n");
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.DrawImage(Image.FromFile("C:\\Users\\Johan Hallin\\Downloads\\S1.png"), new Rectangle(Convert.ToInt32(100 * DrawScale), Convert.ToInt32(100 * DrawScale), Convert.ToInt32(CardSize.X * DrawScale), Convert.ToInt32(CardSize.Y * DrawScale)));
        }
    }
}