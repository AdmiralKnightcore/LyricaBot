namespace Lyrica.Data.Bless
{
    public class Blessing
    {
        public BlessingType Type { get; set; }

        public string Text { get; set; }

        public Blessing(BlessingType type, string text)
        {
            Type = type;
            Text = text;
        }
    }
}