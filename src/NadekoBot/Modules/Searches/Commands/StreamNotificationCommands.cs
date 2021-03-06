﻿using Discord.Commands;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NadekoBot.Services;
using System.Threading;
using System.Collections.Generic;
using NadekoBot.Services.Database.Models;
using System.Net.Http;
using NadekoBot.Attributes;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using NadekoBot.Extensions;
using System.Diagnostics;

namespace NadekoBot.Modules.Searches
{
    public partial class Searches
    {
        public class StreamStatus
        {
            public bool IsLive { get; set; }
            public string ApiLink { get; set; }
            public string Views { get; set; }
            public string Game { get; set; }
            public string Status { get; set; }
            public string PreviewLink { get; set; }
            public string DisplayName { get; set; }
            public string Logo { get; set; }
        }

        public class HitboxResponse {
            public bool Success { get; set; } = true;
            [JsonProperty("media_is_live")]
            public string MediaIsLive { get; set; }
            public bool IsLive  => MediaIsLive == "1";
            [JsonProperty("media_views")]
            public string Views { get; set; }
        }

        public class TwitchResponse
        {
            public string Error { get; set; } = null;
            public bool IsLive => Stream != null;
            public StreamInfo Stream { get; set; }

            public class StreamInfo
            {
                public int Viewers { get; set; }
                public string Game { get; set; }
                public ChannelInfo Channel { get; set; }

                public class ChannelInfo {
                    public string Status { get; set; }
                    public string Display_Name { get; set; }
                    public string Logo { get; set; }
                }

                public PreviewLinks Preview { get; set; }
                public class PreviewLinks {
                    public string Medium { get; set; }
                }
            }
        }

        public class TwitchUsersResponse {
            public List<TwitchUsers> Users { get; set; }
            
            public class TwitchUsers {
                public string Error { get; set; } = null;
                public int _id { get; set; }
            }

        }

        public class BeamResponse
        {
            public string Error { get; set; } = null;

            [JsonProperty("online")]
            public bool IsLive { get; set; }
            public int ViewersCurrent { get; set; }
        }

        public class StreamNotFoundException : Exception
        {
            public StreamNotFoundException(string message) : base("Stream '" + message + "' not found.")
            {
            }
        }

        [Group]
        public class StreamNotificationCommands : ModuleBase
        {
            private static Timer checkTimer { get; }
            private static ConcurrentDictionary<string, StreamStatus> oldCachedStatuses = new ConcurrentDictionary<string, StreamStatus>();
            private static ConcurrentDictionary<string, StreamStatus> cachedStatuses = new ConcurrentDictionary<string, StreamStatus>();
            private static Logger _log { get; }

            private static bool FirstPass { get; set; } = true;

            static StreamNotificationCommands()
            {
                _log = LogManager.GetCurrentClassLogger();

                checkTimer = new Timer(async (state) =>
                {
                    oldCachedStatuses = new ConcurrentDictionary<string, StreamStatus>(cachedStatuses);
                    cachedStatuses.Clear();
                    IEnumerable<FollowedStream> streams;
                    using (var uow = DbHandler.UnitOfWork())
                    {
                        streams = uow.GuildConfigs.GetAllFollowedStreams();
                    }

                    await Task.WhenAll(streams.Select(async fs =>
                    {
                        try
                        {
                            var newStatus = await GetStreamStatus(fs).ConfigureAwait(false);
                            if (FirstPass)
                            {
                                return;
                            }

                            StreamStatus oldStatus;
                            if (oldCachedStatuses.TryGetValue(newStatus.ApiLink, out oldStatus) &&
                                oldStatus.IsLive != newStatus.IsLive)
                            {
                                var server = NadekoBot.Client.GetGuild(fs.GuildId);
                                if (server == null)
                                    return;
                                var channel = await server.GetTextChannelAsync(fs.ChannelId);
                                if (channel == null)
                                    return;
                                try
                                {
                                    var msg = await channel.EmbedAsync(fs.GetEmbed(newStatus)).ConfigureAwait(false);
                                    //await channel.SendMessageAsync(fs.GetLink());
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }));

                    FirstPass = false;
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            }

            private static async Task<StreamStatus> GetStreamStatus(FollowedStream stream, bool checkCache = true)
            {
                string response;
                StreamStatus result;
                switch (stream.Type)
                {
                    case FollowedStream.FollowedStreamType.Hitbox:
                        var hitboxUrl = $"https://api.hitbox.tv/media/status/{stream.Username.ToLowerInvariant()}";
                        if (checkCache && cachedStatuses.TryGetValue(hitboxUrl, out result))
                            return result;
                        using (var http = new HttpClient())
                        {
                            response = await http.GetStringAsync(hitboxUrl).ConfigureAwait(false);
                        }
                        var hbData = JsonConvert.DeserializeObject<HitboxResponse>(response);
                        if (!hbData.Success)
                            throw new StreamNotFoundException($"{stream.Username} [{stream.Type}]");
                        result = new StreamStatus()
                        {
                            IsLive = hbData.IsLive,
                            ApiLink = hitboxUrl,
                            Views = hbData.Views
                        };
                        cachedStatuses.AddOrUpdate(hitboxUrl, result, (key, old) => result);
                        return result;
                    case FollowedStream.FollowedStreamType.Twitch:

                        if (stream.StreamerId == 0)
                            throw new StreamNotFoundException($"Invalid Streamer Id {stream.StreamerId} for {stream.Username} [{stream.Type}]");


                        // API ver 3
                        //var twitchUrl = $"https://api.twitch.tv/kraken/streams/{Uri.EscapeUriString(stream.Username.ToLowerInvariant())}?client_id=67w6z9i09xv2uoojdm9l0wsyph4hxo6";

                        // API ver 5
                        var twitchUrl = $"https://api.twitch.tv/kraken/streams/{stream.StreamerId}?client_id=67w6z9i09xv2uoojdm9l0wsyph4hxo6&&api_version=5";

                        //var _log = LogManager.GetCurrentClassLogger();
                        //_log.Info($"[{stream.Username}] Stream URL: {twitchUrl}");

                        if (checkCache && cachedStatuses.TryGetValue(twitchUrl, out result))
                            return result;
                        using (var http = new HttpClient())
                        {
                            response = await http.GetStringAsync(twitchUrl).ConfigureAwait(false);
                            //_log.Info($"[{stream.Username}] Response: {response}");
                        }

                        var twData = JsonConvert.DeserializeObject<TwitchResponse>(response);
                        if (twData.Error != null)
                        {
                            _log.Info($"Error Response: {response} ||| {twData.Error}");
                            throw new StreamNotFoundException($"{stream.Username} [{stream.Type}]");
                        }
                        //if (twData.Stream == null)
                        //    _log.Info($"[{stream.Username}] Stream: null");
                        //else
                        //    _log.Info($"[{stream.Username}] Stream: not null");

                        result = new StreamStatus()
                        {

                            IsLive = twData.IsLive,
                            ApiLink = twitchUrl,
                            Views = twData.Stream?.Viewers.ToString() ?? "0",
                            Game = twData.Stream?.Game,
                            Status = twData.Stream?.Channel?.Status,
                            PreviewLink = twData.Stream?.Preview?.Medium,
                            DisplayName = twData.Stream?.Channel?.Display_Name,
                            Logo = twData.Stream?.Channel?.Logo
                        };
                        cachedStatuses.AddOrUpdate(twitchUrl, result, (key, old) => result);
                        return result;
                    case FollowedStream.FollowedStreamType.Beam:
                        var beamUrl = $"https://beam.pro/api/v1/channels/{stream.Username.ToLowerInvariant()}";
                        if (checkCache && cachedStatuses.TryGetValue(beamUrl, out result))
                            return result;
                        using (var http = new HttpClient())
                        {
                            response = await http.GetStringAsync(beamUrl).ConfigureAwait(false);
                        }

                        var bmData = JsonConvert.DeserializeObject<BeamResponse>(response);
                        if (bmData.Error != null)
                            throw new StreamNotFoundException($"{stream.Username} [{stream.Type}]");
                        result = new StreamStatus()
                        {
                            IsLive = bmData.IsLive,
                            ApiLink = beamUrl,
                            Views = bmData.ViewersCurrent.ToString()
                        };
                        cachedStatuses.AddOrUpdate(beamUrl, result, (key, old) => result);
                        return result;
                    default:
                        break;
                }
                return null;
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            public async Task Hitbox([Remainder] string username) =>
                await TrackStream((ITextChannel)Context.Channel, username, FollowedStream.FollowedStreamType.Hitbox)
                    .ConfigureAwait(false);

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            public async Task Twitch([Remainder] string username) =>
                await TrackStream((ITextChannel)Context.Channel, username, FollowedStream.FollowedStreamType.Twitch)
                    .ConfigureAwait(false);

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            public async Task Beam([Remainder] string username) =>
                await TrackStream((ITextChannel)Context.Channel, username, FollowedStream.FollowedStreamType.Beam)
                    .ConfigureAwait(false);

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            public async Task ListStreams()
            {
                IEnumerable<FollowedStream> streams;
                using (var uow = DbHandler.UnitOfWork())
                {
                    streams = uow.GuildConfigs
                                 .For(Context.Guild.Id, 
                                      set => set.Include(gc => gc.FollowedStreams))
                                 .FollowedStreams;
                }

                if (!streams.Any())
                {
                    await Context.Channel.SendConfirmAsync("You are not following any streams on this server.").ConfigureAwait(false);
                    return;
                }

                var text = string.Join("\n", await Task.WhenAll(streams.Select(async snc =>
                {
                    var ch = await Context.Guild.GetTextChannelAsync(snc.ChannelId);
                    return $"`{snc.Username}`'s stream on **{(ch)?.Name}** channel. 【`{snc.Type.ToString()}`】";
                })));

                await Context.Channel.SendConfirmAsync($"You are following **{streams.Count()}** streams on this server.\n\n" + text).ConfigureAwait(false);
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            public async Task RemoveStream(FollowedStream.FollowedStreamType type, [Remainder] string username)
            {
                username = username.ToLowerInvariant().Trim();

                var fs = new FollowedStream()
                {
                    ChannelId = Context.Channel.Id,
                    Username = username,
                    Type = type
                };

                bool removed;
                using (var uow = DbHandler.UnitOfWork())
                {
                    var config = uow.GuildConfigs.For(Context.Guild.Id, set => set.Include(gc => gc.FollowedStreams));
                    removed = config.FollowedStreams.Remove(fs);
                    if (removed)
                        await uow.CompleteAsync().ConfigureAwait(false);
                }
                if (!removed)
                {
                    await Context.Channel.SendErrorAsync("No such stream.").ConfigureAwait(false);
                    return;
                }
                await Context.Channel.SendConfirmAsync($"Removed `{username}`'s stream ({type}) from notifications.").ConfigureAwait(false);
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            public async Task CheckStream(FollowedStream.FollowedStreamType platform, [Remainder] string username)
            {
                var stream = username?.Trim();
                if (string.IsNullOrWhiteSpace(stream))
                    return;
                try
                {
                    var streamStatus = (await GetStreamStatus(new FollowedStream
                    {                        
                        Username = stream,
                        Type = platform,
                    }));

                    if (streamStatus.IsLive) {
                        await Context.Channel.SendConfirmAsync($"Streamer {username} is online with {streamStatus.Views} viewers.");
                    }
                    else {
                        await Context.Channel.SendConfirmAsync($"Streamer {username} is offline.");
                    }
                }
                catch
                {
                    await Context.Channel.SendErrorAsync("No channel found.");
                }
            }

            private static async Task TrackStream(ITextChannel channel, string username, FollowedStream.FollowedStreamType type)
            {
                username = username.ToLowerInvariant().Trim();

                int streamerId = 0;
                if (type == FollowedStream.FollowedStreamType.Twitch)
                    streamerId = await GetTwitchStreamerId(username);


                var fs = new FollowedStream
                {
                    GuildId = channel.Guild.Id,
                    ChannelId = channel.Id,
                    Username = username,
                    Type = type,
                    StreamerId = streamerId
                };

                StreamStatus status;
                try
                {
                    status = await GetStreamStatus(fs).ConfigureAwait(false);
                }
                catch
                {
                    await channel.SendErrorAsync("Stream probably doesn't exist.").ConfigureAwait(false);
                    return;
                }

                using (var uow = DbHandler.UnitOfWork())
                {
                    uow.GuildConfigs.For(channel.Guild.Id, set => set.Include(gc => gc.FollowedStreams))
                                    .FollowedStreams
                                    .Add(fs);
                    await uow.CompleteAsync().ConfigureAwait(false);
                }
                if (status.IsLive) {
                    //var _log = LogManager.GetCurrentClassLogger();
                    //_log.Info($"Preview Thumbnail: {status.PreviewLink}");
                    await channel.EmbedAsync(fs.GetEmbed(status), $"🆗 I will notify this channel when status changes.").ConfigureAwait(false);
                    //await channel.EmbedAsync(new EmbedBuilder().WithOkColor()
                    //    .WithImageUrl(status.PreviewLink))
                    //    .ConfigureAwait(false);
                    //await channel.SendMessageAsync(fs.GetLink());
                }
                else
                    await channel.SendMessageAsync($"🆗 I will notify this channel when status changes.");
            }

            private static async Task<int> GetTwitchStreamerId(string username) {
                string response;
                int id = 0;

                var twitchUrl = $"https://api.twitch.tv/kraken/users?login={Uri.EscapeUriString(username.ToLowerInvariant())}&client_id=67w6z9i09xv2uoojdm9l0wsyph4hxo6&api_version=5";

                var _log = LogManager.GetCurrentClassLogger();
                //_log.Info($"Channel URL: {twitchUrl}");
                                                         
                using (var http = new HttpClient()) {
                    response = await http.GetStringAsync(twitchUrl).ConfigureAwait(false);
                    //_log.Info($"Channel URL: {twitchUrl}");
                    //_log.Info($"Reponse: {response}");
                }

                var twData = JsonConvert.DeserializeObject<TwitchUsersResponse>(response);
                if (twData.Users == null || twData.Users.Count == 0) {
                    _log.Info($"Failed finding Twitch User for {username} [Twitch]");
                    throw new StreamNotFoundException($"Failed finding Twitch User for {username} [Twitch]");
                }

                //_log.Info($"Found ID: {twData.Users[0]._id}");
                id = twData.Users[0]._id;

                return id;
            }
        }
    }

    public static class FollowedStreamExtensions
    {
        public static EmbedBuilder GetEmbed(this FollowedStream fs, Searches.StreamStatus status)
        {
            var embed = new EmbedBuilder()
                            .WithTitle(status.DisplayName)
                            .WithUrl(fs.GetLink())
                            .WithThumbnailUrl(fs.GetLink())
                            .WithThumbnailUrl(status.Logo)
                            .WithDescription(status.Status)
                            .AddField(efb => efb.WithName("Status")
                                            .WithValue(status.IsLive ? "Online" : "Offline")
                                            .WithIsInline(true))
                            .AddField(efb => efb.WithName("Viewers")
                                            .WithValue(status.IsLive ? status.Views : "-")
                                            .WithIsInline(true))
                            .AddField(efb => efb.WithName("Game")
                                            .WithValue(status.Game)
                                            .WithIsInline(true))
                            .AddField(efb => efb.WithName("Platform")
                                            .WithValue(fs.Type.ToString())
                                            .WithIsInline(true))
                            .WithColor(status.IsLive ? NadekoBot.OkColor : NadekoBot.ErrorColor)
                            .WithImageUrl(status.PreviewLink);

            //embed.ImageUrl = status.PreviewLink;

            return embed;
        }

        public static string GetLink(this FollowedStream fs) {
            if (fs.Type == FollowedStream.FollowedStreamType.Hitbox)
                return $"http://www.hitbox.tv/{fs.Username}/";
            else if (fs.Type == FollowedStream.FollowedStreamType.Twitch)
                return $"http://www.twitch.tv/{fs.Username}/";
            else if (fs.Type == FollowedStream.FollowedStreamType.Beam)
                return $"https://beam.pro/{fs.Username}/";
            else
                return "??";
        }
    }
}
