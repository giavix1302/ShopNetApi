# API Documentation - Color CRUD

## Base URL
```
/api/colors
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

## 1. Get All Colors (Lấy danh sách tất cả colors)

**Endpoint:** `GET /api/colors`

**Authentication:** Không cần

**Request Parameters:** Không có

**Success Response (200):**
```json
{
  "success": true,
  "message": "Get colors successfully",
  "data": [
    {
      "id": 1,
      "colorName": "Black",
      "hexCode": "#000000"
    },
    {
      "id": 2,
      "colorName": "White",
      "hexCode": "#FFFFFF"
    },
    {
      "id": 3,
      "colorName": "Red",
      "hexCode": "#FF0000"
    }
  ]
}
```

---

## 2. Get Color By ID (Lấy thông tin color theo ID)

**Endpoint:** `GET /api/colors/{id}`

**Authentication:** Không cần

**Path Parameters:**
- `id` (long): ID của color

**Success Response (200):**
```json
{
  "success": true,
  "message": "Get color successfully",
  "data": {
    "id": 1,
    "colorName": "Black",
    "hexCode": "#000000"
  }
}
```

**Error Responses:**
- `404`: Color not found

---

## 3. Create Color (Tạo color mới)

**Endpoint:** `POST /api/colors`

**Authentication:** Cần (Admin role)

**Request Headers:**
```
Authorization: Bearer {accessToken}
Content-Type: application/json
```

**Request Body:**
```json
{
  "colorName": "Black",
  "hexCode": "#000000"
}
```

**Validation:**
- `colorName`: Required, không được trống
- `hexCode`: Required, có thể null hoặc empty string

**Success Response (200):**
```json
{
  "success": true,
  "message": "Color created successfully",
  "data": {
    "id": 1,
    "colorName": "Black",
    "hexCode": "#000000"
  }
}
```

**Error Responses:**
- `400`: Color name already exists
- `400`: Validation errors
  ```json
  {
    "success": false,
    "message": "Dữ liệu không hợp lệ",
    "data": [
      { "field": "colorName", "message": "The ColorName field is required." }
    ]
  }
  ```
- `401`: Unauthorized (thiếu hoặc token không hợp lệ)
- `403`: Forbidden (không có quyền Admin)

---

## 4. Update Color (Cập nhật color)

**Endpoint:** `PUT /api/colors/{id}`

**Authentication:** Cần (Admin role)

**Request Headers:**
```
Authorization: Bearer {accessToken}
Content-Type: application/json
```

**Path Parameters:**
- `id` (long): ID của color cần cập nhật

**Request Body:**
```json
{
  "colorName": "Dark Black",
  "hexCode": "#000001"
}
```

**Validation:**
- `colorName`: Required, không được trống
- `hexCode`: Required, có thể null hoặc empty string

**Success Response (200):**
```json
{
  "success": true,
  "message": "Color updated successfully",
  "data": {
    "id": 1,
    "colorName": "Dark Black",
    "hexCode": "#000001"
  }
}
```

**Error Responses:**
- `404`: Color not found
- `400`: Color name already exists (tên đã được sử dụng bởi color khác)
- `400`: Validation errors
- `401`: Unauthorized (thiếu hoặc token không hợp lệ)
- `403`: Forbidden (không có quyền Admin)

---

## 5. Delete Color (Xóa color)

**Endpoint:** `DELETE /api/colors/{id}`

**Authentication:** Cần (Admin role)

**Request Headers:**
```
Authorization: Bearer {accessToken}
```

**Path Parameters:**
- `id` (long): ID của color cần xóa

**Success Response (200):**
```json
{
  "success": true,
  "message": "Color deleted successfully",
  "data": null
}
```

**Error Responses:**
- `404`: Color not found
- `400`: Cannot delete color with existing products (color đang được sử dụng bởi sản phẩm)
- `401`: Unauthorized (thiếu hoặc token không hợp lệ)
- `403`: Forbidden (không có quyền Admin)

**Lưu ý:** Chỉ có thể xóa color khi color đó không được sử dụng bởi sản phẩm nào.

---

## Color Response DTO Structure

```typescript
interface ColorResponseDto {
  id: number;              // ID của color
  colorName: string;       // Tên màu
  hexCode: string | null; // Mã màu hex (ví dụ: #000000, có thể null)
}
```

---

## Ví dụ Frontend (JavaScript/Fetch)

### Get All Colors
```javascript
const response = await fetch('https://api.example.com/api/colors', {
  method: 'GET',
  headers: {
    'Content-Type': 'application/json',
  }
});

const data = await response.json();
if (data.success) {
  console.log(data.data); // Array of colors
}
```

### Get Color By ID
```javascript
const colorId = 1;
const response = await fetch(`https://api.example.com/api/colors/${colorId}`, {
  method: 'GET',
  headers: {
    'Content-Type': 'application/json',
  }
});

const data = await response.json();
if (data.success) {
  console.log(data.data); // Color object
}
```

### Create Color
```javascript
const accessToken = localStorage.getItem('accessToken');
const response = await fetch('https://api.example.com/api/colors', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    colorName: 'Black',
    hexCode: '#000000'
  })
});

const data = await response.json();
if (data.success) {
  console.log('Color created:', data.data);
}
```

### Update Color
```javascript
const accessToken = localStorage.getItem('accessToken');
const colorId = 1;
const response = await fetch(`https://api.example.com/api/colors/${colorId}`, {
  method: 'PUT',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    colorName: 'Dark Black',
    hexCode: '#000001'
  })
});

const data = await response.json();
if (data.success) {
  console.log('Color updated:', data.data);
}
```

### Delete Color
```javascript
const accessToken = localStorage.getItem('accessToken');
const colorId = 1;
const response = await fetch(`https://api.example.com/api/colors/${colorId}`, {
  method: 'DELETE',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
  }
});

const data = await response.json();
if (data.success) {
  console.log('Color deleted successfully');
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

1. **Tên color phải unique:** Không thể tạo hoặc cập nhật color với tên đã tồn tại
2. **Xóa color:** Chỉ có thể xóa color khi không có sản phẩm nào sử dụng color đó
3. **Phân quyền:** Chỉ Admin mới có thể Create, Update, Delete color
4. **Get operations:** Tất cả user đều có thể xem danh sách và chi tiết color (AllowAnonymous)
5. **HexCode:** Có thể null hoặc empty string, thường là mã màu hex (ví dụ: #000000, #FFFFFF)
