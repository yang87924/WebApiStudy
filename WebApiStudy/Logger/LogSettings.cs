namespace WebApiStudy.Logger;

public static class LogSettings
{
    /// <summary>
    /// enum LogPropertyNames 的 item 順序是重要的，影響最終輸出結果
    /// </summary>
    public enum LogPropertyNames
    {
        TraceId,
        SourcePath,
        SourceLine,
        HttpUrl,
        HttpMethod,
        StatusCode,
        User,
        Message,
        RequestBody,
        SourceContext
    }
    public const string LogTimeKey = "Time";
    public const string LogLevelKey = "Level";
    public const string SourceContext = "DemoAPI";
}
