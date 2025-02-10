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

using Discord.Rest;
using Discord.WebSocket;
using mattbot.utils;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;


namespace mattbot.automod;

/*
 * Modelled after jagrosh's AutoMod in Vortex
 */
public class AutoMod
{
    private readonly MattBot mattbot;

    public AutoMod(MattBot mattbot)
    {
        this.mattbot = mattbot;
    }

    public async Task UserJoin(IGuildUser user)
    {
        // completely ignore bots
        if(user.IsBot)
            return;

        GoogleCredential credential;
        using (FileStream stream = new("mattbot-387506-f9b988a3302e.json", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
        }
        SheetsService service = new(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "MattBot"
        });

        bool kicking = false;
        IList<IList<object>> values = service.Spreadsheets.Values.Get("1omqGLkegUgYs_g28zLm51cd8wwncqamF9_aUtnfrqFw", "Restricted!A2:A100").Execute().Values;
        if (values is null || values.Count == 0)
            return;

        foreach (IList<object> row in values)
        {
            if (row.Count == 0)
                continue;
            if (row[0].ToString() == user.Id.ToString())
            {
                kicking = true;
            }
        }

        if (kicking)
        {
            try
            {
                await user.SendMessageAsync($"Sorry, you are restricted from joining **{user.Guild.Name}** because of your membership in the Donkey Dojo. "
                                            + "You may appeal your restriction [here](<https://unban.cypat.gg>). Sorry for the inconvenience.");
                await user.KickAsync($"Restricted User");
            }
            catch (Exception) { }
        }


        if (user.Guild.Id == ECITADEL_ID)
        {
            values = service.Spreadsheets.Values.Get("18LFfUXsLTsXmljfkOb4qf33ptiNs_aPju09A87l-rTc", "Competitors!A1:A500").Execute().Values;
            if (values is null || values.Count == 0)
                return;

            foreach (IList<object> row in values)
            {
                if (row.Count == 0)
                    continue;
                
                if (row[0].ToString() == user.Id.ToString())
                {
                    IRole competitor23 = user.Guild.GetRole(1112957783848001666);
                    try
                    {
                        await user.AddRoleAsync(competitor23, new() { AuditLogReason = "Restoring 2023 Competitor Role" });
                        return;
                    }
                    catch (Exception) { }
                }
            }

            values = service.Spreadsheets.Values.Get("1utcTxVhse_COubLlGSJ06_ZV73IT6Xk5DYV2EPAjb4g", "Competitors!A1:A600").Execute().Values;
            if (values is null || values.Count == 0)
                return;

            foreach (IList<object> row in values)
            {
                if (row.Count == 0)
                    continue;

                if (row[0].ToString() == user.Id.ToString())
                {
                    IRole competitor24 = user.Guild.GetRole(1241976115610910812);
                    try
                    {
                        await user.AddRoleAsync(competitor24, new() { AuditLogReason = "Restoring 2024 Competitor Role" });
                        return;
                    }
                    catch (Exception) { }
                }
            }
        }
        else if (user.Guild.Id == FINALISTS_ID)
        {
            IRole quarantined = user.Guild.GetRole(1070511023523639406);
            try
            {
                await user.AddRoleAsync(quarantined, new() { AuditLogReason = "Quarantining New User" });
            }
            catch (Exception) { }

            string[] ranges = { "CP-17!E3:E37", "CP-17!G3:G37", "CP-17!I3:I37", "CP-17!K3:K37", "CP-17!M3:M37", "CP-17!O3:O37", "CP-17!Q3:Q37", "CP-17!S3:S37" };
            foreach (string range in ranges)
            {
                values = service.Spreadsheets.Values.Get("1Hdi3Hrr-R-ipUOYjjH-2g3RzBsRJQpGwoRFWTcrT9DY", range).Execute().Values;
                if (values is null || values.Count == 0)
                    continue;

                foreach (IList<object> row in values)
                {
                    if (row.Count == 0)
                        continue;

                    if (row[0].ToString() == user.Id.ToString())
                    {
                        try
                        {
                            IRole cp17 = user.Guild.GetRole(1334333263992459286);
                            await user.AddRoleAsync(cp17, new() { AuditLogReason = "Verified New User" });
                            await user.RemoveRoleAsync(quarantined, new() { AuditLogReason = "Unquarantining New User" });
                            return;
                        }
                        catch (Exception) { }
                    }
                }
            }
        }
    }

    public async Task CreateColorRole(SocketGuildUser user)
    {
        if (!user.Guild.CurrentUser.GuildPermissions.Has(GuildPermission.ManageRoles))
            return;

        SocketRole existing = user.Roles.Where(x => x.Name.Equals(user.Username)).FirstOrDefault();
        if (existing is not null)
            return;

        try
        {
            RestRole cRole = await user.Guild.CreateRoleAsync(user.Username, permissions: GuildPermissions.None);
            int position = user.Guild.Roles.FirstOrDefault(x => x.Name.Equals("Nitro Booster")).Position + (user.Guild.Id == CYBERPATRIOT_ID ? 4 : 1);
            await cRole.ModifyAsync(x => x.Position = position, new() { AuditLogReason = "Creating New Color Role" });
            await user.AddRoleAsync(cRole, new() { AuditLogReason = "Adding New Color Role" });
        }
        catch (Exception) { }
    }

    public async Task UpdateColorRole(SocketUser before, SocketGuildUser after)
    {
        SocketRole cRole = after.Roles.Where(x => x.Name.Equals(before.Username)).FirstOrDefault();
        if (cRole is null)
            return;

        try
        {
            await cRole.ModifyAsync(x => x.Name.Equals(after.Username), new() { AuditLogReason = "Updating Color Role" });
        }
        catch (Exception) { }
    }

    private bool ShouldDeleteColorRole(SocketGuildUser user)
    {
        // users not in the guild
        if (user.Guild.GetUser(user.Id) == null)
            return true;

        // ignore users the bot cant interact with
        if (!PermissionUtil.CanInteract(user.Guild.CurrentUser, user))
            return false;

        // ignore users that can ban
        if (user.GuildPermissions.Has(GuildPermission.BanMembers))
            return false;

        return true;
    }

    public async Task DeleteColorRole(SocketGuildUser user)
    {
        if (!user.Guild.CurrentUser.GuildPermissions.Has(GuildPermission.ManageRoles))
            return;

        if (!ShouldDeleteColorRole(user))
            return;

        SocketRole cRole = user.Roles.Where(x => x.Name.Equals(user.Username)).FirstOrDefault();
        if (cRole is null)
            return;

        try
        {
            await cRole.DeleteAsync(new() { AuditLogReason = "Deleting Color Role" });
        }
        catch (Exception) { }
    }

    private bool ShouldPerformAutomod(SocketGuildUser member, ISocketMessageChannel channel)
    {
        // ignore users not in the guild
        if (member.Guild.GetUser(member.Id) == null)
            return false;

        // ignore broken guilds
        if (member.Guild.Owner == null)
            return false;

        // ignore bots
        if (member.IsBot)
            return false;

        // ignore users the bot cant interact with
        if (!PermissionUtil.CanInteract(member.Guild.CurrentUser, member))
            return false;

        // ignore users that can kick
        if (member.GuildPermissions.Has(GuildPermission.KickMembers))
            return false;

        // ignore users that can ban
        if (member.GuildPermissions.Has(GuildPermission.BanMembers))
            return false;

        // ignore users that can manage server
        if (member.GuildPermissions.Has(GuildPermission.ManageGuild))
            return false;

        // if a channel is specified, ignore users that can manange messages in that channel
        if (channel != null && member.GetPermissions(channel as IGuildChannel).Has(ChannelPermission.ManageMessages))
            return false;

        return true;
    }

    public async Task PerformAutomod(SocketMessage message)
    {
        // ignore users with Manage Messages, Kick Members, Ban Members, Manage Server, or anyone the bot can't interact with
        if (!ShouldPerformAutomod(message.Author as SocketGuildUser, message.Channel))
            return;

        // check the channel for channel-specific settings
        string topic = (message.Channel as SocketTextChannel).Topic;
        Boolean allowDiscussion = topic == null || !topic.ToLower().Contains("{-discussion}");

        // detect status of different automod features
        AutomodStatus currentStatus = new();
        if (!allowDiscussion)
        {
            await RunAntiDiscussion(currentStatus, message);
        }
        await RunFilters(currentStatus, message);

        // delete the message if applicable
        if (currentStatus.deleteMessage)
        {
            try
            {
                await message.DeleteAsync();
            }
            catch (Exception) { }
        }

        // send a short 'warning' message that self-deletes
        if (currentStatus.channelWarning != null)
        {
            try
            {
                IUserMessage msg = await message.Channel.SendMessageAsync($"{message.Author.Mention} {WARN} {currentStatus.channelWarning}");
                await Task.Delay(2500);
                await msg.DeleteAsync();
            }
            catch (Exception) { }
        }
    }

    private async Task RunAntiDiscussion(AutomodStatus currentStatus, SocketMessage message)
    {
        string imageurl;
        imageurl = message.Attachments?.FirstOrDefault()?.ProxyUrl;
        if (imageurl is null)
        {
            currentStatus.AppendReason("Message contained no images");
            currentStatus.channelWarning = "Please only post images in this channel.";
            currentStatus.DeleteMessage();
        }
    }

    private async Task RunFilters(AutomodStatus currentStatus, SocketMessage message)
    {
    }

    private bool ShouldGemMessage()
    {
        return true;
    }

    public async Task GemMessage(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        if (!ShouldGemMessage())
            return;

        // get the settings
        // Am I finally to have to add a database?
        // int.TryParse(_configuration["gem_threshold"], out int GEM_THRESHOLD);
        int gem_threshold = 4;
        // int.TryParse(_configuration["gem_duration"], out int GEM_DURATION);
        int gem_duration = 2;
    }

    private bool ShouldReportMessage()
    {
        return true;
    }

    public async Task DeleteMessage(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        if (!ShouldReportMessage())
            return;
    }

    private bool ShouldCrowdmuteUser()
    {
        return true;
    }

    public async Task CrowdmuteUser(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
    {
        if (!ShouldCrowdmuteUser())
            return; 
    }

    private class AutomodStatus
    {
        public bool deleteMessage = false;
        public string channelWarning = null;
        public StringBuilder reason = new StringBuilder();

        public void DeleteMessage()
        {
            this.deleteMessage = true;
        }

        public void AppendReason(String input)
        {
            reason.Append(", ").Append(input);
        }
    }
}
