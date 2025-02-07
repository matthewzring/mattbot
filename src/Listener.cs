/*
 * Copyright 2023-2025 Matthew Ring
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Discord.WebSocket;
using mattbot.automod;

namespace mattbot;

public class Listener
{
    // Delegates
    public delegate Task AsyncListener<in TEventArgs>(TEventArgs args);

    public delegate Task AsyncListener<in TEventArgs, in TArgs>(TEventArgs args, TArgs arsg2);

    public delegate Task AsyncListener<in TEventArgs, in TArgs, in TEvent>(TEventArgs args, TArgs args2, TEvent args3);

    public delegate Task AsyncListener<in TEventArgs, in TArgs, in TEvent, in TArgs2>(TEventArgs args, TArgs args2, TEvent args3, TArgs2 args4);

    // Events
    public event AsyncListener<SocketMessage>? MessageReceived;
    public event AsyncListener<IGuildUser>? UserJoined;
    public event AsyncListener<IGuild, IUser>? UserLeft;
    public event AsyncListener<Cacheable<SocketGuildUser, ulong>, SocketGuildUser>? GuildMemberUpdated;
    public event AsyncListener<SocketUser, SocketUser>? UserUpdated;
    public event AsyncListener<SocketUser, SocketVoiceState, SocketVoiceState>? UserVoiceStateUpdated;
    public event AsyncListener<Cacheable<IUserMessage, ulong>, Cacheable<IMessageChannel, ulong>, SocketReaction>? ReactionAdded;
    public event AsyncListener<DiscordSocketClient>? Ready;

    private readonly DiscordSocketClient _client;
    private readonly MattBot mattbot;

    public Listener(MattBot mattbot, DiscordSocketClient client)
    {
        this.mattbot = mattbot;
        _client = client;
        client.MessageReceived += ClientOnMessageReceived;
        client.UserJoined += ClientOnUserJoined;
        client.UserLeft += ClientOnUserLeft;
        client.GuildMemberUpdated += ClientOnGuildMemberUpdated;
        client.UserUpdated += ClientOnUserUpdated;
        client.UserVoiceStateUpdated += ClientOnUserVoiceStateUpdated;
        client.ReactionAdded += ClientOnReactionAdded;
        client.Ready += ClientOnReady;
    }

    private Task ClientOnMessageReceived(SocketMessage arg)
    {
        if (!arg.Author.IsBot) // ignore bot messages
        {
            if (arg.Channel is IPrivateChannel)
            {
                // Log the message received
                _ = mattbot.Logger.LogMessageReceived(_client, arg);
            }

            // Run automod on the message
            _ = mattbot.AutoMod.PerformAutomod(arg);
        }

        return Task.CompletedTask;
    }

    private Task ClientOnUserJoined(IGuildUser arg)
    {
        // Log the join
        _ = mattbot.Logger.LogGuildJoin(arg);

        // Perform automod on the newly-joined member
        _ = mattbot.AutoMod.UserJoin(arg);

        return Task.CompletedTask;
    }

    private Task ClientOnUserLeft(IGuild arg1, IUser arg2)
    {
        // Log the member leaving
        _ = mattbot.Logger.LogGuildLeave(arg1, arg2);

        // Siginal the automod if a Nitro Booster left
        SocketRole bRole = (arg2 as SocketGuildUser).Roles.FirstOrDefault(x => x.Name.Equals("Nitro Booster"));
        if ((arg1.Id == CCDC_ID || arg1.Id == CYBERPATRIOT_ID) && bRole != null)
        {
            _ = mattbot.AutoMod.DeleteColorRole(arg2 as SocketGuildUser);
        }

        return Task.CompletedTask;
    }

    private Task ClientOnGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> arg1, SocketGuildUser arg2)
    {
        if (arg2.Guild.Id == CCDC_ID || arg2.Guild.Id == CYBERPATRIOT_ID)
        {
            // Siginal the automod if someone boosted the server
            IEnumerable<SocketRole> addedRoles = arg2.Roles.Except(arg1.Value.Roles);
            if (addedRoles.Any(x => x.Name.Equals("Nitro Booster")))
            {
                _ = mattbot.AutoMod.CreateColorRole(arg2);
            }

            // Siginal the automod if someone stopped boosting the server
            IEnumerable<SocketRole> removedRoles = arg1.Value.Roles.Except(arg2.Roles);
            if (removedRoles.Any(x => x.Name.Equals("Nitro Booster")))
            {
                _ = mattbot.AutoMod.DeleteColorRole(arg2);
            }
        }

        return Task.CompletedTask;
    }

    private Task ClientOnUserUpdated(SocketUser arg1, SocketUser arg2)
    {
        // Make sure the name actually changed
        if (!arg1.Username.Equals(arg2.Username) || !arg1.Discriminator.Equals(arg2.Discriminator))
        {
            // Log the name change
            _ = mattbot.Logger.LogNameChange(arg1, arg2);

            // Siginal the automod because someone changed their username
            foreach (SocketGuild guild in arg2.MutualGuilds)
            {
                SocketGuildUser user = guild.GetUser(arg2.Id);
                SocketRole bRole = user.Roles.FirstOrDefault(x => x.Equals("Nitro Booster"));
                if ((guild.Id == CCDC_ID || guild.Id == CYBERPATRIOT_ID) && (bRole != null || user.GuildPermissions.Has(GuildPermission.BanMembers)))
                {
                    _ = mattbot.AutoMod.UpdateColorRole(arg1, user);
                }
            }
        }

        return Task.CompletedTask;
    }

    private Task ClientOnUserVoiceStateUpdated(SocketUser arg1, SocketVoiceState arg2, SocketVoiceState arg3)
    {
        if (arg2.VoiceChannel == null && arg3.VoiceChannel != null)
        {
            // Log the voice join
            if (!arg1.IsBot) // ignore bots
                _ = mattbot.Logger.LogVoiceJoin(arg1, arg2, arg3);
        }
        else if (arg2.VoiceChannel != null && arg3.VoiceChannel != null && arg2.VoiceChannel != arg3.VoiceChannel)
        {
            // Log the voice move
            if (!arg1.IsBot) // ignore bots
                _ = mattbot.Logger.LogVoiceMove(arg1, arg2, arg3);
        }
        else if (arg2.VoiceChannel != null && arg3.VoiceChannel == null)
        {
            // Log the voice leave
            if (!arg1.IsBot) // ignore bots
                _ = mattbot.Logger.LogVoiceLeave(arg1, arg2, arg3);
        }

        return Task.CompletedTask;
    }

    private Task ClientOnReactionAdded(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction arg3)
    {
        // todo
        if (ReactionAdded is not null)
            _ = ReactionAdded(arg1, arg2, arg3);

        // Siginal the automod because a gem reaction was added
        Emoji gem = new("\uD83D\uDC8E"); // 💎
        if (arg3.Emote.Equals(gem))
        {
            _ = mattbot.AutoMod.GemMessage(arg1, arg2, arg3);
        }

        // Siginal the automod because a wastebasket reaction was added
        Emoji wastebasket = new("\uD83D\uDDD1\uFE0F"); // 🗑️
        if (arg3.Emote.Equals(wastebasket))
        {
            _ = mattbot.AutoMod.DeleteMessage(arg1, arg2, arg3);
        }

        // Siginal the automod because a 1984 reaction was added
        Emote camera = Emote.Parse("<:1984:1025604468559061042>");
        if (arg3.Emote.Equals(camera))
        {
            _ = mattbot.AutoMod.CrowdmuteUser(arg1, arg2, arg3);
        }

        return Task.CompletedTask;
    }

    private Task ClientOnReady()
    {
        if (Ready is not null)
            _ = Ready(_client);
        return Task.CompletedTask;
    }
}
