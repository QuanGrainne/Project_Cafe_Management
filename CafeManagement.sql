-- Tạo CSDL
CREATE DATABASE CafeManagement;
GO

USE CafeManagement;
GO

-- 1. Tài khoản người dùng (chỉ cần phân biệt Admin và Staff)
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL, -- Lưu dạng plain text
    FullName NVARCHAR(100),
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'Staff')) -- Phân quyền đơn giản
);

-- 2. Bàn trong quán cà phê
CREATE TABLE CafeTables (
    TableId INT PRIMARY KEY IDENTITY(1,1),
    TableName NVARCHAR(50) NOT NULL UNIQUE,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Available' -- Available, Occupied
);

-- 3. Thực đơn món
CREATE TABLE MenuItems (
    ItemId INT PRIMARY KEY IDENTITY(1,1),
    ItemName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    Price DECIMAL(10,2) NOT NULL,
    IsAvailable BIT NOT NULL DEFAULT 1
);

-- 4. Đơn đặt món
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    TableId INT NOT NULL,
    StaffId INT NOT NULL,
    OrderTime DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending, Completed
    FOREIGN KEY (TableId) REFERENCES CafeTables(TableId),
    FOREIGN KEY (StaffId) REFERENCES Users(UserId)
);

-- 5. Chi tiết đơn hàng
CREATE TABLE OrderDetails (
    OrderDetailId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ItemId INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    FOREIGN KEY (ItemId) REFERENCES MenuItems(ItemId)
);

-- 6. Hóa đơn thanh toán
CREATE TABLE Bills (
    BillId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    TotalAmount DECIMAL(12,2) NOT NULL,
    Discount DECIMAL(10,2) DEFAULT 0,
    FinalAmount AS (TotalAmount - Discount) PERSISTED,
    PaymentTime DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
);
