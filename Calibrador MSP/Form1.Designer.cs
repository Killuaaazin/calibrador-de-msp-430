namespace Embedded__MSP430
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button Connect_bt;
        private System.Windows.Forms.Button Disconnect_bt;
        private System.Windows.Forms.ComboBox Ports_comboBox;
        private System.Windows.Forms.ComboBox Baud_comboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button calibrarBtn;
        private System.IO.Ports.SerialPort serialPort;
        private System.Windows.Forms.ComboBox Frequency_comboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox calib1TextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox calib2TextBox;
        private System.Windows.Forms.Button resetBtn;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Connect_bt = new System.Windows.Forms.Button();
            this.Disconnect_bt = new System.Windows.Forms.Button();
            this.Ports_comboBox = new System.Windows.Forms.ComboBox();
            this.Baud_comboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Frequency_comboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.calib1TextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.calib2TextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.calibrarBtn = new System.Windows.Forms.Button();
            this.serialPort = new System.IO.Ports.SerialPort(this.components);
            this.gravarCodigo = new System.Windows.Forms.Button();
            this.resetBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Connect_bt
            // 
            this.Connect_bt.Location = new System.Drawing.Point(250, 25);
            this.Connect_bt.Name = "Connect_bt";
            this.Connect_bt.Size = new System.Drawing.Size(100, 25);
            this.Connect_bt.TabIndex = 4;
            this.Connect_bt.Text = "Conectar";
            this.Connect_bt.UseVisualStyleBackColor = true;
            this.Connect_bt.Click += new System.EventHandler(this.Connect_bt_Click);
            // 
            // Disconnect_bt
            // 
            this.Disconnect_bt.Location = new System.Drawing.Point(250, 65);
            this.Disconnect_bt.Name = "Disconnect_bt";
            this.Disconnect_bt.Size = new System.Drawing.Size(100, 25);
            this.Disconnect_bt.TabIndex = 5;
            this.Disconnect_bt.Text = "Desconectar";
            this.Disconnect_bt.UseVisualStyleBackColor = true;
            this.Disconnect_bt.Click += new System.EventHandler(this.Disconnect_bt_Click);
            // 
            // Ports_comboBox
            // 
            this.Ports_comboBox.FormattingEnabled = true;
            this.Ports_comboBox.Location = new System.Drawing.Point(100, 27);
            this.Ports_comboBox.Name = "Ports_comboBox";
            this.Ports_comboBox.Size = new System.Drawing.Size(120, 21);
            this.Ports_comboBox.TabIndex = 1;
            // 
            // Baud_comboBox
            // 
            this.Baud_comboBox.FormattingEnabled = true;
            this.Baud_comboBox.Items.AddRange(new object[] {
            "2400"});
            this.Baud_comboBox.Location = new System.Drawing.Point(100, 67);
            this.Baud_comboBox.Name = "Baud_comboBox";
            this.Baud_comboBox.Size = new System.Drawing.Size(120, 21);
            this.Baud_comboBox.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(30, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Frequência:";
            // 
            // Frequency_comboBox
            // 
            this.Frequency_comboBox.FormattingEnabled = true;
            this.Frequency_comboBox.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16"});
            this.Frequency_comboBox.Location = new System.Drawing.Point(100, 107);
            this.Frequency_comboBox.Name = "Frequency_comboBox";
            this.Frequency_comboBox.Size = new System.Drawing.Size(120, 21);
            this.Frequency_comboBox.TabIndex = 6;
            this.Frequency_comboBox.Text = "1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 150);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Calibração 1:";
            // 
            // calib1TextBox
            // 
            this.calib1TextBox.Location = new System.Drawing.Point(120, 147);
            this.calib1TextBox.Name = "calib1TextBox";
            this.calib1TextBox.ReadOnly = true;
            this.calib1TextBox.Size = new System.Drawing.Size(230, 20);
            this.calib1TextBox.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(30, 185);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Calibração 2:";
            // 
            // calib2TextBox
            // 
            this.calib2TextBox.Location = new System.Drawing.Point(120, 182);
            this.calib2TextBox.Name = "calib2TextBox";
            this.calib2TextBox.ReadOnly = true;
            this.calib2TextBox.Size = new System.Drawing.Size(230, 20);
            this.calib2TextBox.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(30, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "COM Ports:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(30, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Baud Rate:";
            // 
            // calibrarBtn
            // 
            this.calibrarBtn.Location = new System.Drawing.Point(30, 220);
            this.calibrarBtn.Name = "calibrarBtn";
            this.calibrarBtn.Size = new System.Drawing.Size(320, 40);
            this.calibrarBtn.TabIndex = 7;
            this.calibrarBtn.Text = "Calibrar MSP430";
            this.calibrarBtn.UseVisualStyleBackColor = true;
            this.calibrarBtn.Click += new System.EventHandler(this.calibrarBtn_Click);
            // 
            // gravarCodigo
            // 
            this.gravarCodigo.Location = new System.Drawing.Point(250, 104);
            this.gravarCodigo.Name = "gravarCodigo";
            this.gravarCodigo.Size = new System.Drawing.Size(100, 25);
            this.gravarCodigo.TabIndex = 12;
            this.gravarCodigo.Text = "Gravar";
            this.gravarCodigo.UseVisualStyleBackColor = true;
            this.gravarCodigo.Click += new System.EventHandler(this.button1_Click);
            // 
            // resetBtn
            // 
            this.resetBtn.Location = new System.Drawing.Point(30, 270);
            this.resetBtn.Name = "resetBtn";
            this.resetBtn.Size = new System.Drawing.Size(320, 30);
            this.resetBtn.TabIndex = 13;
            this.resetBtn.Text = "Resetar MSP430";
            this.resetBtn.UseVisualStyleBackColor = true;
            this.resetBtn.Click += new System.EventHandler(this.resetBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(396, 320);
            this.Controls.Add(this.resetBtn);
            this.Controls.Add(this.gravarCodigo);
            this.Controls.Add(this.calibrarBtn);
            this.Controls.Add(this.calib2TextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.calib1TextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Frequency_comboBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Disconnect_bt);
            this.Controls.Add(this.Connect_bt);
            this.Controls.Add(this.Baud_comboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Ports_comboBox);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Calibrador MSP430";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button gravarCodigo;
    }
}