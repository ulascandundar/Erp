# Erp API Dokümantasyonu

## İçindekiler
1. [Genel Bakış](#genel-bakış)
2. [Kimlik Doğrulama](#kimlik-doğrulama)
3. [Dil Desteği](#dil-desteği)
4. [API Endpoint'leri](#api-endpointleri)
   - [Auth API](#auth-api)
   - [User API](#user-api)
   - [Supplier API](#supplier-api)
   - [Product API](#product-api)
   - [Unit API](#unit-api)
   - [Category API](#category-api)
   - [Raw Material API](#raw-material-api)
   - [Product Formula API](#product-formula-api)
   - [Order API](#order-api)
5. [Hata Yönetimi](#hata-yönetimi)
6. [Sayfalama ve Filtreleme](#sayfalama-ve-filtreleme)

## Genel Bakış

Erp API, bir işletme kaynak planlama (ERP) sisteminin backend API'sidir. API, RESTful prensiplerini takip eder ve JSON formatında veri alışverişi yapar. Tüm API endpoint'leri `/api/v1/[controller]` formatında yapılandırılmıştır ve API versiyonlaması header üzerinden yapılmaktadır.

Tüm API yanıtları aşağıdaki formatta döner:

```json
{
  "isSuccess": true,
  "message": "Operation successful",
  "data": { ... }
}
```

Hata durumunda:

```json
{
  "isSuccess": false,
  "message": "Error message",
  "data": null
}
```

## Kimlik Doğrulama

API, JWT (JSON Web Token) tabanlı kimlik doğrulama kullanır. Kimlik doğrulama işlemi için aşağıdaki adımlar izlenir:

1. `/api/v1/Auth/login` endpoint'ine kullanıcı bilgileri gönderilir
2. Başarılı giriş sonrası bir JWT token alınır
3. Sonraki tüm isteklerde bu token `Authorization` header'ında `Bearer [token]` formatında gönderilir

### Örnek Login İsteği

```http
POST /api/v1/Auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

### Örnek Yanıt

```json
{
  "isSuccess": true,
  "message": "Operation successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2023-03-15T12:00:00Z"
  }
}
```

## Dil Desteği

API, çoklu dil desteği sunar. İstemciler, isteklerinde tercih ettikleri dili belirtebilirler. Dil seçimi için üç yöntem desteklenir:

1. **HTTP Header Yöntemi (Önerilen)**: `X-Culture-Info` header'ı ile dil belirtme
   ```
   X-Culture-Info: tr-TR
   ```

2. **Cookie Yöntemi**: `.AspNetCore.Culture` cookie'si ile dil tercihi saklama
   ```
   c=tr-TR|uic=tr-TR
   ```

3. **Accept-Language Header**: Tarayıcının kendi dil ayarlarını kullanma
   ```
   Accept-Language: tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7
   ```

Desteklenen diller: `tr-TR` (Türkçe), `en-US` (İngilizce)

## API Endpoint'leri

### Auth API

Kimlik doğrulama ve kullanıcı yönetimi işlemleri için kullanılır.

#### Endpoints

| Metod | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| POST | `/api/v1/Auth/login` | Kullanıcı girişi | Yetki gerekmez |
| POST | `/api/v1/Auth/forgotPassword/send` | Şifre sıfırlama kodu gönderme | Yetki gerekmez |
| PUT | `/api/v1/Auth/forgotPassword/resetPassword` | Şifre sıfırlama | Yetki gerekmez |
| PUT | `/api/v1/Auth/changePassword` | Şifre değiştirme | Kullanıcı |

#### Örnek: Şifre Sıfırlama İsteği

```http
POST /api/v1/Auth/forgotPassword/send
Content-Type: application/json

{
  "email": "user@example.com"
}
```

#### Örnek: Şifre Değiştirme İsteği

```http
PUT /api/v1/Auth/changePassword
Authorization: Bearer [token]
Content-Type: application/json

{
  "currentPassword": "oldPassword123",
  "newPassword": "newPassword123",
  "confirmPassword": "newPassword123"
}
```

### User API

Kullanıcı yönetimi işlemleri için kullanılır.

#### Endpoints

| Metod | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| GET | `/api/v1/User` | Tüm kullanıcıları listeler | CompanyAdmin |
| GET | `/api/v1/User/{id}` | Belirli bir kullanıcıyı getirir | CompanyAdmin |
| POST | `/api/v1/User` | Yeni kullanıcı oluşturur | CompanyAdmin |
| PUT | `/api/v1/User/{id}` | Kullanıcı bilgilerini günceller | CompanyAdmin |
| DELETE | `/api/v1/User/{id}` | Kullanıcıyı siler | CompanyAdmin |
| GET | `/api/v1/User/paged` | Sayfalanmış kullanıcı listesi | CompanyAdmin |

#### Örnek: Kullanıcı Oluşturma İsteği

```http
POST /api/v1/User
Authorization: Bearer [token]
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "password123",
  "roles": ["CompanyAdmin"]
}
```

### Supplier API

Tedarikçi yönetimi işlemleri için kullanılır.

#### Endpoints

| Metod | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| GET | `/api/v1/Supplier` | Tedarikçileri listeler (sayfalama destekli) | CompanyAdmin |
| GET | `/api/v1/Supplier/{id}` | Belirli bir tedarikçiyi getirir | CompanyAdmin |
| POST | `/api/v1/Supplier` | Yeni tedarikçi oluşturur | CompanyAdmin |
| PUT | `/api/v1/Supplier/{id}` | Tedarikçi bilgilerini günceller | CompanyAdmin |
| DELETE | `/api/v1/Supplier/{id}` | Tedarikçiyi siler | CompanyAdmin |

#### Örnek: Tedarikçi Oluşturma İsteği

```http
POST /api/v1/Supplier
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "ABC Suppliers",
  "contactPerson": "Jane Smith",
  "email": "contact@abcsuppliers.com",
  "phoneNumber": "1234567890",
  "address": "123 Main St, City",
  "taxNumber": "TX123456",
  "website": "https://abcsuppliers.com",
  "description": "Office supplies provider"
}
```

#### Örnek: Tedarikçileri Listeleme İsteği (Sayfalama ve Arama)

```http
GET /api/v1/Supplier?pageNumber=1&pageSize=10&search=ABC&orderBy=Name
Authorization: Bearer [token]
```

### Product API

Ürün yönetimi işlemleri için kullanılır.

#### Endpoints

| Metod | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| GET | `/api/v1/Product` | Tüm ürünleri listeler | CompanyAdmin |
| GET | `/api/v1/Product/{id}` | Belirli bir ürünü getirir | CompanyAdmin |
| POST | `/api/v1/Product` | Yeni ürün oluşturur | CompanyAdmin |
| PUT | `/api/v1/Product/{id}` | Ürün bilgilerini günceller | CompanyAdmin |
| DELETE | `/api/v1/Product/{id}` | Ürünü siler | CompanyAdmin |
| GET | `/api/v1/Product/paged` | Sayfalanmış ürün listesi | CompanyAdmin |

#### Örnek: Ürün Oluşturma İsteği

```http
POST /api/v1/Product
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Office Chair",
  "sku": "OFC-001",
  "barcode": "1234567890123",
  "description": "Ergonomic office chair",
  "price": 199.99,
  "categoryIds": ["3fa85f64-5717-4562-b3fc-2c963f66afa6"]
}
```

### Unit API

Birim yönetimi işlemleri için kullanılır (ağırlık, uzunluk, hacim birimleri vb.).

#### Endpoints

| Metod | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| GET | `/api/v1/Unit` | Birimleri listeler (sayfalama destekli) | CompanyAdmin |
| GET | `/api/v1/Unit/{id}` | Belirli bir birimi getirir | CompanyAdmin |
| POST | `/api/v1/Unit` | Yeni birim oluşturur | CompanyAdmin |
| PUT | `/api/v1/Unit/{id}` | Birim bilgilerini günceller | CompanyAdmin |
| DELETE | `/api/v1/Unit/{id}` | Birimi siler | CompanyAdmin |
| GET | `/api/v1/Unit/relationUnits/{unitId}` | İlişkili birimleri listeler | CompanyAdmin |
| GET | `/api/v1/Unit/convertUnit/{unitId}/{rawMaterialId}` | Birim dönüşüm oranını hesaplar | CompanyAdmin |
| GET | `/api/v1/Unit/findRateToRoot/{unitId}` | Kök birime dönüşüm oranını hesaplar | CompanyAdmin |

#### Örnek: Birim Oluşturma İsteği

```http
POST /api/v1/Unit
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Kilogram",
  "shortCode": "kg",
  "description": "Metric weight unit",
  "conversionRate": 1,
  "rootUnitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

#### Örnek: Birim Dönüşüm Oranı Hesaplama

```http
GET /api/v1/Unit/convertUnit/3fa85f64-5717-4562-b3fc-2c963f66afa6/4fa85f64-5717-4562-b3fc-2c963f66afa7
Authorization: Bearer [token]
```

### Category API

Kategori yönetimi işlemleri için kullanılır.

#### Endpoints

| Metod | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| GET | `/api/v1/Category` | Kategorileri listeler (sayfalama destekli) | CompanyAdmin |
| GET | `/api/v1/Category/{id}` | Belirli bir kategoriyi getirir | CompanyAdmin |
| POST | `/api/v1/Category` | Yeni kategori oluşturur | CompanyAdmin |
| PUT | `/api/v1/Category/{id}` | Kategori bilgilerini günceller | CompanyAdmin |
| DELETE | `/api/v1/Category/{id}` | Kategoriyi siler | CompanyAdmin |

#### Örnek: Kategori Oluşturma İsteği

```http
POST /api/v1/Category
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Office Furniture",
  "description": "Desks, chairs, and other office furniture"
}
```

### Raw Material API

Hammadde yönetimi işlemleri için kullanılır.

#### Endpoints

| Metod | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| GET | `/api/v1/RawMaterial` | Hammaddeleri listeler (sayfalama destekli) | CompanyAdmin |
| GET | `/api/v1/RawMaterial/{id}` | Belirli bir hammaddeyi getirir | CompanyAdmin |
| POST | `/api/v1/RawMaterial` | Yeni hammadde oluşturur | CompanyAdmin |
| PUT | `/api/v1/RawMaterial/{id}` | Hammadde bilgilerini günceller | CompanyAdmin |
| DELETE | `/api/v1/RawMaterial/{id}` | Hammaddeyi siler | CompanyAdmin |

#### Örnek: Hammadde Oluşturma İsteği

```http
POST /api/v1/RawMaterial
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Wood",
  "description": "Oak wood for furniture",
  "price": 50.00,
  "barcode": "RM-001",
  "stock": 100,
  "unitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "supplierIds": ["3fa85f64-5717-4562-b3fc-2c963f66afa7"]
}
```

### Product Formula API

Ürün formülü yönetimi işlemleri için kullanılır.

#### Endpoints

| Metod | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| GET | `/api/v1/ProductFormula` | Ürün formüllerini listeler (sayfalama destekli) | CompanyAdmin |
| GET | `/api/v1/ProductFormula/{id}` | Belirli bir ürün formülünü getirir | CompanyAdmin |
| POST | `/api/v1/ProductFormula` | Yeni ürün formülü oluşturur | CompanyAdmin |
| PUT | `/api/v1/ProductFormula/{id}` | Ürün formülü bilgilerini günceller | CompanyAdmin |
| DELETE | `/api/v1/ProductFormula/{id}` | Ürün formülünü siler | CompanyAdmin |

#### Örnek: Ürün Formülü Oluşturma İsteği

```http
POST /api/v1/ProductFormula
Authorization: Bearer [token]
Content-Type: application/json

{
  "name": "Office Chair Formula",
  "description": "Formula for standard office chair",
  "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "items": [
    {
      "rawMaterialId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "quantity": 2.5,
      "unitId": "3fa85f64-5717-4562-b3fc-2c963f66afa8"
    }
  ]
}
```

### Order API

Sipariş yönetimi işlemleri için kullanılır.

#### Endpoints

| Metod | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| POST | `/api/v1/Order/place` | Yeni sipariş oluşturur | CompanyAdmin |
| GET | `/api/v1/OrderReport/daily` | Günlük sipariş raporunu getirir | CompanyAdmin |
| GET | `/api/v1/OrderReport/monthly` | Aylık sipariş raporunu getirir | CompanyAdmin |
| GET | `/api/v1/OrderReport/product/{productId}` | Ürüne göre sipariş raporunu getirir | CompanyAdmin |

#### Örnek: Sipariş Oluşturma İsteği

```http
POST /api/v1/Order/place
Authorization: Bearer [token]
Content-Type: application/json

{
  "customerName": "Acme Corp",
  "customerEmail": "orders@acmecorp.com",
  "customerPhone": "1234567890",
  "customerAddress": "123 Business St, City",
  "orderItems": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 5,
      "unitPrice": 199.99
    }
  ],
  "payments": [
    {
      "amount": 999.95,
      "paymentMethod": "CreditCard",
      "paymentDetails": "XXXX-XXXX-XXXX-1234"
    }
  ]
}
```

## Hata Yönetimi

API, çeşitli hata durumlarını standart bir formatta döndürür. Hata yanıtları her zaman HTTP 400 (Bad Request) veya 500 (Internal Server Error) durum kodlarıyla birlikte gelir ve aşağıdaki formatta olur:

```json
{
  "isSuccess": false,
  "message": "Hata mesajı burada görünür",
  "data": null
}
```

Yaygın hata türleri:

- `NullValueException`: Gerekli bir değer null olduğunda
- `UserAuthException`: Kimlik doğrulama hatası
- `SupplierNameAlreadyExistsException`: Aynı isimde tedarikçi zaten var
- `SupplierTaxNumberAlreadyExistsException`: Aynı vergi numarasına sahip tedarikçi zaten var
- `SupplierHasRawMaterialsException`: Hammaddeleri olan bir tedarikçi silinemez
- `UnitNameAlreadyExistsException`: Aynı isimde birim zaten var
- `UnitShortCodeAlreadyExistsException`: Aynı kısa koda sahip birim zaten var
- `UnitHasProductRawMaterialException`: Hammaddeleri olan bir birim silinemez
- `UnitTypeMismatchException`: Birim türleri uyuşmuyor
- `ProductNameAlreadyExistsException`: Aynı isimde ürün zaten var
- `ProductBarcodeAlreadyExistsException`: Aynı barkoda sahip ürün zaten var

## Sayfalama ve Filtreleme

API, koleksiyon döndüren endpoint'lerde sayfalama ve filtreleme desteği sunar. Sayfalama ve filtreleme parametreleri query string üzerinden gönderilir.

### Sayfalama Parametreleri

- `pageNumber`: Sayfa numarası (1'den başlar)
- `pageSize`: Sayfa başına öğe sayısı
- `orderBy`: Sıralama alanı
- `isDesc`: Azalan sıralama için `true`, artan sıralama için `false`
- `search`: Arama terimi

### Örnek Sayfalama İsteği

```http
GET /api/v1/Supplier?pageNumber=1&pageSize=10&orderBy=Name&isDesc=false&search=ABC
Authorization: Bearer [token]
```

### Sayfalama Yanıtı

```json
{
  "isSuccess": true,
  "message": "Operation successful",
  "data": {
    "items": [ ... ],
    "totalCount": 100,
    "totalPages": 10,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```
