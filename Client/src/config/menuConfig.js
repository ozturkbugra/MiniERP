// src/config/menuConfig.js
import { APP_PERMISSIONS } from '../constants/permissions';

export const sidebarMenu = [
    {
        title: "Stok Yönetimi",
        icon: "inventory",
        children: [
            { title: "Ürünler", path: "/products", permission: APP_PERMISSIONS.Products.View },
            { title: "Stoklar", path: "/stocks", permission: APP_PERMISSIONS.Stocks.View },
            { title: "Depolar", path: "/warehouses", permission: APP_PERMISSIONS.Warehouses.View },
        ]
    },
    {
        title: "Finans",
        icon: "payments",
        children: [
            { title: "Kasa", path: "/cashes", permission: APP_PERMISSIONS.Cashes.View },
            { title: "Banka", path: "/banks", permission: APP_PERMISSIONS.Banks.View },
            { title: "İşlemler", path: "/finance", permission: APP_PERMISSIONS.Finance.View },
        ]
    },
    {
        title: "Satış & Fatura",
        icon: "receipt",
        children: [
            { title: "Faturalar", path: "/invoices", permission: APP_PERMISSIONS.Invoices.View },
            { title: "Siparişler", path: "/orders", permission: APP_PERMISSIONS.Orders.View },
            { title: "Cariler", path: "/customers", permission: APP_PERMISSIONS.Customers.View },
        ]
    },
    {
        title: "Sistem",
        icon: "settings",
        children: [
            { title: "Kullanıcılar", path: "/users", permission: APP_PERMISSIONS.Users.View },
            { title: "Roller", path: "/roles", permission: APP_PERMISSIONS.Roles.View },
        ]
    }
];