namespace Lyrica.Data.Config
{
    public class LyricaConfig
    {
        public ulong Owner { get; }

        public string Token { get; }

        public TwitterSecrets TwitterSecrets { get; }
    }

    public class TwitterSecrets
    {
        public string ConsumerKey { get; }

        public string ConsumerSecret { get; }

        public string AccessToken { get; }

        public string AccessTokenSecret { get; }

        public string Bearer { get; }
    }
}