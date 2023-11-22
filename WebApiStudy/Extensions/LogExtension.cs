using Serilog.Events;
using static WebApiStudy.Logger.LogSettings;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace WebApiStudy.Extensions;

/// <summary>
/// Extension methods of Serilog ILogger
/// </summary>
public static class LogExtension
{
    public static LogEventLevel ToLogEventLevel(this string level) => level switch
    {
        "Verbose" => LogEventLevel.Verbose,
        "Debug" => LogEventLevel.Debug,
        "Information" => LogEventLevel.Information,
        "Warning" => LogEventLevel.Warning,
        "Error" => LogEventLevel.Error,
        "Fatal" => LogEventLevel.Fatal,
        _ => throw new Exception($"Not exist level: {level}"),
    };

    #region Serilog ILogger extensions
    public static void ApiWrite(this Serilog.ILogger logger, LogEventLevel level, string message,
        [CallerFilePath] string sourcePath = "",
        [CallerLineNumber] int sourceLine = 0)
    {
        logger.Write(level, null, MessageTemplate(),
            string.Empty,
            sourcePath,
            sourceLine,
            string.Empty,
            string.Empty,
            string.Empty,
            message,
            string.Empty,
            string.Empty,
            string.Empty);
    }

    public static void ApiWrite(this Serilog.ILogger logger, LogEventLevel level, string message, HttpContext httpContext,
        [CallerFilePath] string sourcePath = "",
        [CallerLineNumber] int sourceLine = 0)
    {
        logger.Write(level, null, MessageTemplate(),
            string.Empty,
            sourcePath,
            sourceLine,
            httpContext.Request.Path + httpContext.Request.QueryString,
            httpContext.Request.Method,
            string.Empty,
            message,
            string.Empty,
            string.Empty,
            string.Empty);
    }

    #endregion

    #region Microsoft ILogger extensions
    public static void ApiTrace(this ILogger logger,
        string message, HttpContext httpContext, int? statusCode = null, string requestBody = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.WriteLog(LogLevel.Trace,
            Activity.Current?.Id ?? Activity.Current?.Id ?? httpContext.TraceIdentifier,
            message,
            httpContext.Request.Path + httpContext.Request.QueryString,
            httpContext.Request.Method,
            statusCode,
            requestBody,
            sourceFilePath,
            sourceLineNumber,
            httpContext.Request.GetSubInToken());
    }

    public static void ApiDebug(this ILogger logger,
        string message, HttpContext httpContext, int? statusCode = null, string requestBody = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.WriteLog(LogLevel.Debug,
            Activity.Current?.Id ?? httpContext.TraceIdentifier,
            message,
            httpContext.Request.Path + httpContext.Request.QueryString,
            httpContext.Request.Method,
            statusCode,
            requestBody,
            sourceFilePath,
            sourceLineNumber,
            httpContext.Request.GetSubInToken());
    }

    public static void ApiInformation(this ILogger logger,
        string message, HttpContext httpContext, int? statusCode = null, string requestBody = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.WriteLog(LogLevel.Information,
            Activity.Current?.Id ?? httpContext.TraceIdentifier,
            message,
            httpContext.Request.Path + httpContext.Request.QueryString,
            httpContext.Request.Method,
            statusCode,
            requestBody,
            sourceFilePath,
            sourceLineNumber,
            httpContext.Request.GetSubInToken());
    }

    public static void ApiWarning(this ILogger logger,
        string message, HttpContext httpContext, int? statusCode = null, string requestBody = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.WriteLog(LogLevel.Warning,
            Activity.Current?.Id ?? httpContext.TraceIdentifier,
            message,
            httpContext.Request.Path + httpContext.Request.QueryString,
            httpContext.Request.Method,
            statusCode,
            requestBody,
            sourceFilePath,
            sourceLineNumber,
            httpContext.Request.GetSubInToken());
    }

    public static void ApiError(this ILogger logger,
        string message, HttpContext httpContext, int? statusCode = null, string requestBody = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.WriteLog(LogLevel.Error,
            Activity.Current?.Id ?? httpContext.TraceIdentifier,
            message,
            httpContext.Request.Path + httpContext.Request.QueryString,
            httpContext.Request.Method,
            statusCode,
            requestBody,
            sourceFilePath,
            sourceLineNumber,
            httpContext.Request.GetSubInToken());
    }

    public static void ApiCritical(this ILogger logger,
        string message, HttpContext httpContext, int? statusCode = null, string requestBody = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.WriteLog(LogLevel.Critical,
            Activity.Current?.Id ?? httpContext.TraceIdentifier,
            message,
            httpContext.Request.Path + httpContext.Request.QueryString,
            httpContext.Request.Method,
            statusCode,
            requestBody,
            sourceFilePath,
            sourceLineNumber,
            httpContext.Request.GetSubInToken());
    }

    public static void ApiNone(this ILogger logger,
        string message, HttpContext httpContext, int? statusCode = null, string requestBody = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.WriteLog(LogLevel.None,
            Activity.Current?.Id ?? httpContext.TraceIdentifier,
            message,
            httpContext.Request.Path + httpContext.Request.QueryString,
            httpContext.Request.Method,
            statusCode,
            requestBody,
            sourceFilePath,
            sourceLineNumber,
            httpContext.Request.GetSubInToken());
    }

    private static void WriteLog(this ILogger logger, LogLevel level, string traceId, string message,
        string controller, string httpMethod, int? statusCode, string requestBody,
        string sourceFilePath, int sourceLineNumber, string user)
    {
        logger.Log(level,
            MessageTemplate(),
            traceId ?? string.Empty,
            sourceFilePath ?? string.Empty,
            sourceLineNumber.ToString(),
            controller ?? string.Empty,
            httpMethod ?? string.Empty,
            statusCode == null ? string.Empty : statusCode.ToString(),
            user ?? string.Empty,
            message ?? string.Empty,
            requestBody ?? string.Empty,
            SourceContext
            );
    }

    public static void SystemWrite(this ILogger logger, LogLevel level,
       string message, int? statusCode = null, string requestBody = "",
       [CallerFilePath] string sourceFilePath = "",
       [CallerLineNumber] int sourceLineNumber = 0)
    {
        logger.WriteLog(level,
            string.Empty,
            message,
            string.Empty,
            string.Empty,
            statusCode,
            requestBody,
            sourceFilePath,
            sourceLineNumber,
            string.Empty);
    }
    #endregion

    /// <summary>
    /// Build a string template with a set of words setting by PropertyNames for logging
    /// </summary>
    /// <returns>Built string template with property name and value assigned when logging</returns>
    private static string MessageTemplate()
    {
        var template = new StringBuilder();
        foreach (string propertyName in Enum.GetNames(typeof(LogPropertyNames)))
        {
            template.AppendLine($"\t\"{propertyName}\": {{{propertyName}}},");
        }
        return template.ToString();
    }
}

