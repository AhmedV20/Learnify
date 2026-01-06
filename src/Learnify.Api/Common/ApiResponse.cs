using System;
using System.Collections.Generic;

namespace Learnify.Api.Common;

/// <summary>
/// Standard API response wrapper for all endpoints
/// </summary>
/// <typeparam name="T">Type of the response data</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates if the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Human-readable message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Response data (null for errors)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// List of errors (null for successful responses)
    /// </summary>
    public List<ErrorDetail>? Errors { get; set; }

    /// <summary>
    /// Pagination information (only for paginated responses)
    /// </summary>
    public PaginationMetadata? Pagination { get; set; }

    /// <summary>
    /// Request metadata (timestamp, request ID, etc.)
    /// </summary>
    public ResponseMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Creates a successful response
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = data,
            Metadata = new ResponseMetadata()
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, List<ErrorDetail>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors,
            Metadata = new ResponseMetadata()
        };
    }

    /// <summary>
    /// Creates a paginated response
    /// </summary>
    public static ApiResponse<T> PaginatedResponse(T data, PaginationMetadata pagination, string message = "Data retrieved successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = 200,
            Message = message,
            Data = data,
            Pagination = pagination,
            Metadata = new ResponseMetadata()
        };
    }
}

/// <summary>
/// Non-generic version for responses without data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static new ApiResponse SuccessResponse(string message = "Operation completed successfully", int statusCode = 200)
    {
        return new ApiResponse
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Metadata = new ResponseMetadata()
        };
    }

    public static new ApiResponse ErrorResponse(string message, int statusCode = 400, List<ErrorDetail>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors,
            Metadata = new ResponseMetadata()
        };
    }
}

/// <summary>
/// Detailed error information
/// </summary>
public class ErrorDetail
{
    /// <summary>
    /// Error code (e.g., "VALIDATION_ERROR", "NOT_FOUND", "UNAUTHORIZED")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly description of the error to help understand what went wrong
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Field name (for validation errors)
    /// </summary>
    public string? Field { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Severity level: "Error", "Warning", "Info"
    /// </summary>
    public string Severity { get; set; } = "Error";
}

/// <summary>
/// Pagination metadata for list responses
/// </summary>
public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public bool HasNext => CurrentPage < TotalPages;
    public bool HasPrevious => CurrentPage > 1;
}

/// <summary>
/// Response metadata (timestamp, request ID, etc.)
/// </summary>
public class ResponseMetadata
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "v1";
    public string? CorrelationId { get; set; }
    public long? DurationMs { get; set; }

    public ResponseMetadata()
    {
    }

    public ResponseMetadata(string requestId, string? correlationId = null, long? durationMs = null)
    {
        RequestId = requestId;
        CorrelationId = correlationId ?? requestId;
        DurationMs = durationMs;
    }
}

/// <summary>
/// Standard error codes used across the API
/// </summary>
public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";
    public const string BadRequest = "BAD_REQUEST";
    public const string InternalError = "INTERNAL_ERROR";
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    public const string RateLimitExceeded = "RATE_LIMIT_EXCEEDED";
    public const string RequestTimeout = "REQUEST_TIMEOUT";
    public const string DatabaseError = "DATABASE_ERROR";
    public const string ExternalServiceError = "EXTERNAL_SERVICE_ERROR";
}
