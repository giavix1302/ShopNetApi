# API Documentation - Admin Order

## Base URL
```
/api/admin/orders
```

**Authentication:** Tất cả endpoints đều yêu cầu Bearer Token + Role `Admin`

---

## Enums

### OrderStatus
- `PENDING`
- `PROCESSING`
- `SHIPPED`
- `DELIVERED`
- `CANCELLED`

**Rule (Admin):** Admin **không được** cập nhật `status = CANCELLED` (API sẽ trả `400`).

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

## 1. Get Orders (Admin list + filter/search/sort + pagination)
`GET /api/admin/orders`

**Query Parameters (optional):**
- `status`: `OrderStatus`
- `paymentStatus`: `PaymentStatus`
- `paymentMethod`: `PaymentMethod`
- `userId`: long
- `orderNumber`: string (contains)
- `from`: datetime (lọc theo `CreatedAt >= from`)
- `to`: datetime (lọc theo `CreatedAt <= to`)
- `minTotal`: decimal
- `maxTotal`: decimal
- `sortBy`: `createdAt` | `totalAmount` | `status` (default: `createdAt`)
- `sortDir`: `asc` | `desc` (default: `desc`)
- `page`: int (>= 1, default: 1)
- `pageSize`: int (1..200, default: 20)

**Example:**
`GET /api/admin/orders?status=PENDING&page=1&pageSize=20&sortBy=createdAt&sortDir=desc`

**Response (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách đơn hàng thành công",
  "data": {
    "items": [
      {
        "id": 10,
        "orderNumber": "ORD-20260128010101-1234",
        "userId": 1,
        "userEmail": "user@example.com",
        "totalAmount": 199.98,
        "status": "PENDING",
        "paymentMethod": "COD",
        "paymentStatus": "PENDING",
        "createdAt": "2026-01-28T01:01:01Z",
        "updatedAt": "2026-01-28T01:01:01Z",
        "itemCount": 2
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalItems": 1,
    "totalPages": 1
  }
}
```

---

## 2. Get Order Detail (Admin)
`GET /api/admin/orders/{orderId}`

**Path Parameters:**
- `orderId` (long): ID đơn hàng

**Response (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin đơn hàng thành công",
  "data": {
    "id": 10,
    "userId": 1,
    "userEmail": "user@example.com",
    "userName": "Nguyễn Văn A",
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

---

## 3. Update Order Status (Admin)
`PUT /api/admin/orders/{orderId}/status`

**Request:**
```json
{
  "status": "PROCESSING"
}
```

**Validation / Rules:**
- `status`: Required (enum `OrderStatus`)
- **Admin không được set `CANCELLED`**
- Workflow hợp lệ:
  - `PENDING` → `PROCESSING`
  - `PROCESSING` → `SHIPPED`
  - `SHIPPED` → `DELIVERED`

**Response (200):**
```json
{
  "success": true,
  "message": "Cập nhật trạng thái đơn hàng thành công",
  "data": null
}
```

**Note:** Khi update status, hệ thống sẽ tự thêm 1 tracking entry.

---

## 4. Update Order Payment (Admin)
`PUT /api/admin/orders/{orderId}/payment`

**Request:**
```json
{
  "paymentMethod": "MOMO",
  "paymentStatus": "PAID"
}
```

**Validation:**
- `paymentStatus`: Required (enum `PaymentStatus`)
- `paymentMethod`: Optional (enum `PaymentMethod`)

**Response (200):**
```json
{
  "success": true,
  "message": "Cập nhật thanh toán đơn hàng thành công",
  "data": null
}
```

---

## 5. Add Tracking (Admin)
`POST /api/admin/orders/{orderId}/tracking`

**Request:**
```json
{
  "status": "SHIPPED",
  "location": "Kho HCM",
  "description": "Đã bàn giao cho đơn vị vận chuyển",
  "note": null,
  "trackingNumber": "VN123456789",
  "shippingPattern": "GHN",
  "estimatedDelivery": "2026-01-30T12:00:00Z"
}
```

**Validation:**
- `status`: Required (enum `OrderStatus`)
- `location`: Optional, max 255
- `description`: Optional, max 1000
- `note`: Optional, max 1000
- `trackingNumber`: Optional, max 100
- `shippingPattern`: Optional, max 100

**Response (200):**
```json
{
  "success": true,
  "message": "Thêm tracking thành công",
  "data": {
    "id": 1001
  }
}
```

---

## 6. Update Tracking (Admin)
`PUT /api/admin/orders/{orderId}/tracking/{trackingId}`

**Request:**
```json
{
  "status": "SHIPPED",
  "location": "Kho HCM",
  "description": "Cập nhật mô tả",
  "note": "Giao dự kiến trễ 1 ngày",
  "trackingNumber": "VN123456789",
  "shippingPattern": "GHN",
  "estimatedDelivery": "2026-01-31T12:00:00Z"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Cập nhật tracking thành công",
  "data": null
}
```

---

## 7. Delete Tracking (Admin)
`DELETE /api/admin/orders/{orderId}/tracking/{trackingId}`

**Response (200):**
```json
{
  "success": true,
  "message": "Xóa tracking thành công",
  "data": null
}
```

---

## 8. Get Order Stats (Admin)
`GET /api/admin/orders/stats`

**Query Parameters (optional):**
- `from`: datetime
- `to`: datetime

**Response (200):**
```json
{
  "success": true,
  "message": "Lấy thống kê đơn hàng thành công",
  "data": {
    "from": "2026-01-01T00:00:00Z",
    "to": "2026-01-31T23:59:59Z",
    "totalOrders": 120,
    "totalRevenue": 25000000,
    "countByStatus": {
      "PENDING": 10,
      "PROCESSING": 15,
      "SHIPPED": 20,
      "DELIVERED": 70,
      "CANCELLED": 5
    }
  }
}
```

---

## Error Codes

- `400`: Bad Request
  - Invalid workflow status transition
  - Admin set `status = CANCELLED`
  - Tracking không thuộc order
  - Validation errors (query/page/pageSize/body)
- `401`: Unauthorized
- `403`: Forbidden (không phải Admin)
- `404`: Not Found (order/tracking không tồn tại)
- `500`: Internal Server Error

