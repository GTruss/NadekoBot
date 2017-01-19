using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace NadekoBot.Services.Database.Models
{
    public class TableRead : DbEntity
    {
        public ulong? GuildId { get; set; }
        public ulong TableReadId { get; set; }
        public bool IsOpen { get; set; }
    }
}