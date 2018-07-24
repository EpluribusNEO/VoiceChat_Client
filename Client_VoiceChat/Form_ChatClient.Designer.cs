namespace Client_VoiceChat
{
    partial class Form_ChatClient
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbServerIP = new System.Windows.Forms.TextBox();
            this.tbPORT = new System.Windows.Forms.TextBox();
            this.btnJoin = new System.Windows.Forms.Button();
            this.btnTalk = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP Сервера";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(13, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "Port Сервера";
            // 
            // tbServerIP
            // 
            this.tbServerIP.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbServerIP.Location = new System.Drawing.Point(104, 13);
            this.tbServerIP.Name = "tbServerIP";
            this.tbServerIP.Size = new System.Drawing.Size(151, 23);
            this.tbServerIP.TabIndex = 2;
            this.tbServerIP.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_KeyPress);
            // 
            // tbPORT
            // 
            this.tbPORT.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbPORT.Location = new System.Drawing.Point(104, 37);
            this.tbPORT.Name = "tbPORT";
            this.tbPORT.Size = new System.Drawing.Size(151, 23);
            this.tbPORT.TabIndex = 3;
            this.tbPORT.Text = "8080";
            this.tbPORT.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxPort_KeyPress);
            // 
            // btnJoin
            // 
            this.btnJoin.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnJoin.Location = new System.Drawing.Point(55, 66);
            this.btnJoin.Name = "btnJoin";
            this.btnJoin.Size = new System.Drawing.Size(200, 45);
            this.btnJoin.TabIndex = 4;
            this.btnJoin.Text = "Присоедениться";
            this.btnJoin.UseVisualStyleBackColor = true;
            this.btnJoin.Click += new System.EventHandler(this.btnJoin_Click);
            // 
            // btnTalk
            // 
            this.btnTalk.Enabled = false;
            this.btnTalk.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnTalk.Location = new System.Drawing.Point(55, 113);
            this.btnTalk.Name = "btnTalk";
            this.btnTalk.Size = new System.Drawing.Size(200, 45);
            this.btnTalk.TabIndex = 5;
            this.btnTalk.Text = "Говорить";
            this.btnTalk.UseVisualStyleBackColor = true;
            this.btnTalk.Click += new System.EventHandler(this.btnTalk_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.btnDisconnect.Location = new System.Drawing.Point(55, 160);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(200, 45);
            this.btnDisconnect.TabIndex = 7;
            this.btnDisconnect.Text = "Отключить";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // Form_ChatClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(274, 221);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.btnTalk);
            this.Controls.Add(this.btnJoin);
            this.Controls.Add(this.tbPORT);
            this.Controls.Add(this.tbServerIP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form_ChatClient";
            this.Text = "Клиент";
            this.Load += new System.EventHandler(this.Form_ChatClient_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbServerIP;
        private System.Windows.Forms.TextBox tbPORT;
        private System.Windows.Forms.Button btnJoin;
        private System.Windows.Forms.Button btnTalk;
        private System.Windows.Forms.Button btnDisconnect;
    }
}

