using System.ComponentModel.DataAnnotations;

namespace ResourceSpider.Server.DTOs;

public record TestExtractionRequest(
    [Required] string Content,
    [Required, StringLength(32)] string ExpressionType,
    [Required, StringLength(1024)] string Expression,
    bool IsHtml = true
);

public record TestExtractionResponse(
    bool Success,
    List<string>? Results,
    string? Error
);

public record TestPageRequest(
    [Required, StringLength(2048)] string Url,
    [Required, StringLength(32)] string ExpressionType,
    [Required, StringLength(1024)] string Expression,
    [StringLength(16)] string Method = "GET",
    string? Body = null,
    Dictionary<string, string>? Headers = null
);

public record TestPageResponse(
    bool Success,
    List<string>? Results,
    string? RawContent,
    string? Error
);

public record ConfigTemplateDto(
    string TemplateId,
    string Name,
    string Description,
    string ConfigContent
);
