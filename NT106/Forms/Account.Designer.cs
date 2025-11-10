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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Account));
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
            pic_Avatar.BorderStyle = BorderStyle.Fixed3D;
            pic_Avatar.Image = Properties.Resources.account_icon;
            pic_Avatar.Location = new Point(310, 26);
            pic_Avatar.Margin = new Padding(2);
            pic_Avatar.Name = "pic_Avatar";
            pic_Avatar.Size = new Size(232, 232);
            pic_Avatar.SizeMode = PictureBoxSizeMode.StretchImage;
            pic_Avatar.TabIndex = 0;
            pic_Avatar.TabStop = false;
            // 
            // btn_ChangeAvatar
            // 
            btn_ChangeAvatar.Location = new Point(339, 273);
            btn_ChangeAvatar.Margin = new Padding(2);
            btn_ChangeAvatar.Name = "btn_ChangeAvatar";
            btn_ChangeAvatar.Size = new Size(176, 27);
            btn_ChangeAvatar.TabIndex = 1;
            btn_ChangeAvatar.Text = "Thay đổi ảnh đại diện";
            btn_ChangeAvatar.UseVisualStyleBackColor = true;
            btn_ChangeAvatar.Click += btn_ChangeAvatar_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Goldenrod;
            label1.Location = new Point(239, 354);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(127, 25);
            label1.TabIndex = 2;
            label1.Text = "Tên tài khoản";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.Goldenrod;
            label2.Location = new Point(239, 401);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(138, 25);
            label2.TabIndex = 3;
            label2.Text = "Tên người chơi";
            // 
            // tb_AccountName
            // 
            tb_AccountName.BackColor = Color.Gainsboro;
            tb_AccountName.Location = new Point(388, 354);
            tb_AccountName.Margin = new Padding(2);
            tb_AccountName.Name = "tb_AccountName";
            tb_AccountName.ReadOnly = true;
            tb_AccountName.Size = new Size(198, 27);
            tb_AccountName.TabIndex = 4;
            // 
            // tb_IngameName
            // 
            tb_IngameName.Location = new Point(388, 402);
            tb_IngameName.Margin = new Padding(2);
            tb_IngameName.Name = "tb_IngameName";
            tb_IngameName.ReadOnly = true;
            tb_IngameName.Size = new Size(198, 27);
            tb_IngameName.TabIndex = 5;
            // 
            // btn_SignOut
            // 
            btn_SignOut.Location = new Point(687, 462);
            btn_SignOut.Margin = new Padding(2);
            btn_SignOut.Name = "btn_SignOut";
            btn_SignOut.Size = new Size(121, 28);
            btn_SignOut.TabIndex = 6;
            btn_SignOut.Text = "Đăng xuất";
            btn_SignOut.UseVisualStyleBackColor = true;
            btn_SignOut.Click += btn_SignOut_Click;
            // 
            // btn_ChangeIngameName
            // 
            btn_ChangeIngameName.BackgroundImage = Properties.Resources.ChangeName_Icon;
            btn_ChangeIngameName.BackgroundImageLayout = ImageLayout.Stretch;
            btn_ChangeIngameName.Location = new Point(609, 404);
            btn_ChangeIngameName.Margin = new Padding(2);
            btn_ChangeIngameName.Name = "btn_ChangeIngameName";
            btn_ChangeIngameName.Size = new Size(31, 25);
            btn_ChangeIngameName.TabIndex = 7;
            btn_ChangeIngameName.UseVisualStyleBackColor = true;
            btn_ChangeIngameName.Click += btn_ChangeIngameName_Click;
            // 
            // btn_Back
            // 
            btn_Back.Location = new Point(11, 462);
            btn_Back.Margin = new Padding(2);
            btn_Back.Name = "btn_Back";
            btn_Back.Size = new Size(121, 26);
            btn_Back.TabIndex = 8;
            btn_Back.Text = "Quay lại";
            btn_Back.UseVisualStyleBackColor = true;
            btn_Back.Click += btn_Back_Click;
            // 
            // Account
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(819, 512);
            Controls.Add(btn_Back);
            Controls.Add(btn_ChangeIngameName);
            Controls.Add(btn_SignOut);
            Controls.Add(tb_IngameName);
            Controls.Add(tb_AccountName);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btn_ChangeAvatar);
            Controls.Add(pic_Avatar);
            Margin = new Padding(2);
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