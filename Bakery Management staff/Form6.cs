using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Bakery_Management_staff
{
    public partial class Form6 : Form
    {
        private readonly string connectionString = "server=localhost;user id=root;password=;database=userdb;";

        public Form6()
        {
            InitializeComponent();
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            LoadProductNames();
            LoadStockData();
        }

        private void LoadProductNames()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT ProductID, ProductName FROM bakeryproducts", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    comboBoxProduct.DisplayMember = "ProductName";
                    comboBoxProduct.ValueMember = "ProductID";
                    comboBoxProduct.DataSource = dt;
                }
            }
        }

        private void btnRestock_Click(object sender, EventArgs e)
        {
            int productId = Convert.ToInt32(comboBoxProduct.SelectedValue);
            int quantity = (int)numericUpDownQuantity.Value;
            DateTime restockDate = dateTimePickerRestock.Value;
            DateTime lastUpdate = DateTime.Now;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // Insert new stock record
                var cmd = new MySqlCommand(
                    "INSERT INTO productstocks (ProductID, Quantity, OrderDate, LastUpdate) VALUES (@pid, @qty, @odate, @lupdate)", conn);
                cmd.Parameters.AddWithValue("@pid", productId);
                cmd.Parameters.AddWithValue("@qty", quantity);
                cmd.Parameters.AddWithValue("@odate", restockDate);
                cmd.Parameters.AddWithValue("@lupdate", lastUpdate);
                cmd.ExecuteNonQuery();
            }
            LoadStockData();
        }

        private void LoadStockData()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT s.StockID, p.ProductName, s.Quantity, s.LastUpdate
                      FROM productstocks s
                      JOIN bakeryproducts p ON s.ProductID = p.ProductID", conn);
                var adapter = new MySqlDataAdapter(cmd);
                var dt = new DataTable();
                adapter.Fill(dt);
                guna2DataGridView1.DataSource = dt;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // Reset ComboBox for product selection
            comboBoxProduct.SelectedIndex = -1;

            // Reset quantity to 0
            numericUpDownQuantity.Value = 0;

            // Reset date to today
            dateTimePickerRestock.Value = DateTime.Today;
        }
        private void btnProduct_Click_1(object sender, EventArgs e)
        {
            Form5 stockForm = new Form5();
            stockForm.Show();
            this.Hide();

        }

        private void btnOrder_Click_1(object sender, EventArgs e)
        {
            Form8 orderForm = new Form8 ();
            orderForm.Show();
            this.Hide();

        }
    }
}
