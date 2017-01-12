using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NadekoBot.Services;
using NadekoBot.Attributes;
using System.Collections.Concurrent;
using NadekoBot.Services.Database.Models;
using Discord;
using NadekoBot.Extensions;
using NLog;
using System.Diagnostics;
using Discord.WebSocket;
using System.Collections.Generic;

namespace NadekoBot.Modules.TableReads
{
    [NadekoModule("Volunteers", ".")]
    public class Volunteers : DiscordModule
    {
        [NadekoCommand, Usage, Description, Aliases]
        public async Task ListVolunteers() {
            IEnumerable<Volunteer> volunteers;

            using (var uow = DbHandler.UnitOfWork()) {
                volunteers = uow.Volunteers.GetVolunteers();
            }

            await Context.Channel.SendConfirmAsync("Registered Volunteers for the next Table Read - Friday at 4pm PST / 7 pm EST:", string.Join("⭐", volunteers.Select(d => d.MentionName))).ConfigureAwait(false);

        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        public async Task Ivolunteer() {
            Volunteer vol;
            IUser volunteer = Context.User;

            using (var uow = DbHandler.UnitOfWork()) {
                vol = uow.Volunteers.AddVolunteer(volunteer.Id, volunteer.Username, volunteer.Mention, 0, Context.Guild.Id);
                await uow.CompleteAsync();
            }

            await Context.Channel.SendConfirmAsync($"Successfuly added {volunteer.Username} as a volunteer Table Reader.").ConfigureAwait(false);
        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        public async Task Iunvolunteer() {
            Volunteer vol;
            IUser volunteer = Context.User;

            using (var uow = DbHandler.UnitOfWork()) {
                vol = uow.Volunteers.RemoveVolunteer(volunteer.Id, 0, Context.Guild.Id);
                await uow.CompleteAsync();
            }

            await Context.Channel.SendConfirmAsync($"Successfuly removed {volunteer.Username} from the volunteer list.").ConfigureAwait(false);
        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task ClearVolunteers() {

            using (var uow = DbHandler.UnitOfWork()) {
                uow.Volunteers.ClearVolunteers(0, Context.Guild.Id);
                await uow.CompleteAsync();
            }

            await Context.Channel.SendConfirmAsync($"The volunteers list has been cleared.").ConfigureAwait(false);
        }
    }
}
