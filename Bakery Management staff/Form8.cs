using Guna.UI2.WinForms;
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
    public partial class Form8 : Form
    {
        private readonly string connectionString = "server=localhost;user id=root;password=;database=userdb;";
        private Dictionary<string, (int ProductID, decimal Price)> productInfo = new Dictionary<string, (int, decimal)>();
        private decimal total = 0;

        public Form8()
        {
            InitializeComponent();
            LoadProducts();
            LoadOrderHistory();
        }

        private void LoadProducts()
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT ProductID, ProductName, Price FROM bakeryproducts", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString("ProductName");
                        int id = reader.GetInt32("ProductID");
                        decimal price = reader.GetDecimal("Price");
                        productInfo[name] = (id, price);
                        comboBoxProducts.Items.Add(name);
                    }
                }
            }
        }

        private void btnAddToCart_Click(object sender, EventArgs e)
        {
            if (comboBoxProducts.SelectedItem == null) return;
            string name = comboBoxProducts.SelectedItem.ToString();
            int qty = 1; // You can add a numeric up/down for quantity
            decimal price = productInfo[name].Price;
            decimal subtotal = price * qty;

            // Add to DataGridView
            cartGrid.Rows.Add(name, qty, subtotal);

            // Update total
            total += subtotal;
            lblTotal.Text = $"RM {total:F2}";

            // Insert into shoppingcart table
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("INSERT INTO shoppingcart (ProductID, Quantity, ItemPrice) VALUES (@pid, @qty, @price)", conn);
                cmd.Parameters.AddWithValue("@pid", productInfo[name].ProductID);
                cmd.Parameters.AddWithValue("@qty", qty);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.ExecuteNonQuery();
            }
        }

        private void btnDeleteOrder_Click(object sender, EventArgs e)
        {
            if (cartGrid.SelectedRows.Count == 0) return;
            var row = cartGrid.SelectedRows[0];
            string name = row.Cells[0].Value.ToString();
            int qty = Convert.ToInt32(row.Cells[1].Value);

            // Remove from DataGridView
            cartGrid.Rows.Remove(row);

            // Update total
            decimal price = productInfo[name].Price;
            total -= price * qty;
            lblTotal.Text = $"RM {total:F2}";

            // Delete from shoppingcart table
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("DELETE FROM shoppingcart WHERE ProductID=@pid LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@pid", productInfo[name].ProductID);
                cmd.ExecuteNonQuery();
            }
        }

        private void btnPlaceOrder_Click(object sender, EventArgs e)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // Insert into orders
                var cmdOrder = new MySqlCommand("INSERT INTO stafforders (OrderDate, TotalCost, Status) VALUES (CURDATE(), @total, 'Ordering')", conn);
                cmdOrder.Parameters.AddWithValue("@total", total);
                cmdOrder.ExecuteNonQuery();
                long orderId = cmdOrder.LastInsertedId;

                // Insert each cart item into orderdetails
                foreach (DataGridViewRow row in cartGrid.Rows)
                {
                    if (row.IsNewRow) continue;
                    string name = row.Cells[0].Value.ToString();
                    int qty = Convert.ToInt32(row.Cells[1].Value);
                    decimal price = productInfo[name].Price;
                    var cmdDetail = new MySqlCommand("INSERT INTO orderdetails (OrderID, ProductID, Quantity, ItemPrice) VALUES (@oid, @pid, @qty, @price)", conn);
                    cmdDetail.Parameters.AddWithValue("@oid", orderId);
                    cmdDetail.Parameters.AddWithValue("@pid", productInfo[name].ProductID);
                    cmdDetail.Parameters.AddWithValue("@qty", qty);
                    cmdDetail.Parameters.AddWithValue("@price", price);
                    cmdDetail.ExecuteNonQuery();
                }

                // Clear shoppingcart table
                var cmdClear = new MySqlCommand("DELETE FROM shoppingcart", conn);
                cmdClear.ExecuteNonQuery();
            }

            cartGrid.Rows.Clear();
            total = 0;
            lblTotal.Text = "RM 0.00";
            LoadOrderHistory();
        }

        private void btnMarkCompleted_Click(object sender, EventArgs e)
        {
            if (orderHistoryGrid.SelectedRows.Count == 0) return;
            var row = orderHistoryGrid.SelectedRows[0];
            int orderId = Convert.ToInt32(row.Cells[0].Value);

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("UPDATE stafforders SET Status='Received' WHERE OrderID=@oid", conn);
                cmd.Parameters.AddWithValue("@oid", orderId);
                cmd.ExecuteNonQuery();
            }
            LoadOrderHistory();
        }

        private void LoadOrderHistory()
        {
            orderHistoryGrid.Rows.Clear();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT OrderID, OrderDate, TotalCost, Status FROM stafforders ORDER BY OrderID DESC", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int orderId = reader.GetInt32("OrderID");
                        DateTime orderDate = reader.GetDateTime("OrderDate");
                        decimal totalCost = reader.GetDecimal("TotalCost");
                        string status = reader.GetString("Status");

                        string displayStatus = status.Equals("Ordering", StringComparison.OrdinalIgnoreCase) ? "Waiting" :
                                               status.Equals("Received", StringComparison.OrdinalIgnoreCase) ? "Completed" :
                                               status;

                        int itemCount = GetOrderItemCount(orderId);

                        // Correct column order and count
                        orderHistoryGrid.Rows.Add(orderId, itemCount, totalCost, orderDate.ToShortDateString(), displayStatus);
                    }
                }
            }
        }

        // Helper to get item count for an order
        private int GetOrderItemCount(int orderId)
        {
            int count = 0;
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT SUM(Quantity) FROM orderdetails WHERE OrderID=@oid", conn);
                cmd.Parameters.AddWithValue("@oid", orderId);
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                    count = Convert.ToInt32(result);
            }
            return count;
        }

        private void btnClearCart_Click(object sender, EventArgs e)
        {
            // Clear the DataGridView rows
            cartGrid.Rows.Clear();

            // Reset the total
            total = 0;
            lblTotal.Text = "RM 0.00";

            // Remove all items from the shoppingcart table
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new MySqlCommand("DELETE FROM shoppingcart", conn);
                cmd.ExecuteNonQuery();
            }
        }
        private void btnProduct_Click_1(object sender, EventArgs e)
        {
            Form5 stockForm = new Form5();
            stockForm.Show();
            this.Hide();

        }

        private void btnStocks_Click_1(object sender, EventArgs e)
        {
            Form6 orderForm = new Form6();
            orderForm.Show();
            this.Hide();

        }
    }
}