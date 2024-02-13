﻿using Discord.Interactions;
using Discord.WebSocket;
using mattbot.utils;

namespace mattbot.modules.owner
{
    [CyberPatriot]
    [EnabledInDm(false)]
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    public class VerifyModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("verify", "Verify a user")]
        public async Task VerifyAsync([Summary("id", "List of user IDs")] string userIDs,
                                      [Choice("National Finalist", 0),
                                       Choice("International Finalist", 1),
                                       Choice("Finalist Coach/Mentor", 2)]
                                      [Summary("verify-as", "The type of verification. Leave blank for National Finalist")] int verificationType = 0)
        {
            StringBuilder sb = new StringBuilder();
            await RespondAsync($"{LOADING} Verifying users...\n");

            foreach (string userID in userIDs.Split(' '))
            {
                SocketGuildUser user = Context.Client.GetGuild(CYBERPATRIOT_ID).GetUser(ulong.Parse(userID));
                if (user == null)
                {
                    sb.Append($"{ERROR} Could not find {FormatUtil.formatFullUser(user)}\n");
                    continue;
                }

                switch (verificationType)
                {
                    case 0:
                        SocketRole nationalFinalist = Context.Guild.GetRole(567746329057427456);
                        await user.AddRoleAsync(nationalFinalist);
                        break;
                    case 1:
                        SocketRole internationalFinalist = Context.Guild.GetRole(939197252512149626);
                        await user.AddRoleAsync(internationalFinalist);
                        break;
                    case 2:
                        break;
                }

                SocketGuild finalists = Context.Client.GetGuild(FINALISTS_ID);
                if (finalists.GetUser(user.Id) != null)
                {
                    SocketRole cpxvi = finalists.GetRole(1201674110477013032);
                    await finalists.GetUser(user.Id).AddRoleAsync(cpxvi);
                    sb.Append($"{SUCCESS} Verified {FormatUtil.formatFullUser(user)}\n");
                }
                else
                {
                    IInviteMetadata invite = await finalists.GetTextChannel(546405227092508685).CreateInviteAsync(maxAge: 86400, maxUses: 1, isUnique: true);
                    try
                    {
                        await user.SendMessageAsync($"You have been succesfully verified as a finalist! You may join the **{finalists.Name}** server here:\n{invite}\n\n"
                                                    + "This invite is valid for 24 hours following receipt of this message.");
                        sb.Append($"{SUCCESS} Verified {FormatUtil.formatFullUser(user)}\n");
                    }
                    catch (Exception)
                    {
                        sb.Append($"{ERROR} Could not invite {FormatUtil.formatFullUser(user)}\n");
                        await invite.DeleteAsync();
                    }
                }
            }
            await ModifyOriginalResponseAsync(msg => msg.Content = $"{sb}");
        }
    }
}
