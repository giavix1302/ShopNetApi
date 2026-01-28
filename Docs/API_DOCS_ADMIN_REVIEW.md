# API Documentation - Admin Review

## Base URL
```
/api/admin/reviews
```

**Authentication:** Tất cả endpoints đều yêu cầu Bearer Token + Role `Admin`

---

## 1. Get Reviews (Admin list + filter/search/sort + pagination)
`GET /api/admin/reviews`

**Query Parameters (optional):**
- `productId`: long - Filter theo sản phẩm
- `userId`: long - Filter theo user
- `rating`: int (1-5) - Filter theo rating
- `from`: datetime - Lọc theo `CreatedAt >= from`
- `to`: datetime - Lọc theo `CreatedAt <= to`
- `sortBy`: `createdAt` | `rating` | `updatedAt` (default: `createdAt`)
- `sortDir`: `asc` | `desc` (default: `desc`)
- `page`: int (>= 1, default: 1)
- `pageSize`: int (1-200, default: 20)

**Example:**
`GET /api/admin/reviews?productId=5&rating=5&page=1&pageSize=20&sortBy=createdAt&sortDir=desc`

**Response (200):**
```json
{
  "success": true,
  "message": "Lấy danh sách review thành công",
  "data": {
    "items": [
      {
        "id": 50,
        "userId": 1,
        "userEmail": "user@example.com",
        "userName": "Nguyễn Văn A",
        "productId": 5,
        "productName": "Nike Air Max 90",
        "orderItemId": 100,
        "rating": 5,
        "comment": "Sản phẩm rất tốt, đóng gói cẩn thận, giao hàng nhanh!",
        "createdAt": "2026-01-28T10:30:00Z",
        "updatedAt": "2026-01-28T10:30:00Z"
      },
      {
        "id": 51,
        "userId": 2,
        "userEmail": "user2@example.com",
        "userName": "Trần Thị B",
        "productId": 5,
        "productName": "Nike Air Max 90",
        "orderItemId": 101,
        "rating": 4,
        "comment": "Sản phẩm đẹp, chất lượng tốt",
        "createdAt": "2026-01-27T15:20:00Z",
        "updatedAt": "2026-01-27T15:20:00Z"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalItems": 2,
    "totalPages": 1
  }
}
```

---

## 2. Get Review Detail (Admin)
`GET /api/admin/reviews/{reviewId}`

**Path Parameters:**
- `reviewId` (long): ID review

**Response (200):**
```json
{
  "success": true,
  "message": "Lấy thông tin review thành công",
  "data": {
    "id": 50,
    "userId": 1,
    "userEmail": "user@example.com",
    "userName": "Nguyễn Văn A",
    "userAvatarUrl": "https://example.com/avatar.jpg",
    "productId": 5,
    "productName": "Nike Air Max 90",
    "productSlug": "nike-air-max-90",
    "orderItemId": 100,
    "rating": 5,
    "comment": "Sản phẩm rất tốt, đóng gói cẩn thận, giao hàng nhanh!",
    "createdAt": "2026-01-28T10:30:00Z",
    "updatedAt": "2026-01-28T10:30:00Z"
  }
}
```

---

## 3. Delete Review (Admin)
`DELETE /api/admin/reviews/{reviewId}`

**Path Parameters:**
- `reviewId` (long): ID review

**Response (200):**
```json
{
  "success": true,
  "message": "Xóa review thành công",
  "data": null
}
```

**Rules:**
- Admin có thể xóa bất kỳ review nào
- Sau khi xóa, `OrderItem.IsReviewed` sẽ được set = false (nếu có orderItemId)

---

## 4. Get Review Statistics (Thống kê reviews tổng quan)
`GET /api/admin/reviews/stats`

**Query Parameters (optional):**
- `from`: datetime - Lọc theo `CreatedAt >= from`
- `to`: datetime - Lọc theo `CreatedAt <= to`

**Example:**
`GET /api/admin/reviews/stats?from=2026-01-01T00:00:00Z&to=2026-01-31T23:59:59Z`

**Response (200):**
```json
{
  "success": true,
  "message": "Lấy thống kê review thành công",
  "data": {
    "totalReviews": 1250,
    "averageRating": 4.32,
    "ratingDistribution": {
      "1": 25,
      "2": 50,
      "3": 250,
      "4": 400,
      "5": 525
    },
    "percentageDistribution": {
      "1": 2.00,
      "2": 4.00,
      "3": 20.00,
      "4": 32.00,
      "5": 42.00
    },
    "reviewsToday": 15,
    "reviewsThisWeek": 85,
    "reviewsThisMonth": 320
  }
}
```

**Notes:**
- `totalReviews`: Tổng số reviews (có thể filter theo from/to)
- `averageRating`: Điểm trung bình, làm tròn 2 chữ số thập phân
- `ratingDistribution`: Số lượng reviews theo từng rating (1-5)
- `percentageDistribution`: Tỷ lệ phần trăm reviews theo từng rating (1-5)
- `reviewsToday`: Số reviews trong ngày hôm nay
- `reviewsThisWeek`: Số reviews trong tuần này (từ đầu tuần)
- `reviewsThisMonth`: Số reviews trong tháng này (từ đầu tháng)

---

## Error Codes

- `400`: Bad Request
  - Validation errors (query parameters)

- `401`: Unauthorized
  - Chưa đăng nhập
  - Token không hợp lệ hoặc đã hết hạn

- `403`: Forbidden
  - Không có quyền Admin

- `404`: Not Found
  - Review không tồn tại

- `500`: Internal Server Error
  - Lỗi server khi xử lý request

---

## Business Rules Summary

1. **Admin Access**: Chỉ Admin mới có thể truy cập các endpoints này

2. **Delete Permission**: Admin có thể xóa bất kỳ review nào, không cần kiểm tra ownership

3. **OrderItem Tracking**: Khi Admin xóa review, `OrderItem.IsReviewed` sẽ được set = false (nếu có orderItemId)

4. **Statistics**: Thống kê có thể filter theo khoảng thời gian (from/to)

5. **Filtering**: Có thể filter theo productId, userId, rating, và date range

6. **Sorting**: Có thể sort theo createdAt, rating, hoặc updatedAt
