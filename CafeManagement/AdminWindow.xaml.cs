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
    /// Interaction logic for AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        private User loggedInUser;

        public AdminWindow(User user)
        {
            InitializeComponent();
            loggedInUser = user;

            // Gán lời chào
            txtWelcome.Text = $"Xin chào, {user.FullName} ({user.Role})";
        }

        private void btnManageMenu_Click(object sender, RoutedEventArgs e)
        {
            var menuWindow = new ManageMenuWindow();
            menuWindow.ShowDialog(); // mở dạng dialog
        }

        private void btnManageTables_Click(object sender, RoutedEventArgs e)
        {
            new ManageTableWindow().ShowDialog();
        }

        private void btnViewBills_Click(object sender, RoutedEventArgs e)
        {
            var billsWindow = new ManageBillsWindow();
            billsWindow.ShowDialog();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }

        private void BtnManageUsers_Click(object sender, RoutedEventArgs e)
        {
            new ManageUsersWindow().ShowDialog();
        }
    }
}
