namespace NT106
{
    partial class Room
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
            btn_Back = new Button();
            btn_CreateNewRoom = new Button();
            btn_JoinRoom = new Button();
            label2 = new Label();
            tb_RoomCode = new TextBox();
            SuspendLayout();
            // 
            // btn_Back
            // 
            btn_Back.BackColor = SystemColors.ActiveCaption;
            btn_Back.Location = new Point(341, 302);
            btn_Back.Name = "btn_Back";
            btn_Back.Size = new Size(112, 34);
            btn_Back.TabIndex = 6;
            btn_Back.Text = "Quay lại";
            btn_Back.UseVisualStyleBackColor = false;
            btn_Back.Click += btn_Back_Click;
            // 
            // btn_CreateNewRoom
            // 
            btn_CreateNewRoom.BackColor = SystemColors.ActiveCaption;
            btn_CreateNewRoom.Location = new Point(148, 159);
            btn_CreateNewRoom.Name = "btn_CreateNewRoom";
            btn_CreateNewRoom.Size = new Size(166, 34);
            btn_CreateNewRoom.TabIndex = 7;
            btn_CreateNewRoom.Text = "Tạo phòng riêng";
            btn_CreateNewRoom.UseVisualStyleBackColor = false;
            btn_CreateNewRoom.Click += btn_CreateNewRoom_Click;
            // 
            // btn_JoinRoom
            // 
            btn_JoinRoom.BackColor = SystemColors.ActiveCaption;
            btn_JoinRoom.Location = new Point(469, 159);
            btn_JoinRoom.Name = "btn_JoinRoom";
            btn_JoinRoom.Size = new Size(160, 34);
            btn_JoinRoom.TabIndex = 8;
            btn_JoinRoom.Text = "Tham gia qua mã";
            btn_JoinRoom.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = Color.Transparent;
            label2.ForeColor = SystemColors.ButtonFace;
            label2.Location = new Point(370, 164);
            label2.Name = "label2";
            label2.Size = new Size(50, 25);
            label2.TabIndex = 9;
            label2.Text = "hoặc";
            // 
            // tb_RoomCode
            // 
            tb_RoomCode.Location = new Point(469, 228);
            tb_RoomCode.Name = "tb_RoomCode";
            tb_RoomCode.Size = new Size(160, 31);
            tb_RoomCode.TabIndex = 10;
            // 
            // Room
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = Properties.Resources.Room_Background;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(758, 484);
            Controls.Add(tb_RoomCode);
            Controls.Add(label2);
            Controls.Add(btn_JoinRoom);
            Controls.Add(btn_CreateNewRoom);
            Controls.Add(btn_Back);
            Name = "Room";
            Text = "Phòng";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btn_Back;
        private Button btn_CreateNewRoom;
        private Button btn_JoinRoom;
        private Label label2;
        private TextBox tb_RoomCode;
    }
}