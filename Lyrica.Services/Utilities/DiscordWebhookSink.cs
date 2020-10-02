using System;
using Discord;
using Discord.Webhook;
using Newtonsoft.Json;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Lyrica.Services.Utilities
{
    public sealed class DiscordWebhookSink
        : ILogEventSink,
            IDisposable
    {
        private readonly DiscordWebhookClient _discordWebhookClient;
        private readonly IFormatProvider _formatProvider;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public DiscordWebhookSink(
            ulong webhookId,
            string webhookToken,
            IFormatProvider formatProvider)
        {
            _discordWebhookClient = new DiscordWebhookClient(webhookId, webhookToken);
            _formatProvider = formatProvider;

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new ExceptionContractResolver()
            };
        }

        public void Dispose()
            => _discordWebhookClient.Dispose();

        public void Emit(LogEvent logEvent)
        {
            const int DiscordStringTruncateLength = 1000;

            var formattedMessage = logEvent.RenderMessage(_formatProvider);

            var message = new EmbedBuilder()
                .WithAuthor("DiscordLogger")
                .WithTitle("Lyrica Bot")
                .WithTimestamp(DateTimeOffset.UtcNow)
                .WithColor(Color.Red);

            try
            {
                var messagePayload = $"{formattedMessage}\n{logEvent.Exception?.Message}";

                message.AddField(new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName($"LogLevel: {logEvent.Level}")
                    .WithValue(Format.Code(messagePayload.TruncateTo(DiscordStringTruncateLength))));

                //var eventAsJson = JsonConvert.SerializeObject(logEvent, _jsonSerializerSettings);

                //var url = _codePasteService.UploadCodeAsync(eventAsJson, "json").GetAwaiter().GetResult();

                //message.AddField(new EmbedFieldBuilder()
                //    .WithIsInline(false)
                //    .WithName("Full Log Event")
                //    .WithValue($"[view on paste.mod.gg]({url})"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to upload log event. {ex}");

                var stackTracePayload =
                    $"{formattedMessage}\n{logEvent.Exception?.ToString().TruncateTo(DiscordStringTruncateLength)}"
                        .TruncateTo(DiscordStringTruncateLength);

                message.AddField(new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Stack Trace")
                    .WithValue(Format.Code(stackTracePayload)));

                message.AddField(new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName("Upload Failure Exception")
                    .WithValue(Format.Code($"{ex.ToString().TruncateTo(DiscordStringTruncateLength)}")));
            }

            _discordWebhookClient.SendMessageAsync(string.Empty, embeds: new[] { message.Build() },
                username: "Lyrica Logger");
        }
    }

    public static class DiscordWebhookSinkExtensions
    {
        public static LoggerConfiguration DiscordWebhookSink(this LoggerSinkConfiguration config, ulong id,
            string token, LogEventLevel minLevel) =>
            config.Sink(new DiscordWebhookSink(id, token, null), minLevel);
    }

    public static class LoggingExtensions
    {
        public static string TruncateTo(this string str, int length)
        {
            if (str.Length < length) return str;

            return str.Substring(0, length);
        }
    }
}