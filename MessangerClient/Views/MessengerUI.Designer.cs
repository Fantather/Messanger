namespace MessangerClient
{
    partial class MessengerUI
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
            sendMessageButton = new Button();
            userChatListBox = new ListBox();
            messageTextBox = new TextBox();
            nameTextBox = new TextBox();
            contactListListBox = new ListBox();
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // sendMessageButton
            // 
            sendMessageButton.Location = new Point(801, 451);
            sendMessageButton.Name = "sendMessageButton";
            sendMessageButton.Size = new Size(41, 35);
            sendMessageButton.TabIndex = 0;
            sendMessageButton.Text = "Send";
            sendMessageButton.UseVisualStyleBackColor = true;
            sendMessageButton.Click += SendMessageButton_Click;
            // 
            // userChatListBox
            // 
            userChatListBox.FormattingEnabled = true;
            userChatListBox.Location = new Point(189, 28);
            userChatListBox.Name = "userChatListBox";
            userChatListBox.Size = new Size(653, 379);
            userChatListBox.TabIndex = 1;
            // 
            // messageTextBox
            // 
            messageTextBox.Location = new Point(189, 434);
            messageTextBox.Multiline = true;
            messageTextBox.Name = "messageTextBox";
            messageTextBox.PlaceholderText = "Введите сообщение";
            messageTextBox.Size = new Size(606, 71);
            messageTextBox.TabIndex = 2;
            // 
            // nameTextBox
            // 
            nameTextBox.Location = new Point(12, 28);
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(159, 23);
            nameTextBox.TabIndex = 3;
            nameTextBox.Text = "Client";
            // 
            // contactListListBox
            // 
            contactListListBox.FormattingEnabled = true;
            contactListListBox.Location = new Point(12, 66);
            contactListListBox.Name = "contactListListBox";
            contactListListBox.Size = new Size(159, 439);
            contactListListBox.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(35, 10);
            label1.Name = "label1";
            label1.Size = new Size(109, 15);
            label1.TabIndex = 5;
            label1.Text = "Имя пользователя";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F);
            label2.Location = new Point(492, 5);
            label2.Name = "label2";
            label2.Size = new Size(36, 21);
            label2.TabIndex = 6;
            label2.Text = "Чат";
            // 
            // MessengerUI
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(883, 524);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(contactListListBox);
            Controls.Add(nameTextBox);
            Controls.Add(messageTextBox);
            Controls.Add(userChatListBox);
            Controls.Add(sendMessageButton);
            Name = "MessengerUI";
            Text = " ";
            FormClosing += MessengerUI_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button sendMessageButton;
        private ListBox userChatListBox;
        private TextBox messageTextBox;
        private TextBox nameTextBox;
        private ListBox contactListListBox;
        private Label label1;
        private Label label2;
    }
}
