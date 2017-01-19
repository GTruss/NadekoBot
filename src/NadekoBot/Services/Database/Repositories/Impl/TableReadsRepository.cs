using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NadekoBot.Services.Database.Models;

namespace NadekoBot.Services.Database.Repositories.Impl
{
    public class TableReadsRepository : Repository<TableRead>, ITableReadsRepository
    {
        public TableReadsRepository(DbContext context) : base(context)
        {
        }

        public void CloseTableRead(ulong guildId) {
            var tableread = _set.Where(d => d.GuildId == guildId).First();

            tableread.IsOpen = false;

            if (tableread != null) {
                _set.Update(tableread);
            }
        }

        public void OpenTableRead(ulong guildId) {
            var tableread = _set.Where(d => d.GuildId == guildId).First();

            tableread.IsOpen = true;

            if (tableread != null) {
                _set.Update(tableread);
            }
        }

        public TableRead GetTableRead(ulong guildId) {
            var tableread = _set.Where(d => d.GuildId == guildId).First();

            return tableread;
        }
    }
}
