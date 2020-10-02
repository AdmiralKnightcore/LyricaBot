namespace Lyrica.Data.Config
{
    public class LyricaConfig
    {
        public ulong Owner { get; }

        public string Token { get; }

        public DiscordWebhook DiscordWebhookSink { get; }

        public TwitterSecrets TwitterSecrets { get; }
    }
}