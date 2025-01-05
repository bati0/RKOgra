using System;
using System.Windows.Forms;

namespace RKOTrainer
{
    public class MainMenu : Form
    {
        private Button _startGameButton;
        private Button _instructionsButton;
        private Button _exitButton;

        public MainMenu()
        {
            InitializeComponents();
        }

        private void InitializeComponents() //TODO ustawnienie elementów menu, dodanie grafiki (?)
        {
            this.Text = "RKO Trainer";
            this.Size = new System.Drawing.Size(1280, 720);
            
            _startGameButton = new Button();
            _startGameButton.Text = "Rozpocznij grę";
            _startGameButton.Location = new System.Drawing.Point(100, 30);
            _startGameButton.Click += StartGameButton_Click;

            _instructionsButton = new Button();
            _instructionsButton.Text = "Wskaźówki";
            _instructionsButton.Location = new System.Drawing.Point(100, 70);
            _instructionsButton.Click += InstructionsButton_Click;

            _exitButton = new Button();
            _exitButton.Text = "Wyjście";
            _exitButton.Location = new System.Drawing.Point(100, 110);
            _exitButton.Click += ExitButton_Click;

            this.Controls.Add(_startGameButton);
            this.Controls.Add(_instructionsButton);
            this.Controls.Add(_exitButton);
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            RkoGame game = new RkoGame();
            
        }

        private void InstructionsButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("RKO powinno być wykonywane w tempie 110 uderzeń/min, pomyśl o tempie " +
                            "Bee Gees - Stayin' Alive, po 30 uciskach przyszła pora na 2 oddechy ratunkowe, po czym znów " +
                            "wykonujemy 30 ucisków i tak do momentu przyjazdu ambulansu lub przywrócenia oddechu poszkodowanego ", "Wskaźówki");
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}