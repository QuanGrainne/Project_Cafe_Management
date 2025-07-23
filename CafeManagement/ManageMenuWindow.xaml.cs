using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CafeManagement.Models;
using Microsoft.EntityFrameworkCore;
using Models = CafeManagement.Models;


namespace CafeManagement
{
    /// <summary>
    /// Interaction logic for ManageMenuWindow.xaml
    /// </summary>
    public partial class ManageMenuWindow : Window
    {
        private CafeManagementContext context = new CafeManagementContext();
        private Models.MenuItem selectedItem; // ✅ dùng alias

        public ManageMenuWindow()
        {
            InitializeComponent();
            LoadMenuItems();
        }

        private void LoadMenuItems()
        {
            dgMenu.ItemsSource = context.MenuItems.ToList();
        }

        private void ClearForm()
        {
            txtName.Text = "";
            txtPrice.Text = "";
            txtDescription.Text = "";
            chkAvailable.IsChecked = true;
            selectedItem = null;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Vui lòng nhập tên món và giá.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Giá không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var item = new Models.MenuItem
            {
                ItemName = txtName.Text.Trim(),
                Price = price,
                Description = txtDescription.Text.Trim(),
                IsAvailable = chkAvailable.IsChecked == true
            };

            context.MenuItems.Add(item);
            context.SaveChanges();
            LoadMenuItems();
            ClearForm();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn món cần sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Giá không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            selectedItem.ItemName = txtName.Text.Trim();
            selectedItem.Price = price;
            selectedItem.Description = txtDescription.Text.Trim();
            selectedItem.IsAvailable = chkAvailable.IsChecked == true;

            context.SaveChanges();
            LoadMenuItems();
            ClearForm();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn món để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa món '{selectedItem.ItemName}' không?",
                                         "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                context.MenuItems.Remove(selectedItem);
                context.SaveChanges();
                LoadMenuItems();
                ClearForm();
            }
        }

        private void dgMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedItem = dgMenu.SelectedItem as Models.MenuItem;
            if (selectedItem != null)
            {
                txtName.Text = selectedItem.ItemName;
                txtPrice.Text = selectedItem.Price.ToString();
                txtDescription.Text = selectedItem.Description;
                chkAvailable.IsChecked = selectedItem.IsAvailable;
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }
    }
}