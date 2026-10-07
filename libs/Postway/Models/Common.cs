using System.Text.Json.Serialization;

namespace Postway.Models;

/// <summary>Paging input shared by every <c>filter</c> endpoint (<c>FilterBaseRequest</c>).</summary>
public class FilterRequest
{
    /// <summary>Free-text search; semantics depend on the endpoint.</summary>
    [JsonPropertyName("filter")]
    public string? Filter { get; set; }

    /// <summary>Page size, 1..1,000,000.</summary>
    [JsonPropertyName("limit")]
    public required int Limit { get; set; }

    /// <summary>1-based page number. Pages past the end wrap back to page 1 on the server.</summary>
    [JsonPropertyName("page")]
    public required int Page { get; set; }
}

/// <summary>Paging output shared by every <c>filter</c> endpoint (<c>FilterBaseResponse&lt;T&gt;</c>).</summary>
public sealed class FilterResponse<T>
{
    /// <summary>Total number of matching items.</summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>Page size.</summary>
    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    /// <summary>1-based page number.</summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>Number of pages.</summary>
    [JsonPropertyName("page_count")]
    public int PageCount { get; set; }

    /// <summary>Items on this page.</summary>
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = [];
}

/// <summary>A generated file returned as JSON with base64 content (<c>FileHttpResponse</c>).</summary>
public sealed class FileHttpResponse
{
    /// <summary>Server-supplied file name. Pass it through <see cref="Path.GetFileName(string)"/> before writing to disk.</summary>
    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = "";

    /// <summary>Base64-encoded file bytes; decode with <see cref="DecodeContent"/>.</summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    /// <summary>MIME type, e.g. <c>application/pdf</c>.</summary>
    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = "";

    /// <summary>Length of the decoded file in bytes.</summary>
    [JsonPropertyName("content_length")]
    public long ContentLength { get; set; }

    /// <summary>Decode the base64 <see cref="Content"/> into bytes.</summary>
    /// <exception cref="FormatException"><see cref="Content"/> is not valid base64.</exception>
    public byte[] DecodeContent() => Convert.FromBase64String(Content);
}
