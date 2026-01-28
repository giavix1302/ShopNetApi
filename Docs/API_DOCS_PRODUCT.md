# API Documentation - Product CRUD

## Base URL
```
/api/products
```

## Response Format

### Success Response
```json
{
  "success": true,
  "message": "Thông báo thành công",
  "data": { ... }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Thông báo lỗi",
  "statusCode": 400
}
```

---

## 1. Get All Products (Lấy danh sách tất cả products)

**Endpoint:** `GET /api/products`

**Authentication:** Không cần

**Request Parameters:** Không có

**Success Response (200):**
```json
{
  "success": true,
  "message": "Get products successfully",
  "data": [
    {
      "id": 1,
      "name": "Nike Air Max 90",
      "slug": "nike-air-max-90",
      "description": "Classic running shoes",
      "price": 120.00,
      "discountPrice": 99.99,
      "stockQuantity": 50,
      "isActive": true,
      "categoryId": 1,
      "categoryName": "Shoes",
      "brandId": 1,
      "brandName": "Nike",
      "colors": [
        {
          "id": 1,
          "colorName": "Black",
          "hexCode": "#000000"
        },
        {
          "id": 2,
          "colorName": "White",
          "hexCode": "#FFFFFF"
        }
      ],
      "specifications": [
        {
          "id": 1,
          "specName": "Material",
          "specValue": "Leather"
        },
        {
          "id": 2,
          "specName": "Size",
          "specValue": "42"
        }
      ],
      "createdAt": "2024-01-01T10:00:00Z"
    }
  ]
}
```

---

## 2. Get Product By ID (Lấy thông tin product theo ID)

**Endpoint:** `GET /api/products/{id}`

**Authentication:** Không cần

**Path Parameters:**
- `id` (long): ID của product

**Success Response (200):**
```json
{
  "success": true,
  "message": "Get product successfully",
  "data": {
    "id": 1,
    "name": "Nike Air Max 90",
    "slug": "nike-air-max-90",
    "description": "Classic running shoes",
    "price": 120.00,
    "discountPrice": 99.99,
    "stockQuantity": 50,
    "isActive": true,
    "categoryId": 1,
    "categoryName": "Shoes",
    "brandId": 1,
    "brandName": "Nike",
    "colors": [
      {
        "id": 1,
        "colorName": "Black",
        "hexCode": "#000000"
      }
    ],
    "specifications": [
      {
        "id": 1,
        "specName": "Material",
        "specValue": "Leather"
      }
    ],
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

**Error Responses:**
- `404`: Product not found

---

## 3. Create Product (Tạo product mới)

**Endpoint:** `POST /api/products`

**Authentication:** Cần (Admin role)

**Request Headers:**
```
Authorization: Bearer {accessToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Nike Air Max 90",
  "description": "Classic running shoes",
  "price": 120.00,
  "discountPrice": 99.99,
  "stockQuantity": 50,
  "categoryId": 1,
  "brandId": 1,
  "colorIds": [1, 2],
  "specifications": [
    {
      "specName": "Material",
      "specValue": "Leather"
    },
    {
      "specName": "Size",
      "specValue": "42"
    }
  ]
}
```

**Validation:**
- `name`: Required, không được trống
- `description`: Optional, có thể null
- `price`: Required, phải là số dương
- `discountPrice`: Optional, có thể null
- `stockQuantity`: Required, phải là số nguyên dương
- `categoryId`: Required, phải tồn tại trong database
- `brandId`: Required, phải tồn tại trong database
- `colorIds`: Required, phải có ít nhất 1 màu, tất cả màu phải tồn tại trong database
- `specifications`: Optional, mảng các specification
  - `specName`: Required
  - `specValue`: Required

**Success Response (200):**
```json
{
  "success": true,
  "message": "Product created successfully",
  "data": {
    "id": 1,
    "name": "Nike Air Max 90",
    "slug": "nike-air-max-90",
    "description": "Classic running shoes",
    "price": 120.00,
    "discountPrice": 99.99,
    "stockQuantity": 50,
    "isActive": true,
    "categoryId": 1,
    "categoryName": "Shoes",
    "brandId": 1,
    "brandName": "Nike",
    "colors": [
      {
        "id": 1,
        "colorName": "Black",
        "hexCode": "#000000"
      },
      {
        "id": 2,
        "colorName": "White",
        "hexCode": "#FFFFFF"
      }
    ],
    "specifications": [
      {
        "id": 1,
        "specName": "Material",
        "specValue": "Leather"
      }
    ],
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

**Error Responses:**
- `400`: Brand không tồn tại
- `400`: Category không tồn tại
- `400`: Phải chọn ít nhất 1 màu
- `400`: Một hoặc nhiều màu không tồn tại
- `400`: Validation errors
  ```json
  {
    "success": false,
    "message": "Dữ liệu không hợp lệ",
    "data": [
      { "field": "name", "message": "The Name field is required." }
    ]
  }
  ```
- `401`: Unauthorized (thiếu hoặc token không hợp lệ)
- `403`: Forbidden (không có quyền Admin)

**Lưu ý:** 
- Slug được tự động tạo từ tên product (URL-friendly)
- Nếu slug đã tồn tại, sẽ tự động thêm số phía sau (ví dụ: `nike-air-max-90-1`)

---

## 4. Update Product (Cập nhật product)

**Endpoint:** `PUT /api/products/{id}`

**Authentication:** Cần (Admin role)

**Request Headers:**
```
Authorization: Bearer {accessToken}
Content-Type: application/json
```

**Path Parameters:**
- `id` (long): ID của product cần cập nhật

**Request Body:**
```json
{
  "name": "Nike Air Max 90 Updated",
  "description": "Updated description",
  "price": 130.00,
  "discountPrice": 109.99,
  "stockQuantity": 60,
  "isActive": true,
  "categoryId": 1,
  "brandId": 1,
  "colorIds": [1, 3],
  "specifications": [
    {
      "specName": "Material",
      "specValue": "Synthetic Leather"
    }
  ]
}
```

**Validation:**
- `name`: Required, không được trống
- `description`: Optional, có thể null
- `price`: Required, phải là số dương
- `discountPrice`: Optional, có thể null
- `stockQuantity`: Required, phải là số nguyên dương
- `isActive`: Optional, boolean
- `categoryId`: Required, phải tồn tại trong database
- `brandId`: Required, phải tồn tại trong database
- `colorIds`: Optional, nếu truyền thì tất cả màu phải tồn tại trong database
- `specifications`: Optional
  - `null`: Không thay đổi specifications hiện tại
  - `[]`: Xóa tất cả specifications
  - `[{...}]`: Thay thế toàn bộ specifications bằng danh sách mới
  - `specName`: Required
  - `specValue`: Required

**Success Response (200):**
```json
{
  "success": true,
  "message": "Product updated successfully",
  "data": {
    "id": 1,
    "name": "Nike Air Max 90 Updated",
    "slug": "nike-air-max-90-updated",
    "description": "Updated description",
    "price": 130.00,
    "discountPrice": 109.99,
    "stockQuantity": 60,
    "isActive": true,
    "categoryId": 1,
    "categoryName": "Shoes",
    "brandId": 1,
    "brandName": "Nike",
    "colors": [
      {
        "id": 1,
        "colorName": "Black",
        "hexCode": "#000000"
      },
      {
        "id": 3,
        "colorName": "Red",
        "hexCode": "#FF0000"
      }
    ],
    "specifications": [
      {
        "id": 2,
        "specName": "Material",
        "specValue": "Synthetic Leather"
      }
    ],
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

**Error Responses:**
- `404`: Product not found
- `400`: Một hoặc nhiều màu không tồn tại (nếu truyền colorIds)
- `400`: Validation errors
- `401`: Unauthorized (thiếu hoặc token không hợp lệ)
- `403`: Forbidden (không có quyền Admin)

**Lưu ý về Specifications:**
- Nếu không truyền field `specifications` (null) → Giữ nguyên specifications hiện tại
- Nếu truyền mảng rỗng `[]` → Xóa tất cả specifications
- Nếu truyền mảng có phần tử `[{...}]` → Thay thế toàn bộ specifications bằng danh sách mới

**Lưu ý về Colors:**
- Nếu không truyền field `colorIds` (null) → Giữ nguyên colors hiện tại
- Nếu truyền mảng `[1, 2, 3]` → Thay thế toàn bộ colors bằng danh sách mới

**Lưu ý về Slug:**
- Nếu tên product thay đổi, slug sẽ được tự động tạo lại từ tên mới
- Nếu slug mới đã tồn tại (bởi product khác), sẽ tự động thêm số phía sau

---

## 5. Delete Product (Xóa product)

**Endpoint:** `DELETE /api/products/{id}`

**Authentication:** Cần (Admin role)

**Request Headers:**
```
Authorization: Bearer {accessToken}
```

**Path Parameters:**
- `id` (long): ID của product cần xóa

**Success Response (200):**
```json
{
  "success": true,
  "message": "Product deleted successfully",
  "data": null
}
```

**Error Responses:**
- `404`: Product not found
- `401`: Unauthorized (thiếu hoặc token không hợp lệ)
- `403`: Forbidden (không có quyền Admin)

---

## Product Response DTO Structure

```typescript
interface ProductResponseDto {
  id: number;                    // ID của product
  name: string;                  // Tên product
  slug: string;                  // URL-friendly slug (tự động tạo)
  description: string | null;    // Mô tả product
  price: number;                 // Giá gốc
  discountPrice: number | null;  // Giá khuyến mãi (nếu có)
  stockQuantity: number;         // Số lượng tồn kho
  isActive: boolean;             // Trạng thái active/inactive
  categoryId: number | null;     // ID category
  categoryName: string | null;   // Tên category
  brandId: number | null;        // ID brand
  brandName: string | null;      // Tên brand
  colors: ProductColorResponseDto[];  // Danh sách màu
  specifications: ProductSpecificationResponseDto[];  // Danh sách thông số kỹ thuật
  createdAt: string;            // Ngày tạo (ISO 8601 format)
}

interface ProductColorResponseDto {
  id: number;                    // ID của color
  colorName: string;             // Tên màu
  hexCode: string | null;        // Mã màu hex (ví dụ: #000000)
}

interface ProductSpecificationResponseDto {
  id: number;                    // ID của specification
  specName: string;              // Tên thông số (ví dụ: "Material", "Size")
  specValue: string;            // Giá trị thông số (ví dụ: "Leather", "42")
}
```

---

## Ví dụ Frontend (JavaScript/Fetch)

### Get All Products
```javascript
const response = await fetch('https://api.example.com/api/products', {
  method: 'GET',
  headers: {
    'Content-Type': 'application/json',
  }
});

const data = await response.json();
if (data.success) {
  console.log(data.data); // Array of products
}
```

### Get Product By ID
```javascript
const productId = 1;
const response = await fetch(`https://api.example.com/api/products/${productId}`, {
  method: 'GET',
  headers: {
    'Content-Type': 'application/json',
  }
});

const data = await response.json();
if (data.success) {
  console.log(data.data); // Product object
}
```

### Create Product
```javascript
const accessToken = localStorage.getItem('accessToken');
const response = await fetch('https://api.example.com/api/products', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    name: 'Nike Air Max 90',
    description: 'Classic running shoes',
    price: 120.00,
    discountPrice: 99.99,
    stockQuantity: 50,
    categoryId: 1,
    brandId: 1,
    colorIds: [1, 2],
    specifications: [
      {
        specName: 'Material',
        specValue: 'Leather'
      },
      {
        specName: 'Size',
        specValue: '42'
      }
    ]
  })
});

const data = await response.json();
if (data.success) {
  console.log('Product created:', data.data);
}
```

### Update Product
```javascript
const accessToken = localStorage.getItem('accessToken');
const productId = 1;
const response = await fetch(`https://api.example.com/api/products/${productId}`, {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    name: 'Nike Air Max 90 Updated',
    description: 'Updated description',
    price: 130.00,
    discountPrice: 109.99,
    stockQuantity: 60,
    isActive: true,
    categoryId: 1,
    brandId: 1,
    colorIds: [1, 3],  // Thay thế toàn bộ colors
    specifications: [  // Thay thế toàn bộ specifications
      {
        specName: 'Material',
        specValue: 'Synthetic Leather'
      }
    ]
  })
});

const data = await response.json();
if (data.success) {
  console.log('Product updated:', data.data);
}
```

### Update Product - Giữ nguyên Specifications
```javascript
const accessToken = localStorage.getItem('accessToken');
const productId = 1;
const response = await fetch(`https://api.example.com/api/products/${productId}`, {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    name: 'Nike Air Max 90 Updated',
    price: 130.00,
    stockQuantity: 60,
    categoryId: 1,
    brandId: 1
    // Không truyền specifications → giữ nguyên
    // Không truyền colorIds → giữ nguyên
  })
});

const data = await response.json();
if (data.success) {
  console.log('Product updated:', data.data);
}
```

### Update Product - Xóa tất cả Specifications
```javascript
const accessToken = localStorage.getItem('accessToken');
const productId = 1;
const response = await fetch(`https://api.example.com/api/products/${productId}`, {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    name: 'Nike Air Max 90',
    price: 130.00,
    stockQuantity: 60,
    categoryId: 1,
    brandId: 1,
    specifications: []  // Mảng rỗng → xóa tất cả specifications
  })
});

const data = await response.json();
if (data.success) {
  console.log('Product updated:', data.data);
}
```

### Delete Product
```javascript
const accessToken = localStorage.getItem('accessToken');
const productId = 1;
const response = await fetch(`https://api.example.com/api/products/${productId}`, {
  method: 'DELETE',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
  }
});

const data = await response.json();
if (data.success) {
  console.log('Product deleted successfully');
}
```

---

## Status Codes

- `200`: Success
- `400`: Bad Request (validation errors, business logic errors)
- `401`: Unauthorized (missing/invalid token)
- `403`: Forbidden (insufficient permissions)
- `404`: Not Found (resource not found)
- `500`: Internal Server Error

---

## Business Rules

1. **Slug tự động:** Slug được tự động tạo từ tên product, nếu trùng sẽ thêm số phía sau
2. **Màu bắt buộc:** Khi tạo product mới, phải chọn ít nhất 1 màu
3. **Specifications:** 
   - Khi tạo: Optional, có thể không có
   - Khi update: 
     - `null` → Giữ nguyên
     - `[]` → Xóa tất cả
     - `[{...}]` → Thay thế toàn bộ
4. **Colors khi update:**
   - `null` → Giữ nguyên
   - `[1, 2, 3]` → Thay thế toàn bộ
5. **Phân quyền:** Chỉ Admin mới có thể Create, Update, Delete product
6. **Get operations:** Tất cả user đều có thể xem danh sách và chi tiết product
7. **Validation:** BrandId, CategoryId, ColorIds phải tồn tại trong database
