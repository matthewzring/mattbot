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

using Discord.Interactions;
using Discord.WebSocket;
using mattbot.utils;

namespace mattbot.modules.owner;

[CyberPatriot]
[DefaultMemberPermissions(GuildPermission.Administrator)]
[RequireTeam]
public class VerifyModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("verify", "Verify a user")]
    public async Task VerifyCyberPatriotAsync([Summary("id", "List of user IDs")] string userIDs,
                                  [Choice("National Finalist", "national-finalist"),
                                   Choice("International Finalist", "international-finalist"),
                                   Choice("Finalist Coach/Mentor", "coach-mentor")]
                                  [Summary("verify-as", "The type of verification. Leave blank for National Finalist")] string verificationType = "national-finalist")
    {
        StringBuilder sb = new StringBuilder();
        await RespondAsync($"{LOADING} Verifying users...\n");

        foreach (string userID in userIDs.Split(' '))
        {
            SocketGuildUser user = Context.Guild.GetUser(ulong.Parse(userID));
            if (user == null)
            {
                sb.Append($"{ERROR} Could not find `{userID}`\n");
                continue;
            }

            switch (verificationType)
            {
                case "national-finalist":
                    SocketRole nationalFinalist = Context.Guild.GetRole(567746329057427456);
                    await user.AddRoleAsync(nationalFinalist);
                    break;
                case "international-finalist":
                    SocketRole internationalFinalist = Context.Guild.GetRole(939197252512149626);
                    await user.AddRoleAsync(internationalFinalist);
                    break;
                case "coach-mentor":
                    break;
                default:
                    break;
            }

            SocketGuild finalists = Context.Client.GetGuild(FINALISTS_ID);
            if (finalists.GetUser(user.Id) != null)
            {
                SocketRole cp17 = finalists.GetRole(1334333263992459286);
                await finalists.GetUser(user.Id).AddRoleAsync(cp17);
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

[CCDC]
[DefaultMemberPermissions(GuildPermission.Administrator)]
[RequireTeam]
public class Verify2Module : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("verify2", "Verify a user")]
    public async Task VerifyCCDCAsync([Summary("id", "List of user IDs")] string userIDs,
                                  [Choice("Nationals 2025", "nationals-2025"),
                                   Choice("Nationals 2024", "nationals-2024")]
                                  [Summary("verify-as", "The type of verification.")] string verificationType = "nationals-2025")
    {
        StringBuilder sb = new StringBuilder();
        await RespondAsync($"{LOADING} Verifying users...\n");

        foreach (string userID in userIDs.Split(' '))
        {
            SocketGuildUser user = Context.Guild.GetUser(ulong.Parse(userID));
            if (user == null)
            {
                sb.Append($"{ERROR} Could not find {FormatUtil.formatFullUser(user)}\n");
                continue;
            }

            switch (verificationType)
            {
                case "nationals-2025":
                    SocketRole nationals2025 = Context.Guild.GetRole(1354250612535066795);
                    await user.AddRoleAsync(nationals2025);
                    sb.Append($"{SUCCESS} Role *Nationals 2025* given to {FormatUtil.formatUser(user)}\n");
                    break;
                case "nationals-2024":
                    SocketRole nationals2024 = Context.Guild.GetRole(1230979011543568559);
                    await user.AddRoleAsync(nationals2024);
                    sb.Append($"{SUCCESS} Role *Nationals 2024* given to {FormatUtil.formatUser(user)}\n");
                    break;
                default:
                    break;
            }
        }

        await ModifyOriginalResponseAsync(msg => msg.Content = $"{sb}");
    }
}