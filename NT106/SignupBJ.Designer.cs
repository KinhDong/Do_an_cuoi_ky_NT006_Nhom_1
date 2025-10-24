namespace NT106
{
    partial class SignupBJ
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
            label5 = new Label();
            label2 = new Label();
            btn_CreateAccount = new Button();
            label1 = new Label();
            tb_Password = new TextBox();
            tb_ConfirmPassword = new TextBox();
            label3 = new Label();
            label6 = new Label();
            tb_Email = new TextBox();
            label4 = new Label();
            tb_AccountName = new TextBox();
            SuspendLayout();
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Times New Roman", 9F);
            label5.Location = new Point(38, 225);
            label5.Name = "label5";
            label5.Size = new Size(64, 17);
            label5.TabIndex = 21;
            label5.Text = "Mật khẩu";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(101, 30);
            label2.Name = "label2";
            label2.Size = new Size(0, 20);
            label2.TabIndex = 18;
            // 
            // btn_CreateAccount
            // 
            btn_CreateAccount.Font = new Font("Times New Roman", 9F);
            btn_CreateAccount.Location = new Point(98, 374);
            btn_CreateAccount.Margin = new Padding(3, 4, 3, 4);
            btn_CreateAccount.Name = "btn_CreateAccount";
            btn_CreateAccount.Size = new Size(86, 38);
            btn_CreateAccount.TabIndex = 17;
            btn_CreateAccount.Text = "Đăng ký ";
            btn_CreateAccount.UseVisualStyleBackColor = true;
            btn_CreateAccount.Click += btn_CreateAccount_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Times New Roman", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(98, 30);
            label1.Name = "label1";
            label1.Size = new Size(99, 27);
            label1.TabIndex = 16;
            label1.Text = "Đăng ký ";
            label1.Visible = false;
            // 
            // tb_Password
            // 
            tb_Password.Font = new Font("Times New Roman", 9F);
            tb_Password.Location = new Point(38, 249);
            tb_Password.Margin = new Padding(3, 4, 3, 4);
            tb_Password.Name = "tb_Password";
            tb_Password.Size = new Size(215, 25);
            tb_Password.TabIndex = 13;
            // 
            // tb_ConfirmPassword
            // 
            tb_ConfirmPassword.Location = new Point(38, 320);
            tb_ConfirmPassword.Margin = new Padding(3, 4, 3, 4);
            tb_ConfirmPassword.Name = "tb_ConfirmPassword";
            tb_ConfirmPassword.Size = new Size(215, 27);
            tb_ConfirmPassword.TabIndex = 22;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Times New Roman", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.Location = new Point(38, 296);
            label3.Name = "label3";
            label3.Size = new Size(122, 17);
            label3.TabIndex = 23;
            label3.Text = "Xác nhận mật khẩu";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Times New Roman", 9F);
            label6.Location = new Point(38, 77);
            label6.Name = "label6";
            label6.Size = new Size(92, 17);
            label6.TabIndex = 25;
            label6.Text = "Tên tài khoản ";
            // 
            // tb_Email
            // 
            tb_Email.Font = new Font("Times New Roman", 9F);
            tb_Email.Location = new Point(38, 178);
            tb_Email.Margin = new Padding(3, 4, 3, 4);
            tb_Email.Name = "tb_Email";
            tb_Email.Size = new Size(215, 25);
            tb_Email.TabIndex = 24;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(38, 147);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(46, 20);
            label4.TabIndex = 26;
            label4.Text = "Email";
            // 
            // tb_AccountName
            // 
            tb_AccountName.Location = new Point(38, 106);
            tb_AccountName.Margin = new Padding(2, 2, 2, 2);
            tb_AccountName.Name = "tb_AccountName";
            tb_AccountName.Size = new Size(215, 27);
            tb_AccountName.TabIndex = 27;
            // 
            // SignupBJ
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(290, 434);
            Controls.Add(tb_AccountName);
            Controls.Add(label4);
            Controls.Add(label6);
            Controls.Add(tb_Email);
            Controls.Add(label3);
            Controls.Add(tb_ConfirmPassword);
            Controls.Add(label5);
            Controls.Add(label2);
            Controls.Add(btn_CreateAccount);
            Controls.Add(label1);
            Controls.Add(tb_Password);
            Margin = new Padding(3, 4, 3, 4);
            Name = "SignupBJ";
            Text = "SignupBJ";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label5;
        private Label label2;
        private Button btn_CreateAccount;
        private Label label1;
        private TextBox tb_Password;
        private TextBox tb_ConfirmPassword;
        private Label label3;
        private Label label6;
        private TextBox tb_Email;
        private Label label4;
        private TextBox tb_AccountName;
    }
}