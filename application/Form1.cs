using Poker.application;
using Poker.application.online;
using System.CodeDom.Compiler;
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
        List<List<Image>> Cards = new List<List<Image>>();


        double DrawScale = 1.0;
        Point CardSize = new Point(100, 140);
        int PlayerIndex;
        Point[] PlayerPosition = { new Point(200, 563), new Point(30, 309), new Point(200,30), new Point(621, 30), new Point(1041, 30), new Point(1150, 309), new Point(1041, 563) };

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

        private string CardName(int Value, Card.CardType Suit) {
            if (Suit == Card.CardType.Heart) {
                return "H" + Value;
            } else if (Suit == Card.CardType.Spade) {
                return "S" + Value;
            } else if (Suit == Card.CardType.Diamond) {
                return "D" + Value;
            } else {
                return "C" + Value;
            }
        }

        private Card.CardType RandomCardType() {
            Random rnd = new Random();
            int temp = rnd.Next(3);
            if (temp == 0) {
                return Card.CardType.Heart;
            } else if (temp == 1) {
                return Card.CardType.Spade;
            } else if (temp == 2) {
                return Card.CardType.Diamond;
            } else {
                return Card.CardType.Clover;
            }
        }

        private void Test(int PlayerCount, int PlayerIndex) {
            Random rnd = new Random();
            this.PlayerIndex = PlayerIndex;
            for (int i = 0; i < PlayerCount; i++) {
                List<Card> player = new List<Card>();
                for (int x = 0; x < 2; x++) {
                    player.Add(new Card(rnd.Next(1, 13), RandomCardType()));
                }
                Players.Add(player);
            }
            foreach(List<Card> Player in Players) {
                Debug.WriteLine(Player[0] + " " + Player[1] + "\n");
            }
            foreach(Card card in Players[PlayerIndex]) {
                
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            int tmp = 0;
            foreach(List<Card> Player in Players) {
                for (int i = 0; i < Players.Count; i++) {
                    if (i != PlayerIndex) {
                        if (tmp == 0) {
                            g.DrawImage(Image.FromFile("G:\\Min enhet\\Programmering 1\\Eget\\Ny mapp\\Pokerkort\\back.png"), new Rectangle((int)(PlayerPosition[tmp].X * DrawScale), (int)(PlayerPosition[tmp].Y * DrawScale), (int)(CardSize.X * DrawScale), (int)(CardSize.Y * DrawScale)));
                            g.DrawImage(Image.FromFile("G:\\Min enhet\\Programmering 1\\Eget\\Ny mapp\\Pokerkort\\back.png"), new Rectangle((int)(PlayerPosition[tmp].X * DrawScale + 0.25 * DrawScale * CardSize.X), (int)(PlayerPosition[tmp].Y * DrawScale + 0.25 * DrawScale * CardSize.Y), (int)(CardSize.X * DrawScale), (int)(CardSize.Y * DrawScale)));
                        } else if (tmp == 1) {
                            g.DrawImage(Image.FromFile("G:\\Min enhet\\Programmering 1\\Eget\\Ny mapp\\Pokerkort\\back2.png"), new Rectangle((int)(PlayerPosition[tmp].X * DrawScale), (int)(PlayerPosition[tmp].Y * DrawScale), (int)(CardSize.Y * DrawScale), (int)(CardSize.X * DrawScale)));
                            g.DrawImage(Image.FromFile("G:\\Min enhet\\Programmering 1\\Eget\\Ny mapp\\Pokerkort\\back2.png"), new Rectangle((int)(PlayerPosition[tmp].X * DrawScale + 0.25 * DrawScale * CardSize.X), (int)(PlayerPosition[tmp].Y * DrawScale + 0.25 * DrawScale * CardSize.Y), (int)(CardSize.Y * DrawScale), (int)(CardSize.X * DrawScale)));
                        } else if (tmp <= 4) {
                            g.DrawImage(Image.FromFile("G:\\Min enhet\\Programmering 1\\Eget\\Ny mapp\\Pokerkort\\back.png"), new Rectangle((int)(PlayerPosition[tmp].X * DrawScale + 0.25 * DrawScale * CardSize.X), (int)(PlayerPosition[tmp].Y * DrawScale + 0.25 * DrawScale * CardSize.Y), (int)(CardSize.X * DrawScale), (int)(CardSize.Y * DrawScale)));
                            g.DrawImage(Image.FromFile("G:\\Min enhet\\Programmering 1\\Eget\\Ny mapp\\Pokerkort\\back.png"), new Rectangle((int)(PlayerPosition[tmp].X * DrawScale), (int)(PlayerPosition[tmp].Y * DrawScale), (int)(CardSize.X * DrawScale), (int)(CardSize.Y * DrawScale)));
                        } else if (tmp == 5) {
                            g.DrawImage(Image.FromFile("G:\\Min enhet\\Programmering 1\\Eget\\Ny mapp\\Pokerkort\\back2.png"), new Rectangle((int)(PlayerPosition[tmp].X * DrawScale + 0.25 * DrawScale * CardSize.X), (int)(PlayerPosition[tmp].Y * DrawScale + 0.25 * DrawScale * CardSize.Y), (int)(CardSize.Y * DrawScale), (int)(CardSize.X * DrawScale)));
                            g.DrawImage(Image.FromFile("G:\\Min enhet\\Programmering 1\\Eget\\Ny mapp\\Pokerkort\\back2.png"), new Rectangle((int)(PlayerPosition[tmp].X * DrawScale), (int)(PlayerPosition[tmp].Y * DrawScale), (int)(CardSize.Y * DrawScale), (int)(CardSize.X * DrawScale)));
                        } else if (tmp == 6) {
                            g.DrawImage(Image.FromFile("G:\\Min enhet\\Programmering 1\\Eget\\Ny mapp\\Pokerkort\\back.png"), new Rectangle((int)(PlayerPosition[tmp].X * DrawScale), (int)(PlayerPosition[tmp].Y * DrawScale), (int)(CardSize.X * DrawScale), (int)(CardSize.Y * DrawScale)));
                            g.DrawImage(Image.FromFile("G:\\Min enhet\\Programmering 1\\Eget\\Ny mapp\\Pokerkort\\back.png"), new Rectangle((int)(PlayerPosition[tmp].X * DrawScale + 0.25 * DrawScale * CardSize.X), (int)(PlayerPosition[tmp].Y * DrawScale + 0.25 * DrawScale * CardSize.Y), (int)(CardSize.X * DrawScale), (int)(CardSize.Y * DrawScale)));
                        }
                        tmp++;
                    } else {
                        g.DrawImage(Image.FromFile("G:\\Min enhet\\Programmering 1\\Eget\\Ny mapp\\Pokerkort\\" + CardName(Players[i][0].Value, Players[i][0].Suite) + ".png"), new Rectangle((int)(618 * DrawScale), (int)(688 * DrawScale), (int)(50 * DrawScale), (int)(70 * DrawScale)));
                        g.DrawImage(Image.FromFile("G:\\Min enhet\\Programmering 1\\Eget\\Ny mapp\\Pokerkort\\" + CardName(Players[i][1].Value, Players[i][1].Suite) + ".png"), new Rectangle((int)(698 * DrawScale), (int)(688 * DrawScale), (int)(50 * DrawScale), (int)(70 * DrawScale)));
                    }
                }
            }
        }
    }
}