using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace VarastoHallinta
{
    public partial class Tuotteet : Form
    {
        private readonly HttpClient client = new HttpClient { BaseAddress = new Uri("http://localhost:3000/") };
        private List<Product> products = new List<Product>();
        private List<CartItem> cart = new List<CartItem>();

        public Tuotteet()
        {
            InitializeComponent();
            LoadProducts();
        }

        private async void LoadProducts()
        {
            try
            {
                var response = await client.GetAsync("products");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                // Deserialisoidaan ja lisätään ALV hintaan
                products = JsonSerializer.Deserialize<List<Product>>(json)
                    .Select(p =>
                    {
                        p.PriceWithTax = p.GetPriceAsDecimal() * 1.255M; // Lisää ALV suoraan
                        return p;
                    })
                    .ToList();

                // Päivitetään DataGridView
                ProductsDataGridView.DataSource = products.Select(p => new
                {
                    p.Id,
                    Tuote = p.Name,
                    Hinta = p.PriceWithTax.ToString("C2"), // Näytetään hinta euroina
                    Varastossa = p.Quantity
                }).ToList();

                // Päivitetään sarakkeiden otsikot
                ProductsDataGridView.Columns["Tuote"].HeaderText = "Tuote";
                ProductsDataGridView.Columns["Hinta"].HeaderText = "Hinta (€)";
                ProductsDataGridView.Columns["Varastossa"].HeaderText = "Varastossa";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Virhe tuotteiden lataamisessa: {ex.Message}");
            }
        }

        private void AddToCartButton_Click(object sender, EventArgs e)
        {
            if (ProductsDataGridView.SelectedRows.Count > 0)
            {
                // Haetaan valittu tuote
                var selectedRow = ProductsDataGridView.SelectedRows[0];
                int productId = (int)selectedRow.Cells["Id"].Value;
                var selectedProduct = products.First(p => p.Id == productId);

                // Pyydetään käyttäjältä määrä
                var quantityText = Prompt.ShowDialog("Anna määrä:", "Lisää ostoskoriin");

                if (int.TryParse(quantityText, out int qty) && qty > 0 && qty <= selectedProduct.Quantity)
                {
                    // Tarkistetaan, onko tuote jo ostoskorissa
                    var cartItem = cart.FirstOrDefault(c => c.ProductId == productId);
                    if (cartItem == null)
                    {
                        // Lisätään uusi tuote ostoskoriin
                        cart.Add(new CartItem
                        {
                            ProductId = selectedProduct.Id,
                            ProductName = selectedProduct.Name,
                            UnitPrice = selectedProduct.PriceWithTax, // Hinta ALV:n kanssa
                            Quantity = qty
                        });
                    }
                    else
                    {
                        // Päivitetään olemassa olevan tuotteen määrä
                        cartItem.Quantity += qty;
                    }

                    UpdateCart();
                }
                else
                {
                    MessageBox.Show("Virheellinen määrä.");
                }
            }
            else
            {
                MessageBox.Show("Valitse tuote lisättäväksi ostoskoriin.");
            }
        }

        private void UpdateCart()
        {
            // Päivitetään ostoskorin näkymä
            CartDataGridView.DataSource = null;
            CartDataGridView.DataSource = cart.Select(c => new
            {
                c.ProductId,
                Tuote = c.ProductName,
                Määrä = c.Quantity,
                Hinta = c.UnitPrice.ToString("C2"),
                Yhteensä = (c.UnitPrice * c.Quantity).ToString("C2")
            }).ToList();

            // Päivitetään sarakkeiden otsikot
            CartDataGridView.Columns["Tuote"].HeaderText = "Tuote";
            CartDataGridView.Columns["Määrä"].HeaderText = "Määrä";
            CartDataGridView.Columns["Hinta"].HeaderText = "Hinta (€)";
            CartDataGridView.Columns["Yhteensä"].HeaderText = "Yhteensä (€)";

            // Päivitetään kokonaisarvo
            decimal totalWithTax = cart.Sum(c => c.UnitPrice * c.Quantity);
            TotalLabel.Text = $"Yhteensä (ALV 25.5%): {totalWithTax:C2}";
        }

        private async void BuyButton_Click(object sender, EventArgs e)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Ostoskorisi on tyhjä.");
                return;
            }

            var name = NameTextBox.Text;
            var code = CodeTextBox.Text;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Syötä nimi ja kutsutunnus.");
                return;
            }

            var purchase = new Purchase
            {
                CustomerName = name,
                Code = code,
                Items = cart.Select(c => new PurchaseItem { ProductId = c.ProductId, Quantity = c.Quantity }).ToList()
            };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(purchase), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("purchase", content);
                response.EnsureSuccessStatusCode();
                MessageBox.Show("Ostoksesi on vahvistettu!");
                cart.Clear();
                UpdateCart();
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Virhe ostoksessa: {ex.Message}");
            }
        }
    }

    public class Product
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonIgnore]
        public decimal PriceWithTax { get; set; }

        public decimal GetPriceAsDecimal()
        {
            Price = Price.Replace(',', '.');
            return decimal.TryParse(Price, out decimal result) ? result : 0;
        }
    }

    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }

    public class Purchase
    {
        public string CustomerName { get; set; }
        public string Code { get; set; }
        public List<PurchaseItem> Items { get; set; }
    }

    public class PurchaseItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 300,
                Height = 150,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };

            Label textLabel = new Label() { Left = 10, Top = 20, Text = text, Width = 260 };
            TextBox inputBox = new TextBox() { Left = 10, Top = 50, Width = 260 };
            Button confirmation = new Button() { Text = "OK", Left = 190, Width = 80, Top = 80 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(confirmation);
            prompt.ShowDialog();
            return inputBox.Text;
        }
    }
}
