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
    /// Interaction logic for ManageUsersWindow.xaml
    /// </summary>
    public partial class ManageUsersWindow : Window
    {
        private CafeManagementContext context = new CafeManagementContext();
        private User selectedUser;

        public ManageUsersWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            dgUsers.ItemsSource = context.Users.ToList();
        }

        private void ClearForm()
        {
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtFullName.Text = "";
            cbRole.SelectedIndex = 0;

            txtUsername.IsEnabled = true;
            cbRole.IsEnabled = true;

            selectedUser = null;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string fullName = txtFullName.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin: Username, Mật khẩu, Họ tên.");
                return;
            }

            if (fullName.Length < 3 || fullName.Length > 50)
            {
                MessageBox.Show("Họ tên phải từ 3 đến 50 ký tự.");
                return;
            }

            if (fullName.Any(char.IsDigit))
            {
                MessageBox.Show("Họ tên không được chứa số.");
                return;
            }

            if (context.Users.Any(u => u.Username == username))
            {
                MessageBox.Show("Username đã tồn tại.");
                return;
            }

            var user = new User
            {
                Username = username,
                Password = password,
                FullName = fullName,
                Role = "Staff",
                Status = "Active"
            };

            context.Users.Add(user);
            context.SaveChanges();
            LoadUsers();
            ClearForm();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser == null)
            {
                MessageBox.Show("Vui lòng chọn người dùng để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string fullName = txtFullName.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng không để trống họ tên và mật khẩu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (fullName.Length < 3 || fullName.Length > 50)
            {
                MessageBox.Show("Họ tên phải từ 3 đến 50 ký tự.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (fullName.Any(char.IsDigit))
            {
                MessageBox.Show("Họ tên không được chứa số.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 4)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 4 ký tự.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            selectedUser.FullName = fullName;
            selectedUser.Password = password;

            if (selectedUser.Role != "Admin")
            {
                string newRole = (cbRole.SelectedItem as ComboBoxItem)?.Content.ToString();
                selectedUser.Role = newRole;
            }

            context.SaveChanges();
            LoadUsers();
            ClearForm();

            MessageBox.Show("Cập nhật người dùng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ToggleLock_Click(object sender, RoutedEventArgs e)
        {
            if (selectedUser == null)
            {
                MessageBox.Show("Vui lòng chọn người dùng để khóa/mở.");
                return;
            }

            if (selectedUser.Role == "Admin")
            {
                MessageBox.Show("Không thể khóa tài khoản Admin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string newStatus = selectedUser.Status == "Active" ? "Locked" : "Active";
            string action = newStatus == "Locked" ? "khóa" : "mở khóa";

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn {action} tài khoản '{selectedUser.Username}'?", "Xác nhận", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                selectedUser.Status = newStatus;
                context.SaveChanges();
                LoadUsers();
                ClearForm();

                MessageBox.Show($"Tài khoản đã được {action}.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedUser = dgUsers.SelectedItem as User;
            if (selectedUser != null)
            {
                txtUsername.Text = selectedUser.Username;
                txtUsername.IsEnabled = false;

                txtPassword.Text = selectedUser.Password;
                txtFullName.Text = selectedUser.FullName;

                cbRole.SelectedItem = cbRole.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(i => i.Content.ToString() == selectedUser.Role);

                cbRole.IsEnabled = selectedUser.Role != "Admin";
            }
        }
    }
}
