using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NadekoBot.Services.Database.Models;

namespace NadekoBot.Services.Database.Repositories.Impl
{
    public class VolunteersRepository : Repository<Volunteer>, IVolunteersRepository
    {
        public VolunteersRepository(DbContext context) : base(context)
        {
        }

        public Volunteer AddVolunteer(ulong userId, string name, string mentionName, ulong tableReadId, ulong guildId)
        {
            var volunteer = _set.Where(d => d.UserId == userId && d.TableReadId == tableReadId && d.GuildId == guildId)
                                .FirstOrDefault();

            if (volunteer == null)
            {
                _set.Add(volunteer = new Volunteer
                {
                    GuildId = guildId,
                    TableReadId = tableReadId,
                    UserId = userId,
                    Name = name,
                    MentionName = mentionName
                });
            }

            return volunteer;
        }

        public Volunteer RemoveVolunteer(ulong userId, ulong tableReadId, ulong guildId) {
            var volunteer = _set.Where(d => d.UserId == userId && d.TableReadId == tableReadId && d.GuildId == guildId)
                                .FirstOrDefault();

            if (volunteer != null) {
                _set.Remove(volunteer);
            }

            return volunteer;

        }

        public void ClearVolunteers(ulong tableReadId, ulong guildId) {
            var volunteers = _set.Where(d => d.TableReadId == tableReadId && d.GuildId == guildId).ToList();

            if (volunteers != null) {
                foreach (Volunteer v in volunteers) {
                    _set.Remove(v);
                }
            }
        }

        public IEnumerable<Volunteer> GetVolunteers() => 
            _set.ToList();
    }
}
