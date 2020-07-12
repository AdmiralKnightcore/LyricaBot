using System.Collections.Generic;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Lyrica.Services.Interactive.Criteria
{
    public interface IPromptCriteria<T>
    {
        TypeReader? TypeReader { get; set; }

        ICollection<ICriterion<T>>? Criteria { get; }
    }
}