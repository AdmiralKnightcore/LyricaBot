namespace Lyrica.Data.Bless
{
    public class Blessing
    {
        public Blessing(BlessingType type, string text)
        {
            Type = type;
            Text = text;
        }

        public BlessingType Type { get; set; }

        public string Text { get; set; }
    }
}