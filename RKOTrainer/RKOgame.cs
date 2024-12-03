using System;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace RKOTrainer
{
    public partial class RkoGame
    {
        private GameLogic _gameLogic;
        private GameState _gameState;
        private GameUi _gameUi;
        private Stopwatch _stopwatch;
        private System.Windows.Forms.Timer _animationTimer;
        private System.Windows.Forms.Timer _gameTimer;

        public RkoGame()
        {
            _gameLogic = new GameLogic();
            _gameState = new GameState();
            _gameUi = new GameUi(_gameState);
            _stopwatch = new Stopwatch();
            _animationTimer = new System.Windows.Forms.Timer();
            _gameTimer = new System.Windows.Forms.Timer();

            InitializeCustomComponents();

            // Konfiguracja timera animacji
            _animationTimer.Interval = 16; // około 60 FPS
            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Start();

            // Konfiguracja timera gry
            _gameTimer.Interval = 1000; // 1 sekunda
            _gameTimer.Tick += GameTimer_Tick;
        }

        private void InitializeCustomComponents()
        {
            // Show the GameUi form
            _gameUi.Show();

            // Add event handlers for buttons
            _gameUi.CompressionButton.Click += CompressionButton_Click;
            _gameUi.BreathButton.Click += BreathButton_Click;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (!_stopwatch.IsRunning) return;

            long currentTime = _stopwatch.ElapsedMilliseconds;

            // Oblicz aktualne tempo na podstawie historii uciśnięć
            if (_gameState.CompressionTimes.Count >= 2)
            {
                double instantRate = _gameLogic.CalculateCurrentRate(_gameState.CompressionTimes);
                // Płynne przejście do nowego tempa
                _gameState.CurrentRate = _gameState.CurrentRate + (instantRate - _gameState.CurrentRate) * GameLogic.RateSmoothing;
            }
            else
            {
                // Jeśli nie ma wystarczającej historii, powoli opadaj w dół
                _gameState.CurrentRate = Math.Max(60, _gameState.CurrentRate - GameLogic.Gravity);
            }

            // Konwersja tempa na pozycję wskaźnika (odwrotnie, bo wskaźnik jest pionowy)
            double targetPosition = (160 - _gameState.CurrentRate) * GameUi.IndicatorHeight / (160 - 60);

            // Płynne przejście do docelowej pozycji
            double positionDiff = targetPosition - _gameState.IndicatorPosition;
            _gameState.IndicatorVelocity += positionDiff * 0.6; // Siła przyciągania do docelowej pozycji
            _gameState.IndicatorVelocity *= GameLogic.VelocityDamping;

            // Aktualizacja pozycji
            _gameState.IndicatorPosition += (int)_gameState.IndicatorVelocity;

            // Ograniczenie pozycji wskaźnika
            _gameState.IndicatorPosition = Math.Max(0, Math.Min(GameUi.IndicatorHeight, _gameState.IndicatorPosition));

            UpdateStatusLabel(_gameState.CurrentRate);
            _gameUi.TempoIndicatorPanel.Invalidate();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsed = _stopwatch.Elapsed;
            _gameUi.TimerLabel.Text = $"Czas: {elapsed.Minutes}:{elapsed.Seconds:D2}";
        }

        private void UpdateStatusLabel(double rate)
        {
            if (rate < 100 || rate > 120)
            {
                _gameUi.StatusLabel.Text = $"Tempo: {rate:F0} uderzeń/min - Dostosuj tempo! (100-120)";
                _gameUi.StatusLabel.ForeColor = Color.Red;
            }
            else
            {
                _gameUi.StatusLabel.Text = $"Tempo: {rate:F0} uderzeń/min - Dobre tempo!";
                _gameUi.StatusLabel.ForeColor = Color.Green;
            }
        }

        private async void CompressionButton_Click(object sender, EventArgs e)
        {
            if (!_stopwatch.IsRunning)
            {
                _stopwatch.Start();
                _gameTimer.Start();
                _gameState.CompressionTimes.Clear();
                _gameState.CurrentRate = 110; // Startujemy od optymalnego tempa
                _gameState.IndicatorPosition = GameUi.IndicatorHeight / 2;
            }

            long currentTime = _stopwatch.ElapsedMilliseconds;

            // Dodaj nowy czas do historii
            _gameState.CompressionTimes.Enqueue(currentTime);
            while (_gameState.CompressionTimes.Count > GameLogic.MaxCompressionHistory)
            {
                _gameState.CompressionTimes.Dequeue();
            }

            // Oblicz aktualne tempo dla punktacji
            double instantRate = 110; // Domyślne tempo
            if (_gameState.CompressionTimes.Count >= 2)
            {
                var times = _gameState.CompressionTimes.ToArray();
                long lastInterval = times[times.Length - 1] - times[times.Length - 2];
                if (lastInterval > 0)
                {
                    instantRate = 60000.0 / lastInterval;
                }
            }

            // Oblicz i dodaj punkty
            int points = _gameLogic.CalculatePoints(instantRate);
            _gameState.TotalScore += points;
            _gameUi.ScoreLabel.Text = $"Punkty: {_gameState.TotalScore}";

            _gameState.CompressionCount++;
            if (_gameState.CompressionCount >= 30)
            {
                _gameState.IsCompressionPhase = false;
                _gameUi.CompressionButton.Enabled = false;
                _gameUi.BreathButton.Enabled = true;
                _gameUi.StatusLabel.Text = "Teraz wykonaj 2 oddechy ratunkowe";
                _animationTimer.Stop();
            }

            UpdateLabels();

            // Zmień obraz na CPRdown.webp
            _gameUi.CprPictureBox.Image = _gameUi.CprDownImage;

            // Czekaj 10ms
            await Task.Delay(150);

            // Przywróć obraz na CPRup.webp
            _gameUi.CprPictureBox.Image = _gameUi.CprUpImage;
        }

        private void UpdateLabels()
        {
            _gameUi.CompressionCountLabel.Text = $"Liczba uciśnięć: {_gameState.CompressionCount}/30";
            _gameUi.BreathCountLabel.Text = $"Liczba oddechów: {_gameState.BreathCount}/2";
        }

        private void BreathButton_Click(object sender, EventArgs e)
        {
            _gameState.BreathCount++;
            SystemSounds.Asterisk.Play();

            if (_gameState.BreathCount >= 2)
            {
                MessageBox.Show($"Cykl zakończony!\nTwój wynik: {_gameState.TotalScore} punktów", "Sukces!");
                StartNewCycle();
            }
            else
            {
                _gameUi.StatusLabel.Text = $"Wykonaj jeszcze {2 - _gameState.BreathCount} oddech(y)";
            }

            UpdateLabels();
        }

        private void StartNewCycle()
        {
            _gameState.CompressionCount = 0;
            _gameState.BreathCount = 0;
            _gameState.IsCompressionPhase = true;
            _stopwatch.Reset();
            _gameTimer.Stop();
            _gameUi.CompressionButton.Enabled = true;
            _gameUi.BreathButton.Enabled = false;
            _gameState.CurrentRate = 110;
            _gameState.TotalScore = 0;
            _gameState.IndicatorPosition = GameUi.IndicatorHeight / 2;
            _gameState.IndicatorVelocity = 0;
            _gameState.CompressionTimes.Clear();

            UpdateLabels();
            _gameUi.ScoreLabel.Text = "Punkty: 0";
            _gameUi.StatusLabel.Text = "Rozpocznij uciskanie klatki piersiowej";
            _gameUi.TimerLabel.Text = "Czas: 0:00";
            _gameUi.TempoIndicatorPanel.Invalidate();
            _animationTimer.Start();
        }
    }
}