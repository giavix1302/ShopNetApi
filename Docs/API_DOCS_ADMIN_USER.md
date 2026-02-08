# API Documentation - Admin User

## Base URL
```
/api/admin/users
```

**Authentication:** Tất cả endpoints đều yêu cầu Bearer Token + Role `Admin`

---

## 1. Get Users (Admin list + filter/search/sort + pagination)
`GET /api/admin/users`

**Query Parameters (optional):**
- `email`: string (contains, case-insensitive)
- `fullName`: string (contains, case-insensitive)
- `enabled`: boolean (true/false)
- `from`: datetime (lọc theo `CreatedAt >= from`)
- `to`: datetime (lọc theo `CreatedAt <= to`)
- `sortBy`: `createdAt` | `email` | `fullName` (default: `createdAt`)
- `sortDir`: `asc` | `desc` (default: `desc`)
- `page`: int (>= 1, default: 1)
- `pageSize`: int (1..200, default: 20)

**Example:**
`GET /api/admin/users?email=user@example.com&enabled=true&page=1&pageSize=20&sortBy=createdAt&sortDir=desc`

**Response (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách người dùng thành công",
  "data": {
    "items": [
      {
        "id": 1,
        "email": "user@example.com",
        "fullName": "Nguyễn Văn A",
        "phoneNumber": "0123456789",
        "address": "12 Nguyễn Trãi, Q1, TP.HCM",
        "avatarUrl": "https://example.com/avatar.jpg",
        "enabled": true,
        "createdAt": "2026-01-15T10:00:00Z",
        "orderCount": 5,
        "reviewCount": 3,
        "roles": ["User"]
      },
      {
        "id": 2,
        "email": "admin@example.com",
        "fullName": "Admin User",
        "phoneNumber": null,
        "address": null,
        "avatarUrl": null,
        "enabled": true,
        "createdAt": "2026-01-10T08:00:00Z",
        "orderCount": 0,
        "reviewCount": 0,
        "roles": ["Admin"]
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalItems": 2,
    "totalPages": 1
  }
}
```

**Lưu ý:**
- `orderCount`: Tổng số đơn hàng của user
- `reviewCount`: Tổng số review của user
- `roles`: Danh sách roles của user (ví dụ: ["User"], ["Admin"], ["User", "Admin"])

---

## 2. Get User Detail (Admin)
`GET /api/admin/users/{userId}`

**Path Parameters:**
- `userId` (long): ID người dùng

**Response (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin người dùng thành công",
  "data": {
    "id": 1,
    "email": "user@example.com",
    "userName": "user123",
    "fullName": "Nguyễn Văn A",
    "phoneNumber": "0123456789",
    "address": "12 Nguyễn Trãi, Q1, TP.HCM",
    "note": "Khách hàng VIP",
    "avatarUrl": "https://example.com/avatar.jpg",
    "enabled": true,
    "emailConfirmed": true,
    "phoneNumberConfirmed": true,
    "createdAt": "2026-01-15T10:00:00Z",
    "roles": ["User"],
    "orderCount": 2,
    "orders": [
      {
        "id": 10,
        "userId": 1,
        "orderNumber": "ORD-20260128010101-1234",
        "totalAmount": 199.98,
        "status": "DELIVERED",
        "shippingAddress": "12 Nguyễn Trãi, Q1, TP.HCM",
        "paymentMethod": "COD",
        "paymentStatus": "PAID",
        "createdAt": "2026-01-28T01:01:01Z",
        "updatedAt": "2026-01-30T10:00:00Z",
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
          },
          {
            "id": 1001,
            "status": "SHIPPED",
            "location": "Kho HCM",
            "description": "Đã bàn giao cho đơn vị vận chuyển",
            "note": "Giao dự kiến trong 2 ngày",
            "trackingNumber": "VN123456789",
            "shippingPattern": "GHN",
            "estimatedDelivery": "2026-01-30T12:00:00Z",
            "createdAt": "2026-01-29T08:00:00Z",
            "updatedAt": "2026-01-29T08:00:00Z"
          },
          {
            "id": 1002,
            "status": "DELIVERED",
            "location": "TP.HCM",
            "description": "Đã giao hàng thành công",
            "note": null,
            "trackingNumber": "VN123456789",
            "shippingPattern": "GHN",
            "estimatedDelivery": null,
            "createdAt": "2026-01-30T10:00:00Z",
            "updatedAt": "2026-01-30T10:00:00Z"
          }
        ]
      },
      {
        "id": 11,
        "userId": 1,
        "orderNumber": "ORD-20260125020202-5678",
        "totalAmount": 299.99,
        "status": "PENDING",
        "shippingAddress": "12 Nguyễn Trãi, Q1, TP.HCM",
        "paymentMethod": "MOMO",
        "paymentStatus": "PENDING",
        "createdAt": "2026-01-25T14:30:00Z",
        "updatedAt": "2026-01-25T14:30:00Z",
        "items": [
          {
            "id": 101,
            "productId": 8,
            "productName": "Adidas Ultraboost",
            "productSlug": "adidas-ultraboost",
            "colorId": 1,
            "colorName": "White",
            "colorHexCode": "#FFFFFF",
            "quantity": 1,
            "unitPrice": 299.99,
            "subtotal": 299.99
          }
        ],
        "trackings": [
          {
            "id": 1003,
            "status": "PENDING",
            "location": null,
            "description": "Đơn hàng đã được tạo",
            "note": null,
            "trackingNumber": null,
            "shippingPattern": null,
            "estimatedDelivery": null,
            "createdAt": "2026-01-25T14:30:00Z",
            "updatedAt": "2026-01-25T14:30:00Z"
          }
        ]
      }
    ],
    "reviewCount": 2,
    "reviews": [
      {
        "id": 50,
        "userId": 1,
        "userName": "Nguyễn Văn A",
        "userAvatarUrl": "https://example.com/avatar.jpg",
        "productId": 5,
        "productName": "Nike Air Max 90",
        "productSlug": "nike-air-max-90",
        "orderItemId": 100,
        "rating": 5,
        "comment": "Sản phẩm rất tốt, đúng như mô tả. Giao hàng nhanh!",
        "createdAt": "2026-01-31T09:00:00Z",
        "updatedAt": "2026-01-31T09:00:00Z"
      },
      {
        "id": 51,
        "userId": 1,
        "userName": "Nguyễn Văn A",
        "userAvatarUrl": "https://example.com/avatar.jpg",
        "productId": 8,
        "productName": "Adidas Ultraboost",
        "productSlug": "adidas-ultraboost",
        "orderItemId": null,
        "rating": 4,
        "comment": "Sản phẩm tốt nhưng giá hơi cao",
        "createdAt": "2026-01-20T15:30:00Z",
        "updatedAt": "2026-01-20T15:30:00Z"
      }
    ]
  }
}
```

**Lưu ý:**
- `orders`: Danh sách tất cả đơn hàng của user, sắp xếp theo `createdAt` giảm dần (mới nhất trước)
- Mỗi order bao gồm đầy đủ `items` và `trackings`
- `reviews`: Danh sách tất cả review của user, sắp xếp theo `createdAt` giảm dần (mới nhất trước)
- `orderCount`: Số lượng đơn hàng (bằng `orders.length`)
- `reviewCount`: Số lượng review (bằng `reviews.length`)

---

## Error Codes

- `400`: Bad Request
  - Validation errors (query/page/pageSize)
  - Invalid sortBy/sortDir values
- `401`: Unauthorized (thiếu hoặc token không hợp lệ)
- `403`: Forbidden (không phải Admin)
- `404`: Not Found (user không tồn tại)
- `500`: Internal Server Error

---

## Ví dụ sử dụng

### Get Users với filter
```javascript
// Lấy danh sách users đã enabled, sắp xếp theo email
const response = await fetch('https://api.example.com/api/admin/users?enabled=true&sortBy=email&sortDir=asc&page=1&pageSize=20', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
});

const data = await response.json();
if (data.success) {
  console.log('Users:', data.data.items);
  console.log('Total:', data.data.totalItems);
}
```

### Get User Detail
```javascript
// Lấy thông tin chi tiết user kèm orders và reviews
const userId = 1;
const response = await fetch(`https://api.example.com/api/admin/users/${userId}`, {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
});

const data = await response.json();
if (data.success) {
  const user = data.data;
  console.log('User:', user.email);
  console.log('Orders:', user.orders);
  console.log('Reviews:', user.reviews);
  console.log('Order Count:', user.orderCount);
  console.log('Review Count:', user.reviewCount);
}
```

### Search Users
```javascript
// Tìm kiếm users theo email hoặc fullName
const searchTerm = 'user@example.com';
const response = await fetch(`https://api.example.com/api/admin/users?email=${encodeURIComponent(searchTerm)}`, {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
});

const data = await response.json();
if (data.success) {
  console.log('Search results:', data.data.items);
}
```

### Filter Users by Date Range
```javascript
// Lấy users đăng ký trong khoảng thời gian
const from = '2026-01-01T00:00:00Z';
const to = '2026-01-31T23:59:59Z';
const response = await fetch(`https://api.example.com/api/admin/users?from=${from}&to=${to}`, {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
});

const data = await response.json();
if (data.success) {
  console.log('Users in date range:', data.data.items);
}
```
