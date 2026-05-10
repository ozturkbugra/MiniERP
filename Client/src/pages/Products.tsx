import { useState, useEffect, useCallback } from 'react';
import DataTable from '../components/Common/DataTable';
import AppModal from '../components/Common/AppModal';
import AppSelect, { type SelectOption } from '../components/Common/AppSelect'; 
import api from '../api/axiosInstance';
import { useAuthStore } from '../store/useAuthStore';
import { APP_PERMISSIONS } from '../constants/permissions';

interface Product {
  id: string;
  code: string;
  name: string;
  barcode: string;
  defaultPrice: number;
  criticalStockLevel: number;
  vatRate: number;
  categoryId: string;
  categoryName?: string;
  brandId: string;
  brandName?: string;
  unitId: string;
  unitName?: string;
}

const Products = () => {
  const { hasPermission } = useAuthStore();
  
  // Ana veriler
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [showModal, setShowModal] = useState<boolean>(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  // Bağımlılıklar
  const [categories, setCategories] = useState<SelectOption[]>([]);
  const [brands, setBrands] = useState<SelectOption[]>([]);
  const [units, setUnits] = useState<SelectOption[]>([]);

  // 🚀 Fiyat Görünümü İçin Intermediate State (Virgül hatasını çözer)
  const [displayPrice, setDisplayPrice] = useState<string>("");

  // Form State
  const [formData, setFormData] = useState({
    code: "",
    name: "",
    barcode: "",
    defaultPrice: 0,
    criticalStockLevel: 0,
    vatRate: 20,
    categoryId: "",
    brandId: "",
    unitId: ""
  });

  // 🛠️ YARDIMCI FONKSİYONLAR (Formatlama)
  const maskCurrency = (val: string) => {
    let clean = val.replace(/[^0-9,]/g, "");
    const parts = clean.split(",");
    if (parts.length > 2) clean = parts[0] + "," + parts[1];
    const integerPart = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ".");
    return parts.length > 1 ? `${integerPart},${parts[1].slice(0, 2)}` : integerPart;
  };

  const unmaskPrice = (val: string) => {
    if (!val) return 0;
    return parseFloat(val.replace(/\./g, "").replace(",", ".")) || 0;
  };

  const fetchDependencies = useCallback(async () => {
    try {
      const [catRes, brandRes, unitRes] = await Promise.all([
        api.get("/Categories"),
        api.get("/Brands"),
        api.get("/Units")
      ]);
      
      if (catRes.data.isSuccess) setCategories(catRes.data.data.map((x: any) => ({ label: x.name, value: x.id })));
      if (brandRes.data.isSuccess) setBrands(brandRes.data.data.map((x: any) => ({ label: x.name, value: x.id })));
      if (unitRes.data.isSuccess) setUnits(unitRes.data.data.map((x: any) => ({ label: x.name, value: x.id })));
    } catch (error) {
      console.error("Bağımlı veriler çekilemedi (403 veya Bağlantı Hatası):", error);
    }
  }, []);

  const fetchProducts = useCallback(async () => {
    try {
      setLoading(true);
      const response = await api.get("/Products");
      if (response.data.isSuccess) setProducts(response.data.data);
    } catch (error) {
      console.error("Ürünler çekilemedi:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchProducts();
    fetchDependencies();
  }, [fetchProducts, fetchDependencies]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({ 
      ...prev, 
      [name]: type === 'number' ? parseFloat(value) : (name === "code" ? value.toUpperCase() : value) 
    }));
  };

  const handleSave = async () => {
    try {
      const payload = { ...formData };
      if (selectedId) {
        await api.put(`/Products/${selectedId}`, { id: selectedId, ...payload });
      } else {
        await api.post("/Products", payload);
      }
      setShowModal(false);
      fetchProducts();
    } catch (error) {
      console.error("Ürün kaydedilirken hata oluştu");
    }
  };

  const handleDelete = async (id: string) => {
    if (window.confirm("Bu ürünü silmek istediğinize emin misiniz?")) {
      try {
        await api.delete(`/Products/${id}`);
        fetchProducts();
      } catch (error) {
        console.error("Silme işlemi başarısız");
      }
    }
  };

  const openCreateModal = () => {
    setSelectedId(null);
    setFormData({ code: "", name: "", barcode: "", defaultPrice: 0, criticalStockLevel: 0, vatRate: 20, categoryId: "", brandId: "", unitId: "" });
    setDisplayPrice(""); // Inputu sıfırla
    setShowModal(true);
  };

 const openEditModal = (p: any) => {
    setSelectedId(p.id);

    // 🚀 İsimden ID Bulma Operasyonu
    // Elimizdeki isimle, baştan çektiğimiz bağımlılıklar listesinde eşleşen ID'yi yakalıyoruz.
    const foundCategory = categories.find(c => c.label === p.categoryName);
    const foundBrand = brands.find(b => b.label === p.brandName);
    const foundUnit = units.find(u => u.label === p.unitName);

    setFormData({ 
      code: p.code || "", 
      name: p.name || "", 
      barcode: p.barcode || "", 
      defaultPrice: p.defaultPrice || 0, 
      criticalStockLevel: p.criticalStockLevel || 0, 
      vatRate: p.vatRate || 20, 
      // 🎯 Eğer isimden ID bulduysak onu kullan, yoksa boş bırak
      categoryId: foundCategory ? foundCategory.value.toString() : "", 
      brandId: foundBrand ? foundBrand.value.toString() : "", 
      unitId: foundUnit ? foundUnit.value.toString() : "" 
    });

    const initialPrice = p.defaultPrice || 0;
    const initialFormatted = initialPrice > 0 ? maskCurrency(initialPrice.toString().replace(".", ",")) : "";
    setDisplayPrice(initialFormatted);
    
    setShowModal(true);
  };

  const columns = [
    { 
      header: "ÜRÜN BİLGİSİ", 
      accessor: (p: Product) => (
        <div>
          <div className="fw-medium text-light">{p.name}</div>
          <div className="text-muted small font-monospace">{p.code} | {p.barcode}</div>
        </div>
      ) 
    },
    { 
      header: "KATEGORİ / MARKA", 
      accessor: (p: Product) => (
        <div className="small">
          <span className="text-info">{p.categoryName || "-"}</span>
          <br />
          <span className="text-muted">{p.brandName || "-"}</span>
        </div>
      ) 
    },
    { 
      header: "FİYAT & KDV", 
      accessor: (p: Product) => (
        <div>
          <div className="text-light">{p.defaultPrice.toLocaleString('tr-TR', { minimumFractionDigits: 2 })} ₺</div>
          <span className="badge bg-secondary-subtle text-secondary small">KDV: %{p.vatRate}</span>
        </div>
      ) 
    },
    {
      header: "İŞLEMLER",
      className: "text-end",
      accessor: (p: Product) => (
        <div className="d-flex justify-content-end gap-2">
          {hasPermission(APP_PERMISSIONS.Products.Update) && (
            <button className="btn btn-sm btn-outline-info border-0 p-2" onClick={() => openEditModal(p)} title="Düzenle">
              <i className="bi bi-pencil-square fs-6"></i>
            </button>
          )}
          {hasPermission(APP_PERMISSIONS.Products.Delete) && (
            <button className="btn btn-sm btn-outline-danger border-0 p-2" onClick={() => handleDelete(p.id)} title="Sil">
              <i className="bi bi-trash3 fs-6"></i>
            </button>
          )}
        </div>
      )
    }
  ];

  return (
    <div className="page-products animate__animated animate__fadeIn">
      {loading ? (
        <div className="d-flex justify-content-center align-items-center" style={{ height: '300px' }}>
          <div className="spinner-border text-primary" role="status"></div>
        </div>
      ) : (
        <DataTable<Product>
          title="Ürün Kartları"
          description="Sistemdeki tüm ürünlerinizi, fiyatlarını ve stok seviyelerini yönetin."
          columns={columns}
          data={products}
          onAdd={hasPermission(APP_PERMISSIONS.Products.Create) ? openCreateModal : undefined}
          addText="Yeni Ürün Ekle"
        />
      )}

      <AppModal 
        show={showModal} 
        title={selectedId ? "Ürün Güncelle" : "Yeni Ürün Kartı"} 
        onClose={() => setShowModal(false)}
        onSave={handleSave}
        size="lg"
      >
        <div className="row g-3">
          <div className="col-md-4">
            <label className="form-label small fw-bold">Ürün Kodu</label>
            <input type="text" name="code" value={formData.code} onChange={handleInputChange} className="form-control form-control-sm text-uppercase" />
          </div>
          <div className="col-md-4">
            <label className="form-label small fw-bold">Barkod</label>
            <input type="text" name="barcode" value={formData.barcode} onChange={handleInputChange} className="form-control form-control-sm" />
          </div>
          <div className="col-md-4">
            <label className="form-label small fw-bold">KDV Oranı (%)</label>
            <input type="number" name="vatRate" value={formData.vatRate} onChange={handleInputChange} className="form-control form-control-sm" />
          </div>

          <div className="col-md-12">
            <label className="form-label small fw-bold">Ürün Adı</label>
            <input type="text" name="name" value={formData.name} onChange={handleInputChange} className="form-control form-control-sm" />
          </div>

          <div className="col-md-4">
            <AppSelect 
              label="Kategori"
              options={categories}
              value={formData.categoryId}
              onChange={(val) => setFormData(prev => ({...prev, categoryId: val}))}
              placeholder="Ara ve Seç..."
              isSearchable={true} 
            />
          </div>
          <div className="col-md-4">
            <AppSelect 
              label="Marka"
              options={brands}
              value={formData.brandId}
              onChange={(val) => setFormData(prev => ({...prev, brandId: val}))}
              placeholder="Ara ve Seç..."
              isSearchable={true} 
            />
          </div>
          <div className="col-md-4">
            <AppSelect 
              label="Birim"
              options={units}
              value={formData.unitId}
              onChange={(val) => setFormData(prev => ({...prev, unitId: val}))}
              placeholder="Ara ve Seç..."
              isSearchable={true} 
            />
          </div>

          <div className="col-md-6">
            <label className="form-label small fw-bold">Varsayılan Satış Fiyatı (₺)</label>
            <input 
              type="text" 
              className="form-control form-control-sm text-end fw-bold"
              placeholder="0,00"
              value={displayPrice}
              onChange={(e) => {
                const masked = maskCurrency(e.target.value);
                setDisplayPrice(masked);
                setFormData(prev => ({ ...prev, defaultPrice: unmaskPrice(masked) }));
              }}
              onFocus={(e) => e.target.select()}
            />
          </div>
          <div className="col-md-6">
            <label className="form-label small fw-bold">Kritik Stok Seviyesi</label>
            <input type="number" name="criticalStockLevel" value={formData.criticalStockLevel} onChange={handleInputChange} className="form-control form-control-sm" />
          </div>
        </div>
      </AppModal>
    </div>
  );
};

export default Products;