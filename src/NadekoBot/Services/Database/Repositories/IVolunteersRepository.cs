using NadekoBot.Services.Database.Models;
using System.Collections.Generic;

namespace NadekoBot.Services.Database.Repositories
{
    public interface IVolunteersRepository : IRepository<Volunteer>
    {
        IEnumerable<Volunteer> GetVolunteers();
        Volunteer AddVolunteer(ulong userId, string name, string mentionName, ulong tableReadId, ulong guildId);
        Volunteer RemoveVolunteer(ulong userId, ulong tableReadId, ulong guildId);
        void ClearVolunteers(ulong tableReadId, ulong guildId);
    }
}
