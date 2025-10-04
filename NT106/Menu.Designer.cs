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
            button5 = new Button();
            button6 = new Button();
            button7 = new Button();
            textBox1 = new TextBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(412, 138);
            button1.Name = "button1";
            button1.Size = new Size(225, 52);
            button1.TabIndex = 0;
            button1.Text = "Chơi với người ";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(412, 196);
            button2.Name = "button2";
            button2.Size = new Size(225, 53);
            button2.TabIndex = 1;
            button2.Text = "Chơi với máy";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Location = new Point(412, 255);
            button3.Name = "button3";
            button3.Size = new Size(225, 53);
            button3.TabIndex = 2;
            button3.Text = "Luật chơi ";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(412, 314);
            button4.Name = "button4";
            button4.Size = new Size(225, 53);
            button4.TabIndex = 3;
            button4.Text = "Thoát";
            button4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            button5.Location = new Point(957, 12);
            button5.Name = "button5";
            button5.Size = new Size(75, 45);
            button5.TabIndex = 4;
            button5.Text = "Tài khoản";
            button5.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            button6.BackgroundImage = Properties.Resources.ChatGPT_Image_13_53_42_29_thg_9__2025;
            button6.BackgroundImageLayout = ImageLayout.Stretch;
            button6.Location = new Point(957, 63);
            button6.Name = "button6";
            button6.Size = new Size(75, 45);
            button6.TabIndex = 5;
            button6.Text = "Cài đặt";
            button6.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            button7.Location = new Point(12, 12);
            button7.Name = "button7";
            button7.Size = new Size(75, 45);
            button7.TabIndex = 6;
            button7.Text = "Điểm danh ";
            button7.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(274, 12);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(151, 23);
            textBox1.TabIndex = 7;
            // 
            // Menu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1044, 450);
            Controls.Add(textBox1);
            Controls.Add(button7);
            Controls.Add(button6);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "Menu";
            Text = "Menu";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
        private Button button7;
        private TextBox textBox1;
    }
}