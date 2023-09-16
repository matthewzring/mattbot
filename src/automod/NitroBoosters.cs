using Discord.WebSocket;
using mattbot.services;

namespace mattbot.automod
{
    public class NitroBoosters
    {
        private readonly Listener _listener;

        public NitroBoosters(Listener listener)
        {
            _listener = listener;
        }

        public async Task InitializeAsync()
        {
            _listener.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
            _listener.UserUpdated += OnUserUpdatedAsync;
        }

        private async Task OnGuildMemberUpdatedAsync(Cacheable<SocketGuildUser, ulong> before, SocketGuildUser after)
        {
            var guildID = after.Guild.Id;
            if (guildID == CYBERPATRIOT_ID || guildID == CCDC_ID)
            {
                var addedRoles = after.Roles.Except(before.Value.Roles);
                if (addedRoles.Any())
                {
                    foreach (var role in addedRoles)
                    {
                        SocketRole staffRole = after.Roles.Where(r => r.Name == after.Username).FirstOrDefault();
                        if (role.Name == "Nitro Booster" && staffRole == null)
                        {
                            var colorRole = await after.Guild.CreateRoleAsync(after.Username, permissions: GuildPermissions.None);

                            await after.AddRoleAsync(colorRole);

                            int position = guildID == CYBERPATRIOT_ID ? 4 : 6;
                            await colorRole.ModifyAsync(x => x.Position = role.Position + position);
                            break;
                        }
                    }
                }

                var removedRoles = before.Value.Roles.Except(after.Roles);
                if (removedRoles.Any())
                {
                    foreach (var role in removedRoles)
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
            if (arg1.Username != arg2.Username)
            {
                var user = arg2 as SocketGuildUser;

                foreach (var guild in user.MutualGuilds)
                {
                    if (guild.Id == CYBERPATRIOT_ID || guild.Id == CCDC_ID)
                    {
                        if (user.Roles.FirstOrDefault(x => x.Name == "Nitro Booster") != null || user.GuildPermissions.Has(GuildPermission.BanMembers))
                        {
                            var colorRole = user.Roles.Where(x => x.Name.Contains(arg1.Username)).FirstOrDefault();

                            if (colorRole != null)
                                await colorRole.ModifyAsync(x => x.Name = arg2.Username);
                        }
                    }
                }
            }
        }
    }
}
