using mattbot.services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using mattbot.utils;
using Discord.WebSocket;
using Discord.Rest;
using Google.Apis.Sheets.v4.Data;

namespace mattbot.automod
{
    public class Users
    {
        private readonly Listener _listener;

        public Users(Listener listener)
        {
            _listener = listener;
        }

        public async Task InitializeAsync()
        {
            _listener.UserJoined += OnUserJoinedAsync;
            _listener.UserLeft += OnUserLeftAsync;
            _listener.UserUpdated += OnUserUpdatedAsync;
            _listener.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
        }

        private async Task OnUserJoinedAsync(IGuildUser arg)
        {
            // Assign roles upon joining CyberDiscord
            if (arg.Guild.Id == CYBERDISCORD_ID)
            {
                string[] scopes = { SheetsService.Scope.SpreadsheetsReadonly };
                GoogleCredential credential;
                using (FileStream stream = new FileStream("mattbot-387506-f9b988a3302e.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
                }

                string spreadsheetId = "18LFfUXsLTsXmljfkOb4qf33ptiNs_aPju09A87l-rTc";
                string[] ranges = { "Form Responses 1!F2:F200", "Form Responses 1!J2:J200", "Form Responses 1!N2:N200", "Form Responses 1!R2:R200" };

                SheetsService service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "MattBot"
                });

                foreach (string range in ranges)
                {
                    SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetId, range);
                    ValueRange response = request.Execute();
                    IList<IList<object>> values = response.Values;

                    if (values != null && values.Count > 0)
                    {
                        foreach (IList<object> row in values)
                        {
                            if (row.Count > 0 && row[0].ToString() == arg.Id.ToString())
                            {
                                IRole competitor23 = arg.Guild.Roles.FirstOrDefault(x => x.Name == "2023 Competitor");
                                if (competitor23 == null)
                                    return;
                                await arg.AddRoleAsync(competitor23);
                                break;
                            }
                        }
                    }
                }
            }

            // Assign roles upon joining CyberPatriot Finalists
            if (arg.Guild.Id == FINALISTS_ID)
            {
                IRole unverified = arg.Guild.Roles.FirstOrDefault(x => x.Name == "Unverified");
                if (unverified == null)
                    return;
                await arg.AddRoleAsync(unverified);

                ITextChannel tc = (await arg.Guild.GetTextChannelsAsync()).FirstOrDefault(x => x.Name == "serverlog");
                if (tc == null)
                    return;
                DateTimeOffset now = DateTimeOffset.UtcNow;
                await tc.SendMessageAsync($"{ERROR} Could not verify {FormatUtil.formatFullUser(arg)}!");
            }
        }

        private async Task OnGuildMemberUpdatedAsync(Cacheable<SocketGuildUser, ulong> before, SocketGuildUser after)
        {
            ulong guildID = after.Guild.Id;
            if (guildID == CYBERPATRIOT_ID || guildID == CCDC_ID)
            {
                // Create color role upon boosting the server
                IEnumerable<SocketRole> addedRoles = after.Roles.Except(before.Value.Roles);
                if (addedRoles.Any())
                {
                    foreach (SocketRole role in addedRoles)
                    {
                        SocketRole staffRole = after.Roles.Where(r => r.Name == after.Username).FirstOrDefault();
                        if (role.Name == "Nitro Booster" && staffRole == null)
                        {
                            RestRole colorRole = await after.Guild.CreateRoleAsync(after.Username, permissions: GuildPermissions.None);

                            await after.AddRoleAsync(colorRole);

                            int position = guildID == CYBERPATRIOT_ID ? 4 : 6;
                            await colorRole.ModifyAsync(x => x.Position = role.Position + position);
                            break;
                        }
                    }
                }

                // Remove color role if no longer boosting
                IEnumerable<SocketRole> removedRoles = before.Value.Roles.Except(after.Roles);
                if (removedRoles.Any())
                {
                    foreach (SocketRole role in removedRoles)
                    {
                        if (role.Name == "Nitro Booster" && !after.GuildPermissions.Has(GuildPermission.BanMembers))
                        {
                            SocketRole colorRole = after.Roles.Where(r => r.Name == after.Username).FirstOrDefault();

                            if (colorRole != null)
                                await colorRole.DeleteAsync();

                            break;
                        }
                    }
                }
            }
        }

        private async Task OnUserUpdatedAsync(SocketUser arg1, SocketUser arg2)
        {
            // Handle username changes
            if (arg1.Username != arg2.Username)
            {
                SocketGuildUser user = arg2 as SocketGuildUser;
                foreach (SocketGuild guild in user.MutualGuilds)
                {
                    if (guild.Id == CYBERPATRIOT_ID || guild.Id == CCDC_ID)
                    {
                        if (user.Roles.FirstOrDefault(x => x.Name == "Nitro Booster") != null || user.GuildPermissions.Has(GuildPermission.BanMembers))
                        {
                            SocketRole colorRole = user.Roles.Where(x => x.Name.Equals(arg1.Username)).FirstOrDefault();

                            if (colorRole != null)
                                await colorRole.ModifyAsync(x => x.Name = arg2.Username);
                        }
                    }
                }
            }
        }

        private async Task OnUserLeftAsync(IGuild arg1, IUser arg2)
        {
            // Remove color role when leaving the server
            if (arg1.Id == CYBERPATRIOT_ID || arg1.Id == CCDC_ID)
            {
                SocketGuildUser user = arg2 as SocketGuildUser;
                if (user.Roles.FirstOrDefault(x => x.Name == "Nitro Booster") != null)
                {
                    SocketRole colorRole = user.Roles.Where(x => x.Name.Equals(arg2.Username)).FirstOrDefault();

                    if (colorRole != null)
                        await colorRole.DeleteAsync();
                }
            }

            // Unwhitelist user upon leaving CyberPatriot
            if (arg1.Id == CYBERPATRIOT_ID)
            {
                SocketGuildUser user = arg2 as SocketGuildUser;
                if (user.Roles.FirstOrDefault(x => x.Name == "MC Registered") != null)
                {
                    // Todo
                }
            }
        }
    }
}
