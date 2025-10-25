namespace NT106
{
    partial class Account
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
            pic_Avatar = new PictureBox();
            btn_ChangeAvatar = new Button();
            label1 = new Label();
            label2 = new Label();
            tb_AccountName = new TextBox();
            tb_IngameName = new TextBox();
            btn_SignOut = new Button();
            btn_ChangeIngameName = new Button();
            btn_Back = new Button();
            ((System.ComponentModel.ISupportInitialize)pic_Avatar).BeginInit();
            SuspendLayout();
            // 
            // pic_Avatar
            // 
            pic_Avatar.Image = Properties.Resources.account_icon;
            pic_Avatar.Location = new Point(76, 69);
            pic_Avatar.Name = "pic_Avatar";
            pic_Avatar.Size = new Size(387, 349);
            pic_Avatar.SizeMode = PictureBoxSizeMode.StretchImage;
            pic_Avatar.TabIndex = 0;
            pic_Avatar.TabStop = false;
            // 
            // btn_ChangeAvatar
            // 
            btn_ChangeAvatar.Location = new Point(179, 476);
            btn_ChangeAvatar.Name = "btn_ChangeAvatar";
            btn_ChangeAvatar.Size = new Size(201, 34);
            btn_ChangeAvatar.TabIndex = 1;
            btn_ChangeAvatar.Text = "Thay đổi ảnh đại diện";
            btn_ChangeAvatar.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Goldenrod;
            label1.Location = new Point(546, 75);
            label1.Name = "label1";
            label1.Size = new Size(127, 25);
            label1.TabIndex = 2;
            label1.Text = "Tên tài khoản";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.Goldenrod;
            label2.Location = new Point(546, 176);
            label2.Name = "label2";
            label2.Size = new Size(138, 25);
            label2.TabIndex = 3;
            label2.Text = "Tên người chơi";
            // 
            // tb_AccountName
            // 
            tb_AccountName.BackColor = Color.Gainsboro;
            tb_AccountName.Location = new Point(707, 69);
            tb_AccountName.Name = "tb_AccountName";
            tb_AccountName.ReadOnly = true;
            tb_AccountName.Size = new Size(207, 31);
            tb_AccountName.TabIndex = 4;
            // 
            // tb_IngameName
            // 
            tb_IngameName.Location = new Point(707, 173);
            tb_IngameName.Name = "tb_IngameName";
            tb_IngameName.ReadOnly = true;
            tb_IngameName.Size = new Size(207, 31);
            tb_IngameName.TabIndex = 5;
            // 
            // btn_SignOut
            // 
            btn_SignOut.Location = new Point(786, 513);
            btn_SignOut.Name = "btn_SignOut";
            btn_SignOut.Size = new Size(112, 34);
            btn_SignOut.TabIndex = 6;
            btn_SignOut.Text = "Đăng xuất";
            btn_SignOut.UseVisualStyleBackColor = true;
            // 
            // btn_ChangeIngameName
            // 
            btn_ChangeIngameName.BackgroundImage = Properties.Resources.ChangeName_Icon;
            btn_ChangeIngameName.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ChangeIngameName.Location = new Point(921, 173);
            btn_ChangeIngameName.Name = "btn_ChangeIngameName";
            btn_ChangeIngameName.Size = new Size(39, 31);
            btn_ChangeIngameName.TabIndex = 7;
            btn_ChangeIngameName.UseVisualStyleBackColor = true;
            // 
            // btn_Back
            // 
            btn_Back.Location = new Point(533, 513);
            btn_Back.Name = "btn_Back";
            btn_Back.Size = new Size(112, 34);
            btn_Back.TabIndex = 8;
            btn_Back.Text = "Quay lại";
            btn_Back.UseVisualStyleBackColor = true;
            btn_Back.Click += btn_Back_Click;
            // 
            // Account
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.Account_Background;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1024, 640);
            Controls.Add(btn_Back);
            Controls.Add(btn_ChangeIngameName);
            Controls.Add(btn_SignOut);
            Controls.Add(tb_IngameName);
            Controls.Add(tb_AccountName);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btn_ChangeAvatar);
            Controls.Add(pic_Avatar);
            Name = "Account";
            Text = "Tài khoản";
            Load += Account_Load;
            ((System.ComponentModel.ISupportInitialize)pic_Avatar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pic_Avatar;
        private Button btn_ChangeAvatar;
        private Label label1;
        private Label label2;
        private TextBox tb_AccountName;
        private TextBox tb_IngameName;
        private Button btn_SignOut;
        private Button btn_ChangeIngameName;
        private Button btn_Back;
    }
}