USE SupermarketDB;
GO

SET NOCOUNT ON;

IF OBJECT_ID(N'dbo.Category', N'U') IS NULL
BEGIN
    RAISERROR(N'Chua co database/schema. Hay chay create_supermarket_db.sql truoc.', 16, 1);
    RETURN;
END
GO

INSERT INTO dbo.Category (CategoryName)
SELECT v.CategoryName
FROM (VALUES
    (N'Fresh Food'),
    (N'Frozen Food'),
    (N'Dairy'),
    (N'Snacks'),
    (N'Beverage'),
    (N'Household'),
    (N'Personal Care'),
    (N'Baby Care'),
    (N'Stationery'),
    (N'Pet Supplies')
) AS v(CategoryName)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Category c WHERE c.CategoryName = v.CategoryName
);
GO

INSERT INTO dbo.Supplier (SupplierName, IsActive)
SELECT v.SupplierName, 1
FROM (VALUES
    (N'Vinamilk Supplier'),
    (N'Masan Consumer'),
    (N'Unilever Distributor'),
    (N'PepsiCo Distributor'),
    (N'Nestle Vietnam'),
    (N'CP Fresh Food'),
    (N'Vissan Food'),
    (N'Kimberly-Clark Distributor'),
    (N'Thien Long Stationery'),
    (N'Minh Long Household')
) AS v(SupplierName)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Supplier s WHERE s.SupplierName = v.SupplierName
);
GO

INSERT INTO dbo.Employee (FullName, Phone, Salary, HireDate, IsActive)
SELECT v.FullName, v.Phone, v.Salary, v.HireDate, 1
FROM (VALUES
    (N'Nguyen Van An',   N'0901000001', CAST(8500000 AS DECIMAL(12,2)), CAST('2025-01-10' AS DATE)),
    (N'Tran Thi Bich',   N'0901000002', CAST(7800000 AS DECIMAL(12,2)), CAST('2025-02-18' AS DATE)),
    (N'Le Minh Chau',    N'0901000003', CAST(7200000 AS DECIMAL(12,2)), CAST('2025-03-05' AS DATE)),
    (N'Pham Quoc Dat',   N'0901000004', CAST(9000000 AS DECIMAL(12,2)), CAST('2024-11-22' AS DATE)),
    (N'Hoang Thanh Mai', N'0901000005', CAST(7600000 AS DECIMAL(12,2)), CAST('2025-04-14' AS DATE)),
    (N'Do Gia Huy',      N'0901000006', CAST(7000000 AS DECIMAL(12,2)), CAST('2025-06-01' AS DATE)),
    (N'Vo Ngoc Linh',    N'0901000007', CAST(8200000 AS DECIMAL(12,2)), CAST('2024-09-12' AS DATE)),
    (N'Bui Anh Khoa',    N'0901000008', CAST(7300000 AS DECIMAL(12,2)), CAST('2025-05-20' AS DATE))
) AS v(FullName, Phone, Salary, HireDate)
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Employee e WHERE e.Phone = v.Phone
);
GO

INSERT INTO dbo.Product
    (ProductName, CategoryId, SupplierId, CostPrice, SellPrice, Quantity, ExpiryDate, IsActive)
SELECT v.ProductName, c.CategoryId, s.SupplierId, v.CostPrice, v.SellPrice, v.Quantity, v.ExpiryDate, 1
FROM (VALUES
    (N'Fresh Chicken Breast 500g', N'Fresh Food',    N'CP Fresh Food',              CAST(52000 AS DECIMAL(12,2)),  CAST(69000 AS DECIMAL(12,2)),  45, CAST('2026-07-15' AS DATE)),
    (N'Pork Sausage 500g',         N'Fresh Food',    N'Vissan Food',                CAST(48000 AS DECIMAL(12,2)),  CAST(65000 AS DECIMAL(12,2)),  38, CAST('2026-08-01' AS DATE)),
    (N'Frozen Fish Fillet 1kg',    N'Frozen Food',   N'CP Fresh Food',              CAST(83000 AS DECIMAL(12,2)),  CAST(109000 AS DECIMAL(12,2)), 22, CAST('2026-12-20' AS DATE)),
    (N'UHT Milk 1L',               N'Dairy',         N'Vinamilk Supplier',          CAST(22000 AS DECIMAL(12,2)),  CAST(28500 AS DECIMAL(12,2)), 120, CAST('2026-09-30' AS DATE)),
    (N'Greek Yogurt 100g',         N'Dairy',         N'Vinamilk Supplier',          CAST(8500 AS DECIMAL(12,2)),   CAST(12000 AS DECIMAL(12,2)), 160, CAST('2026-07-28' AS DATE)),
    (N'Instant Noodles Pack',      N'Snacks',        N'Masan Consumer',             CAST(3600 AS DECIMAL(12,2)),   CAST(5500 AS DECIMAL(12,2)),  350, CAST('2027-01-01' AS DATE)),
    (N'Potato Chips 90g',          N'Snacks',        N'PepsiCo Distributor',        CAST(11500 AS DECIMAL(12,2)),  CAST(17000 AS DECIMAL(12,2)), 95, CAST('2026-10-12' AS DATE)),
    (N'Chocolate Cereal 330g',     N'Snacks',        N'Nestle Vietnam',             CAST(52000 AS DECIMAL(12,2)),  CAST(69000 AS DECIMAL(12,2)), 42, CAST('2027-02-15' AS DATE)),
    (N'Bottled Water 500ml',       N'Beverage',      N'PepsiCo Distributor',        CAST(2800 AS DECIMAL(12,2)),   CAST(5000 AS DECIMAL(12,2)),  500, CAST('2027-03-01' AS DATE)),
    (N'Cola Can 330ml',            N'Beverage',      N'PepsiCo Distributor',        CAST(6800 AS DECIMAL(12,2)),   CAST(10000 AS DECIMAL(12,2)), 240, CAST('2027-01-20' AS DATE)),
    (N'Coffee Mix 20 Sachets',     N'Beverage',      N'Nestle Vietnam',             CAST(39000 AS DECIMAL(12,2)),  CAST(55000 AS DECIMAL(12,2)), 70, CAST('2027-04-05' AS DATE)),
    (N'Dishwashing Liquid 750ml',  N'Household',     N'Unilever Distributor',       CAST(31000 AS DECIMAL(12,2)),  CAST(45000 AS DECIMAL(12,2)), 65, NULL),
    (N'Laundry Detergent 3kg',     N'Household',     N'Unilever Distributor',       CAST(118000 AS DECIMAL(12,2)), CAST(158000 AS DECIMAL(12,2)), 28, NULL),
    (N'Ceramic Bowl Set',          N'Household',     N'Minh Long Household',        CAST(145000 AS DECIMAL(12,2)), CAST(219000 AS DECIMAL(12,2)), 16, NULL),
    (N'Shampoo 650ml',             N'Personal Care', N'Unilever Distributor',       CAST(74000 AS DECIMAL(12,2)),  CAST(99000 AS DECIMAL(12,2)), 55, NULL),
    (N'Toothpaste 180g',           N'Personal Care', N'Unilever Distributor',       CAST(26000 AS DECIMAL(12,2)),  CAST(39000 AS DECIMAL(12,2)), 88, CAST('2027-06-01' AS DATE)),
    (N'Baby Diapers M40',          N'Baby Care',     N'Kimberly-Clark Distributor', CAST(185000 AS DECIMAL(12,2)), CAST(245000 AS DECIMAL(12,2)), 24, NULL),
    (N'Ballpoint Pen Blue',        N'Stationery',    N'Thien Long Stationery',      CAST(2500 AS DECIMAL(12,2)),   CAST(5000 AS DECIMAL(12,2)),  400, NULL),
    (N'Notebook A5 120 Pages',     N'Stationery',    N'Thien Long Stationery',      CAST(8500 AS DECIMAL(12,2)),   CAST(15000 AS DECIMAL(12,2)), 180, NULL),
    (N'Dog Food 2kg',              N'Pet Supplies',  N'Masan Consumer',             CAST(92000 AS DECIMAL(12,2)),  CAST(129000 AS DECIMAL(12,2)), 32, CAST('2027-05-10' AS DATE))
) AS v(ProductName, CategoryName, SupplierName, CostPrice, SellPrice, Quantity, ExpiryDate)
INNER JOIN dbo.Category c ON c.CategoryName = v.CategoryName
INNER JOIN dbo.Supplier s ON s.SupplierName = v.SupplierName
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Product p WHERE p.ProductName = v.ProductName
);
GO

INSERT INTO dbo.ImportOrder (ProductId, SupplierId, ImportPrice, Quantity, Status, IssueDate)
SELECT p.ProductId, s.SupplierId, v.ImportPrice, v.Quantity, v.Status, v.IssueDate
FROM (VALUES
    (N'UHT Milk 1L',              N'Vinamilk Supplier',          CAST(22000 AS DECIMAL(12,2)), 100, N'Issued', CAST('2026-01-05T09:15:00' AS DATETIME2)),
    (N'Instant Noodles Pack',     N'Masan Consumer',             CAST(3600 AS DECIMAL(12,2)),  300, N'Issued', CAST('2026-01-08T10:30:00' AS DATETIME2)),
    (N'Bottled Water 500ml',      N'PepsiCo Distributor',        CAST(2800 AS DECIMAL(12,2)),  420, N'Issued', CAST('2026-02-03T14:20:00' AS DATETIME2)),
    (N'Cola Can 330ml',           N'PepsiCo Distributor',        CAST(6800 AS DECIMAL(12,2)),  240, N'Issued', CAST('2026-02-12T15:00:00' AS DATETIME2)),
    (N'Shampoo 650ml',            N'Unilever Distributor',       CAST(74000 AS DECIMAL(12,2)), 50,  N'Issued', CAST('2026-03-01T11:30:00' AS DATETIME2)),
    (N'Dishwashing Liquid 750ml', N'Unilever Distributor',       CAST(31000 AS DECIMAL(12,2)), 70,  N'Issued', CAST('2026-03-15T08:40:00' AS DATETIME2)),
    (N'Greek Yogurt 100g',        N'Vinamilk Supplier',          CAST(8500 AS DECIMAL(12,2)),  160, N'Issued', CAST('2026-04-07T09:50:00' AS DATETIME2)),
    (N'Baby Diapers M40',         N'Kimberly-Clark Distributor', CAST(185000 AS DECIMAL(12,2)), 24, N'Issued', CAST('2026-04-20T16:10:00' AS DATETIME2)),
    (N'Coffee Mix 20 Sachets',    N'Nestle Vietnam',             CAST(39000 AS DECIMAL(12,2)), 60,  N'Issued', CAST('2026-05-06T13:45:00' AS DATETIME2)),
    (N'Notebook A5 120 Pages',    N'Thien Long Stationery',      CAST(8500 AS DECIMAL(12,2)),  150, N'Issued', CAST('2026-05-18T10:05:00' AS DATETIME2)),
    (N'Fresh Chicken Breast 500g',N'CP Fresh Food',              CAST(52000 AS DECIMAL(12,2)), 25,  N'New',    NULL),
    (N'Potato Chips 90g',         N'PepsiCo Distributor',        CAST(11500 AS DECIMAL(12,2)), 80,  N'New',    NULL)
) AS v(ProductName, SupplierName, ImportPrice, Quantity, Status, IssueDate)
INNER JOIN dbo.Product p ON p.ProductName = v.ProductName
INNER JOIN dbo.Supplier s ON s.SupplierName = v.SupplierName
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.ImportOrder io
    WHERE io.ProductId = p.ProductId
      AND io.SupplierId = s.SupplierId
      AND io.Quantity = v.Quantity
      AND io.ImportPrice = v.ImportPrice
      AND ISNULL(io.IssueDate, '19000101') = ISNULL(v.IssueDate, '19000101')
);
GO

INSERT INTO dbo.SalesOrder (ProductId, Quantity, SellPrice, Status, IssueDate)
SELECT p.ProductId, v.Quantity, v.SellPrice, v.Status, v.IssueDate
FROM (VALUES
    (N'UHT Milk 1L',              18, CAST(28500 AS DECIMAL(12,2)),  N'Issued', CAST('2026-01-10T17:25:00' AS DATETIME2)),
    (N'Instant Noodles Pack',     55, CAST(5500 AS DECIMAL(12,2)),   N'Issued', CAST('2026-01-15T18:05:00' AS DATETIME2)),
    (N'Bottled Water 500ml',      80, CAST(5000 AS DECIMAL(12,2)),   N'Issued', CAST('2026-02-08T12:15:00' AS DATETIME2)),
    (N'Cola Can 330ml',           42, CAST(10000 AS DECIMAL(12,2)),  N'Issued', CAST('2026-02-18T19:30:00' AS DATETIME2)),
    (N'Shampoo 650ml',            12, CAST(99000 AS DECIMAL(12,2)),  N'Issued', CAST('2026-03-06T20:10:00' AS DATETIME2)),
    (N'Dishwashing Liquid 750ml', 16, CAST(45000 AS DECIMAL(12,2)),  N'Issued', CAST('2026-03-22T16:45:00' AS DATETIME2)),
    (N'Greek Yogurt 100g',        65, CAST(12000 AS DECIMAL(12,2)),  N'Issued', CAST('2026-04-10T11:20:00' AS DATETIME2)),
    (N'Baby Diapers M40',         6,  CAST(245000 AS DECIMAL(12,2)), N'Issued', CAST('2026-04-25T18:40:00' AS DATETIME2)),
    (N'Coffee Mix 20 Sachets',    20, CAST(55000 AS DECIMAL(12,2)),  N'Issued', CAST('2026-05-09T07:55:00' AS DATETIME2)),
    (N'Notebook A5 120 Pages',    45, CAST(15000 AS DECIMAL(12,2)),  N'Issued', CAST('2026-05-21T13:25:00' AS DATETIME2)),
    (N'Toothpaste 180g',          10, CAST(39000 AS DECIMAL(12,2)),  N'New',    NULL),
    (N'Potato Chips 90g',         15, CAST(17000 AS DECIMAL(12,2)),  N'New',    NULL)
) AS v(ProductName, Quantity, SellPrice, Status, IssueDate)
INNER JOIN dbo.Product p ON p.ProductName = v.ProductName
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.SalesOrder so
    WHERE so.ProductId = p.ProductId
      AND so.Quantity = v.Quantity
      AND so.SellPrice = v.SellPrice
      AND ISNULL(so.IssueDate, '19000101') = ISNULL(v.IssueDate, '19000101')
);
GO

SELECT
    (SELECT COUNT(*) FROM dbo.Category) AS CategoryCount,
    (SELECT COUNT(*) FROM dbo.Supplier) AS SupplierCount,
    (SELECT COUNT(*) FROM dbo.Product) AS ProductCount,
    (SELECT COUNT(*) FROM dbo.Employee) AS EmployeeCount,
    (SELECT COUNT(*) FROM dbo.ImportOrder) AS ImportOrderCount,
    (SELECT COUNT(*) FROM dbo.SalesOrder) AS SalesOrderCount;
GO
