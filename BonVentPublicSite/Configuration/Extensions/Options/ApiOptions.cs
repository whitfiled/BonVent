using System.ComponentModel.DataAnnotations;

namespace BonVentPublicSite.Configuration.Extensions.Options;

public class ApiOptions
{
    public const string SectionName = "ApiEndPoint";
    [Required, Url]
    public string BaseAddress { get; set; } = string.Empty;

    [Range(1, 120)]
    public int Timeout { get; set; }
}
