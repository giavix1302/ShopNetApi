# API Documentation - Product Image CRUD

## Base URL (nested theo Product)
```
/api/products/{productId}/images
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

---

## 1) Get All Images of a Product

- **Endpoint**: `GET /api/products/{productId}/images`
- **Auth**: Không cần
- **Path param**:
  - **productId**: `long`

**Response 200**
```json
{
  "success": true,
  "message": "Get product images successfully",
  "data": [
    {
      "id": 10,
      "productId": 1,
      "imageUrl": "https://res.cloudinary.com/.../image/upload/v123/abc.jpg",
      "altText": "Front view",
      "isPrimary": true
    },
    {
      "id": 11,
      "productId": 1,
      "imageUrl": "https://res.cloudinary.com/.../image/upload/v123/def.jpg",
      "altText": "Side view",
      "isPrimary": false
    }
  ]
}
```

**Lưu ý**: danh sách được sort **primary trước** (`isPrimary` desc).

---

## 2) Create Product Image (Upload)

- **Endpoint**: `POST /api/products/{productId}/images`
- **Auth**: **Admin** (JWT Bearer)
- **Content-Type**: `multipart/form-data`
- **Path param**:
  - **productId**: `long`

**Form fields**
- **image**: file (bắt buộc)
- **altText**: string (optional)
- **isPrimary**: boolean (optional, default `false`)

**curl example**
```bash
curl -X POST "https://api.example.com/api/products/1/images" ^
  -H "Authorization: Bearer {accessToken}" ^
  -F "image=@C:\path\to\photo.jpg" ^
  -F "altText=Front view" ^
  -F "isPrimary=false"
```

**Response 200**
```json
{
  "success": true,
  "message": "Product image created successfully",
  "data": {
    "id": 10,
    "productId": 1,
    "imageUrl": "https://res.cloudinary.com/.../image/upload/v123/abc.jpg",
    "altText": "Front view",
    "isPrimary": true
  }
}
```

**Errors**
- **404**: `Product not found`
- **401**: Unauthorized
- **403**: Forbidden (không có role Admin)

**Rule về Primary khi CREATE**
- **Ảnh đầu tiên của product luôn auto `isPrimary=true`** (bất kể `isPrimary` FE gửi).
- Nếu product đã có primary và FE gửi `isPrimary=true` cho ảnh mới → BE sẽ **unset primary cũ** và set ảnh mới thành primary.

---

## 3) Update Product Image (Update meta / replace file / set primary)

- **Endpoint**: `PUT /api/products/{productId}/images/{imageId}`
- **Auth**: **Admin** (JWT Bearer)
- **Content-Type**: `multipart/form-data`
- **Path params**:
  - **productId**: `long`
  - **imageId**: `long`

**Form fields (tất cả optional)**
- **image**: file (optional) — nếu có thì BE upload file mới và xóa file cũ trên Cloudinary
- **altText**: string (optional) — nếu có field này thì update
- **isPrimary**: boolean (optional) — nếu gửi `true` sẽ set ảnh này làm primary

**curl example (set primary)**
```bash
curl -X PUT "https://api.example.com/api/products/1/images/11" ^
  -H "Authorization: Bearer {accessToken}" ^
  -F "isPrimary=true"
```

**curl example (replace file + update altText)**
```bash
curl -X PUT "https://api.example.com/api/products/1/images/11" ^
  -H "Authorization: Bearer {accessToken}" ^
  -F "image=@C:\path\to\new-photo.jpg" ^
  -F "altText=Side view updated"
```

**Response 200**
```json
{
  "success": true,
  "message": "Product image updated successfully",
  "data": {
    "id": 11,
    "productId": 1,
    "imageUrl": "https://res.cloudinary.com/.../image/upload/v123/new.jpg",
    "altText": "Side view updated",
    "isPrimary": true
  }
}
```

**Errors**
- **404**: `Product image not found` hoặc `Product image not found for this product`
- **409**: `Cannot unset primary image` (không cho set `isPrimary=false` nếu ảnh đang là primary)
- **401**: Unauthorized
- **403**: Forbidden

**Rule về Primary khi UPDATE**
- FE **không được unset primary**: nếu ảnh đang primary mà gửi `isPrimary=false` → **409**.
- Nếu gửi `isPrimary=true` cho ảnh không-primary → BE sẽ **unset primary cũ** (nếu có) rồi set ảnh này primary.
- Nếu không gửi field `isPrimary` → BE **không đụng** tới primary logic.

---

## 4) Delete Product Image

- **Endpoint**: `DELETE /api/products/{productId}/images/{imageId}`
- **Auth**: **Admin** (JWT Bearer)
- **Path params**:
  - **productId**: `long`
  - **imageId**: `long`

**Response 200**
```json
{
  "success": true,
  "message": "Product image deleted successfully",
  "data": null
}
```

**Errors**
- **404**: `Product image not found` hoặc `Product image not found for this product`
- **409**: `Cannot delete primary image. Please set another image as primary first.` (nếu xóa primary mà vẫn còn ảnh khác)
- **401**: Unauthorized
- **403**: Forbidden

**Rule khi DELETE**
- Nếu ảnh là **primary**:
  - Nếu **đang là ảnh cuối cùng** → cho phép xóa.
  - Nếu **còn ảnh khác** → **không cho xóa** (409). FE phải gọi **UPDATE** set ảnh khác primary trước.

---

## ProductImageResponseDto (data trả về)

```ts
interface ProductImageResponseDto {
  id: number;
  productId: number;
  imageUrl: string;
  altText: string | null;
  isPrimary: boolean;
}
```

