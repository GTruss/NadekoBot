using NadekoBot.Services.Database.Models;
using System.Collections.Generic;

namespace NadekoBot.Services.Database.Repositories
{
    public interface ITableReadsRepository : IRepository<TableRead>
    {
        void CloseTableRead(ulong guildId);
        void OpenTableRead(ulong guildId);
        TableRead GetTableRead(ulong guildId);
    }
}
