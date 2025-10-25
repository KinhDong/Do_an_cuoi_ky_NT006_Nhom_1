namespace NT106
{
    partial class ForgetPassword
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
            btn_Confirm = new Button();
            label1 = new Label();
            tb_AccountName = new TextBox();
            SuspendLayout();
            // 
            // btn_Confirm
            // 
            btn_Confirm.Location = new Point(110, 103);
            btn_Confirm.Margin = new Padding(2);
            btn_Confirm.Name = "btn_Confirm";
            btn_Confirm.Size = new Size(78, 20);
            btn_Confirm.TabIndex = 0;
            btn_Confirm.Text = "Xác nhận";
            btn_Confirm.UseVisualStyleBackColor = true;
            btn_Confirm.Click += this.btn_Confirm_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(94, 34);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(108, 15);
            label1.TabIndex = 1;
            label1.Text = "Nhập tên tài khoản";
            // 
            // tb_AccountName
            // 
            tb_AccountName.Location = new Point(94, 61);
            tb_AccountName.Margin = new Padding(2);
            tb_AccountName.Name = "tb_AccountName";
            tb_AccountName.Size = new Size(115, 23);
            tb_AccountName.TabIndex = 2;
            // 
            // ForgetPassword
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(289, 161);
            Controls.Add(tb_AccountName);
            Controls.Add(label1);
            Controls.Add(btn_Confirm);
            Margin = new Padding(2);
            Name = "ForgetPassword";
            Text = "Quên mật khẩu? Gà";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btn_Confirm;
        private Label label1;
        private TextBox tb_AccountName;
    }
}