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
            this.ProductsDataGridView = new System.Windows.Forms.DataGridView();
            this.CartDataGridView = new System.Windows.Forms.DataGridView();
            this.AddToCartButton = new System.Windows.Forms.Button();
            this.BuyButton = new System.Windows.Forms.Button();
            this.TotalLabel = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.CodeTextBox = new System.Windows.Forms.TextBox();
            this.NameLabel = new System.Windows.Forms.Label();
            this.CodeLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // ProductsDataGridView
            this.ProductsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ProductsDataGridView.Location = new System.Drawing.Point(12, 12);
            this.ProductsDataGridView.Name = "ProductsDataGridView";
            this.ProductsDataGridView.Size = new System.Drawing.Size(400, 200);
            this.ProductsDataGridView.AutoGenerateColumns = true; // Luo sarakkeet automaattisesti datan mukaan
            this.ProductsDataGridView.ReadOnly = true;           // Estää muokkaamisen
            this.ProductsDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Valinta rivitasolla
            this.ProductsDataGridView.MultiSelect = false;       // Estää useamman rivin valinnan


            // CartDataGridView
            this.CartDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.CartDataGridView.Location = new System.Drawing.Point(450, 12);
            this.CartDataGridView.Name = "CartDataGridView";
            this.CartDataGridView.Size = new System.Drawing.Size(400, 200);

            // AddToCartButton
            this.AddToCartButton.Location = new System.Drawing.Point(12, 230);
            this.AddToCartButton.Name = "AddToCartButton";
            this.AddToCartButton.Size = new System.Drawing.Size(100, 30);
            this.AddToCartButton.Text = "Lisää koriin";
            this.AddToCartButton.Click += new System.EventHandler(this.AddToCartButton_Click);

            // BuyButton
            this.BuyButton.Location = new System.Drawing.Point(750, 230);
            this.BuyButton.Name = "BuyButton";
            this.BuyButton.Size = new System.Drawing.Size(100, 30);
            this.BuyButton.Text = "Osta";
            this.BuyButton.Click += new System.EventHandler(this.BuyButton_Click);

            // TotalLabel
            this.TotalLabel.Location = new System.Drawing.Point(450, 230);
            this.TotalLabel.Name = "TotalLabel";
            this.TotalLabel.Size = new System.Drawing.Size(300, 30);
            this.TotalLabel.Text = "Yhteensä (ALV 25.5%): 0.00";

            // NameTextBox
            this.NameTextBox.Location = new System.Drawing.Point(450, 280);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(200, 20);

            // CodeTextBox
            this.CodeTextBox.Location = new System.Drawing.Point(450, 320);
            this.CodeTextBox.Name = "CodeTextBox";
            this.CodeTextBox.Size = new System.Drawing.Size(200, 20);

            // NameLabel
            this.NameLabel.Location = new System.Drawing.Point(360, 280);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(80, 20);
            this.NameLabel.Text = "Nimi:";

            // CodeLabel
            this.CodeLabel.Location = new System.Drawing.Point(360, 320);
            this.CodeLabel.Name = "CodeLabel";
            this.CodeLabel.Size = new System.Drawing.Size(80, 20);
            this.CodeLabel.Text = "Kutsutunnus:";

            // Tuotteet
            this.ClientSize = new System.Drawing.Size(900, 400);
            this.Controls.Add(this.ProductsDataGridView);
            this.Controls.Add(this.CartDataGridView);
            this.Controls.Add(this.AddToCartButton);
            this.Controls.Add(this.BuyButton);
            this.Controls.Add(this.TotalLabel);
            this.Controls.Add(this.NameTextBox);
            this.Controls.Add(this.CodeTextBox);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.CodeLabel);
            this.Name = "Tuotteet";
            this.Text = "Varasto Hallinta - Tuotteet";
            this.ResumeLayout(false);
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