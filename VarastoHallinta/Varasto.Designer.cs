namespace VarastoHallinta
{
    partial class Varasto
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
            productNameTextBox = new TextBox();
            productPriceTextBox = new TextBox();
            productQuantityTextBox = new TextBox();
            addProductButton = new Button();
            ProductsDataGridView = new DataGridView();
            quantity = new TextBox();
            addquantity = new Button();
            ((System.ComponentModel.ISupportInitialize)ProductsDataGridView).BeginInit();
            SuspendLayout();
            // 
            // productNameTextBox
            // 
            productNameTextBox.Location = new Point(46, 42);
            productNameTextBox.Name = "productNameTextBox";
            productNameTextBox.Size = new Size(100, 23);
            productNameTextBox.TabIndex = 0;
            // 
            // productPriceTextBox
            // 
            productPriceTextBox.Location = new Point(46, 71);
            productPriceTextBox.Name = "productPriceTextBox";
            productPriceTextBox.Size = new Size(100, 23);
            productPriceTextBox.TabIndex = 1;
            // 
            // productQuantityTextBox
            // 
            productQuantityTextBox.Location = new Point(46, 100);
            productQuantityTextBox.Name = "productQuantityTextBox";
            productQuantityTextBox.Size = new Size(100, 23);
            productQuantityTextBox.TabIndex = 2;
            // 
            // addProductButton
            // 
            addProductButton.Location = new Point(46, 129);
            addProductButton.Name = "addProductButton";
            addProductButton.Size = new Size(75, 23);
            addProductButton.TabIndex = 4;
            addProductButton.Text = "Lisää tuote";
            addProductButton.UseVisualStyleBackColor = true;
            addProductButton.Click += AddProductButton_Click;
            // 
            // ProductsDataGridView
            // 
            ProductsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ProductsDataGridView.Location = new Point(386, 12);
            ProductsDataGridView.Name = "ProductsDataGridView";
            ProductsDataGridView.Size = new Size(402, 244);
            ProductsDataGridView.TabIndex = 5;
            //ProductsDataGridView.CellContentClick += ProductsDataGridView_CellContentClick;
            ProductsDataGridView.SelectionChanged += ProductsDataGridView_SelectionChanged;

            // 
            // quantity
            // 
            quantity.Location = new Point(386, 272);
            quantity.Name = "quantity";
            quantity.Size = new Size(100, 23);
            quantity.TabIndex = 6;
            quantity.TextChanged += quantity_TextChanged;
            // 
            // addquantity
            // 
            addquantity.Location = new Point(503, 272);
            addquantity.Name = "addquantity";
            addquantity.Size = new Size(75, 23);
            addquantity.TabIndex = 7;
            addquantity.Text = "Vaihda";
            addquantity.UseVisualStyleBackColor = true;
            addquantity.Click += AddQuantityButton_Click;
            // 
            // Varasto
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(addquantity);
            Controls.Add(quantity);
            Controls.Add(ProductsDataGridView);
            Controls.Add(addProductButton);
            Controls.Add(productQuantityTextBox);
            Controls.Add(productPriceTextBox);
            Controls.Add(productNameTextBox);
            Name = "Varasto";
            Text = "Varasto";
            ((System.ComponentModel.ISupportInitialize)ProductsDataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox productNameTextBox;
        private TextBox productPriceTextBox;
        private TextBox productQuantityTextBox;
        private Button addProductButton;
        private DataGridView ProductsDataGridView;
        private TextBox quantity;
        private Button addquantity;
    }
}