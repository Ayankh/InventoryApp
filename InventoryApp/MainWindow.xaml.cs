using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace InventoryManagementApp
{
    public partial class MainWindow : Window
    {
        private const string DefaultFilePath = "default_inventory.json";
        private ObservableCollection<Item> items = new ObservableCollection<Item>();
        private string currentFilePath = DefaultFilePath;
        public MainWindow()
        {
            InitializeComponent();
            dgInventory.ItemsSource = items;
            LoadInventoryFromFile(); // Load inventory data at startup
        }

        // Define Item class for data structure
        private class Item
        {
            public int ItemID { get; set; }
            public string ItemName { get; set; }
            public int Quantity { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }

        }


        private bool IsValidItemID(string input)
        {
            // Validate input data for Item ID
            if (!int.TryParse(input, out int newItemID) || newItemID <= 0)
            {
                MessageBox.Show("Please enter a valid positive Item ID.");
                return false;
            }

            // Check if the Item ID is a duplicate
            if (items.Any(item => item.ItemID == newItemID))
            {
                MessageBox.Show("Duplicate Item ID. Please enter a unique Item ID.");
                return false;
            }
            return true;
        }

        private bool IsValidQuantity(string input)
        {
            // Validate Quantity and Price
            if (!int.TryParse(input, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid positive Quantity.");
                return false;
            }
            return true;
        }

        private bool IsValidPrice(string input)
        {
            if (!decimal.TryParse(input, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid positive Price.");
                return false;
            }
            return true;
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            // Check if any field is empty
            if (string.IsNullOrWhiteSpace(txtItemID.Text) ||
                string.IsNullOrWhiteSpace(txtItemName.Text) ||
                string.IsNullOrWhiteSpace(txtItemQuantity.Text) ||
                string.IsNullOrWhiteSpace(txtItemDescription.Text) ||
                string.IsNullOrWhiteSpace(txtItemPrice.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (!IsValidItemID(txtItemID.Text) || !IsValidQuantity(txtItemQuantity.Text) || !IsValidPrice(txtItemPrice.Text)) 
            {
                return;
            }
            int newItemID = int.Parse(txtItemID.Text);
            int quantity = int.Parse(txtItemQuantity.Text);
            int price = int.Parse(txtItemPrice.Text);
            // Create a new item and add it to the ObservableCollection
            Item newItem = new Item
            {
                ItemID = newItemID,
                ItemName = txtItemName.Text,
                Quantity = quantity,
                Description = txtItemDescription.Text,
                Price = price
            };

            items.Add(newItem);

            // Save the updated inventory
            SaveInventoryToFile();

            // Clear the text boxes
            txtItemID.Clear();
            txtItemName.Clear();
            txtItemDescription.Clear();
            txtItemPrice.Clear();
            txtItemQuantity.Clear();
        }



        private void btnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            // Check if an item is selected in the DataGrid
            if (dgInventory.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to delete.");
                return;
            }

            // Remove the selected item from the ObservableCollection
            Item selectedItem = (Item)dgInventory.SelectedItem;
            items.Remove(selectedItem);

            // Save the updated inventory
            SaveInventoryToFile();
        }

        private void btnModifyItem_Click(object sender, RoutedEventArgs e)
        {
            // Check if an item is selected in the DataGrid
            if (dgInventory.SelectedItem == null)
            {
                MessageBox.Show("Please select an item to modify.");
                return;
            }


            // Modify the selected item in the ObservableCollection
            Item selectedItem = (Item)dgInventory.SelectedItem;


            // Save the updated inventory
            SaveInventoryToFile();

            MessageBox.Show("Changes have been saved.");
        }




        private void LoadInventoryFromFile()
        {
            if (File.Exists(this.currentFilePath))
            {
                try
                {
                    string json = File.ReadAllText(currentFilePath);
                    items = JsonConvert.DeserializeObject<ObservableCollection<Item>>(json);
                    dgInventory.ItemsSource = items;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading inventory: {ex.Message}");
                }
            }
        }

        private void SaveInventoryToFile()
        {
            try
            {
                string json = JsonConvert.SerializeObject(items);
                File.WriteAllText(currentFilePath, json);
                isDataChanged = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving inventory: {ex.Message}");
            }
        }

        private void MenuLoad_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                currentFilePath = openFileDialog.FileName;
                LoadInventoryFromFile();
            }
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                this.currentFilePath = saveFileDialog.FileName;
                SaveInventoryToFile(); 
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = txtSearch.Text.ToLower();

            IEnumerable<Item> searchResults = items.Where(item =>
                item.ItemID.ToString().ToLower().Contains(searchQuery) ||
                item.ItemName.ToLower().Contains(searchQuery)
            );

            dgInventory.ItemsSource = new ObservableCollection<Item>(searchResults);
        }
        private void btnSearchClear_Click(object sender, RoutedEventArgs e)
        {
            dgInventory.ItemsSource = this.items;
        }
        

        private void dgInventory_CellEditEnding_1(object sender, DataGridCellEditEndingEventArgs e)
        {
            Item editedItem = e.Row.Item as Item;

            DataGridColumn editedColumn = e.Column;

            if (editedItem != null)
            {
                if (editedColumn.Header.ToString() == "ItemID")
                {
                    TextBox textBox = e.EditingElement as TextBox;
                    if (!IsValidItemID(textBox.Text))
                    {
                        return;
                    }
                }
                else if (editedColumn.Header.ToString() == "Quantity")
                {
                    TextBox textBox = e.EditingElement as TextBox;
                    if (!IsValidQuantity(textBox.Text))
                    {
                        return;
                    }
                }
                else if (editedColumn.Header.ToString() == "Price")
                {
                    TextBox textBox = e.EditingElement as TextBox;
                    if (!IsValidPrice(textBox.Text))
                    {
                        return;
                    }
                }
            }
        }

    }
}
