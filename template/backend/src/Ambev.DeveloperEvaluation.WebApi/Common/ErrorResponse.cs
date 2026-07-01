namespace Ambev.DeveloperEvaluation.WebApi.Common;

/// <summary>
/// Standard error payload as described in the general API definitions:
/// a machine-readable type, a short human-readable summary and a detailed explanation.
/// </summary>
public class ErrorResponse
{
    /// <summary>Machine-readable error type identifier (e.g. "ResourceNotFound").</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Short, human-readable summary of the problem.</summary>
    public string Error { get; set; } = string.Empty;

    /// <summary>Human-readable explanation specific to this occurrence.</summary>
    public string Detail { get; set; } = string.Empty;
}
