# API Documentation - Order (User)

## Base URL
```
/api/orders
```

**Authentication:** Tất cả endpoints đều yêu cầu Bearer Token

---

## Enums

### OrderStatus
- `PENDING`
- `PROCESSING`
- `SHIPPED`
- `DELIVERED`
- `CANCELLED`

### PaymentMethod
- `COD`
- `MOMO`
- `BANK`

### PaymentStatus
- `PENDING`
- `PAID`
- `FAILED`
- `REFUNDED`

---

## 1. Create Order (Tạo đơn hàng từ giỏ hàng)
`POST /api/orders`

**Request:**
```json
{
  "shippingAddress": "12 Nguyễn Trãi, Q1, TP.HCM",
  "paymentMethod": "COD"
}
```

**Validation:**
- `shippingAddress`: Required, max 500 ký tự
- `paymentMethod`: Required, chỉ chấp nhận: `COD`, `MOMO`, `BANK`

**Success Response (200):**
```json
{
  "success": true,
  "message": "Tạo đơn hàng thành công",
  "data": {
    "id": 10,
    "userId": 1,
    "orderNumber": "ORD-20260128010101-1234",
    "totalAmount": 199.98,
    "status": "PENDING",
    "shippingAddress": "12 Nguyễn Trãi, Q1, TP.HCM",
    "paymentMethod": "COD",
    "paymentStatus": "PENDING",
    "createdAt": "2026-01-28T01:01:01Z",
    "updatedAt": "2026-01-28T01:01:01Z",
    "items": [
      {
        "id": 100,
        "productId": 5,
        "productName": "Nike Air Max 90",
        "productSlug": "nike-air-max-90",
        "colorId": 2,
        "colorName": "Black",
        "colorHexCode": "#000000",
        "quantity": 2,
        "unitPrice": 99.99,
        "subtotal": 199.98
      }
    ],
    "trackings": [
      {
        "id": 1000,
        "status": "PENDING",
        "location": null,
        "description": "Đơn hàng đã được tạo",
        "note": null,
        "trackingNumber": null,
        "shippingPattern": null,
        "estimatedDelivery": null,
        "createdAt": "2026-01-28T01:01:01Z",
        "updatedAt": "2026-01-28T01:01:01Z"
      }
    ]
  }
}
```

**Notes:**
- Tạo đơn hàng dựa trên items trong cart hiện tại.
- Nếu cart trống sẽ trả về lỗi `400`.
- Service sẽ validate stock và trừ stock khi tạo order.

---

## 2. Get My Orders (Lấy danh sách đơn hàng của tôi)
`GET /api/orders/me`

**Response (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách đơn hàng thành công",
  "data": [
    {
      "id": 10,
      "orderNumber": "ORD-20260128010101-1234",
      "totalAmount": 199.98,
      "status": "PENDING",
      "createdAt": "2026-01-28T01:01:01Z",
      "itemCount": 2
    }
  ]
}
```

---

## 3. Get My Order By Id (Lấy chi tiết đơn hàng của tôi)
`GET /api/orders/me/{orderId}`

**Path Parameters:**
- `orderId` (long): ID đơn hàng

**Response (200):** giống response của endpoint Create Order (bao gồm `items` + `trackings`)

---

## 4. Cancel Order (Hủy đơn hàng)
`PUT /api/orders/{orderId}/cancel`

**Rules:**
- Chỉ hủy được khi `status = PENDING`
- Chỉ hủy được đơn hàng thuộc user hiện tại

**Response (200):**
```json
{
  "success": true,
  "message": "Hủy đơn hàng thành công",
  "data": null
}
```

---

## 5. Get Order Tracking (Xem tracking đơn hàng)
`GET /api/orders/{orderId}/tracking`

**Response (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin tracking thành công",
  "data": [
    {
      "id": 1000,
      "status": "PENDING",
      "location": null,
      "description": "Đơn hàng đã được tạo",
      "note": null,
      "trackingNumber": null,
      "shippingPattern": null,
      "estimatedDelivery": null,
      "createdAt": "2026-01-28T01:01:01Z",
      "updatedAt": "2026-01-28T01:01:01Z"
    }
  ]
}
```

---

## Error Codes

- `400`: Bad Request
  - Cart trống
  - Stock không đủ / product không active
  - Hủy đơn không phải trạng thái `PENDING`
  - Validation errors (shippingAddress/paymentMethod)
- `401`: Unauthorized (chưa đăng nhập)
- `403`: Forbidden (không có quyền truy cập đơn hàng/tracking)
- `404`: Not Found (đơn hàng không tồn tại)
- `500`: Internal Server Error

