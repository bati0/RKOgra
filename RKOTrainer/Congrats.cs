using System;
using System.Drawing;
using System.Windows.Forms;

namespace RKOTrainer
{
    public class Congrats : Form
    {
        private Label _congratsLabel;
        private PictureBox _congratsPictureBox;
        private Button _backToMenuButton;

        public Congrats()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Gratulacje!";
            this.Size = new Size(1280, 720);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            _congratsLabel = new Label
            {
                Text = "Gratulacje! Pozytywnie ukończyłeś trening!",
                Size = new Size(700, 50),
                Location = new Point(640-350, 20),
                Font = new Font("Arial", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _congratsPictureBox = new PictureBox
            {
                Size = new Size(500, 500),
                Location = new Point(640 - 250, 100),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Image.FromFile("congrats.jpg") // Ensure the image file is in the correct path
            };
            
            _backToMenuButton = new Button()
            {
                Text = "Powrót do menu",
                Size = new Size(300, 60),
                Location = new Point(640-150, 610),
                BackColor = Color.LightGray
            };
            
            _backToMenuButton.Click += BackToMenuButton_Click!;

            

            this.Controls.Add(_congratsLabel);
            this.Controls.Add(_congratsPictureBox);
            this.Controls.Add(_backToMenuButton);
        }
        private void BackToMenuButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            MainMenu mainMenu = new MainMenu();
            mainMenu.Show();
        }
    }
}