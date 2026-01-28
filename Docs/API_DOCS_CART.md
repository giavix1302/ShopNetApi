# API Documentation - Cart CRUD

## Base URL
```
/api/carts
```

**Authentication:** Tất cả endpoints đều yêu cầu Bearer Token

---

## 1. Get My Cart
`GET /api/carts/me`

**Response:**
```json
{
  "success": true,
  "message": "Lấy giỏ hàng thành công",
  "data": {
    "id": 1,
    "userId": 1,
    "createdAt": "2024-01-15T10:30:00Z",
    "items": [
      {
        "id": 1,
        "productId": 5,
        "productName": "Nike Air Max 90",
        "productSlug": "nike-air-max-90",
        "colorId": 2,
        "colorName": "Black",
        "colorHexCode": "#000000",
        "quantity": 2,
        "unitPrice": 99.99,
        "totalPrice": 199.98
      }
    ],
    "totalAmount": 199.98,
    "totalItems": 2
  }
}
```

---

## 2. Add Item to Cart
`POST /api/carts/items`

**Request:**
```json
{
  "productId": 5,
  "colorId": 2,
  "quantity": 2
}
```

**Validation:**
- `productId`: Required
- `colorId`: Optional (nếu có phải thuộc product)
- `quantity`: Required, > 0, <= StockQuantity

**Response:**
```json
{
  "success": true,
  "message": "Thêm sản phẩm vào giỏ hàng thành công",
  "data": {
    "id": 1,
    "productId": 5,
    "productName": "Nike Air Max 90",
    "quantity": 2,
    "unitPrice": 99.99,
    "totalPrice": 199.98
  }
}
```

**Note:** Nếu item đã tồn tại → cộng dồn quantity

---

## 3. Update Cart Item
`PUT /api/carts/items/{itemId}`

**Request:**
```json
{
  "quantity": 3
}
```

**Validation:**
- `quantity`: Required, > 0, <= StockQuantity

**Response:**
```json
{
  "success": true,
  "message": "Cập nhật sản phẩm trong giỏ hàng thành công",
  "data": {
    "id": 1,
    "productId": 5,
    "quantity": 3,
    "unitPrice": 99.99,
    "totalPrice": 299.97
  }
}
```

---

## 4. Delete Cart Item
`DELETE /api/carts/items/{itemId}`

**Response:**
```json
{
  "success": true,
  "message": "Xóa sản phẩm khỏi giỏ hàng thành công",
  "data": null
}
```

---

## 5. Clear Cart
`DELETE /api/carts/clear`

**Response:**
```json
{
  "success": true,
  "message": "Xóa toàn bộ giỏ hàng thành công",
  "data": null
}
```

**Note:** Xóa cart và tất cả items (không tạo cart mới)

---

## Error Codes

- `400`: Bad Request (validation, stock không đủ, product không active)
- `401`: Unauthorized (chưa đăng nhập)
- `403`: Forbidden (không có quyền truy cập)
- `404`: Not Found (product/item không tồn tại)
- `500`: Internal Server Error
