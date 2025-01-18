namespace VarastoHallinta
{
    partial class Etusivu
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
            BuyButton = new Button();
            FillButton = new Button();
            SuspendLayout();
            // 
            // BuyButton
            // 
            BuyButton.Location = new Point(330, 167);
            BuyButton.Name = "BuyButton";
            BuyButton.Size = new Size(75, 23);
            BuyButton.TabIndex = 0;
            BuyButton.Text = "Osta";
            BuyButton.UseVisualStyleBackColor = true;
            BuyButton.Click += BuyButton_Click;
            // 
            // FillButton
            // 
            FillButton.Location = new Point(330, 196);
            FillButton.Name = "FillButton";
            FillButton.Size = new Size(75, 23);
            FillButton.TabIndex = 1;
            FillButton.Text = "Täydennä";
            FillButton.UseVisualStyleBackColor = true;
            FillButton.Click += FillButton_Click;
            // 
            // Etusivu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(FillButton);
            Controls.Add(BuyButton);
            Name = "Etusivu";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private Button BuyButton;
        private Button FillButton;
    }
}
