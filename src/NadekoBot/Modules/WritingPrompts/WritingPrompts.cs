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
    [NadekoModule("WritingPrompts", "wp!")]
    public class WritingPrompts : DiscordModule
    {

        [NadekoCommand, Usage, Description, Aliases]
        public async Task AddWritingPrompt([Remainder] string prompt) {

            var wp = new WritingPrompt()
            {
                Prompt = prompt
            };

            using (var uow = DbHandler.UnitOfWork()) {
                //vol = uow.Volunteers.AddVolunteer(volunteer.Id, volunteer.Username, volunteer.Mention, 0, Context.Guild.Id);
                uow.WritingPrompts.Add(wp);
                await uow.CompleteAsync();
            }


            //await Context.Channel.SendConfirmAsync($"The Table Read has been closed to new volunteers.").ConfigureAwait(false);
            await Context.Channel.EmbedAsync(new EmbedBuilder().WithOkColor()
                .WithTitle("New Writing Prompt")
                .WithDescription($"#{wp.Id}")
                .AddField(efb => efb.WithName("Prompt").WithValue(prompt))
                ).ConfigureAwait(false);
        }

        [NadekoCommand, Usage, Description, Aliases]
        public async Task LastWritingPrompt() {

            WritingPrompt wp;

            using (var uow = DbHandler.UnitOfWork()) {
                //vol = uow.Volunteers.AddVolunteer(volunteer.Id, volunteer.Username, volunteer.Mention, 0, Context.Guild.Id);
                wp = uow.WritingPrompts.GetLatestWritingPrompt();
                await uow.CompleteAsync();
            }

            await Context.Channel.SendMessageAsync(wp.Prompt).ConfigureAwait(false);
        }

        [NadekoCommand, Usage, Description, Aliases]
        public async Task RandomWritingPrompt() {

            WritingPrompt wp;

            using (var uow = DbHandler.UnitOfWork()) {
                //vol = uow.Volunteers.AddVolunteer(volunteer.Id, volunteer.Username, volunteer.Mention, 0, Context.Guild.Id);
                wp = uow.WritingPrompts.GetRandomWritingPrompt();
                await uow.CompleteAsync();
            }

            await Context.Channel.SendMessageAsync(wp.Prompt).ConfigureAwait(false);
        }

        private void TestWP() {

        }
    }
}
