using NadekoBot.Services.Database.Models;
using System.Collections.Generic;

namespace NadekoBot.Services.Database.Repositories
{
    public interface IWritingPromptsRepository : IRepository<WritingPrompt>
    {
        WritingPrompt GetLatestWritingPrompt();
        WritingPrompt GetRandomWritingPrompt();
    }
}
