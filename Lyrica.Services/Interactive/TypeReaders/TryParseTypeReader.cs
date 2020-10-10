﻿using System;
using System.Threading.Tasks;
using Discord.Commands;
using Lyrica.Services.Interactive.TryParse;

namespace Lyrica.Services.Interactive.TypeReaders
{
    public class TryParseTypeReader<T> : TypeReader
    {
        private readonly TryParseDelegate<T> _tryParse;

        public TryParseTypeReader(TryParseDelegate<T> tryParse)
        {
            _tryParse = tryParse;
        }

        public override async Task<TypeReaderResult> ReadAsync(
            ICommandContext context, string input, IServiceProvider services) =>
            _tryParse(input, out var result)
                ? TypeReaderResult.FromSuccess(result)
                : TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid input");
    }

    public class EnumTryParseTypeReader<T> : TypeReader where T : struct, Enum
    {
        private readonly bool _ignoreCase;
        private readonly EnumTryParseDelegate<T> _tryParse;

        public EnumTryParseTypeReader(EnumTryParseDelegate<T> tryParse, bool ignoreCase = true)
        {
            _tryParse = tryParse;
            _ignoreCase = ignoreCase;
        }

        public override async Task<TypeReaderResult> ReadAsync(
            ICommandContext context, string input, IServiceProvider services) =>
            _tryParse(input, _ignoreCase, out var result) && Enum.IsDefined(result)
                ? TypeReaderResult.FromSuccess(result)
                : TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid input");
    }
}