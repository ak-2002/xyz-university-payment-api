namespace xyz_university_payment_api.Core.Shared.Constants
{
    /// <summary>
    /// API version configuration settings
    /// </summary>
    public class ApiVersionConfig
    {
        public string DefaultVersion { get; set; } = "2.0";
        public string[] SupportedVersions { get; set; } = { "1.0", "2.0" };
        public string[] DeprecatedVersions { get; set; } = { "1.0" };
        public bool EnableVersioning { get; set; } = true;
        public bool ShowDeprecationWarnings { get; set; } = true;
        public int DeprecationWarningDays { get; set; } = 30;
    }

    /// <summary>
    /// API version information
    /// </summary>
    public class ApiVersionInfo
    {
        public string Version { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Current", "Deprecated", "Sunset"
        public DateTime? DeprecationDate { get; set; }
        public DateTime? SunsetDate { get; set; }
        public string[] Features { get; set; } = Array.Empty<string>();
        public string[] BreakingChanges { get; set; } = Array.Empty<string>();
        public string DocumentationUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// API version deprecation warning
    /// </summary>
    public class ApiVersionDeprecationWarning
    {
        public string Version { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? DeprecationDate { get; set; }
        public DateTime? SunsetDate { get; set; }
        public string MigrationGuide { get; set; } = string.Empty;
    }
}