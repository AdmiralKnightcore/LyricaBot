namespace Lyrica.Services.Interactive.TryParse
{
    public delegate bool TryParseDelegate<T>(string input, out T result);
}