﻿namespace NT106
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
            btn_PVP = new Button();
            btn_PVE = new Button();
            btn_HowToPlay = new Button();
            btn_Exit = new Button();
            btn_Account = new Button();
            btn_Setting = new Button();
            tb_Money = new TextBox();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // btn_PVP
            // 
            btn_PVP.Location = new Point(287, 110);
            btn_PVP.Margin = new Padding(3, 4, 3, 4);
            btn_PVP.Name = "btn_PVP";
            btn_PVP.Size = new Size(257, 70);
            btn_PVP.TabIndex = 0;
            btn_PVP.Text = "Chơi với người ";
            btn_PVP.UseVisualStyleBackColor = true;
            btn_PVP.Click += btn_PVP_Click;
            // 
            // btn_PVE
            // 
            btn_PVE.Location = new Point(287, 187);
            btn_PVE.Margin = new Padding(3, 4, 3, 4);
            btn_PVE.Name = "btn_PVE";
            btn_PVE.Size = new Size(257, 70);
            btn_PVE.TabIndex = 1;
            btn_PVE.Text = "Chơi với máy";
            btn_PVE.UseVisualStyleBackColor = true;
            // 
            // btn_HowToPlay
            // 
            btn_HowToPlay.Location = new Point(287, 264);
            btn_HowToPlay.Margin = new Padding(3, 4, 3, 4);
            btn_HowToPlay.Name = "btn_HowToPlay";
            btn_HowToPlay.Size = new Size(257, 70);
            btn_HowToPlay.TabIndex = 2;
            btn_HowToPlay.Text = "Luật chơi ";
            btn_HowToPlay.UseVisualStyleBackColor = true;
            // 
            // btn_Exit
            // 
            btn_Exit.Location = new Point(287, 342);
            btn_Exit.Margin = new Padding(3, 4, 3, 4);
            btn_Exit.Name = "btn_Exit";
            btn_Exit.Size = new Size(257, 70);
            btn_Exit.TabIndex = 3;
            btn_Exit.Text = "Thoát";
            btn_Exit.UseVisualStyleBackColor = true;
            // 
            // btn_Account
            // 
            btn_Account.BackgroundImage = Properties.Resources.account_icon;
            btn_Account.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Account.Location = new Point(745, 16);
            btn_Account.Margin = new Padding(3, 4, 3, 4);
            btn_Account.Name = "btn_Account";
            btn_Account.Size = new Size(53, 49);
            btn_Account.TabIndex = 4;
            btn_Account.UseVisualStyleBackColor = true;
            btn_Account.Click += btn_Account_Click;
            // 
            // btn_Setting
            // 
            btn_Setting.BackgroundImage = Properties.Resources.Setting_Icon;
            btn_Setting.BackgroundImageLayout = ImageLayout.Stretch;
            btn_Setting.Location = new Point(745, 83);
            btn_Setting.Margin = new Padding(3, 4, 3, 4);
            btn_Setting.Name = "btn_Setting";
            btn_Setting.Size = new Size(53, 45);
            btn_Setting.TabIndex = 5;
            btn_Setting.UseVisualStyleBackColor = true;
            // 
            // tb_Money
            // 
            tb_Money.Location = new Point(64, 16);
            tb_Money.Margin = new Padding(3, 4, 3, 4);
            tb_Money.Name = "tb_Money";
            tb_Money.ReadOnly = true;
            tb_Money.Size = new Size(172, 27);
            tb_Money.TabIndex = 7;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.money_Icon;
            pictureBox1.Location = new Point(17, 16);
            pictureBox1.Margin = new Padding(2, 2, 2, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(46, 25);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 8;
            pictureBox1.TabStop = false;
            // 
            // Menu
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.Menu_Screen2;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(819, 512);
            Controls.Add(pictureBox1);
            Controls.Add(tb_Money);
            Controls.Add(btn_Setting);
            Controls.Add(btn_Account);
            Controls.Add(btn_Exit);
            Controls.Add(btn_HowToPlay);
            Controls.Add(btn_PVE);
            Controls.Add(btn_PVP);
            Margin = new Padding(3, 4, 3, 4);
            Name = "Menu";
            Text = "Menu";
            Load += Menu_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_PVP;
        private Button btn_PVE;
        private Button btn_HowToPlay;
        private Button btn_Exit;
        private Button btn_Account;
        private Button btn_Setting;
        private TextBox tb_Money;
        private PictureBox pictureBox1;
    }
}