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
    public partial class Varasto : Form
    {
        private readonly HttpClient client;
        private List<Product> products = new();

        public Varasto()
        {
            InitializeComponent();
            client = new HttpClient
            {
                BaseAddress = new Uri(publix.Domain)
            };
            LoadProducts();

            productNameTextBox.PlaceholderText = "Nimi";
            productPriceTextBox.PlaceholderText = "Hinta";
            productQuantityTextBox.PlaceholderText = "Määrä";
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

        private async void AddProductButton_Click(object sender, EventArgs e)
        {
            string name = productNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(name) || !decimal.TryParse(productPriceTextBox.Text, out decimal price) ||
                !int.TryParse(productQuantityTextBox.Text, out int quantity))
            {
                MessageBox.Show("Täytä kaikki kentät oikein.");
                return;
            }

            var newProduct = new { Name = name, Price = price, Quantity = quantity };
            var content = new StringContent(JsonSerializer.Serialize(newProduct), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("addProduct", content);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Tuote lisätty onnistuneesti!");
                    LoadProducts();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    MessageBox.Show("Tuote on jo olemassa!");
                }
                else
                {
                    MessageBox.Show($"Virhe tuotetta lisättäessä: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Virhe yhteydessä: {ex.Message}");
            }
        }

        private async void AddQuantityButton_Click(object sender, EventArgs e)
        {
            if (ProductsDataGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Valitse tuote, jonka määrää haluat päivittää.");
                return;
            }

            var selectedRow = ProductsDataGridView.SelectedRows[0];
            int productId = (int)selectedRow.Cells["Id"].Value;
            var product = products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                MessageBox.Show("Tuotetta ei löytynyt.");
                return;
            }

            if (!int.TryParse(quantity.Text, out int newQuantity) || newQuantity < 0)
            {
                MessageBox.Show("Syötä kelvollinen määrä.");
                return;
            }

            product.Quantity = newQuantity;

            var updateContent = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");

            try
            {
                MessageBox.Show($"Päivitetään tuotteen ID:llä {product.Id}, uusi määrä: {newQuantity}");

                var response = await client.PutAsync($"updateProduct/{product.Id}", updateContent);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Määrä päivitetty onnistuneesti!");
                    LoadProducts();
                }
                else
                {
                    MessageBox.Show($"Virhe tuotteen määrän päivityksessä: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Virhe yhteydessä: {ex.Message}");
            }
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

        private void ProductsDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (ProductsDataGridView.SelectedRows.Count > 0)
            {
                var selectedRow = ProductsDataGridView.SelectedRows[0];

                int productId = (int)selectedRow.Cells["Id"].Value;
                int productQuantity = (int)selectedRow.Cells["Varastossa"].Value;

                quantity.Text = productQuantity.ToString();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void quantity_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
