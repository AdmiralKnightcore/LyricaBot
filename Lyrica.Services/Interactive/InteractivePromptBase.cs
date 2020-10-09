﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Lyrica.Services.Image;
using Lyrica.Services.Interactive.Criteria;
using Lyrica.Services.Interactive.TypeReaders;
using Lyrica.Services.Utilities;
using Optional = Lyrica.Services.Interactive.TypeReaders.Optional;

namespace Lyrica.Services.Interactive
{
    public class InteractivePromptBase : InteractiveBase<SocketCommandContext>
    {
        public IImageService ImageService { get; set; }

        internal async Task<(SocketMessage? response, IUserMessage message)> Prompt(
            string question,
            IUserMessage? message = null, IEnumerable<EmbedFieldBuilder>? fields = null, int secondsTimeout = 30,
            CriteriaCriterion<SocketMessage>? criterion = null, bool isRequired = true)
        {
            message = await ModifyOrSendMessage(question, message, fields, isRequired: isRequired);

            SocketMessage? response;
            var timeout = TimeSpan.FromSeconds(secondsTimeout);
            if (criterion is null)
                response = await NextMessageAsync(timeout: timeout);
            else
                response = await NextMessageAsync(timeout: timeout, criterion: criterion);

            _ = response?.DeleteAsync();

            if (!isRequired && response.IsSkipped())
                response = null;

            return (response, message);
        }

        internal async Task<IUserMessage> ModifyOrSendMessage(
            string question, IUserMessage? message = null, IEnumerable<EmbedFieldBuilder>? fields = null,
            Color? color = null, bool isRequired = true)
        {
            var embed = new EmbedBuilder()
                .WithUserAsAuthor(Context.User)
                .WithDescription(question)
                .WithColor(color
                           ?? await ImageService.GetDominantColorAsync(new Uri(Context.User.GetDefiniteAvatarUrl())))
                .WithFields(fields ?? Enumerable.Empty<EmbedFieldBuilder>());

            if (!isRequired)
                embed.WithFooter($"Reply '{Optional.SkipString}' if you don't need this.");

            if (message is null)
                return await ReplyAsync(embed: embed.Build());

            await message.ModifyAsync(msg => msg.Embed = embed.Build());
            return message;
        }
    }
}