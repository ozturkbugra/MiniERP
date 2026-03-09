namespace MiniERP.Domain.Enums
{
    public static class AppPermissions
    {
        // Raporlama Yetkileri
        public static class Reports
        {
            public const string View = "Permissions.Reports.View";
        }

        // Stok Yetkileri
        public static class Stocks
        {
            public const string View = "Permissions.Stocks.View";
            public const string Create = "Permissions.Stocks.Create";
            public const string Delete = "Permissions.Stocks.Delete";
        }

        // Finans/Kasa Yetkileri
        public static class Finance
        {
            public const string View = "Permissions.Finance.View";
            public const string Transaction = "Permissions.Finance.Transaction"; // Ödeme, Tahsilat, Virman yapabilme
        }

        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Create = "Permissions.Users.Create";
            public const string Update = "Permissions.Users.Update";
            public const string Delete = "Permissions.Users.Delete";
        }

        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string Create = "Permissions.Roles.Create";
            public const string Update = "Permissions.Roles.Update";
            public const string Delete = "Permissions.Roles.Delete";
        }

        public static class Banks
        {
            public const string View = "Permissions.Banks.View";
            public const string Create = "Permissions.Banks.Create";
            public const string Update = "Permissions.Banks.Update";
            public const string Delete = "Permissions.Banks.Delete";
        }

        public static class Cashes
        {
            public const string View = "Permissions.Cashes.View";
            public const string Create = "Permissions.Cashes.Create";
            public const string Update = "Permissions.Cashes.Update";
            public const string Delete = "Permissions.Cashes.Delete";
        }

        public static class Brands
        {
            public const string View = "Permissions.Brands.View";
            public const string Create = "Permissions.Brands.Create";
            public const string Update = "Permissions.Brands.Update";
            public const string Delete = "Permissions.Brands.Delete";
        }

        public static class Categories
        {
            public const string View = "Permissions.Categories.View";
            public const string Create = "Permissions.Categories.Create";
            public const string Update = "Permissions.Categories.Update";
            public const string Delete = "Permissions.Categories.Delete";
        }

        public static class Customers
        {
            public const string View = "Permissions.Customers.View";
            public const string Create = "Permissions.Customers.Create";
            public const string Update = "Permissions.Customers.Update";
            public const string Delete = "Permissions.Customers.Delete";
        }

        public static class Invoices
        {
            public const string View = "Permissions.Invoices.View";
            public const string Create = "Permissions.Invoices.Create";
            public const string Approve = "Permissions.Invoices.Approve";
            public const string Cancel = "Permissions.Invoices.Cancel";
            public const string Return = "Permissions.Invoices.Return";
        }

        public static class Orders
        {
            public const string View = "Permissions.Orders.View";
            public const string Create = "Permissions.Orders.Create";
            public const string Approve = "Permissions.Orders.Approve";
            public const string Cancel = "Permissions.Orders.Cancel";
        }

        public static class Products
        {
            public const string View = "Permissions.Products.View";
            public const string Create = "Permissions.Products.Create";
            public const string Update = "Permissions.Products.Update";
            public const string Delete = "Permissions.Products.Delete";
        }

        public static class StockTransactions
        {
            public const string View = "Permissions.StockTransactions.View";
            public const string Create = "Permissions.StockTransactions.Create";
            public const string Delete = "Permissions.StockTransactions.Delete";
        }

        public static class Transactions
        {
            public const string View = "Permissions.Transactions.View";
            public const string Create = "Permissions.Transactions.Create";
            public const string Cancel = "Permissions.Transactions.Cancel";
        }

        public static class Units
        {
            public const string View = "Permissions.Units.View";
            public const string Create = "Permissions.Units.Create";
            public const string Update = "Permissions.Units.Update";
            public const string Delete = "Permissions.Units.Delete";
        }

        public static class Warehouses
        {
            public const string View = "Permissions.Warehouses.View";
            public const string Create = "Permissions.Warehouses.Create";
            public const string Update = "Permissions.Warehouses.Update";
            public const string Delete = "Permissions.Warehouses.Delete";
        }
    }
}
