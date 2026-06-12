IF DB_ID(N'SupermarketDB') IS NULL
BEGIN
    CREATE DATABASE SupermarketDB;
END
GO

USE SupermarketDB;
GO

IF OBJECT_ID(N'dbo.Role', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Role
    (
        RoleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Role PRIMARY KEY,
        RoleName NVARCHAR(50) NOT NULL CONSTRAINT UQ_Role_RoleName UNIQUE
    );
END
GO

IF OBJECT_ID(N'dbo.Category', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Category
    (
        CategoryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Category PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL CONSTRAINT UQ_Category_CategoryName UNIQUE
    );
END
GO

IF OBJECT_ID(N'dbo.Supplier', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Supplier
    (
        SupplierId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Supplier PRIMARY KEY,
        SupplierName NVARCHAR(150) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Supplier_IsActive DEFAULT (1)
    );
END
GO

IF OBJECT_ID(N'dbo.Employee', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employee
    (
        EmployeeId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Employee PRIMARY KEY,
        FullName NVARCHAR(150) NOT NULL,
        Phone NVARCHAR(20) NOT NULL,
        Salary DECIMAL(12,2) NOT NULL CONSTRAINT DF_Employee_Salary DEFAULT (0),
        HireDate DATE NOT NULL CONSTRAINT DF_Employee_HireDate DEFAULT (CONVERT(date, GETDATE())),
        IsActive BIT NOT NULL CONSTRAINT DF_Employee_IsActive DEFAULT (1)
    );
END
GO

IF OBJECT_ID(N'dbo.Product', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Product
    (
        ProductId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Product PRIMARY KEY,
        ProductName NVARCHAR(150) NOT NULL,
        CategoryId INT NOT NULL,
        SupplierId INT NULL,
        CostPrice DECIMAL(12,2) NOT NULL CONSTRAINT DF_Product_CostPrice DEFAULT (0),
        SellPrice DECIMAL(12,2) NOT NULL CONSTRAINT DF_Product_SellPrice DEFAULT (0),
        Quantity INT NOT NULL CONSTRAINT DF_Product_Quantity DEFAULT (0),
        ExpiryDate DATE NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Product_IsActive DEFAULT (1),
        CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryId) REFERENCES dbo.Category(CategoryId),
        CONSTRAINT FK_Product_Supplier FOREIGN KEY (SupplierId) REFERENCES dbo.Supplier(SupplierId)
    );
END
GO

IF OBJECT_ID(N'dbo.[User]', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.[User]
    (
        UserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_User PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL CONSTRAINT UQ_User_Username UNIQUE,
        PasswordHash NVARCHAR(200) NOT NULL,
        RoleId INT NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_User_IsActive DEFAULT (1),
        CONSTRAINT FK_User_Role FOREIGN KEY (RoleId) REFERENCES dbo.Role(RoleId)
    );
END
GO

IF OBJECT_ID(N'dbo.ImportOrder', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ImportOrder
    (
        OrderId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ImportOrder PRIMARY KEY,
        ProductId INT NOT NULL,
        SupplierId INT NOT NULL,
        ImportPrice DECIMAL(12,2) NOT NULL,
        Quantity INT NOT NULL,
        TotalAmount AS (CONVERT(DECIMAL(23,2), ImportPrice * Quantity)) PERSISTED,
        Status NVARCHAR(10) NOT NULL CONSTRAINT DF_ImportOrder_Status DEFAULT (N'New'),
        IssueDate DATETIME2(7) NULL,
        CONSTRAINT FK_ImportOrder_Product FOREIGN KEY (ProductId) REFERENCES dbo.Product(ProductId),
        CONSTRAINT FK_ImportOrder_Supplier FOREIGN KEY (SupplierId) REFERENCES dbo.Supplier(SupplierId)
    );
END
GO

IF OBJECT_ID(N'dbo.SalesOrder', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SalesOrder
    (
        OrderId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SalesOrder PRIMARY KEY,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        SellPrice DECIMAL(12,2) NOT NULL,
        TotalAmount AS (CONVERT(DECIMAL(23,2), SellPrice * Quantity)) PERSISTED,
        Status NVARCHAR(10) NOT NULL CONSTRAINT DF_SalesOrder_Status DEFAULT (N'New'),
        IssueDate DATETIME2(7) NULL,
        CONSTRAINT FK_SalesOrder_Product FOREIGN KEY (ProductId) REFERENCES dbo.Product(ProductId)
    );
END
GO

IF OBJECT_ID(N'dbo.trg_ImportOrder_UpdateProduct', N'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_ImportOrder_UpdateProduct;
GO

CREATE TRIGGER dbo.trg_ImportOrder_UpdateProduct
ON dbo.ImportOrder
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.Quantity = p.Quantity + i.Quantity,
        p.CostPrice = i.ImportPrice,
        p.SupplierId = i.SupplierId
    FROM dbo.Product p
    INNER JOIN inserted i ON p.ProductId = i.ProductId
    LEFT JOIN deleted d ON d.OrderId = i.OrderId
    WHERE i.Status = N'Issued'
      AND ISNULL(d.Status, N'') <> N'Issued';
END
GO

IF OBJECT_ID(N'dbo.trg_SalesOrder_UpdateProduct', N'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_SalesOrder_UpdateProduct;
GO

CREATE TRIGGER dbo.trg_SalesOrder_UpdateProduct
ON dbo.SalesOrder
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE p
    SET p.Quantity = p.Quantity - i.Quantity
    FROM dbo.Product p
    INNER JOIN inserted i ON p.ProductId = i.ProductId
    LEFT JOIN deleted d ON d.OrderId = i.OrderId
    WHERE i.Status = N'Issued'
      AND ISNULL(d.Status, N'') <> N'Issued';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Role)
BEGIN
    INSERT INTO dbo.Role (RoleName)
    VALUES (N'Manager'), (N'Staff');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.[User])
BEGIN
    INSERT INTO dbo.[User] (Username, PasswordHash, RoleId, IsActive)
    SELECT N'admin',
           CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(VARCHAR(100), 'admin123')), 2),
           RoleId,
           1
    FROM dbo.Role
    WHERE RoleName = N'Manager';

    INSERT INTO dbo.[User] (Username, PasswordHash, RoleId, IsActive)
    SELECT N'staff',
           CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', CONVERT(VARCHAR(100), 'staff123')), 2),
           RoleId,
           1
    FROM dbo.Role
    WHERE RoleName = N'Staff';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Category)
BEGIN
    INSERT INTO dbo.Category (CategoryName)
    VALUES (N'Food'), (N'Beverage'), (N'Household'), (N'Personal Care');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Supplier)
BEGIN
    INSERT INTO dbo.Supplier (SupplierName, IsActive)
    VALUES (N'Vinamilk Supplier', 1),
           (N'Masan Consumer', 1),
           (N'Unilever Distributor', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Product)
BEGIN
    INSERT INTO dbo.Product (ProductName, CategoryId, SupplierId, CostPrice, SellPrice, Quantity, ExpiryDate, IsActive)
    SELECT N'Milk 1L', c.CategoryId, s.SupplierId, 22000, 28000, 50, DATEADD(month, 6, CONVERT(date, GETDATE())), 1
    FROM dbo.Category c CROSS JOIN dbo.Supplier s
    WHERE c.CategoryName = N'Beverage' AND s.SupplierName = N'Vinamilk Supplier';

    INSERT INTO dbo.Product (ProductName, CategoryId, SupplierId, CostPrice, SellPrice, Quantity, ExpiryDate, IsActive)
    SELECT N'Instant Noodles', c.CategoryId, s.SupplierId, 3500, 5000, 200, DATEADD(month, 8, CONVERT(date, GETDATE())), 1
    FROM dbo.Category c CROSS JOIN dbo.Supplier s
    WHERE c.CategoryName = N'Food' AND s.SupplierName = N'Masan Consumer';

    INSERT INTO dbo.Product (ProductName, CategoryId, SupplierId, CostPrice, SellPrice, Quantity, ExpiryDate, IsActive)
    SELECT N'Shampoo', c.CategoryId, s.SupplierId, 65000, 85000, 30, NULL, 1
    FROM dbo.Category c CROSS JOIN dbo.Supplier s
    WHERE c.CategoryName = N'Personal Care' AND s.SupplierName = N'Unilever Distributor';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Employee)
BEGIN
    INSERT INTO dbo.Employee (FullName, Phone, Salary, HireDate, IsActive)
    VALUES (N'Nguyen Van A', N'0900000001', 7000000, CONVERT(date, GETDATE()), 1),
           (N'Tran Thi B', N'0900000002', 6500000, CONVERT(date, GETDATE()), 1);
END
GO

SELECT N'Database SupermarketDB is ready.' AS [Message];
GO
