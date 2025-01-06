using System;
using System.Windows.Forms;

namespace RKOTrainer
{
    public class MainMenu : Form
    {
        private Button _startGameButton;
        private Button _instructionsButton;
        private Button _exitButton;
        private PictureBox _mainMenuBackgroundBox;
        private Image _mainMenuBackground;

        public MainMenu()
        {
            InitializeComponents();
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void InitializeComponents()
        {
            this.Text = "RKO Trainer";
            this.Size = new System.Drawing.Size(1280, 720);

            _startGameButton = new Button();
            _startGameButton.Text = "Rozpocznij grę";
            _startGameButton.Size = new System.Drawing.Size(200, 50);
            _startGameButton.Visible = true;

            _instructionsButton = new Button();
            _instructionsButton.Text = "Wskazówki";
            _instructionsButton.Size = new System.Drawing.Size(200, 50);
            _instructionsButton.Visible = true;

            _exitButton = new Button();
            _exitButton.Text = "Wyjście";
            _exitButton.Size = new System.Drawing.Size(200, 50);
            _exitButton.Visible = true;

            // Calculate positions to center the buttons
            int centerX = 20+(this.ClientSize.Width - _startGameButton.Width) / 2;
            int startY = (this.ClientSize.Height - (_startGameButton.Height + _instructionsButton.Height + _exitButton.Height + 20)) / 2;

            _startGameButton.Location = new System.Drawing.Point(centerX, startY);
            _instructionsButton.Location = new System.Drawing.Point(centerX, startY + _startGameButton.Height + 10);
            _exitButton.Location = new System.Drawing.Point(centerX, startY + _startGameButton.Height + _instructionsButton.Height + 20);

            _startGameButton.Click += StartGameButton_Click;
            _instructionsButton.Click += InstructionsButton_Click;
            _exitButton.Click += ExitButton_Click;

          
            this.BackgroundImage = Image.FromFile("rko.jpg");
            this.BackgroundImageLayout = ImageLayout.Stretch; 
           
            foreach (Button button in this.Controls.OfType<Button>())
            {
                StyleButton(button);
            }
            

            // Add the buttons after the PictureBox
            this.Controls.Add(_startGameButton);
            this.Controls.Add(_instructionsButton);
            this.Controls.Add(_exitButton);
        }
        
        private void StyleButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 2;
            button.BackColor = Color.FromArgb(150, 255, 255, 255); // Semi-transparent white
            button.ForeColor = Color.Black;
            button.Font = new Font("Arial", 12, FontStyle.Bold);
    
            // Optional: Add hover effect
            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(200, 255, 255, 255);
            button.MouseLeave += (s, e) => button.BackColor = Color.FromArgb(150, 255, 255, 255);
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            RkoGame game = new RkoGame();
            
        }

        private void InstructionsButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("RKO powinno być wykonywane w tempie 110 uderzeń/min, pomyśl o tempie \n" +
                            "Bee Gees - Stayin' Alive, po 30 uciskach przyszła pora na 2 oddechy ratunkowe, po czym znów \n" +
                            "wykonujemy 30 ucisków i tak do momentu przyjazdu ambulansu lub przywrócenia oddechu poszkodowanego ", "Wskaźówki");
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}