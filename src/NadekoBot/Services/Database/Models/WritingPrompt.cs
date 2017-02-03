using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace NadekoBot.Services.Database.Models
{
    public class WritingPrompt : DbEntity
    {
        public string Prompt { get; set; }
    }
}