# Supermarket Management System

A Windows Forms desktop application for managing supermarket operations. The project is built with C#, .NET Framework, Entity Framework, and SQL Server.

## Tech Stack

- C#
- .NET Framework 4.8
- Windows Forms
- Entity Framework 6
- SQL Server
- Microsoft ReportViewer

## Main Features

- Login with user role checking
- Product and inventory management
- Supplier management
- Employee management
- Import order management
- Sales order management
- Stock quantity validation
- Soft delete for products, suppliers, and employees
- Monthly cost and profit report
- SQL Server database scripts with sample data

## Project Structure

```text
BS_layer/       Business logic classes
DB_layer/       Entity Framework database model
UI/             WinForms user controls
database/       SQL scripts for creating and seeding the database
App.config      Database connection configuration
```

## Database Setup

This project uses SQL Server. The default connection string is configured for:

```text
localhost\SQLEXPRESS
```

To create the database:

1. Open SQL Server Management Studio.
2. Connect to `localhost\SQLEXPRESS`.
3. Open and run:

```text
database/create_supermarket_db.sql
```

4. To add more sample data, run:

```text
database/seed_supermarket_professional_data.sql
```

The scripts create the `SupermarketDB` database, required tables, relationships, triggers, and sample records.

## Demo Accounts

```text
Username: admin
Password: admin123
Role: Manager

Username: staff
Password: staff123
Role: Staff
```

## Configuration

If your SQL Server instance is different, update the connection strings in:

```text
App.config
Properties/Settings.settings
```

Replace `localhost\SQLEXPRESS` with your SQL Server instance name.

## How to Run

1. Open `Supermarket.sln` in Visual Studio.
2. Restore NuGet packages if required.
3. Make sure SQL Server is running.
4. Run the database scripts.
5. Build and run the project.

## Notes

This is a student project focused on basic enterprise desktop application features: CRUD operations, layered code structure, SQL Server database integration, role-based access, and reporting.
