# API Documentation - Category CRUD

## Base URL
```
/api/categories
```

## Response Format

### Success
```json
{
  "success": true,
  "message": "Thông báo thành công",
  "data": { }
}
```

### Error (ExceptionMiddleware)
```json
{
  "success": false,
  "message": "Thông báo lỗi",
  "statusCode": 400
}
```

### Error (Validation - InvalidModelStateResponseFactory)
```json
{
  "success": false,
  "message": "Dữ liệu không hợp lệ",
  "data": [
    { "field": "name", "message": "The Name field is required." }
  ]
}
```

---

## 1) Get All Categories

- **Endpoint**: `GET /api/categories`
- **Auth**: Không cần
- **Query/Body**: Không có

**Response 200**
```json
{
  "success": true,
  "message": "Lấy danh sách loại thành công",
  "data": [
    {
      "id": 1,
      "name": "Shoes",
      "description": "Giày dép",
      "createdAt": "2024-01-01T10:00:00Z"
    }
  ]
}
```

---

## 2) Get Category By ID

- **Endpoint**: `GET /api/categories/{id}`
- **Auth**: Không cần
- **Path param**:
  - **id**: `long`

**Response 200**
```json
{
  "success": true,
  "message": "Lấy chi tiết thành công",
  "data": {
    "id": 1,
    "name": "Shoes",
    "description": "Giày dép",
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

**Errors**
- **404**: `Category not found`

---

## 3) Create Category

- **Endpoint**: `POST /api/categories`
- **Auth**: **Admin** (JWT Bearer)
- **Headers**
```
Authorization: Bearer {accessToken}
Content-Type: application/json
```

**Body**
```json
{
  "name": "Shoes",
  "description": "Giày dép"
}
```

**Validation**
- **name**: Required
- **description**: Required (DTO cho phép null nhưng có `[Required]`)

**Response 200**
```json
{
  "success": true,
  "message": "Category created successfully",
  "data": {
    "id": 1,
    "name": "Shoes",
    "description": "Giày dép",
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

**Errors**
- **400**: `Category name already exists`
- **400**: Validation (`Dữ liệu không hợp lệ`)
- **401**: Unauthorized (token thiếu/sai)
- **403**: Forbidden (không có role Admin)

---

## 4) Update Category

- **Endpoint**: `PUT /api/categories/{id}`
- **Auth**: **Admin** (JWT Bearer)
- **Headers**
```
Authorization: Bearer {accessToken}
Content-Type: application/json
```
- **Path param**:
  - **id**: `long`

**Body**
```json
{
  "name": "Shoes Updated",
  "description": "Giày dép - updated"
}
```

**Response 200**
```json
{
  "success": true,
  "message": "Category updated successfully",
  "data": {
    "id": 1,
    "name": "Shoes Updated",
    "description": "Giày dép - updated",
    "createdAt": "2024-01-01T10:00:00Z"
  }
}
```

**Errors**
- **404**: `Category not found`
- **400**: `Category name already exists`
- **400**: Validation (`Dữ liệu không hợp lệ`)
- **401**: Unauthorized
- **403**: Forbidden (không có role Admin)

---

## 5) Delete Category

- **Endpoint**: `DELETE /api/categories/{id}`
- **Auth**: **Theo code hiện tại: Không cần** (controller chưa gắn `[Authorize]`)
- **Path param**:
  - **id**: `long`

**Response 200**
```json
{
  "success": true,
  "message": "Category deleted successfully",
  "data": null
}
```

**Errors**
- **404**: `Category not found`
- **400**: `Cannot delete category with existing products`

**Note (khuyến nghị)**: Nếu API này chỉ dành cho Admin, BE nên thêm `[Authorize(Roles = "Admin")]` cho endpoint delete để tránh xóa trái phép.

---

## Category DTO (data trả về)

```ts
interface CategoryResponseDto {
  id: number;
  name: string;
  description: string | null;
  createdAt: string; // ISO 8601
}
```

