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
    [NadekoModule("TableReads", "tr!")]
    public class TableReads : DiscordModule
    {

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManagePermissions)]
        public async Task CloseTableRead() {

            using (var uow = DbHandler.UnitOfWork()) {
                //vol = uow.Volunteers.AddVolunteer(volunteer.Id, volunteer.Username, volunteer.Mention, 0, Context.Guild.Id);
                uow.TableReads.CloseTableRead(Context.Guild.Id);
                await uow.CompleteAsync();
            }

            await Context.Channel.SendConfirmAsync($"The Table Read has been closed to new volunteers.").ConfigureAwait(false);
        }

        [NadekoCommand, Usage, Description, Aliases]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManagePermissions)]
        public async Task OpenTableRead() {

            using (var uow = DbHandler.UnitOfWork()) {
                //vol = uow.Volunteers.AddVolunteer(volunteer.Id, volunteer.Username, volunteer.Mention, 0, Context.Guild.Id);
                uow.TableReads.OpenTableRead(Context.Guild.Id);
                await uow.CompleteAsync();
            }

            await Context.Channel.SendConfirmAsync($"The Table Read has been opened to new volunteers.").ConfigureAwait(false);
        }
    }
}
