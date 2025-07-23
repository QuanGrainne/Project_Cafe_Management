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

namespace CafeManagement
{
    /// <summary>
    /// Interaction logic for ManageTableWindow.xaml
    /// </summary>
    public partial class ManageTableWindow : Window
    {
        private CafeManagementContext context = new CafeManagementContext();
        private CafeTable selectedTable;

        public ManageTableWindow()
        {
            InitializeComponent();
            LoadTables();
        }

        private void LoadTables()
        {
            dgTables.ItemsSource = context.CafeTables.ToList();
        }

        private void ClearForm()
        {
            txtTableName.Text = "";
            cbStatus.SelectedIndex = 0;
            selectedTable = null;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTableName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên bàn.");
                return;
            }

            var table = new CafeTable
            {
                TableName = txtTableName.Text.Trim(),
                Status = (cbStatus.SelectedItem as ComboBoxItem)?.Content.ToString()
            };

            context.CafeTables.Add(table);
            context.SaveChanges();
            LoadTables();
            ClearForm();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTable == null)
            {
                MessageBox.Show("Vui lòng chọn bàn để sửa.");
                return;
            }

            selectedTable.TableName = txtTableName.Text.Trim();
            selectedTable.Status = (cbStatus.SelectedItem as ComboBoxItem)?.Content.ToString();

            context.SaveChanges();
            LoadTables();
            ClearForm();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTable == null)
            {
                MessageBox.Show("Vui lòng chọn bàn để xóa.");
                return;
            }

            var result = MessageBox.Show($"Bạn chắc chắn muốn xóa bàn '{selectedTable.TableName}'?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                context.CafeTables.Remove(selectedTable);
                context.SaveChanges();
                LoadTables();
                ClearForm();
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void dgTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedTable = dgTables.SelectedItem as CafeTable;
            if (selectedTable != null)
            {
                txtTableName.Text = selectedTable.TableName;
                cbStatus.SelectedItem = cbStatus.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content.ToString() == selectedTable.Status);
            }
        }
    }
}
