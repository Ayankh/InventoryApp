using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace InventoryManagementApp
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Item> items = new ObservableCollection<Item>();
        private string currentFilePath = null;
        private bool isDataChanged = false;

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
            public string Description { get; set; }
            public decimal Price { get; set; }
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            // Validate input data
            if (!ValidateInput())
                return;

            int newItemID = int.Parse(txtItemID.Text);

            // Check if the item with the same ID already exists
            if (items.Any(item => item.ItemID == newItemID))
            {
                MessageBox.Show("Duplicate Item ID");
                return;
            }

            // Create a new item and add it to the ObservableCollection
            Item newItem = new Item
            {
                ItemID = int.Parse(txtItemID.Text),
                ItemName = txtItemName.Text,
                Description = txtItemDescription.Text,
                Price = decimal.Parse(txtItemPrice.Text)
            };


            items.Add(newItem);

            // Save the updated inventory chaged
            SaveInventoryToFile();

            // Clear the text boxes
            txtItemID.Text = "";
            txtItemName.Text = "";
            txtItemDescription.Text = "";
            txtItemPrice.Text = "";
            txtItemQuantity.Text = "";


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
            //Item selectedItem = (Item)dgInventory.SelectedItem;
            // MessageBox.Show(selectedItem.ItemID.ToString());
            // Validate input data
            if (!ValidateInput())
                return;

            // Modify the selected item in the ObservableCollection
            Item selectedItem = (Item)dgInventory.SelectedItem;
            //selectedItem.ItemID = int.Parse(txtItemID.Text);
            //selectedItem.ItemName = txtItemName.Text;
            //selectedItem.Description = txtItemDescription.Text;
            //selectedItem.Price = decimal.Parse(txtItemPrice.Text);

            // Save the updated inventory
            SaveInventoryToFile();

            MessageBox.Show("Changes have been saved.");
        }


        private bool ValidateInput()
        {
            Item selectedItem = (Item)dgInventory.SelectedItem;
            // Data validation for Item ID
            if (selectedItem.ItemID <= 0)
            {
                MessageBox.Show("Please enter a valid positive Item ID.");
                return false;
            }

            // Data validation for Price
            if (selectedItem.Price <= 0)
            {
                MessageBox.Show("Please enter a valid positive Price.");
                return false;
            }

            return true;
        }

        private void LoadInventoryFromFile()
        {
            if (File.Exists(currentFilePath))
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
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                currentFilePath = openFileDialog.FileName;
                SaveInventoryToFile();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // Implement search functionality
            string searchQuery = txtSearch.Text.ToLower();

            IEnumerable<Item> searchResults = items.Where(item =>
                item.ItemID.ToString().ToLower().Contains(searchQuery) ||
                item.ItemName.ToLower().Contains(searchQuery)
            );

            dgInventory.ItemsSource = new ObservableCollection<Item>(searchResults);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isDataChanged)
            {
                var result = MessageBox.Show("Do you want to save changes before exiting?", "Save Changes",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveInventoryToFile();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true; // Cancel the form close
                }
                // Otherwise, allow the form to close
            }
        }
    }
}
