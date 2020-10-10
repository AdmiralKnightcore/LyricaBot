using System;
using Discord.Commands;
using Lyrica.Services.Interactive.TryParse;

namespace Lyrica.Services.Interactive.TypeReaders
{
    public static class TypeReaderExtensions
    {
        public static TryParseTypeReader<T> AsTypeReader<T>(this TryParseDelegate<T> tryParse) =>
            new TryParseTypeReader<T>(tryParse);

        public static EnumTryParseTypeReader<T> AsTypeReader<T>(this EnumTryParseDelegate<T> tryParse, bool ignoreCase = true) where T : struct, Enum =>
            new EnumTryParseTypeReader<T>(tryParse, ignoreCase);

        public static TypeReaderCriterion AsCriterion(this TypeReader reader, IServiceProvider? services = null) =>
            new TypeReaderCriterion(reader, services);
    }
}