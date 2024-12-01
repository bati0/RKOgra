using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Media;
using Timer = System.Windows.Forms.Timer;

namespace RKOTrainer
{
    public partial class RKOGame : Form
    {
        private Button compressionButton;
        private Button breathButton;
        private Label statusLabel;
        private Label timerLabel;
        private Label compressionCountLabel;
        private Label breathCountLabel;
        private Label scoreLabel;
        private Panel tempoIndicatorPanel;
        private int indicatorPosition = 0;
        private Stopwatch stopwatch;
        private Timer animationTimer;
        private Timer gameTimer;
        private int compressionCount = 0;
        private int breathCount = 0;
        private long lastCompressionTime = 0;
        private bool isCompressionPhase = true;
        private double targetRate = 0;
        private const int INDICATOR_WIDTH = 100;
        private const int INDICATOR_HEIGHT = 600;
        private int totalScore = 0;
        private double indicatorVelocity = 0;
        
        private PictureBox cprPictureBox;
        private Image cprUpImage;
        private Image cprDownImage;
        private Image pulseImage;
        
        private const double GRAVITY = 0.2; // Zmniejszona siła grawitacji
        private const double VELOCITY_DAMPING = 0.92; // Zwiększone tłumienie
        private const double VELOCITY_BOOST = 15.0; // Zmniejszona siła podskoku
        private const double RATE_SMOOTHING = 0.5; // Współczynnik wygładzania tempa
        private double currentRate = 110; // Startujemy od optymalnego tempa
        private Queue<long> compressionTimes = new Queue<long>(); // Kolejka czasów uciśnięć
        private const int MAX_COMPRESSION_HISTORY = 3; // Ile ostatnich uciśnięć bierzemy pod uwagę

        public RKOGame()
        {
            stopwatch = new Stopwatch();
            animationTimer = new Timer();
            gameTimer = new Timer();
            
            InitializeCustomComponents();
            
            // Konfiguracja timera animacji
            animationTimer.Interval = 16; // około 60 FPS
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();

            // Konfiguracja timera gry
            gameTimer.Interval = 1000; // 1 sekunda
            gameTimer.Tick += GameTimer_Tick;
        }

        private void InitializeCustomComponents()
        {
            this.Size = new Size(1280, 720);
            this.Text = "RKO Trainer";
            
            stopwatch = new Stopwatch();

            // Panel wskaźnika tempa
            tempoIndicatorPanel = new Panel
            {
                Size = new Size(INDICATOR_WIDTH, INDICATOR_HEIGHT),
                Location = new Point(750, 40),
                BorderStyle = BorderStyle.FixedSingle
            };
            tempoIndicatorPanel.Paint += TempoIndicator_Paint;

            // Przycisk do uciskania
            compressionButton = new Button
            {
                Text = "Uciśnij klatkę piersiową",
                Size = new Size(300, 60),
                Location = new Point(900, 80),
                BackColor = Color.LightBlue
            };
            compressionButton.Click += CompressionButton_Click;

            // Przycisk do oddechów
            breathButton = new Button
            {
                Text = "Wykonaj oddech ratunkowy",
                Size = new Size(300, 60),
                Location = new Point(900, 150),
                BackColor = Color.LightGreen,
                Enabled = false
            };
            breathButton.Click += BreathButton_Click;

            // Etykiety
            scoreLabel = new Label
            {
                Text = "Punkty: 0",
                Size = new Size(200, 20),
                Location = new Point(850, 230),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };

            statusLabel = new Label
            {
                Size = new Size(350, 20),
                Location = new Point(850, 260),
                TextAlign = ContentAlignment.MiddleCenter
            };

            timerLabel = new Label
            {
                Text = "Czas: 0:00",
                Size = new Size(350, 20),
                Location = new Point(850, 290),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12)
            };

            compressionCountLabel = new Label
            {
                Text = "Liczba uciśnięć: 0/30",
                Size = new Size(150, 20),
                Location = new Point(850, 320)
            };

            breathCountLabel = new Label
            {
                Text = "Liczba oddechów: 0/2",
                Size = new Size(150, 20),
                Location = new Point(850, 320)
            };
            
            cprPictureBox = new PictureBox
            {
                Size = new Size(720, 720),
                Location = new Point(0, 0),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            Controls.Add(cprPictureBox);

            // Załaduj obrazy
            cprUpImage = Image.FromFile("CPRup.jpg");
            cprDownImage = Image.FromFile("CPRdown.jpg");
            pulseImage = Image.FromFile("pulse.png");

            // Ustaw domyślny obraz
            cprPictureBox.Image = cprUpImage;

            Controls.Add(tempoIndicatorPanel);
            Controls.Add(compressionButton);
            Controls.Add(breathButton);
            Controls.Add(statusLabel);
            Controls.Add(timerLabel);
            Controls.Add(compressionCountLabel);
            Controls.Add(breathCountLabel);
            Controls.Add(scoreLabel);

            StartNewCycle();
        }

        private void TempoIndicator_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Tworzenie pionowego gradientu
            using (LinearGradientBrush gradientBrush = new LinearGradientBrush(
                new Point(0, INDICATOR_HEIGHT),
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
                g.FillRectangle(gradientBrush, 0, 0, INDICATOR_WIDTH, INDICATOR_HEIGHT);
            }

            // Rysowanie wskaźnika (trójkąt)
            if (pulseImage != null)
            {
                int imageHeight = pulseImage.Height;
                int imageWidth = pulseImage.Width;
                int yPosition = indicatorPosition - (imageHeight / 2);
                g.DrawImage(pulseImage, 0, yPosition, imageWidth, imageHeight);
            }

            // Dodanie oznaczeń tempa (teraz po prawej stronie)
            using (Font font = new Font("Arial", 8))
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                g.DrawString("160", font, textBrush, INDICATOR_WIDTH + 2, 5);
                g.DrawString("120", font, textBrush, INDICATOR_WIDTH + 2, INDICATOR_HEIGHT * 0.3f);
                g.DrawString("110", font, textBrush, INDICATOR_WIDTH + 2, INDICATOR_HEIGHT * 0.5f);
                g.DrawString("100", font, textBrush, INDICATOR_WIDTH + 2, INDICATOR_HEIGHT * 0.7f);
                g.DrawString("60", font, textBrush, INDICATOR_WIDTH + 2, INDICATOR_HEIGHT - 15);
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!stopwatch.IsRunning) return;

            long currentTime = stopwatch.ElapsedMilliseconds;
            
            // Oblicz aktualne tempo na podstawie historii uciśnięć
            if (compressionTimes.Count >= 2)
            {
                double instantRate = CalculateCurrentRate();
                // Płynne przejście do nowego tempa
                currentRate = currentRate + (instantRate - currentRate) * RATE_SMOOTHING;
            }
            else
            {
                // Jeśli nie ma wystarczającej historii, powoli opadaj w dół
                currentRate = Math.Max(60, currentRate - GRAVITY);
            }

            // Konwersja tempa na pozycję wskaźnika (odwrotnie, bo wskaźnik jest pionowy)
            double targetPosition = (160 - currentRate) * INDICATOR_HEIGHT / (160 - 60);
            
            // Płynne przejście do docelowej pozycji
            double positionDiff = targetPosition - indicatorPosition;
            indicatorVelocity += positionDiff * 0.6; // Siła przyciągania do docelowej pozycji
            indicatorVelocity *= VELOCITY_DAMPING;

            // Aktualizacja pozycji
            indicatorPosition += (int)indicatorVelocity;

            // Ograniczenie pozycji wskaźnika
            indicatorPosition = Math.Max(0, Math.Min(INDICATOR_HEIGHT, indicatorPosition));

            UpdateStatusLabel(currentRate);
            tempoIndicatorPanel.Invalidate();
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = stopwatch.Elapsed;
            timerLabel.Text = $"Czas: {elapsed.Minutes}:{elapsed.Seconds:D2}";
        }

        private void UpdateStatusLabel(double rate)
        {
            if (rate < 100 || rate > 120)
            {
                statusLabel.Text = $"Tempo: {rate:F0} uderzeń/min - Dostosuj tempo! (100-120)";
                statusLabel.ForeColor = Color.Red;
            }
            else
            {
                statusLabel.Text = $"Tempo: {rate:F0} uderzeń/min - Dobre tempo!";
                statusLabel.ForeColor = Color.Green;
            }
        }
        
         private double CalculateCurrentRate()
        {
            if (compressionTimes.Count < 2) return 110; // Domyślne tempo

            var times = compressionTimes.ToArray();
            double totalRate = 0;
            int validIntervals = 0;

            for (int i = 1; i < times.Length; i++)
            {
                long interval = times[i] - times[i - 1];
                if (interval > 0)
                {
                    double rate = 60000.0 / interval; // Konwersja interwału na uderzenia na minutę
                    // Ograniczenie zakresu tempa
                    rate = Math.Max(60, Math.Min(160, rate));
                    totalRate += rate;
                    validIntervals++;
                }
            }

            return validIntervals > 0 ? totalRate / validIntervals : 110;
        }

        private async void CompressionButton_Click(object sender, EventArgs e)
        {
            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
                gameTimer.Start();
                lastCompressionTime = stopwatch.ElapsedMilliseconds;
                compressionTimes.Clear();
                currentRate = 110; // Startujemy od optymalnego tempa
                indicatorPosition = INDICATOR_HEIGHT / 2;
            }

            long currentTime = stopwatch.ElapsedMilliseconds;

            // Dodaj nowy czas do historii
            compressionTimes.Enqueue(currentTime);
            while (compressionTimes.Count > MAX_COMPRESSION_HISTORY)
            {
                compressionTimes.Dequeue();
            }

            // Oblicz aktualne tempo dla punktacji
            double instantRate = 110; // Domyślne tempo
            if (compressionTimes.Count >= 2)
            {
                var times = compressionTimes.ToArray();
                long lastInterval = times[times.Length - 1] - times[times.Length - 2];
                if (lastInterval > 0)
                {
                    instantRate = 60000.0 / lastInterval;
                }
            }

            // Oblicz i dodaj punkty
            int points = CalculatePoints(instantRate);
            totalScore += points;
            scoreLabel.Text = $"Punkty: {totalScore}";

            lastCompressionTime = currentTime;

            compressionCount++;
            if (compressionCount >= 30)
            {
                isCompressionPhase = false;
                compressionButton.Enabled = false;
                breathButton.Enabled = true;
                statusLabel.Text = "Teraz wykonaj 2 oddechy ratunkowe";
                animationTimer.Stop();
            }

            UpdateLabels();

            // Zmień obraz na CPRdown.webp
            cprPictureBox.Image = cprDownImage;

            // Czekaj 10ms
            await Task.Delay(150);

            // Przywróć obraz na CPRup.webp
            cprPictureBox.Image = cprUpImage;
        }

        private int CalculatePoints(double rate)
        {
            // Optymalne tempo to 110 uderzeń/min
            const double OPTIMAL_RATE = 110;
            const double RATE_TOLERANCE = 15; // +/- 15 uderzeń/min

            double deviation = Math.Abs(rate - OPTIMAL_RATE);
            
            if (deviation <= 3) // Prawie idealne tempo
                return 100;
            else if (deviation <= RATE_TOLERANCE) // W akceptowalnym zakresie
            {
                // Liniowa interpolacja punktów od 100 do -100 based on deviation
                return (int)(100 * (1 - deviation / RATE_TOLERANCE));
            }
            else // Poza zakresem
                return -100;
        }
        

        private void StartNewCycle()
        {
            compressionCount = 0;
            breathCount = 0;
            isCompressionPhase = true;
            stopwatch.Reset();
            gameTimer.Stop();
            compressionButton.Enabled = true;
            breathButton.Enabled = false;
            currentRate = 110;
            totalScore = 0;
            indicatorPosition = INDICATOR_HEIGHT / 2;
            indicatorVelocity = 0;
            compressionTimes.Clear();
            
            UpdateLabels();
            scoreLabel.Text = "Punkty: 0";
            statusLabel.Text = "Rozpocznij uciskanie klatki piersiowej";
            timerLabel.Text = "Czas: 0:00";
            tempoIndicatorPanel.Invalidate();
            animationTimer.Start();
        }

        private void BreathButton_Click(object sender, EventArgs e)
        {
            breathCount++;
            SystemSounds.Asterisk.Play();

            if (breathCount >= 2)
            {
                MessageBox.Show($"Cykl zakończony!\nTwój wynik: {totalScore} punktów", "Sukces!");
                StartNewCycle();
            }
            else
            {
                statusLabel.Text = $"Wykonaj jeszcze {2 - breathCount} oddech(y)";
            }

            UpdateLabels();
        }

        private void UpdateLabels()
        {
            compressionCountLabel.Text = $"Liczba uciśnięć: {compressionCount}/30";
            breathCountLabel.Text = $"Liczba oddechów: {breathCount}/2";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            animationTimer.Stop();
            animationTimer.Dispose();
            gameTimer.Stop();
            gameTimer.Dispose();
        }
    }
}
