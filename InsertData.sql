-- Tài khoản mẫu
INSERT INTO Users (Username, Password, FullName, Role) VALUES
('admin', 'admin123', N'Quản trị viên', 'Admin'),
('staff1', 'staff123', N'Nhân viên 1', 'Staff');

-- Bàn mẫu
INSERT INTO CafeTables (TableName, Status) VALUES
('Bàn 1', 'Available'),
('Bàn 2', 'Available'),
('Bàn 3', 'Occupied');

-- Thực đơn mẫu
INSERT INTO MenuItems (ItemName, Description, Price) VALUES
(N'Cà phê sữa', N'Cà phê pha với sữa đặc', 25000),
(N'Trà đào', N'Trà với miếng đào', 30000),
(N'Bánh mì', N'Bánh mì thịt', 20000);

select * from MenuItems

select * from OrderDetails

select * from Orders

select * from Users

select * from Bills