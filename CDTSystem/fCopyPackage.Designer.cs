namespace CDTSystem
{
    partial class fCopyPackage
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
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.textEdit1 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(412, 27);
            this.simpleButton1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(150, 44);
            this.simpleButton1.TabIndex = 0;
            this.simpleButton1.Text = "Copy";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // textEdit1
            // 
            this.textEdit1.EditValue = "E1";
            this.textEdit1.Location = new System.Drawing.Point(200, 40);
            this.textEdit1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Size = new System.Drawing.Size(200, 32);
            this.textEdit1.TabIndex = 1;
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(24, 46);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(143, 25);
            this.labelControl1.TabIndex = 2;
            this.labelControl1.Text = "Phần mềm mới";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(684, 46);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(228, 29);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "Tạo phía máy client";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // fCopyPackage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1010, 98);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.textEdit1);
            this.Controls.Add(this.simpleButton1);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "fCopyPackage";
            this.Text = "fCopyPackage";
            this.Load += new System.EventHandler(this.fCopyPackage_Load);
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraEditors.TextEdit textEdit1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}