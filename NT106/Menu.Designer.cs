namespace NT106
{
    partial class Menu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button4 = new Button();
            btn_Account = new Button();
            btn_Setting = new Button();
            textBox1 = new TextBox();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(359, 137);
            button1.Margin = new Padding(4, 5, 4, 5);
            button1.Name = "button1";
            button1.Size = new Size(321, 87);
            button1.TabIndex = 0;
            button1.Text = "Chơi với người ";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(359, 234);
            button2.Margin = new Padding(4, 5, 4, 5);
            button2.Name = "button2";
            button2.Size = new Size(321, 88);
            button2.TabIndex = 1;
            button2.Text = "Chơi với máy";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(359, 330);
            button3.Margin = new Padding(4, 5, 4, 5);
            button3.Name = "button3";
            button3.Size = new Size(321, 88);
            button3.TabIndex = 2;
            button3.Text = "Luật chơi ";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(359, 428);
            button4.Margin = new Padding(4, 5, 4, 5);
            button4.Name = "button4";
            button4.Size = new Size(321, 88);
            button4.TabIndex = 3;
            button4.Text = "Thoát";
            button4.UseVisualStyleBackColor = true;
            // 
            // btn_Account
            // 
            btn_Account.BackgroundImage = Properties.Resources.account_icon;
            btn_Account.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Account.Location = new Point(931, 20);
            btn_Account.Margin = new Padding(4, 5, 4, 5);
            btn_Account.Name = "btn_Account";
            btn_Account.Size = new Size(66, 61);
            btn_Account.TabIndex = 4;
            btn_Account.UseVisualStyleBackColor = true;
            btn_Account.Click += btn_Account_Click;
            // 
            // btn_Setting
            // 
            btn_Setting.BackgroundImage = Properties.Resources.Setting_Icon;
            btn_Setting.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Setting.Location = new Point(931, 104);
            btn_Setting.Margin = new Padding(4, 5, 4, 5);
            btn_Setting.Name = "btn_Setting";
            btn_Setting.Size = new Size(66, 56);
            btn_Setting.TabIndex = 5;
            btn_Setting.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(80, 20);
            textBox1.Margin = new Padding(4, 5, 4, 5);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(214, 31);
            textBox1.TabIndex = 7;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.money_Icon;
            pictureBox1.Location = new Point(21, 20);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(57, 31);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 8;
            pictureBox1.TabStop = false;
            // 
            // Menu
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.Menu_Screen2;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1024, 640);
            Controls.Add(pictureBox1);
            Controls.Add(textBox1);
            Controls.Add(btn_Setting);
            Controls.Add(btn_Account);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Margin = new Padding(4, 5, 4, 5);
            Name = "Menu";
            Text = "Menu";
            Load += Menu_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button btn_Account;
        private Button btn_Setting;
        private TextBox textBox1;
        private PictureBox pictureBox1;
    }
}