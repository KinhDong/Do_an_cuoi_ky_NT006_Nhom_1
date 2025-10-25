namespace NT106
{
    partial class SigninBJ
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            tb_AccountName = new TextBox();
            tb_Password = new TextBox();
            btn_SignIn = new Button();
            label1 = new Label();
            btn_CreateAccount = new Button();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            btn_ForgotPassword = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.ChatGPT_Image_13_53_42_29_thg_9__2025;
            pictureBox1.Location = new Point(306, 0);
            pictureBox1.Margin = new Padding(3, 4, 3, 4);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(562, 514);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // tb_AccountName
            // 
            tb_AccountName.Font = new Font("Times New Roman", 9F);
            tb_AccountName.Location = new Point(66, 159);
            tb_AccountName.Margin = new Padding(3, 4, 3, 4);
            tb_AccountName.Name = "tb_AccountName";
            tb_AccountName.Size = new Size(172, 25);
            tb_AccountName.TabIndex = 1;
            // 
            // tb_Password
            // 
            tb_Password.Font = new Font("Times New Roman", 9F);
            tb_Password.Location = new Point(66, 237);
            tb_Password.Margin = new Padding(3, 4, 3, 4);
            tb_Password.Name = "tb_Password";
            tb_Password.Size = new Size(172, 25);
            tb_Password.TabIndex = 2;
            // 
            // btn_SignIn
            // 
            btn_SignIn.Font = new Font("Times New Roman", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_SignIn.Location = new Point(31, 287);
            btn_SignIn.Margin = new Padding(3, 4, 3, 4);
            btn_SignIn.Name = "btn_SignIn";
            btn_SignIn.Size = new Size(86, 38);
            btn_SignIn.TabIndex = 3;
            btn_SignIn.Text = "Đăng nhập";
            btn_SignIn.UseVisualStyleBackColor = true;
            btn_SignIn.Click += btn_SignIn_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Times New Roman", 15F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(95, 77);
            label1.Name = "label1";
            label1.Size = new Size(136, 29);
            label1.TabIndex = 5;
            label1.Text = "Đăng nhập ";
            // 
            // btn_CreateAccount
            // 
            btn_CreateAccount.Font = new Font("Times New Roman", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_CreateAccount.Location = new Point(104, 412);
            btn_CreateAccount.Margin = new Padding(3, 4, 3, 4);
            btn_CreateAccount.Name = "btn_CreateAccount";
            btn_CreateAccount.Size = new Size(86, 38);
            btn_CreateAccount.TabIndex = 6;
            btn_CreateAccount.Text = "Đăng ký ";
            btn_CreateAccount.UseVisualStyleBackColor = true;
            btn_CreateAccount.Click += btn_CreateAccount_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(117, 30);
            label2.Name = "label2";
            label2.Size = new Size(0, 20);
            label2.TabIndex = 7;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Times New Roman", 9F);
            label3.Location = new Point(86, 379);
            label3.Name = "label3";
            label3.Size = new Size(126, 17);
            label3.TabIndex = 8;
            label3.Text = "Chưa có tài khoản? ";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Times New Roman", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(66, 135);
            label4.Name = "label4";
            label4.Size = new Size(102, 17);
            label4.TabIndex = 9;
            label4.Text = "Tên tài khoản ";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Times New Roman", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.Location = new Point(66, 213);
            label5.Name = "label5";
            label5.Size = new Size(71, 17);
            label5.TabIndex = 10;
            label5.Text = "Mật khẩu";
            // 
            // btn_ForgotPassword
            // 
            btn_ForgotPassword.Font = new Font("Times New Roman", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn_ForgotPassword.Location = new Point(144, 287);
            btn_ForgotPassword.Margin = new Padding(2);
            btn_ForgotPassword.Name = "btn_ForgotPassword";
            btn_ForgotPassword.Size = new Size(123, 38);
            btn_ForgotPassword.TabIndex = 11;
            btn_ForgotPassword.Text = "Quên mật khẩu";
            btn_ForgotPassword.UseVisualStyleBackColor = true;
            // 
            // SigninBJ
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(819, 512);
            Controls.Add(btn_ForgotPassword);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(btn_CreateAccount);
            Controls.Add(label1);
            Controls.Add(btn_SignIn);
            Controls.Add(tb_Password);
            Controls.Add(tb_AccountName);
            Controls.Add(pictureBox1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "SigninBJ";
            Text = "BLACKJACK";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private TextBox tb_AccountName;
        private TextBox tb_Password;
        private Button btn_SignIn;
        private Label label1;
        private Button btn_CreateAccount;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Button btn_ForgotPassword;
    }
}
