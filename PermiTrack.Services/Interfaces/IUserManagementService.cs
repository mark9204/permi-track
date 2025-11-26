using PermiTrack.DataContext.DTOs;

namespace PermiTrack.Services.Interfaces;

public interface IUserManagementService
{
    Task<(bool Success, string Message, long? UserId)> CreateUserAsync(CreateUserRequest request, long adminUserId);
    
    Task<(bool Success, string Message, List<UserListItemDTO> Users, int TotalCount)> GetUsersAsync(
        int page = 1, 
        int pageSize = 20, 
        string? searchTerm = null, 
        bool? isActive = null);
    
    Task<(bool Success, string Message, UserDetailsDTO? User)> GetUserByIdAsync(long userId);
    
    Task<(bool Success, string Message)> UpdateUserAsync(long userId, UpdateUserRequest request, long adminUserId);
    
    Task<(bool Success, string Message)> DeleteUserAsync(long userId, long adminUserId, string? reason = null);
    
    Task<(bool Success, string Message)> SetUserActiveStatusAsync(long userId, bool isActive, long adminUserId, string? reason = null);
    
    Task<BulkUserImportResult> BulkImportUsersAsync(BulkUserImportRequest request, long adminUserId);
}
