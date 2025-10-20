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
            btn_Confirm.Location = new Point(157, 171);
            btn_Confirm.Name = "btn_Confirm";
            btn_Confirm.Size = new Size(112, 34);
            btn_Confirm.TabIndex = 0;
            btn_Confirm.Text = "Xác nhận";
            btn_Confirm.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(134, 57);
            label1.Name = "label1";
            label1.Size = new Size(163, 25);
            label1.TabIndex = 1;
            label1.Text = "Nhập tên tài khoản";
            // 
            // tb_AccountName
            // 
            tb_AccountName.Location = new Point(134, 102);
            tb_AccountName.Name = "tb_AccountName";
            tb_AccountName.Size = new Size(163, 31);
            tb_AccountName.TabIndex = 2;
            // 
            // ForgetPassword
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(413, 268);
            Controls.Add(tb_AccountName);
            Controls.Add(label1);
            Controls.Add(btn_Confirm);
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