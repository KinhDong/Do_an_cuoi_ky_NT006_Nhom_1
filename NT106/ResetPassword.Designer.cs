namespace NT106
{
    partial class ResetPassword
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
            label2 = new Label();
            this.tb_NewPassword = new TextBox();
            btn_Confirm = new Button();
            SuspendLayout();
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(143, 50);
            label2.Name = "label2";
            label2.Size = new Size(170, 50);
            label2.TabIndex = 1;
            label2.Text = "Nhập mật khẩu mới\r\n\r\n";
            // 
            // tb_NewPassword
            // 
            this.tb_NewPassword.Location = new Point(143, 91);
            this.tb_NewPassword.Name = "tb_NewPassword";
            this.tb_NewPassword.Size = new Size(150, 31);
            this.tb_NewPassword.TabIndex = 3;
            // 
            // btn_Confirm
            // 
            btn_Confirm.Location = new Point(160, 157);
            btn_Confirm.Name = "btn_Confirm";
            btn_Confirm.Size = new Size(112, 34);
            btn_Confirm.TabIndex = 4;
            btn_Confirm.Text = "Xác nhận";
            btn_Confirm.UseVisualStyleBackColor = true;
            // 
            // ResetPassword
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(450, 226);
            Controls.Add(btn_Confirm);
            Controls.Add(this.tb_NewPassword);
            Controls.Add(label2);
            Name = "ResetPassword";
            Text = "ResetPassword";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox textBox1;
        private TextBox textBox2;
        private Button btn_Confirm;
        private Label label3;
        private TextBox textBox3;
    }
}