# PermiTrack Authentication API Documentation

## ?? Authentication System

PermiTrack now includes a complete authentication and authorization system with the following features:

### ? Implemented Features

#### 1. **User Registration & Login**
- ? Email and Username validation
- ? Password strength validation (min 8 chars, uppercase, lowercase, number, special char)
- ? BCrypt password hashing
- ? JWT token generation
- ? Refresh token mechanism
- ? Email verification (optional)
- ? Remember me functionality

#### 2. **Password Management**
- ? Forgot password with email reset link
- ? Password reset with token validation
- ? Change password (for authenticated users)
- ? Password reset token expiration (1 hour)

#### 3. **Session Management**
- ? Track user sessions (IP, User Agent, timestamps)
- ? View all active sessions
- ? Terminate specific session
- ? Terminate all other sessions
- ? Force logout from all devices

#### 4. **Security Features**
- ? JWT-based authentication
- ? Failed login attempt tracking
- ? Account lockout mechanism
- ? Token blacklisting on logout
- ? Refresh token rotation
- ? Claims-based authorization (roles & permissions)

---

## ?? API Endpoints

### **Authentication Endpoints** (`/api/auth`)

#### 1. Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "dGVzdHRva2Vu...",
  "expiresAt": "2024-11-26T16:00:00Z",
  "user": {
    "id": 1,
    "username": "johndoe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "isActive": true,
    "createdAt": "2024-11-26T15:00:00Z"
  }
}
```

#### 2. Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "emailOrUsername": "johndoe",
  "password": "Password123!",
  "rememberMe": true
}
```

**Response (200 OK):** Same as register

#### 3. Refresh Token
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "dGVzdHRva2Vu..."
}
```

#### 4. Logout
```http
POST /api/auth/logout
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "refreshToken": "dGVzdHRva2Vu..."
}
```

#### 5. Verify Email
```http
GET /api/auth/verify-email?token={verificationToken}
```

#### 6. Request Password Reset
```http
POST /api/auth/request-password-reset
Content-Type: application/json

{
  "email": "john@example.com"
}
```

#### 7. Reset Password
```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "token": "resetToken123",
  "newPassword": "NewPassword123!"
}
```

---

### **Account Management Endpoints** (`/api/account`) ?? *Requires Authentication*

#### 1. Get Profile
```http
GET /api/account/profile
Authorization: Bearer {accessToken}
```

#### 2. Update Profile
```http
PUT /api/account/profile
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "email": "newemail@example.com",
  "firstName": "Jane",
  "lastName": "Smith"
}
```

#### 3. Change Password
```http
POST /api/account/change-password
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

---

### **Session Management Endpoints** (`/api/session`) ?? *Requires Authentication*

#### 1. Get My Sessions
```http
GET /api/session/my-sessions
Authorization: Bearer {accessToken}
```

**Response:**
```json
[
  {
    "id": 1,
    "ipAddress": "192.168.1.1",
    "userAgent": "Mozilla/5.0...",
    "createdAt": "2024-11-26T10:00:00Z",
    "lastActivity": "2024-11-26T15:00:00Z",
    "expiresAt": "2024-11-27T10:00:00Z"
  }
]
```

#### 2. Terminate Specific Session
```http
DELETE /api/session/{sessionId}
Authorization: Bearer {accessToken}
```

#### 3. Terminate All Other Sessions
```http
POST /api/session/terminate-all-others
Authorization: Bearer {accessToken}
```

#### 4. Terminate All Sessions (Logout Everywhere)
```http
POST /api/session/terminate-all
Authorization: Bearer {accessToken}
```

---

## ?? Configuration

### **appsettings.json**

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "PermiTrack",
    "Audience": "PermiTrackUsers",
    "ExpiresInMinutes": "60"
  },
  "Email": {
    "FromName": "PermiTrack",
    "FromAddress": "noreply@permitrack.com",
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "AppUrl": "http://localhost:5000"
}
```

### **User Secrets** (Recommended for sensitive data)

```bash
dotnet user-secrets set "Jwt:Key" "YourProductionSecretKey"
dotnet user-secrets set "Email:Username" "your-email@gmail.com"
dotnet user-secrets set "Email:Password" "your-app-password"
```

---

## ?? Testing the API

### **1. Using Swagger**
1. Run the application
2. Navigate to `https://localhost:{port}/swagger`
3. Test endpoints directly in the UI

### **2. Using Postman/Thunder Client**

#### Step 1: Register a User
```http
POST https://localhost:5001/api/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "password": "Test123!@#",
  "confirmPassword": "Test123!@#",
  "firstName": "Test",
  "lastName": "User"
}
```

#### Step 2: Login
```http
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "emailOrUsername": "testuser",
  "password": "Test123!@#",
  "rememberMe": true
}
```

Copy the `accessToken` from the response.

#### Step 3: Test Protected Endpoint
```http
GET https://localhost:5001/api/account/profile
Authorization: Bearer {paste-your-access-token-here}
```

---

## ??? Database Migration

The authentication system adds the following fields to the `Users` table:

- `EmailVerified` (bool)
- `EmailVerificationToken` (string, nullable)
- `EmailVerificationTokenExpiry` (DateTime, nullable)
- `PasswordResetToken` (string, nullable)
- `PasswordResetTokenExpiry` (DateTime, nullable)
- `LastLoginAt` (DateTime, nullable)
- `FailedLoginAttempts` (int)
- `LockoutEnd` (DateTime, nullable)

### Apply Migration

```bash
# Using Package Manager Console in Visual Studio
Update-Database

# Or using dotnet CLI
cd PermiTrack.DataContext
dotnet ef database update --startup-project ../PermiTrack/PermiTrack.csproj
```

---

## ?? Authorization in Controllers

All non-authentication controllers are now protected:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // JWT required
public class UsersController : ControllerBase
{
    // Endpoints require valid JWT token
}
```

You can also use role-based authorization:

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(long id)
{
    // Only users with "Admin" role can access
}
```

Or permission-based:

```csharp
[Authorize(Policy = "CanDeleteUsers")]
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(long id)
{
    // Custom policy authorization
}
```

---

## ?? JWT Token Structure

The generated JWT contains:

```json
{
  "nameid": "1",                    // User ID
  "unique_name": "johndoe",         // Username
  "email": "john@example.com",      // Email
  "firstName": "John",              // First Name
  "lastName": "Doe",                // Last Name
  "role": ["Admin", "User"],        // Roles
  "permission": ["users.read"],     // Permissions
  "exp": 1700000000                 // Expiration
}
```

---

## ??? Security Best Practices Implemented

1. ? Passwords are hashed with BCrypt (12 rounds)
2. ? JWT tokens expire after 60 minutes (configurable)
3. ? Refresh tokens for extended sessions
4. ? Password reset tokens expire after 1 hour
5. ? Email verification tokens expire after 24 hours
6. ? Failed login attempts are tracked
7. ? Account lockout after multiple failed attempts
8. ? Sessions are tracked with IP and User-Agent
9. ? All sessions invalidated on password change
10. ? HTTPS redirection enabled

---

## ?? Next Steps

### Recommended Enhancements:

1. **Two-Factor Authentication (2FA)**
   - Add TOTP support
   - SMS verification

2. **OAuth2/OpenID Connect**
   - Google login
   - Microsoft login
   - GitHub login

3. **Rate Limiting**
   - Prevent brute force attacks
   - API throttling

4. **Audit Trail**
   - Log all authentication events
   - Track permission changes

5. **Email Templates**
   - Professional HTML email templates
   - Localization support

---

## ?? Troubleshooting

### Issue: "Unable to connect to SMTP server"
**Solution:** Configure email settings in user secrets or use a testing SMTP service like MailTrap

### Issue: "JWT signature is invalid"
**Solution:** Ensure the `Jwt:Key` is the same in configuration and is at least 32 characters

### Issue: "Unauthorized" on protected endpoints
**Solution:** Include `Authorization: Bearer {token}` header with a valid access token

---

## ?? Support

For issues or questions, create an issue in the GitHub repository.
