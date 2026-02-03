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

**User thường:**
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": null,
    "user": {
      "id": 1,
      "email": "user@example.com",
      "fullName": "Nguyễn Văn A",
      "roles": ["User"]
    }
  }
}
```

**Admin:**
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "abc123xyz789...",
    "user": {
      "id": 2,
      "email": "admin@example.com",
      "fullName": "Admin User",
      "roles": ["Admin"]
    }
  }
}
```

**Error Responses:**
- `401`: Invalid credentials (email không tồn tại, password sai, hoặc user bị disabled)
- `400`: Validation errors

**Lưu ý:** 
- Sau khi login thành công, refresh token được tự động set vào cookie `refreshToken` (HttpOnly)
- **Admin** sẽ nhận thêm `refreshToken` trong JSON response để lưu vào storage (localStorage/memory)
- **User thường** chỉ có refresh token trong cookie, không có trong JSON response

---

## 4. Refresh Token (Làm mới Access Token) - User thường

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
- API này dành cho **user thường**, admin nên dùng `/api/auth/refresh-admin`

---

## 5. Refresh Token Admin (Làm mới Access Token cho Admin)

**Endpoint:** `POST /api/auth/refresh-admin`

**Authentication:** Không cần

**Request Body:**
```json
{
  "refreshToken": "abc123xyz789..."
}
```

**Validation:**
- `refreshToken`: Required

**Success Response (200):**
```json
{
  "success": true,
  "message": "Refresh thành công",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "newRefreshToken123..."
  }
}
```

**Error Responses:**
- `401`: Invalid refresh token hoặc user không phải Admin

**Lưu ý:** 
- Refresh token được gửi trong request body (không dùng cookie)
- API này chỉ dành cho **Admin**
- Response trả về cả `accessToken` và `refreshToken` mới
- Frontend cần lưu `refreshToken` mới để dùng cho lần refresh tiếp theo

---

## 6. Logout (Đăng xuất)

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

#### User thường:
- Được tự động set vào cookie `refreshToken` (HttpOnly, Secure)
- Frontend không cần lưu trữ, browser tự động gửi kèm request
- Dùng để refresh access token khi hết hạn qua API `/api/auth/refresh`

#### Admin:
- Được trả về trong JSON response khi login (`data.refreshToken`)
- Frontend cần lưu vào localStorage/sessionStorage hoặc memory
- Dùng để refresh access token khi hết hạn qua API `/api/auth/refresh-admin`
- Gửi refreshToken trong request body (không dùng cookie)

### Flow đề xuất:

#### User thường:
1. **Login/Register** → Nhận `accessToken` → Lưu vào storage
2. **Gọi API** → Gửi `accessToken` trong header `Authorization`
3. **Token hết hạn (401)** → Gọi `/api/auth/refresh` (cookie tự động gửi) → Nhận `accessToken` mới → Retry request
4. **Logout** → Xóa `accessToken` khỏi storage → Cookie `refreshToken` tự động bị xóa

#### Admin:
1. **Login** → Nhận `accessToken` và `refreshToken` → Lưu cả hai vào storage
2. **Gọi API** → Gửi `accessToken` trong header `Authorization`
3. **Token hết hạn (401)** → Gọi `/api/auth/refresh-admin` với `refreshToken` trong body → Nhận `accessToken` và `refreshToken` mới → Lưu lại → Retry request
4. **Logout** → Xóa `accessToken` và `refreshToken` khỏi storage

---

## Ví dụ Frontend (JavaScript/Fetch)

### Login (User thường)
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
  // refreshToken được lưu trong cookie tự động
}
```

### Login (Admin)
```javascript
const response = await fetch('https://api.example.com/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  credentials: 'include',
  body: JSON.stringify({
    email: 'admin@example.com',
    password: 'password123'
  })
});

const data = await response.json();
if (data.success) {
  localStorage.setItem('accessToken', data.data.accessToken);
  // Admin cần lưu refreshToken vào storage
  if (data.data.refreshToken) {
    localStorage.setItem('refreshToken', data.data.refreshToken);
  }
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

### Refresh Token (User thường)
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

### Refresh Token (Admin)
```javascript
const refreshToken = localStorage.getItem('refreshToken');
const response = await fetch('https://api.example.com/api/auth/refresh-admin', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    refreshToken: refreshToken
  })
});

const data = await response.json();
if (data.success) {
  localStorage.setItem('accessToken', data.data.accessToken);
  // Lưu refreshToken mới để dùng cho lần refresh tiếp theo
  localStorage.setItem('refreshToken', data.data.refreshToken);
}
```

### Logout (User thường)
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
  // Cookie refreshToken tự động bị xóa bởi server
}
```

### Logout (Admin)
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
  localStorage.removeItem('refreshToken'); // Xóa refreshToken khỏi storage
}
```

---

## Status Codes

- `200`: Success
- `400`: Bad Request (validation errors, bad data)
- `401`: Unauthorized (invalid credentials, missing/invalid token)
- `500`: Internal Server Error
