using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace VarastoHallinta
{
    public partial class Tuotteet : Form
    {
        private readonly HttpClient client;
        private List<Product> products = new();
        private List<CartItem> cart = new();

        public Tuotteet()
        {
            InitializeComponent();
            client = new HttpClient
            {
                BaseAddress = new Uri(publix.Domain)
            };
            LoadProducts();

        }

        private async void LoadProducts()
        {
            try
            {
                var response = await client.GetAsync("products");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                products = JsonSerializer.Deserialize<List<Product>>(json, options) ?? new List<Product>();

                ProductsDataGridView.DataSource = products.Select(p => new
                {
                    p.Id,
                    Tuote = p.Name,
                    Hinta = p.Price.ToString("C2"),
                    Varastossa = p.Quantity
                }).ToList();

                ProductsDataGridView.Columns["Id"].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Virhe tuotteiden lataamisessa: {ex.Message}");
            }
        }

        private void AddToCartButton_Click(object sender, EventArgs e)
        {
            if (ProductsDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Valitse tuote lisättäväksi ostoskoriin.");
                return;
            }

            var selectedRow = ProductsDataGridView.SelectedRows[0];
            int productId = (int)selectedRow.Cells["Id"].Value;
            var product = products.FirstOrDefault(p => p.Id == productId);

            if (product == null) return;

            var quantityText = Prompt.ShowDialog("Anna määrä:", "Lisää ostoskoriin");
            if (!int.TryParse(quantityText, out var qty) || qty <= 0 || qty > product.Quantity)
            {
                MessageBox.Show("Virheellinen määrä.");
                return;
            }

            var cartItem = cart.FirstOrDefault(c => c.ProductId == product.Id);
            if (cartItem == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = qty
                });
            }
            else
            {
                cartItem.Quantity += qty;
            }

            UpdateCart();
        }

        private void UpdateCart()
        {
            CartDataGridView.DataSource = null;
            CartDataGridView.DataSource = cart.Select(c => new
            {
                c.ProductId,
                Tuote = c.ProductName,
                Määrä = c.Quantity,
                Hinta = c.UnitPrice.ToString("C2"),
                Yhteensä = (c.UnitPrice * c.Quantity).ToString("C2")
            }).ToList();

            CartDataGridView.Columns["ProductId"].Visible = false;
            decimal totalWithTax = cart.Sum(c => c.UnitPrice * c.Quantity);
            TotalLabel.Text = $"Yhteensä (ALV 25.5%): {totalWithTax:C2}";
        }

        
        public class Product
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("price")]
            public decimal Price { get; set; }
            [JsonPropertyName("quantity")]
            public int Quantity { get; set; }
        }

        public class CartItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
        }

        public static class Prompt
        {
            public static string ShowDialog(string text, string caption)
            {
                using var prompt = new Form
                {
                    Width = 300,
                    Height = 150,
                    Text = caption,
                    StartPosition = FormStartPosition.CenterScreen
                };

                var textLabel = new Label { Left = 10, Top = 20, Text = text, Width = 260 };
                var inputBox = new TextBox { Left = 10, Top = 50, Width = 260 };
                var confirmation = new Button { Text = "OK", Left = 190, Width = 80, Top = 80 };

                confirmation.Click += (_, _) => prompt.Close();
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(inputBox);
                prompt.Controls.Add(confirmation);
                prompt.ShowDialog();
                return inputBox.Text;
            }
        }

        private async void BuyButton_Click(object sender, EventArgs e)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Ostoskori on tyhjä. Lisää tuotteita ostoskoriin ennen ostamista.");
                return;
            }

            string userName = NameTextBox.Text.Trim();
            string callSign = CodeTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(callSign))
            {
                MessageBox.Show("Nimi ja kutsutunnus ovat pakollisia.");
                return;
            }

            var purchase = new
            {
                callsign = callSign,
                name = userName,
                purchased_items = cart.Select(c => new
                {
                    id = c.ProductId,
                    quantity = c.Quantity
                }).ToList(),
                total_amount = cart.Sum(c => c.UnitPrice * c.Quantity),
                date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            try
            {
                var jsonContent = JsonSerializer.Serialize(purchase);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("purchase", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Osto onnistui!");
                    cart.Clear();
                    UpdateCart();
                    LoadProducts();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Virhe ostoa tehdessä: {response.StatusCode}\n{error}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Virhe ostoa tehdessä: {ex.Message}");
            }
        }

        private void exit_Click(object sender, EventArgs e)
        {
            var log = new Etusivu();
            log.Show();
            this.Hide();
        }
    }
}
