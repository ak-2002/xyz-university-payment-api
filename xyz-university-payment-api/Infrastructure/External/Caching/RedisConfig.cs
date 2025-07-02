namespace xyz_university_payment_api.Infrastructure.External.Caching
{
    /// <summary>
    /// Redis configuration settings
    /// </summary>
    public class RedisConfig
    {
        public string InstanceName { get; set; } = string.Empty;
        public int DefaultExpirationMinutes { get; set; } = 30;
        public int PaymentCacheExpirationMinutes { get; set; } = 60;
        public int StudentCacheExpirationMinutes { get; set; } = 120;
        public int SummaryCacheExpirationMinutes { get; set; } = 45;
    }
} 