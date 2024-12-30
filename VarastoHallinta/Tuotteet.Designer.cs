namespace VarastoHallinta
{
    partial class Tuotteet
    {
        private System.ComponentModel.IContainer components = null;

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
            ProductsDataGridView = new DataGridView();
            CartDataGridView = new DataGridView();
            AddToCartButton = new Button();
            BuyButton = new Button();
            TotalLabel = new Label();
            NameTextBox = new TextBox();
            CodeTextBox = new TextBox();
            NameLabel = new Label();
            CodeLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)ProductsDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CartDataGridView).BeginInit();
            SuspendLayout();
            // 
            // ProductsDataGridView
            // 
            ProductsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ProductsDataGridView.Location = new Point(12, 12);
            ProductsDataGridView.MultiSelect = false;
            ProductsDataGridView.Name = "ProductsDataGridView";
            ProductsDataGridView.ReadOnly = true;
            ProductsDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ProductsDataGridView.Size = new Size(400, 200);
            ProductsDataGridView.TabIndex = 0;
            // 
            // CartDataGridView
            // 
            CartDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            CartDataGridView.Location = new Point(450, 12);
            CartDataGridView.Name = "CartDataGridView";
            CartDataGridView.Size = new Size(400, 200);
            CartDataGridView.TabIndex = 1;
            // 
            // AddToCartButton
            // 
            AddToCartButton.Location = new Point(12, 230);
            AddToCartButton.Name = "AddToCartButton";
            AddToCartButton.Size = new Size(100, 30);
            AddToCartButton.TabIndex = 2;
            AddToCartButton.Text = "Lisää koriin";
            AddToCartButton.Click += AddToCartButton_Click;
            // 
            // BuyButton
            // 
            BuyButton.Location = new Point(750, 230);
            BuyButton.Name = "BuyButton";
            BuyButton.Size = new Size(100, 30);
            BuyButton.TabIndex = 3;
            BuyButton.Text = "Osta";
            BuyButton.Click += BuyButton_Click;
            // 
            // TotalLabel
            // 
            TotalLabel.Location = new Point(450, 230);
            TotalLabel.Name = "TotalLabel";
            TotalLabel.Size = new Size(300, 30);
            TotalLabel.TabIndex = 4;
            TotalLabel.Text = "Yhteensä (ALV 25.5%): 0.00";
            // 
            // NameTextBox
            // 
            NameTextBox.Location = new Point(450, 280);
            NameTextBox.Name = "NameTextBox";
            NameTextBox.Size = new Size(200, 23);
            NameTextBox.TabIndex = 5;
            // 
            // CodeTextBox
            // 
            CodeTextBox.Location = new Point(450, 320);
            CodeTextBox.Name = "CodeTextBox";
            CodeTextBox.Size = new Size(200, 23);
            CodeTextBox.TabIndex = 6;
            // 
            // NameLabel
            // 
            NameLabel.Location = new Point(360, 280);
            NameLabel.Name = "NameLabel";
            NameLabel.Size = new Size(80, 20);
            NameLabel.TabIndex = 7;
            NameLabel.Text = "Nimi:";
            // 
            // CodeLabel
            // 
            CodeLabel.Location = new Point(360, 320);
            CodeLabel.Name = "CodeLabel";
            CodeLabel.Size = new Size(80, 20);
            CodeLabel.TabIndex = 8;
            CodeLabel.Text = "Kutsutunnus:";
            // 
            // Tuotteet
            // 
            ClientSize = new Size(900, 400);
            Controls.Add(ProductsDataGridView);
            Controls.Add(CartDataGridView);
            Controls.Add(AddToCartButton);
            Controls.Add(BuyButton);
            Controls.Add(TotalLabel);
            Controls.Add(NameTextBox);
            Controls.Add(CodeTextBox);
            Controls.Add(NameLabel);
            Controls.Add(CodeLabel);
            Name = "Tuotteet";
            Text = "Varasto Hallinta - Tuotteet";
            ((System.ComponentModel.ISupportInitialize)ProductsDataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)CartDataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.DataGridView ProductsDataGridView;
        private System.Windows.Forms.DataGridView CartDataGridView;
        private System.Windows.Forms.Button AddToCartButton;
        private System.Windows.Forms.Button BuyButton;
        private System.Windows.Forms.Label TotalLabel;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.TextBox CodeTextBox;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.Label CodeLabel;
    }
}