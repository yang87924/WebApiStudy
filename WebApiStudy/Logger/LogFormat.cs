using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Formatting;
using Serilog.Parsing;
using static WebApiStudy.Logger.LogSettings;
namespace WebApiStudy.Logger;

/// <summary>
/// An <see cref="T:Serilog.Formatting.ITextFormatter" /> that writes events in a compact JSON format.
/// </summary>
public class LogFormat : ITextFormatter
{
    private readonly JsonValueFormatter _valueFormatter;

    /// <summary>
    /// Construct a <see cref="T:ReformAPI.Logger.LogFormat" />, optionally supplying a formatter for
    /// <see cref="T:Serilog.Events.LogEventPropertyValue" />s on the event.
    /// </summary>
    /// <param name="valueFormatter">A value formatter, or null.</param>
    public LogFormat(JsonValueFormatter valueFormatter = null) => this._valueFormatter = valueFormatter ?? new JsonValueFormatter("$type");

    /// <summary>
    /// Format the log event into the output. Subsequent events will be newline-delimited.
    /// </summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="output">The output.</param>
    public void Format(LogEvent logEvent, TextWriter output)
    {
        LogFormat.FormatEvent(logEvent, output, this._valueFormatter);
        output.WriteLine();
    }

    /// <summary>Format the log event into the output.</summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="output">The output.</param>
    /// <param name="valueFormatter">A value formatter for <see cref="T:Serilog.Events.LogEventPropertyValue" />s on the event.</param>
    public static void FormatEvent(LogEvent logEvent, TextWriter output, JsonValueFormatter valueFormatter)
    {
        if (logEvent == null)
            throw new ArgumentNullException(nameof(logEvent));
        if (output == null)
            throw new ArgumentNullException(nameof(output));
        if (valueFormatter == null)
            throw new ArgumentNullException(nameof(valueFormatter));

        var outputProperties = ToOutputProperty(logEvent);

        output.Write("{\n");
        output.Write($"\t\"{LogLevelKey}\":\"{logEvent.Level}\",\n");
        output.Write($"\t\"{LogTimeKey}\":{{ \"$date\" : \"{logEvent.Timestamp.UtcDateTime:o}\" }},\n");

        if (logEvent.Properties.Count != 0)
        {
            int nowIndex = 0;

            foreach (var keyValuePair in outputProperties)
            {
                if (keyValuePair.Key == nameof(LogPropertyNames.Message))
                {
                    if (!SourceFromAPI(logEvent))
                    {
                        var list = logEvent.Properties
                            .Select(pair => pair.ToString())
                            .ToList();

                        string exceptionString = logEvent.Exception != null
                            ? $"[Exception,\"{logEvent.Exception}\"]"
                            : string.Empty;

                        var microsoftLogDetailInMessage =
                            new KeyValuePair<string, LogEventPropertyValue>(
                                keyValuePair.Key,
                                new ScalarValue(
                                    $"From {logEvent.Properties["SourceContext"]}: [" +
                                    $"[MessageTemplate,\"{logEvent.MessageTemplate.Text}\"]," +
                                    $"{string.Join(",", list)}" +
                                    $"{exceptionString}" +
                                    "]"));

                        WriteJsonProperty(
                            microsoftLogDetailInMessage,
                            ++nowIndex,
                            outputProperties.Count,
                            output,
                            valueFormatter);

                        continue;
                    }
                }

                WriteJsonProperty(keyValuePair, ++nowIndex, outputProperties.Count, output, valueFormatter);
            }
        }

        output.Write("}");
    }

    /// <summary>
    /// Convert input value pair to JSON formatting output (Key:Value)
    /// </summary>
    /// <param name="pair">Key Value pair for output</param>
    /// <param name="nowIndex">Index of current value pair in values array</param>
    /// <param name="arrayCount">Array of value pairs</param>
    /// <param name="output">The output.</param>
    /// <param name="valueFormatter">A value formatter for <see cref="T:Serilog.Events.LogEventPropertyValue" />s on the event.</param>
    private static void WriteJsonProperty(
        KeyValuePair<string, LogEventPropertyValue> pair,
        int nowIndex, int arrayCount,
        TextWriter output,
        JsonValueFormatter valueFormatter)
    {
        output.Write('\t');
        JsonValueFormatter.WriteQuotedJsonString(pair.Key, output);
        output.Write(':');
        valueFormatter.Format(pair.Value, output);

        if (nowIndex < arrayCount)
            output.Write(',');

        output.Write('\n');
    }

    /// <summary>
    /// Check if log source comes from Microsoft library
    /// </summary>
    /// <param name="logEvent">Specify log for checking</param>
    /// <returns>True if log comes from Microsoft library, if not, return False</returns>
    private static bool SourceFromAPI(LogEvent logEvent)
    {
        try
        {
            return logEvent.Properties["SourceContext"].ToString().Contains(SourceContext);
        }
        catch
        {
            //When 'SourceContext' not exist.
            return false;
        }
    }

    /// <summary>
    /// Getting log properties
    /// </summary>
    /// <param name="logEvent">Specify log for processing</param>
    /// <returns>Properties of specific log</returns>
    private static Dictionary<string, LogEventPropertyValue> ToOutputProperty(LogEvent logEvent)
    {
        var outPutMessageTemplate = OutPutMessageTemplate();
        var propertyList = new Dictionary<string, LogEventPropertyValue>();

        if (logEvent.Properties.Count != 0)
        {
            foreach ((string key, var value) in logEvent.Properties)
            {
                propertyList.Add(key, value);
            }
        }

        var tokensWithFormat = outPutMessageTemplate.Tokens
            .OfType<PropertyToken>()
            .GroupBy(pt => pt.PropertyName)
            .ToArray();

        var outputProperties = new Dictionary<string, LogEventPropertyValue>();

        foreach (var token in tokensWithFormat)
        {
            bool has = false;

            foreach (var propertyValue in propertyList.Where(propertyValue => propertyValue.Key == token.Key))
            {
                outputProperties.Add(token.Key, propertyValue.Value);
                has = true;
                break;
            }

            if (has == false)
                outputProperties.Add(token.Key, new ScalarValue(string.Empty));
        }

        return outputProperties;
    }

    /// <summary>
    /// Generate a message template based on PropertyNames
    /// </summary>
    /// <returns>MessageTemplate transformed from PropertyNames</returns>
    private static MessageTemplate OutPutMessageTemplate()
    {
        var tokens = new List<MessageTemplateToken>();
        foreach (string propertyName in Enum.GetNames(typeof(LogPropertyNames)))
        {
            AddMessageTemplateTokenPair(tokens, $"\t\"{propertyName}\": ", propertyName);
        }
        return new MessageTemplate(tokens);
    }

    /// <summary>
    /// Build MessageTemplateToken with title and propertyField
    /// </summary>
    /// <param name="tokens">List of MessageTemplateToken</param>
    /// <param name="title">Value for TextToken</param>
    /// <param name="propertyField">Value for PropertyToken</param>
    private static void AddMessageTemplateTokenPair(List<MessageTemplateToken> tokens, string title, string propertyField)
    {
        tokens.Add(new TextToken(title, tokens.Count == 0 ? 0 : tokens[^1].StartIndex + tokens[^1].Length));
        tokens.Add(
            new PropertyToken(
                propertyField,
                $"{{{propertyField}}}",
                startIndex: tokens[^1].StartIndex + tokens[^1].Length));
    }
}

