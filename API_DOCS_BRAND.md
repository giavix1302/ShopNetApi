# API Documentation - Brand CRUD

## Base URL
```
/api/brands
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

## 1. Get All Brands (Lấy danh sách tất cả brands)

**Endpoint:** `GET /api/brands`

**Authentication:** Không cần

**Request Parameters:** Không có

**Success Response (200):**
```json
{
  "success": true,
  "message": "Get brands successfully",
  "data": [
    {
      "id": 1,
      "name": "Nike",
      "description": "Just Do It",
      "createdAt": "2024-01-01T10:00:00Z"
    },
    {
      "id": 2,
      "name": "Adidas",
      "description": "Impossible is Nothing",
      "createdAt": "2024-01-02T10:00:00Z"
    }
  ]
}
```

**Lưu ý:** Danh sách được sắp xếp theo `createdAt` giảm dần (mới nhất trước).

---

## 2. Get Brand By ID (Lấy thông tin brand theo ID)

**Endpoint:** `GET /api/brands/{id}`

**Authentication:** Không cần

**Path Parameters:**
- `id` (long): ID của brand

**Success Response (200):**
```json
{
  "success": true,
  "message": "Get brand successfully",
  "data": {
    "id": 1,
    "name": "Nike",
    "description": "Just Do It",
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

**Error Responses:**
- `404`: Brand not found

---

## 3. Create Brand (Tạo brand mới)

**Endpoint:** `POST /api/brands`

**Authentication:** Cần (Admin role)

**Request Headers:**
```
Authorization: Bearer {accessToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "name": "Nike",
  "description": "Just Do It"
}
```

**Validation:**
- `name`: Required, không được trống
- `description`: Required, có thể null hoặc empty string

**Success Response (200):**
```json
{
  "success": true,
  "message": "Brand created successfully",
  "data": {
    "id": 1,
    "name": "Nike",
    "description": "Just Do It",
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

**Error Responses:**
- `400`: Brand name already exists
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

---

## 4. Update Brand (Cập nhật brand)

**Endpoint:** `PUT /api/brands/{id}`

**Authentication:** Cần (Admin role)

**Request Headers:**
```
Authorization: Bearer {accessToken}
Content-Type: application/json
```

**Path Parameters:**
- `id` (long): ID của brand cần cập nhật

**Request Body:**
```json
{
  "name": "Nike Updated",
  "description": "Just Do It - Updated"
}
```

**Validation:**
- `name`: Required, không được trống
- `description`: Optional, có thể null hoặc empty string

**Success Response (200):**
```json
{
  "success": true,
  "message": "Brand updated successfully",
  "data": {
    "id": 1,
    "name": "Nike Updated",
    "description": "Just Do It - Updated",
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

**Error Responses:**
- `404`: Brand not found
- `400`: Brand name already exists (tên đã được sử dụng bởi brand khác)
- `400`: Validation errors
- `401`: Unauthorized (thiếu hoặc token không hợp lệ)
- `403`: Forbidden (không có quyền Admin)

---

## 5. Delete Brand (Xóa brand)

**Endpoint:** `DELETE /api/brands/{id}`

**Authentication:** Cần (Admin role)

**Request Headers:**
```
Authorization: Bearer {accessToken}
```

**Path Parameters:**
- `id` (long): ID của brand cần xóa

**Success Response (200):**
```json
{
  "success": true,
  "message": "Brand deleted successfully",
  "data": null
}
```

**Error Responses:**
- `404`: Brand not found
- `400`: Cannot delete brand with existing products (brand đang có sản phẩm)
- `401`: Unauthorized (thiếu hoặc token không hợp lệ)
- `403`: Forbidden (không có quyền Admin)

**Lưu ý:** Chỉ có thể xóa brand khi brand đó không có sản phẩm nào.

---

## Brand Response DTO Structure

```typescript
interface BrandResponseDto {
  id: number;              // ID của brand
  name: string;            // Tên brand
  description: string | null; // Mô tả brand (có thể null)
  createdAt: string;      // Ngày tạo (ISO 8601 format)
}
```

---

## Ví dụ Frontend (JavaScript/Fetch)

### Get All Brands
```javascript
const response = await fetch('https://api.example.com/api/brands', {
  method: 'GET',
  headers: {
    'Content-Type': 'application/json',
  }
});

const data = await response.json();
if (data.success) {
  console.log(data.data); // Array of brands
}
```

### Get Brand By ID
```javascript
const brandId = 1;
const response = await fetch(`https://api.example.com/api/brands/${brandId}`, {
  method: 'GET',
  headers: {
    'Content-Type': 'application/json',
  }
});

const data = await response.json();
if (data.success) {
  console.log(data.data); // Brand object
}
```

### Create Brand
```javascript
const accessToken = localStorage.getItem('accessToken');
const response = await fetch('https://api.example.com/api/brands', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    name: 'Nike',
    description: 'Just Do It'
  })
});

const data = await response.json();
if (data.success) {
  console.log('Brand created:', data.data);
}
```

### Update Brand
```javascript
const accessToken = localStorage.getItem('accessToken');
const brandId = 1;
const response = await fetch(`https://api.example.com/api/brands/${brandId}`, {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    name: 'Nike Updated',
    description: 'Just Do It - Updated'
  })
});

const data = await response.json();
if (data.success) {
  console.log('Brand updated:', data.data);
}
```

### Delete Brand
```javascript
const accessToken = localStorage.getItem('accessToken');
const brandId = 1;
const response = await fetch(`https://api.example.com/api/brands/${brandId}`, {
  method: 'DELETE',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
  }
});

const data = await response.json();
if (data.success) {
  console.log('Brand deleted successfully');
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

1. **Tên brand phải unique:** Không thể tạo hoặc cập nhật brand với tên đã tồn tại
2. **Xóa brand:** Chỉ có thể xóa brand khi không có sản phẩm nào thuộc brand đó
3. **Phân quyền:** Chỉ Admin mới có thể Create, Update, Delete brand
4. **Get operations:** Tất cả user đều có thể xem danh sách và chi tiết brand
