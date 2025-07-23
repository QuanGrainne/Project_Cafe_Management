using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CafeManagement.Models;
using Models = CafeManagement.Models;

namespace CafeManagement
{
    public partial class StaffWindow : Window
    {
        private CafeManagementContext context = new();
        private Dictionary<int, TempOrderData> tableOrderDetails = new();
        private List<OrderDetail> orderDetails = new();
        private User currentUser;
        private decimal globalDiscount = 0;
        private CafeTable selectedTable;

        public StaffWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            LoadTables();
            LoadMenu();

            globalDiscount = GlobalDiscountStorage.Load();
            txtDiscount.Text = globalDiscount.ToString();

            tableOrderDetails = TempOrderStorage.Load();
        }

        private void LoadTables()
        {
            wpTables.Children.Clear();
            var tables = context.CafeTables.ToList();
            foreach (var table in tables)
            {
                var button = new Button
                {
                    Content = table.TableName,
                    Tag = table,
                    Width = 80,
                    Height = 40,
                    Margin = new Thickness(5),
                    Background = table.Status == "Available" ? Brushes.Green : Brushes.Red,
                    Foreground = Brushes.White
                };
                button.Click += TableButton_Click;
                wpTables.Children.Add(button);
            }
        }

        private void TableButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var table = button.Tag as CafeTable;

            if (selectedTable != null && selectedTable.TableId != table.TableId)
            {
                tableOrderDetails[selectedTable.TableId] = new TempOrderData
                {
                    OrderDetails = orderDetails.Select(od => new OrderDetail
                    {
                        ItemId = od.ItemId,
                        Item = od.Item,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList()
                };
            }

            selectedTable = table;
            txtTableStatus.Text = $"Trạng thái: {table.Status}";

            if (tableOrderDetails.ContainsKey(table.TableId))
            {
                var tempData = tableOrderDetails[table.TableId];
                orderDetails = tempData.OrderDetails.Select(od => new OrderDetail
                {
                    ItemId = od.ItemId,
                    Item = od.Item,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice
                }).ToList();
            }
            else
            {
                orderDetails.Clear();
            }

            RefreshOrderItems();
            TempOrderStorage.Save(tableOrderDetails);

            foreach (Button btn in wpTables.Children)
            {
                var btnTable = btn.Tag as CafeTable;
                btn.Background = btnTable.TableId == selectedTable.TableId ? Brushes.Blue : (btnTable.Status == "Available" ? Brushes.Green : Brushes.Red);
            }
        }

        private void LoadMenu()
        {
            dgMenu.ItemsSource = context.MenuItems.Where(i => i.IsAvailable).ToList();
        }

        private void AddToOrder_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTable == null)
            {
                MessageBox.Show("Vui lòng chọn bàn.");
                return;
            }

            var button = sender as Button;
            var row = FindParent<DataGridRow>(button);
            var item = row?.Item as Models.MenuItem;

            var textbox = FindChild<TextBox>(row);
            if (textbox == null || !int.TryParse(textbox.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Số lượng không hợp lệ.");
                return;
            }

            var existing = orderDetails.FirstOrDefault(x => x.ItemId == item.ItemId);
            if (existing != null)
                existing.Quantity += qty;
            else
                orderDetails.Add(new OrderDetail
                {
                    ItemId = item.ItemId,
                    Item = item,
                    Quantity = qty,
                    UnitPrice = item.Price
                });

            if (selectedTable.Status != "Occupied")
            {
                selectedTable.Status = "Occupied";
                context.SaveChanges();
                txtTableStatus.Text = $"Trạng thái: {selectedTable.Status}";
                UpdateTableButtons();
            }

            RefreshOrderItems();
        }

        private void RefreshOrderItems()
        {
            dgOrderItems.ItemsSource = null;
            dgOrderItems.ItemsSource = orderDetails;
            UpdateTotal();

            if (selectedTable != null)
            {
                if (orderDetails.Count == 0)
                {
                    if (selectedTable.Status != "Available")
                    {
                        selectedTable.Status = "Available";
                        context.SaveChanges();
                        txtTableStatus.Text = $"Trạng thái: {selectedTable.Status}";
                        UpdateTableButtons();
                    }
                    tableOrderDetails.Remove(selectedTable.TableId);
                }
                else
                {
                    tableOrderDetails[selectedTable.TableId] = new TempOrderData
                    {
                        OrderDetails = orderDetails.Select(od => new OrderDetail
                        {
                            ItemId = od.ItemId,
                            Item = od.Item,
                            Quantity = od.Quantity,
                            UnitPrice = od.UnitPrice
                        }).ToList()
                    };
                }

                TempOrderStorage.Save(tableOrderDetails);
            }
        }

        private void UpdateTableButtons()
        {
            foreach (Button btn in wpTables.Children)
            {
                var table = btn.Tag as CafeTable;
                btn.Background = table.TableId == selectedTable?.TableId ? Brushes.Blue : (table.Status == "Available" ? Brushes.Green : Brushes.Red);
            }
        }

        private void UpdateTotal()
        {
            decimal total = orderDetails.Sum(x => x.Quantity * x.UnitPrice);
            txtTotal.Text = $"Tổng tiền: {total:N0} VNĐ";
        }

        private void dgOrderItems_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "SL")
            {
                var editedTextbox = e.EditingElement as TextBox;
                if (int.TryParse(editedTextbox.Text, out int newQty) && newQty > 0)
                {
                    UpdateTotal();
                }
                else
                {
                    MessageBox.Show("Số lượng không hợp lệ.");
                    e.Cancel = true;
                }
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var row = FindParent<DataGridRow>(sender as Button);
            var item = (dynamic)row?.Item;
            var name = item?.Item?.ItemName;

            var toRemove = orderDetails.FirstOrDefault(x => x.Item.ItemName == name);
            if (toRemove != null)
            {
                orderDetails.Remove(toRemove);
                RefreshOrderItems();
            }
        }

        private void txtDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtDiscount.Text.Trim(), out decimal discount) && discount >= 0 && discount <= 100)
            {
                globalDiscount = discount;
                GlobalDiscountStorage.Save(discount);
            }
        }

        private void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTable == null || orderDetails.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn bàn và ít nhất một món.");
                return;
            }

            var attachedOrderDetails = orderDetails.Select(od => new OrderDetail
            {
                ItemId = od.ItemId,
                Item = context.MenuItems.First(i => i.ItemId == od.ItemId),
                Quantity = od.Quantity,
                UnitPrice = od.UnitPrice
            }).ToList();

            var order = new Order
            {
                TableId = selectedTable.TableId,
                StaffId = currentUser.UserId,
                Status = "Completed",
                OrderTime = DateTime.Now,
                OrderDetails = attachedOrderDetails
            };

            context.Orders.Add(order);
            context.SaveChanges();

            decimal total = attachedOrderDetails.Sum(x => x.Quantity * x.UnitPrice);
            decimal discountAmount = total * globalDiscount / 100;
            decimal finalAmount = total - discountAmount;

            var bill = new Bill
            {
                OrderId = order.OrderId,
                TotalAmount = total,
                Discount = discountAmount,
                FinalAmount = finalAmount,
                PaymentTime = DateTime.Now
            };

            context.Bills.Add(bill);
            selectedTable.Status = "Available";
            context.SaveChanges();

            string fileName = $"Bill_{order.OrderId}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

            try
            {
                using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("===== CAFE INVOICE =====");
                    writer.WriteLine($"Bàn: {selectedTable.TableName}");
                    writer.WriteLine($"Thời gian: {DateTime.Now}");
                    writer.WriteLine();
                    writer.WriteLine("Món\t\tSL\tĐơn giá\tThành tiền");

                    foreach (var od in attachedOrderDetails)
                    {
                        string itemName = od.Item.ItemName;
                        if (itemName.Length < 8) itemName += "\t";
                        writer.WriteLine($"{itemName}\t{od.Quantity}\t{od.UnitPrice:N0}\t{(od.Quantity * od.UnitPrice):N0}");
                    }

                    writer.WriteLine("\n-------------------------");
                    writer.WriteLine($"Tổng tiền: {total:N0} VNĐ");
                    writer.WriteLine($"Giảm giá: {discountAmount:N0} VNĐ");
                    writer.WriteLine($"Thành tiền: {finalAmount:N0} VNĐ");
                    writer.WriteLine("=========================");
                }

                System.Diagnostics.Process.Start("notepad.exe", filePath);
                MessageBox.Show("Thanh toán & in hóa đơn thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi ghi file: " + ex.Message);
            }

            orderDetails.Clear();
            tableOrderDetails.Remove(selectedTable.TableId);
            TempOrderStorage.Save(tableOrderDetails);
            RefreshOrderItems();
            LoadTables();
            selectedTable = null;
            txtTableStatus.Text = "";
        }

        private T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}