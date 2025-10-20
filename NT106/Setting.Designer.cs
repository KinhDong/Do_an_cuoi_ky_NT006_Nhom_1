namespace NT106
{
    partial class Setting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Setting));
            label1 = new Label();
            label2 = new Label();
            btn_Apply = new Button();
            trackBar_Sound = new TrackBar();
            trackBar_Music = new TrackBar();
            btn_Back = new Button();
            ((System.ComponentModel.ISupportInitialize)trackBar_Sound).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBar_Music).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label1.ForeColor = SystemColors.ButtonHighlight;
            label1.Location = new Point(72, 46);
            label1.Name = "label1";
            label1.Size = new Size(96, 25);
            label1.TabIndex = 0;
            label1.Text = "Âm thanh";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.ForeColor = SystemColors.ButtonHighlight;
            label2.Location = new Point(72, 121);
            label2.Name = "label2";
            label2.Size = new Size(56, 25);
            label2.TabIndex = 1;
            label2.Text = "Nhạc";
            // 
            // btn_Apply
            // 
            btn_Apply.BackColor = Color.LightSkyBlue;
            btn_Apply.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Apply.Location = new Point(129, 212);
            btn_Apply.Name = "btn_Apply";
            btn_Apply.Size = new Size(112, 34);
            btn_Apply.TabIndex = 2;
            btn_Apply.Text = "Áp dụng";
            btn_Apply.UseVisualStyleBackColor = false;
            // 
            // trackBar_Sound
            // 
            trackBar_Sound.BackColor = Color.FromArgb(0, 64, 64);
            trackBar_Sound.Location = new Point(202, 46);
            trackBar_Sound.Name = "trackBar_Sound";
            trackBar_Sound.Size = new Size(379, 69);
            trackBar_Sound.TabIndex = 3;
            // 
            // trackBar_Music
            // 
            trackBar_Music.BackColor = Color.FromArgb(0, 64, 64);
            trackBar_Music.Location = new Point(202, 121);
            trackBar_Music.Name = "trackBar_Music";
            trackBar_Music.Size = new Size(379, 69);
            trackBar_Music.TabIndex = 4;
            // 
            // btn_Back
            // 
            btn_Back.BackColor = Color.SkyBlue;
            btn_Back.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_Back.Location = new Point(414, 212);
            btn_Back.Name = "btn_Back";
            btn_Back.Size = new Size(112, 34);
            btn_Back.TabIndex = 5;
            btn_Back.Text = "Quay lại";
            btn_Back.UseVisualStyleBackColor = false;
            // 
            // Setting
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Indigo;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(663, 302);
            Controls.Add(btn_Back);
            Controls.Add(trackBar_Music);
            Controls.Add(trackBar_Sound);
            Controls.Add(btn_Apply);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "Setting";
            Text = "Cài đặt";
            ((System.ComponentModel.ISupportInitialize)trackBar_Sound).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBar_Music).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Button btn_Apply;
        private TrackBar trackBar_Sound;
        private TrackBar trackBar_Music;
        private Button btn_Back;
    }
}