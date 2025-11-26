using System.ComponentModel.DataAnnotations;

namespace PermiTrack.DataContext.DTOs;

public class BulkUserImportResult
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BulkUserImportError> Errors { get; set; } = new();
}

public class BulkUserImportError
{
    public int LineNumber { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
