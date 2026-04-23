import { APP_PERMISSIONS } from '../constants/permissions';

export const MENU_ITEMS = [
  {
    title: "Dashboard",
    isHeader: false,
    path: "/",
    icon: "ph-squares-four",
    permission: null // Herkes görebilir
  },
  {
    title: "Tanımlamalar",
    isHeader: true,
    permissions: [APP_PERMISSIONS.Categories.View, APP_PERMISSIONS.Brands.View, APP_PERMISSIONS.Units.View],
    children: [
      { title: "Kategoriler", path: "/categories", icon: "ph-tag", permission: APP_PERMISSIONS.Categories.View },
      { title: "Markalar", path: "/brands", icon: "ph-bookmarks", permission: APP_PERMISSIONS.Brands.View },
      { title: "Birimler", path: "/units", icon: "ph-scales", permission: APP_PERMISSIONS.Units.View },
    ]
  },
  {
    title: "Stok Yönetimi",
    isHeader: true,
    permissions: [APP_PERMISSIONS.Products.View, APP_PERMISSIONS.Stocks.View, APP_PERMISSIONS.Warehouses.View],
    children: [
      { title: "Ürünler", path: "/products", icon: "ph-package", permission: APP_PERMISSIONS.Products.View },
      { title: "Stoklar", path: "/stocktransactions", icon: "ph-stack", permission: APP_PERMISSIONS.Stocks.View },
      { title: "Depolar", path: "/warehouses", icon: "ph-buildings", permission: APP_PERMISSIONS.Warehouses.View },
    ]
  },
  {
    title: "Finans",
    isHeader: true,
    permissions: [APP_PERMISSIONS.Cashes.View, APP_PERMISSIONS.Banks.View, APP_PERMISSIONS.Finance.View],
    children: [
      { title: "Kasalar", path: "/cashes", icon: "ph-money", permission: APP_PERMISSIONS.Cashes.View },
      { title: "Bankalar", path: "/banks", icon: "ph-bank", permission: APP_PERMISSIONS.Banks.View },
      { title: "Finans İşlemleri", path: "/finance", icon: "ph-arrows-left-right", permission: APP_PERMISSIONS.Finance.View },
    ]
  },
  {
    title: "Satış & Cari",
    isHeader: true,
    permissions: [APP_PERMISSIONS.Customers.View, APP_PERMISSIONS.Invoices.View, APP_PERMISSIONS.Orders.View],
    children: [
      { title: "Cari Kartlar", path: "/customers", icon: "ph-users", permission: APP_PERMISSIONS.Customers.View },
      { title: "Siparişler", path: "/orders", icon: "ph-shopping-cart", permission: APP_PERMISSIONS.Orders.View },
      { title: "Faturalar", path: "/invoices", icon: "ph-receipt", permission: APP_PERMISSIONS.Invoices.View },
    ]
  },
  {
    title: "Raporlar",
    isHeader: true,
    permissions: [APP_PERMISSIONS.Reports.View],
    children: [
      { title: "Stokta Kalan", path: "/reports/stock-snapshot", icon: "ph-chart-bar", permission: APP_PERMISSIONS.Reports.View },
      { title: "Kritik Stoklar", path: "/reports/critical-stocks", icon: "ph-warning-circle", permission: APP_PERMISSIONS.Reports.View },
      { title: "Ürün Defteri", path: "/reports/product-ledger", icon: "ph-book-open", permission: APP_PERMISSIONS.Reports.View },
    ]
  },
  {
    title: "Sistem",
    isHeader: true,
    permissions: [APP_PERMISSIONS.Users.View, APP_PERMISSIONS.Roles.View],
    children: [
      { title: "Kullanıcılar", path: "/users", icon: "ph-users-three", permission: APP_PERMISSIONS.Users.View },
      { title: "Roller", path: "/roles", icon: "ph-keyhole", permission: APP_PERMISSIONS.Roles.View },
    ]
  }
];