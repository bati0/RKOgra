using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace RKOTrainer
{
    public class GameUi : Form
    {
        private GameState _gameState;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Button CompressionButton { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Button BreathButton { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Button BackToMenuButton { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Label StatusLabel { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Label TimerLabel { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Label CompressionCountLabel { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Label BreathCountLabel { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Label ScoreLabel { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel TempoIndicatorPanel { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PictureBox CprPictureBox { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image CprUpImage { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image CprDownImage { get; private set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image PulseImage { get; private set; }

        private const int IndicatorWidth = 100;
        public const int IndicatorHeight = 600;

        public GameUi(GameState gameState)
        {
            this._gameState = gameState;
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Size = new Size(1280, 720);
            this.Text = "RKO Trainer";

            // Panel wskaźnika tempa
            TempoIndicatorPanel = new Panel
            {
                Size = new Size(IndicatorWidth, IndicatorHeight),
                Location = new Point(750, 40),
                BorderStyle = BorderStyle.FixedSingle,
                Visible = true
            };
            TempoIndicatorPanel.Paint += TempoIndicator_Paint;

            // Przycisk do uciskania
            CompressionButton = new Button
            {
                Text = "Uciśnij klatkę piersiową",
                Size = new Size(300, 60),
                Location = new Point(900, 80),
                BackColor = Color.LightBlue
            };

            // Przycisk do oddechów
            BreathButton = new Button
            {
                Text = "Wykonaj oddech ratunkowy",
                Size = new Size(300, 60),
                Location = new Point(900, 150),
                BackColor = Color.LightGreen,
            };

            BackToMenuButton = new Button()
            {
                Text = "Powrót do menu",
                Size = new Size(300, 60),
                Location = new Point(900, 550),
                BackColor = Color.LightGray
            };

            // Etykiety
            ScoreLabel = new Label
            {
                Text = "Punkty: 0",
                Size = new Size(200, 20),
                Location = new Point(850, 230),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            StatusLabel = new Label
            {
                Size = new Size(350, 20),
                Location = new Point(850, 260),
                TextAlign = ContentAlignment.MiddleCenter
            };

            TimerLabel = new Label
            {
                Text = "Czas: 0:00",
                Size = new Size(350, 20),
                Location = new Point(850, 290),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12)
            };

            CompressionCountLabel = new Label
            {
                Text = "Liczba uciśnięć: 0/30",
                Size = new Size(150, 20),
                Location = new Point(850, 320),
                Visible = true
            };

            BreathCountLabel = new Label
            {
                Text = "Liczba oddechów: 0/2",
                Size = new Size(150, 20),
                Location = new Point(850, 320),
                Visible = true
            };

            CprPictureBox = new PictureBox
            {
                Size = new Size(720, 720),
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            Controls.Add(CprPictureBox);

            // Załaduj obrazy
            CprUpImage = Image.FromFile("CPRup.jpg");
            CprDownImage = Image.FromFile("CPRdown.jpg");
            PulseImage = Image.FromFile("pulse.png");

            // Ustaw domyślny obraz
            CprPictureBox.Image = CprUpImage;

            Controls.Add(TempoIndicatorPanel);
            Controls.Add(CompressionButton);
            Controls.Add(BreathButton);
            Controls.Add(BackToMenuButton);
            Controls.Add(StatusLabel);
            Controls.Add(TimerLabel);
            Controls.Add(CompressionCountLabel);
            Controls.Add(BreathCountLabel);
            Controls.Add(ScoreLabel);
        }

        private void TempoIndicator_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Tworzenie pionowego gradientu
            using (LinearGradientBrush gradientBrush = new LinearGradientBrush(
                       new Point(0, IndicatorHeight),
                       new Point(0, 0),
                       Color.Red,
                       Color.Red))
            {
                ColorBlend colorBlend = new ColorBlend(5);
                colorBlend.Colors = new Color[] {
                    Color.Red,      // Za wolno
                    Color.Yellow,   // Przejście
                    Color.Green,    // Optymalnie
                    Color.Yellow,   // Przejście
                    Color.Red       // Za szybko
                };
                colorBlend.Positions = new float[] { 0.0f, 0.3f, 0.5f, 0.7f, 1.0f };
                gradientBrush.InterpolationColors = colorBlend;

                // Rysowanie tła wskaźnika
                g.FillRectangle(gradientBrush, 0, 0, IndicatorWidth, IndicatorHeight);
            }

            // Rysowanie wskaźnika
            if (PulseImage != null)
            {
                int imageHeight = PulseImage.Height;
                int imageWidth = PulseImage.Width;
                int yPosition = _gameState.IndicatorPosition - (imageHeight / 2);
                g.DrawImage(PulseImage, 0, yPosition, imageWidth, imageHeight);
            }

            // Dodanie oznaczeń tempa (teraz po prawej stronie)
            using (Font font = new Font("Arial", 8))
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                g.DrawString("160", font, textBrush, new PointF(IndicatorWidth + 2, 5));
                g.DrawString("120", font, textBrush, new PointF(IndicatorWidth + 2, IndicatorHeight * 0.3f));
                g.DrawString("110", font, textBrush, new PointF(IndicatorWidth + 2, IndicatorHeight * 0.5f));
                g.DrawString("100", font, textBrush, new PointF(IndicatorWidth + 2, IndicatorHeight * 0.7f));
                g.DrawString("60", font, textBrush, new PointF(IndicatorWidth + 2, IndicatorHeight - 15));
            }
        }
    }
}