# API Documentation - Authentication

## Base URL
```
/api/auth
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

## 1. Register (Đăng ký)

**Endpoint:** `POST /api/auth/register`

**Authentication:** Không cần

**Request Body:**
```json
{
  "email": "user@example.com",
  "fullName": "Nguyễn Văn A"
}
```

**Validation:**
- `email`: Required, phải đúng định dạng email
- `fullName`: Required

**Success Response (200):**
```json
{
  "success": true,
  "message": "OTP đã được gửi tới email. Vui lòng xác thực",
  "data": null
}
```

**Error Responses:**
- `400`: Email đã tồn tại hoặc dữ liệu không hợp lệ
- `400`: Validation errors
  ```json
  {
    "success": false,
    "message": "Dữ liệu không hợp lệ",
    "data": [
      { "field": "email", "message": "Email không đúng định dạng" }
    ]
  }
  ```

---

## 2. Verify Register OTP (Xác thực OTP đăng ký)

**Endpoint:** `POST /api/auth/verify-register-otp`

**Authentication:** Không cần

**Request Body:**
```json
{
  "email": "user@example.com",
  "otp": "123456",
  "password": "password123"
}
```

**Validation:**
- `email`: Required
- `otp`: Required
- `password`: Required, tối thiểu 6 ký tự

**Success Response (200):**
```json
{
  "success": true,
  "message": "Đăng ký & đăng nhập thành công",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Error Responses:**
- `400`: OTP không hợp lệ hoặc đã hết hạn
- `400`: Validation errors

**Lưu ý:** Sau khi verify thành công, refresh token được tự động set vào cookie `refreshToken` (HttpOnly).

---

## 3. Login (Đăng nhập)

**Endpoint:** `POST /api/auth/login`

**Authentication:** Không cần

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Validation:**
- `email`: Required, phải đúng định dạng email
- `password`: Required

**Success Response (200):**
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Error Responses:**
- `401`: Invalid credentials (email không tồn tại, password sai, hoặc user bị disabled)
- `400`: Validation errors

**Lưu ý:** Sau khi login thành công, refresh token được tự động set vào cookie `refreshToken` (HttpOnly).

---

## 4. Refresh Token (Làm mới Access Token)

**Endpoint:** `POST /api/auth/refresh`

**Authentication:** Không cần (nhưng cần refresh token trong cookie)

**Request Body:** Không có

**Request Headers:** Không cần (refresh token tự động lấy từ cookie)

**Success Response (200):**
```json
{
  "success": true,
  "message": "Refresh thành công",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Error Responses:**
- `401`: Missing refresh token hoặc Invalid refresh token

**Lưu ý:** 
- Refresh token được lấy tự động từ cookie `refreshToken`
- Frontend cần đảm bảo gửi cookie khi gọi API này (credentials: 'include' nếu dùng fetch)

---

## 5. Logout (Đăng xuất)

**Endpoint:** `POST /api/auth/logout`

**Authentication:** Cần (Bearer Token)

**Request Headers:**
```
Authorization: Bearer {accessToken}
```

**Request Body:** Không có

**Success Response (200):**
```json
{
  "success": true,
  "message": "Logout thành công",
  "data": null
}
```

**Error Responses:**
- `401`: Unauthorized (thiếu hoặc token không hợp lệ)

**Lưu ý:** 
- Refresh token cookie sẽ được xóa tự động sau khi logout thành công
- Frontend nên xóa access token khỏi storage sau khi logout

---

## Cách sử dụng Token

### Access Token
- Được trả về trong response `data.accessToken`
- Lưu vào localStorage/sessionStorage hoặc memory
- Gửi kèm trong header `Authorization: Bearer {accessToken}` cho các API cần authentication
- Có thời hạn (thường ngắn, ví dụ 15 phút - 1 giờ)

### Refresh Token
- Được tự động set vào cookie `refreshToken` (HttpOnly, Secure)
- Frontend không cần lưu trữ, browser tự động gửi kèm request
- Dùng để refresh access token khi hết hạn
- Có thời hạn dài hơn access token

### Flow đề xuất:
1. **Login/Register** → Nhận `accessToken` → Lưu vào storage
2. **Gọi API** → Gửi `accessToken` trong header `Authorization`
3. **Token hết hạn (401)** → Gọi `/api/auth/refresh` → Nhận `accessToken` mới → Retry request
4. **Logout** → Xóa `accessToken` khỏi storage → Cookie `refreshToken` tự động bị xóa

---

## Ví dụ Frontend (JavaScript/Fetch)

### Login
```javascript
const response = await fetch('https://api.example.com/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  credentials: 'include', // Quan trọng: để nhận cookie refreshToken
  body: JSON.stringify({
    email: 'user@example.com',
    password: 'password123'
  })
});

const data = await response.json();
if (data.success) {
  localStorage.setItem('accessToken', data.data.accessToken);
}
```

### Gọi API có authentication
```javascript
const accessToken = localStorage.getItem('accessToken');
const response = await fetch('https://api.example.com/api/products', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  },
  credentials: 'include'
});
```

### Refresh Token
```javascript
const response = await fetch('https://api.example.com/api/auth/refresh', {
  method: 'POST',
  credentials: 'include' // Quan trọng: để gửi cookie refreshToken
});

const data = await response.json();
if (data.success) {
  localStorage.setItem('accessToken', data.data.accessToken);
}
```

### Logout
```javascript
const accessToken = localStorage.getItem('accessToken');
const response = await fetch('https://api.example.com/api/auth/logout', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
  },
  credentials: 'include'
});

if (response.ok) {
  localStorage.removeItem('accessToken');
}
```

---

## Status Codes

- `200`: Success
- `400`: Bad Request (validation errors, bad data)
- `401`: Unauthorized (invalid credentials, missing/invalid token)
- `500`: Internal Server Error
