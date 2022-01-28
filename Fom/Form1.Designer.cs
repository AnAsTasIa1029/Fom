namespace ShadowWinForms
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxRotationSpeed = new System.Windows.Forms.GroupBox();
            this.labelDegreesInSec = new System.Windows.Forms.Label();
            this.textBoxRotationSpeed = new System.Windows.Forms.TextBox();
            this.Prism = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBoxRotationSpeed.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(746, 426);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.groupBoxRotationSpeed, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.Prism, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(599, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(144, 420);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // groupBoxRotationSpeed
            // 
            this.groupBoxRotationSpeed.Controls.Add(this.labelDegreesInSec);
            this.groupBoxRotationSpeed.Controls.Add(this.textBoxRotationSpeed);
            this.groupBoxRotationSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxRotationSpeed.Location = new System.Drawing.Point(3, 3);
            this.groupBoxRotationSpeed.Name = "groupBoxRotationSpeed";
            this.groupBoxRotationSpeed.Size = new System.Drawing.Size(138, 64);
            this.groupBoxRotationSpeed.TabIndex = 1;
            this.groupBoxRotationSpeed.TabStop = false;
            this.groupBoxRotationSpeed.Text = "Rotation Speed";
            // 
            // labelDegreesInSec
            // 
            this.labelDegreesInSec.AutoSize = true;
            this.labelDegreesInSec.Location = new System.Drawing.Point(6, 42);
            this.labelDegreesInSec.Name = "labelDegreesInSec";
            this.labelDegreesInSec.Size = new System.Drawing.Size(69, 13);
            this.labelDegreesInSec.TabIndex = 1;
            this.labelDegreesInSec.Text = "Degrees/sec";
            // 
            // textBoxRotationSpeed
            // 
            this.textBoxRotationSpeed.Location = new System.Drawing.Point(6, 19);
            this.textBoxRotationSpeed.Name = "textBoxRotationSpeed";
            this.textBoxRotationSpeed.Size = new System.Drawing.Size(100, 20);
            this.textBoxRotationSpeed.TabIndex = 1;
            this.textBoxRotationSpeed.TextChanged += new System.EventHandler(this.textBoxRotationSpeed_TextChanged);
            // 
            // Prism
            // 
            this.Prism.Location = new System.Drawing.Point(3, 73);
            this.Prism.Name = "Prism";
            this.Prism.Size = new System.Drawing.Size(138, 23);
            this.Prism.TabIndex = 2;
            this.Prism.Text = "Шестиугольная призма";
            this.Prism.UseVisualStyleBackColor = true;
            this.Prism.Click += new System.EventHandler(this.Prism_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(746, 426);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Shadow";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBoxRotationSpeed.ResumeLayout(false);
            this.groupBoxRotationSpeed.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBoxRotationSpeed;
        private System.Windows.Forms.Label labelDegreesInSec;
        private System.Windows.Forms.TextBox textBoxRotationSpeed;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.Button Prism;
    }
}

