using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace NadekoBot.Services.Database.Models
{
    public class Volunteer : DbEntity
    {
        public ulong? GuildId { get; set; }
        public ulong TableReadId { get; set; }
        public ulong UserId { get; set; }
        public string Name { get; set; }
        public string MentionName { get; set; }
    }
}