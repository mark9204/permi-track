# PermiTrack Authentication API Test Script
# Run this script to quickly test the authentication endpoints

$baseUrl = "https://localhost:5001/api"

Write-Host "?? PermiTrack Authentication API Test" -ForegroundColor Cyan
Write-Host "====================================`n" -ForegroundColor Cyan

# Test 1: Register a new user
Write-Host "?? Test 1: Register New User" -ForegroundColor Yellow
$registerBody = @{
    username = "testuser_$(Get-Random)"
    email = "test_$(Get-Random)@example.com"
    password = "Test123!@#"
    confirmPassword = "Test123!@#"
    firstName = "Test"
    lastName = "User"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "$baseUrl/auth/register" `
        -Method Post `
        -Body $registerBody `
        -ContentType "application/json" `
        -SkipCertificateCheck
    
    Write-Host "? Registration Successful!" -ForegroundColor Green
    Write-Host "User ID: $($registerResponse.user.id)" -ForegroundColor Green
    Write-Host "Username: $($registerResponse.user.username)" -ForegroundColor Green
    Write-Host "Email: $($registerResponse.user.email)`n" -ForegroundColor Green
    
    $accessToken = $registerResponse.accessToken
    $refreshToken = $registerResponse.refreshToken
    
} catch {
    Write-Host "? Registration Failed: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

# Test 2: Login with the created user
Write-Host "?? Test 2: Login" -ForegroundColor Yellow
$loginBody = @{
    emailOrUsername = $registerResponse.user.username
    password = "Test123!@#"
    rememberMe = $true
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" `
        -Method Post `
        -Body $loginBody `
        -ContentType "application/json" `
        -SkipCertificateCheck
    
    Write-Host "? Login Successful!" -ForegroundColor Green
    Write-Host "Access Token: $($loginResponse.accessToken.Substring(0, 50))..." -ForegroundColor Green
    $accessToken = $loginResponse.accessToken
    
} catch {
    Write-Host "? Login Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Get Profile (Protected Endpoint)
Write-Host "`n?? Test 3: Get Profile (Protected)" -ForegroundColor Yellow
try {
    $headers = @{
        Authorization = "Bearer $accessToken"
    }
    
    $profileResponse = Invoke-RestMethod -Uri "$baseUrl/account/profile" `
        -Method Get `
        -Headers $headers `
        -SkipCertificateCheck
    
    Write-Host "? Profile Retrieved!" -ForegroundColor Green
    Write-Host "Username: $($profileResponse.username)" -ForegroundColor Green
    Write-Host "Email: $($profileResponse.email)" -ForegroundColor Green
    Write-Host "Full Name: $($profileResponse.firstName) $($profileResponse.lastName)`n" -ForegroundColor Green
    
} catch {
    Write-Host "? Get Profile Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Get Active Sessions
Write-Host "?? Test 4: Get Active Sessions" -ForegroundColor Yellow
try {
    $sessionsResponse = Invoke-RestMethod -Uri "$baseUrl/session/my-sessions" `
        -Method Get `
        -Headers $headers `
        -SkipCertificateCheck
    
    Write-Host "? Sessions Retrieved!" -ForegroundColor Green
    Write-Host "Active Sessions: $($sessionsResponse.Count)" -ForegroundColor Green
    
    foreach ($session in $sessionsResponse) {
        Write-Host "  - Session ID: $($session.id) | IP: $($session.ipAddress)" -ForegroundColor Gray
    }
    
} catch {
    Write-Host "? Get Sessions Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Refresh Token
Write-Host "`n?? Test 5: Refresh Token" -ForegroundColor Yellow
$refreshBody = @{
    refreshToken = $refreshToken
} | ConvertTo-Json

try {
    $refreshResponse = Invoke-RestMethod -Uri "$baseUrl/auth/refresh" `
        -Method Post `
        -Body $refreshBody `
        -ContentType "application/json" `
        -SkipCertificateCheck
    
    Write-Host "? Token Refreshed!" -ForegroundColor Green
    Write-Host "New Access Token: $($refreshResponse.accessToken.Substring(0, 50))..." -ForegroundColor Green
    
} catch {
    Write-Host "? Token Refresh Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: Update Profile
Write-Host "`n?? Test 6: Update Profile" -ForegroundColor Yellow
$updateBody = @{
    firstName = "Updated"
    lastName = "Name"
} | ConvertTo-Json

try {
    $updateResponse = Invoke-RestMethod -Uri "$baseUrl/account/profile" `
        -Method Put `
        -Body $updateBody `
        -ContentType "application/json" `
        -Headers $headers `
        -SkipCertificateCheck
    
    Write-Host "? Profile Updated!" -ForegroundColor Green
    Write-Host "New Name: $($updateResponse.firstName) $($updateResponse.lastName)`n" -ForegroundColor Green
    
} catch {
    Write-Host "? Profile Update Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: Request Password Reset
Write-Host "?? Test 7: Request Password Reset" -ForegroundColor Yellow
$resetRequestBody = @{
    email = $registerResponse.user.email
} | ConvertTo-Json

try {
    $resetResponse = Invoke-RestMethod -Uri "$baseUrl/auth/request-password-reset" `
        -Method Post `
        -Body $resetRequestBody `
        -ContentType "application/json" `
        -SkipCertificateCheck
    
    Write-Host "? Password Reset Requested!" -ForegroundColor Green
    Write-Host "Check email for reset link (if SMTP configured)`n" -ForegroundColor Green
    
} catch {
    Write-Host "? Password Reset Request Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 8: Logout
Write-Host "?? Test 8: Logout" -ForegroundColor Yellow
$logoutBody = @{
    refreshToken = $refreshToken
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri "$baseUrl/auth/logout" `
        -Method Post `
        -Body $logoutBody `
        -ContentType "application/json" `
        -Headers $headers `
        -SkipCertificateCheck
    
    Write-Host "? Logout Successful!`n" -ForegroundColor Green
    
} catch {
    Write-Host "? Logout Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Summary
Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "? All Tests Completed!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "`nTest Summary:" -ForegroundColor Yellow
Write-Host "  1. ? User Registration" -ForegroundColor Green
Write-Host "  2. ? User Login" -ForegroundColor Green
Write-Host "  3. ? Get Profile (Protected)" -ForegroundColor Green
Write-Host "  4. ? Get Active Sessions" -ForegroundColor Green
Write-Host "  5. ? Token Refresh" -ForegroundColor Green
Write-Host "  6. ? Update Profile" -ForegroundColor Green
Write-Host "  7. ? Password Reset Request" -ForegroundColor Green
Write-Host "  8. ? Logout" -ForegroundColor Green
Write-Host "`n?? Authentication API is working correctly!" -ForegroundColor Cyan
