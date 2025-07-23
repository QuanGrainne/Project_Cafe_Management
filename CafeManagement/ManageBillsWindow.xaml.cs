using System;
using System.Collections.Generic;
using System.IO;
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

namespace CafeManagement
{
    /// <summary>
    /// Interaction logic for ManageBillsWindow.xaml
    /// </summary>
    public partial class ManageBillsWindow : Window
    {
        private CafeManagementContext context = new CafeManagementContext();
        private Bill selectedBill;

        public ManageBillsWindow()
        {
            InitializeComponent();
            LoadStaffs();
            LoadTables();
            LoadBills();
        }

        private void LoadStaffs()
        {
            var staffList = new List<object>
            {
                new { FullName = "Tất cả", UserId = 0 }
            };
            staffList.AddRange(context.Users
                .Where(u => u.Role == "Staff")
                .Select(u => new { FullName = u.FullName, UserId = u.UserId })
                .ToList());
            cbStaff.ItemsSource = staffList;
            cbStaff.DisplayMemberPath = "FullName";
            cbStaff.SelectedValuePath = "UserId";
            cbStaff.SelectedIndex = 0;
        }

        private void LoadTables()
        {
            var tableList = new List<object>
            {
                new { TableName = "Tất cả", TableId = 0 }
            };
            tableList.AddRange(context.CafeTables
                .Select(t => new { TableName = t.TableName, TableId = t.TableId })
                .ToList());
            cbTable.ItemsSource = tableList;
            cbTable.DisplayMemberPath = "TableName";
            cbTable.SelectedValuePath = "TableId";
            cbTable.SelectedIndex = 0;
        }

        private void LoadBills()
        {
            var query = context.Bills
                .Include(b => b.Order)
                .ThenInclude(o => o.Table)
                .Include(b => b.Order)
                .ThenInclude(o => o.Staff)
                .AsQueryable();

            if (dpFilterDate.SelectedDate.HasValue)
            {
                var selectedDate = dpFilterDate.SelectedDate.Value.Date;
                query = query.Where(b => b.PaymentTime.Date == selectedDate);
            }

            if (cbStaff.SelectedIndex > 0)
            {
                int? selectedStaffId = (int?)cbStaff.SelectedValue;
                if (selectedStaffId.HasValue && selectedStaffId > 0)
                {
                    query = query.Where(b => b.Order.StaffId == selectedStaffId);
                }
            }

            if (cbTable.SelectedIndex > 0)
            {
                int? selectedTableId = (int?)cbTable.SelectedValue;
                if (selectedTableId.HasValue && selectedTableId > 0)
                {
                    query = query.Where(b => b.Order.TableId == selectedTableId);
                }
            }

            dgBills.ItemsSource = query.ToList();
            dgBillDetails.ItemsSource = null;
            selectedBill = null;
        }

        private void FilterBills(object sender, EventArgs e)
        {
            LoadBills();
        }

        private void ClearFilter_Click(object sender, RoutedEventArgs e)
        {
            dpFilterDate.SelectedDate = null;
            cbStaff.SelectedIndex = 0;
            cbTable.SelectedIndex = 0;
            LoadBills();
        }

        private void dgBills_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedBill = dgBills.SelectedItem as Bill;
            if (selectedBill != null)
            {
                var orderDetails = context.OrderDetails
                    .Where(od => od.OrderId == selectedBill.OrderId)
                    .Select(od => new
                    {
                        Item = context.MenuItems.FirstOrDefault(i => i.ItemId == od.ItemId),
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        Total = od.Quantity * od.UnitPrice
                    })
                    .ToList();
                dgBillDetails.ItemsSource = orderDetails;
            }
            else
            {
                dgBillDetails.ItemsSource = null;
            }
        }

        private void PrintBill_Click(object sender, RoutedEventArgs e)
        {
            if (selectedBill == null)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn để in.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var order = context.Orders
                .Include(o => o.Table)
                .Include(o => o.Staff)
                .FirstOrDefault(o => o.OrderId == selectedBill.OrderId);

            var orderDetails = context.OrderDetails
                .Include(od => od.Item)
                .Where(od => od.OrderId == selectedBill.OrderId)
                .ToList();

            string fileName = $"Bill_{selectedBill.BillId}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = System.IO.Path.Combine(Environment.CurrentDirectory, fileName);

            try
            {
                using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("===== CAFE INVOICE =====");
                    writer.WriteLine($"Mã hóa đơn: {selectedBill.BillId}");
                    writer.WriteLine($"Bàn: {order?.Table?.TableName}");
                    writer.WriteLine($"Nhân viên: {order?.Staff?.FullName}");
                    writer.WriteLine($"Thời gian: {selectedBill.PaymentTime:dd/MM/yyyy HH:mm}");
                    writer.WriteLine();
                    writer.WriteLine("Món\t\tSL\tĐơn giá\tThành tiền");

                    foreach (var od in orderDetails)
                    {
                        string itemName = od.Item?.ItemName ?? "Không xác định";
                        if (itemName.Length < 8) itemName += "\t";
                        writer.WriteLine($"{itemName}\t{od.Quantity}\t{od.UnitPrice:N0}\t{(od.Quantity * od.UnitPrice):N0}");
                    }

                    writer.WriteLine("\n-------------------------");
                    writer.WriteLine($"Tổng tiền: {selectedBill.TotalAmount:N0} VNĐ");
                    writer.WriteLine($"Giảm giá: {selectedBill.Discount:N0} VNĐ");
                    writer.WriteLine($"Thành tiền: {selectedBill.FinalAmount:N0} VNĐ");
                    writer.WriteLine("=========================");
                }

                System.Diagnostics.Process.Start("notepad.exe", filePath);
                MessageBox.Show("In hóa đơn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi in hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
