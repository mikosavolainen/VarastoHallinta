using Microsoft.VisualBasic.Logging;

namespace VarastoHallinta
{
    public partial class Etusivu : Form
    {
        public Etusivu()
        {
            InitializeComponent();
        }

        private void BuyButton_Click(object sender, EventArgs e)
        {
            var log = new Tuotteet();
            log.Show();
            this.Hide();
        }
    }
}