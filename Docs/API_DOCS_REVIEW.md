# API Documentation - Review

## Base URLs
```
User Endpoints: /api/reviews
Public Endpoints: /api/products/{productId}/reviews
```

**Authentication:** 
- User endpoints yêu cầu Bearer Token
- Public endpoints không cần authentication

---

## 1. Create Review (Tạo review)
`POST /api/reviews`

**Authentication:** Required (Bearer Token)

**Request:**
```json
{
  "productId": 5,
  "orderItemId": 100,
  "rating": 5,
  "comment": "Sản phẩm rất tốt, đóng gói cẩn thận, giao hàng nhanh!"
}
```

**Validation:**
- `productId`: Required, long
- `orderItemId`: Optional, long (khuyến nghị cung cấp để tracking)
- `rating`: Required, Range 1-5
- `comment`: Optional, max 1000 ký tự

**Success Response (200):**
```json
{
  "success": true,
  "message": "Tạo review thành công",
  "data": {
    "id": 50,
    "userId": 1,
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

**Rules:**
- Chỉ có thể review sản phẩm đã mua (Order.Status = DELIVERED)
- Mỗi user chỉ có thể review 1 product 1 lần (unique constraint)
- Nếu `orderItemId` được cung cấp:
  - OrderItem phải thuộc về user hiện tại
  - Order phải có Status = DELIVERED
  - OrderItem.IsReviewed phải = false
- Nếu `orderItemId` không được cung cấp, hệ thống sẽ kiểm tra xem user đã mua product chưa

**Notes:**
- Sau khi tạo review thành công, `OrderItem.IsReviewed` sẽ được set = true (nếu có orderItemId)
- Comment có thể để trống, chỉ cần rating

---

## 2. Get My Reviews (Lấy danh sách reviews của tôi)
`GET /api/reviews/me`

**Authentication:** Required (Bearer Token)

**Query Parameters (optional):**
- `rating`: int (1-5) - Filter theo rating
- `sortBy`: `createdAt` | `rating` (default: `createdAt`)
- `sortDir`: `asc` | `desc` (default: `desc`)
- `page`: int (>= 1, default: 1)
- `pageSize`: int (1-200, default: 20)

**Example:**
`GET /api/reviews/me?rating=5&page=1&pageSize=20&sortBy=createdAt&sortDir=desc`

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
        "userName": "Nguyễn Văn A",
        "userAvatarUrl": "https://example.com/avatar.jpg",
        "productId": 5,
        "productName": "Nike Air Max 90",
        "productSlug": "nike-air-max-90",
        "rating": 5,
        "comment": "Sản phẩm rất tốt, đóng gói cẩn thận, giao hàng nhanh!",
        "createdAt": "2026-01-28T10:30:00Z",
        "updatedAt": "2026-01-28T10:30:00Z"
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

## 3. Get My Review By Id (Lấy chi tiết review của tôi)
`GET /api/reviews/me/{reviewId}`

**Authentication:** Required (Bearer Token)

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

**Rules:**
- Chỉ có thể xem review của chính mình

---

## 4. Update Review (Cập nhật review)
`PUT /api/reviews/{reviewId}`

**Authentication:** Required (Bearer Token)

**Request:**
```json
{
  "rating": 4,
  "comment": "Sản phẩm tốt nhưng có một số điểm cần cải thiện"
}
```

**Validation:**
- `rating`: Optional, Range 1-5
- `comment`: Optional, max 1000 ký tự
- Ít nhất một trong hai field phải được cung cấp

**Response (200):**
```json
{
  "success": true,
  "message": "Cập nhật review thành công",
  "data": {
    "id": 50,
    "userId": 1,
    "userName": "Nguyễn Văn A",
    "userAvatarUrl": "https://example.com/avatar.jpg",
    "productId": 5,
    "productName": "Nike Air Max 90",
    "productSlug": "nike-air-max-90",
    "orderItemId": 100,
    "rating": 4,
    "comment": "Sản phẩm tốt nhưng có một số điểm cần cải thiện",
    "createdAt": "2026-01-28T10:30:00Z",
    "updatedAt": "2026-01-28T11:00:00Z"
  }
}
```

**Rules:**
- Chỉ có thể cập nhật review của chính mình
- `UpdatedAt` sẽ được tự động cập nhật

---

## 5. Delete Review (Xóa review)
`DELETE /api/reviews/{reviewId}`

**Authentication:** Required (Bearer Token)

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
- Chỉ có thể xóa review của chính mình
- Sau khi xóa, `OrderItem.IsReviewed` sẽ được set = false (nếu có orderItemId)

---

## 6. Get Product Reviews (Lấy danh sách reviews của sản phẩm) - PUBLIC
`GET /api/products/{productId}/reviews`

**Authentication:** Not required

**Path Parameters:**
- `productId` (long): ID sản phẩm

**Query Parameters (optional):**
- `rating`: int (1-5) - Filter theo rating
- `sortBy`: `createdAt` | `rating` (default: `createdAt`)
- `sortDir`: `asc` | `desc` (default: `desc`)
- `page`: int (>= 1, default: 1)
- `pageSize`: int (1-200, default: 20)

**Example:**
`GET /api/products/5/reviews?rating=5&page=1&pageSize=20&sortBy=rating&sortDir=desc`

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
        "userName": "Nguyễn Văn A",
        "userAvatarUrl": "https://example.com/avatar.jpg",
        "productId": 5,
        "productName": "Nike Air Max 90",
        "productSlug": "nike-air-max-90",
        "rating": 5,
        "comment": "Sản phẩm rất tốt, đóng gói cẩn thận, giao hàng nhanh!",
        "createdAt": "2026-01-28T10:30:00Z",
        "updatedAt": "2026-01-28T10:30:00Z"
      },
      {
        "id": 51,
        "userId": 2,
        "userName": "Trần Thị B",
        "userAvatarUrl": null,
        "productId": 5,
        "productName": "Nike Air Max 90",
        "productSlug": "nike-air-max-90",
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

**Notes:**
- Endpoint này là public, không cần authentication
- Response không bao gồm `orderItemId` để bảo mật thông tin đơn hàng

---

## 7. Get Product Review Statistics (Lấy thống kê reviews của sản phẩm) - PUBLIC
`GET /api/products/{productId}/reviews/stats`

**Authentication:** Not required

**Path Parameters:**
- `productId` (long): ID sản phẩm

**Response (200):**
```json
{
  "success": true,
  "message": "Lấy thống kê review thành công",
  "data": {
    "totalReviews": 25,
    "averageRating": 4.32,
    "ratingDistribution": {
      "1": 1,
      "2": 2,
      "3": 5,
      "4": 8,
      "5": 9
    },
    "percentageDistribution": {
      "1": 4.00,
      "2": 8.00,
      "3": 20.00,
      "4": 32.00,
      "5": 36.00
    }
  }
}
```

**Notes:**
- `averageRating`: Điểm trung bình, làm tròn 2 chữ số thập phân
- `ratingDistribution`: Số lượng reviews theo từng rating (1-5)
- `percentageDistribution`: Tỷ lệ phần trăm reviews theo từng rating (1-5), làm tròn 2 chữ số thập phân
- Nếu không có review nào, tất cả giá trị sẽ là 0

---

## Error Codes

### User Endpoints (`/api/reviews`)

- `400`: Bad Request
  - Product không tồn tại hoặc không active
  - Chưa mua sản phẩm (Order.Status != DELIVERED)
  - OrderItem không thuộc về user hiện tại
  - OrderItem đã được review rồi (IsReviewed = true)
  - OrderItem không khớp với ProductId
  - Validation errors (rating, comment)
  - Đã review sản phẩm này rồi (unique constraint)

- `401`: Unauthorized
  - Chưa đăng nhập
  - Token không hợp lệ hoặc đã hết hạn

- `403`: Forbidden
  - Review không thuộc về user hiện tại
  - OrderItem không thuộc về user hiện tại

- `404`: Not Found
  - Review không tồn tại
  - Product không tồn tại
  - OrderItem không tồn tại

- `409`: Conflict
  - Đã review sản phẩm này rồi (unique constraint violation)

- `500`: Internal Server Error
  - Lỗi server khi tạo/cập nhật/xóa review

### Public Endpoints (`/api/products/{productId}/reviews`)

- `400`: Bad Request
  - Validation errors (query parameters)

- `404`: Not Found
  - Product không tồn tại

- `500`: Internal Server Error
  - Lỗi server khi lấy reviews hoặc statistics

---

## Business Rules Summary

1. **Purchase Requirement**: Chỉ có thể review sản phẩm đã mua (Order.Status = DELIVERED)

2. **Unique Constraint**: Mỗi user chỉ có thể review 1 product 1 lần

3. **OrderItem Tracking**: 
   - Khi tạo review với `orderItemId`, `OrderItem.IsReviewed` sẽ được set = true
   - Khi xóa review, `OrderItem.IsReviewed` sẽ được set = false (nếu có orderItemId)

4. **Rating Range**: Rating phải từ 1 đến 5

5. **Comment Length**: Comment tối đa 1000 ký tự

6. **Ownership**: User chỉ có thể xem/cập nhật/xóa review của chính mình

7. **Public Endpoints**: Không hiển thị `orderItemId` để bảo mật thông tin đơn hàng

---

## Example Usage Flow

1. User mua sản phẩm và đơn hàng được giao (Status = DELIVERED)
2. User gọi `POST /api/reviews` với `productId` và `orderItemId` để tạo review
3. Hệ thống validate:
   - User đã mua sản phẩm
   - OrderItem chưa được review
   - User chưa review sản phẩm này
4. Review được tạo và `OrderItem.IsReviewed` = true
5. Public có thể xem reviews qua `GET /api/products/{productId}/reviews`
6. User có thể cập nhật/xóa review của mình
