using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NadekoBot.Services.Database.Models;

namespace NadekoBot.Services.Database.Repositories.Impl
{
    public class WritingPromptsRepository : Repository<WritingPrompt>, IWritingPromptsRepository {
        public WritingPromptsRepository(DbContext context) : base(context)
        {
        }

        public WritingPrompt GetLatestWritingPrompt() {
            var prompt = _set.OrderByDescending(p => p.Id).FirstOrDefault();

            return prompt;
        }

        public WritingPrompt GetRandomWritingPrompt() {

            int count = _set.Count();

            System.Random rnd = new System.Random(System.DateTime.Now.Millisecond);
            int id = rnd.Next(1, count);

            var prompt = _set.Where(p => p.Id == id).FirstOrDefault();

            return prompt;
        }
    }
}
