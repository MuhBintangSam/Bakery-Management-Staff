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
    public partial class Form5 : Form
    {
        private readonly string connectionString = "server=localhost;user id=root;password=;database=userdb;";
        int productId = 1;
        int selectedRowIndex = -1;

        public Form5()
        {
            InitializeComponent();
            InitializeCategory();
            InitializeDataGridView();
        }

        private void InitializeCategory()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("Wet Ingredient");
            cmbCategory.Items.Add("Dry Ingredient");
            cmbCategory.Items.Add("Bread");
            cmbCategory.Items.Add("Cake");
            cmbCategory.Items.Add("Cookies");
            cmbCategory.SelectedIndex = 0;
        }

        private void InitializeDataGridView()
        {
            guna2DataGridView1.ColumnCount = 4;
            guna2DataGridView1.Columns[0].Name = "ID";
            guna2DataGridView1.Columns[1].Name = "NAME";
            guna2DataGridView1.Columns[2].Name = "CATEGORY";
            guna2DataGridView1.Columns[3].Name = "PRICE";

            guna2DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            guna2DataGridView1.MultiSelect = false;

            // Optional: make ID column read-only
            guna2DataGridView1.Columns[0].ReadOnly = true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string name = txtProductName.Text.Trim();
            string category = cmbCategory.SelectedItem.ToString();
            string priceText = guna2TextBox1.Text.Trim();
            decimal price;

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a product name.");
                return;
            }

            if (!decimal.TryParse(priceText, out price))
            {
                MessageBox.Show("Please enter a valid price.");
                return;
            }

            // Save to database
            string connectionString = "server=localhost;database=userdb;uid=root;pwd=;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO bakeryproducts (ProductName, Category, Price) VALUES (@ProductName, @Category, @Price)";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductName", name);
                        command.Parameters.AddWithValue("@Category", category);
                        command.Parameters.AddWithValue("@Price", price);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Product added successfully to the database.");
                        }
                        else
                        {
                            MessageBox.Show("Failed to add the product to the database.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }

            // Add to DataGridView
            guna2DataGridView1.Rows.Add(productId++, name, category, price.ToString("0.00"));
            ClearInputs();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (guna2DataGridView1.SelectedRows.Count > 0)
            {
                guna2DataGridView1.Rows.RemoveAt(guna2DataGridView1.SelectedRows[0].Index);
                ClearInputs();
            }
            else
            {
                MessageBox.Show("Please select a row to delete.");
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedRowIndex >= 0 && selectedRowIndex < guna2DataGridView1.Rows.Count)
            {
                string name = txtProductName.Text.Trim();
                string category = cmbCategory.SelectedItem.ToString();
                string priceText = guna2TextBox1.Text.Trim();
                decimal price;

                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Please enter a product name.");
                    return;
                }

                if (!decimal.TryParse(priceText, out price))
                {
                    MessageBox.Show("Please enter a valid price.");
                    return;
                }

                DataGridViewRow row = guna2DataGridView1.Rows[selectedRowIndex];
                row.Cells[1].Value = name;
                row.Cells[2].Value = category;
                row.Cells[3].Value = price.ToString("0.00");

                ClearInputs();
            }
            else
            {
                MessageBox.Show("Please select a row to update.");
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
            txtProductName.Clear();
            cmbCategory.SelectedIndex = 0;
            txtPrice.Clear();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selectedRowIndex = e.RowIndex;
                DataGridViewRow row = guna2DataGridView1.Rows[e.RowIndex];

                txtProductName.Text = row.Cells[1].Value.ToString();
                cmbCategory.SelectedItem = row.Cells[2].Value.ToString();
                txtPrice.Text = row.Cells[3].Value.ToString();
            }
        }

        private void ClearInputs()
        {
            txtProductName.Clear();
            txtPrice.Clear();
            cmbCategory.SelectedIndex = 0;
            selectedRowIndex = -1;
            guna2DataGridView1.ClearSelection();
        }
        // Navigation buttons

        private void btnStocks_Click_1(object sender, EventArgs e)
        {
            Form6 stockForm = new Form6();
            stockForm.Show();
            this.Hide();

        }

        private void btnOrder_Click_1(object sender, EventArgs e)
        {
            Form8 orderForm = new Form8();
            orderForm.Show();
            this.Hide();
            
        }


    }


}