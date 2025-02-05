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
using Color = Discord.Color;

namespace mattbot.modules.general;

[Ignore]
[Group("matt", "matt bucks commands")]
public class MattBucksModule : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly string CONSTRUCTION = "\uD83D\uDEA7"; // 🚧
    private static readonly string SWORD = "\u2694\uFE0F"; // ⚔️
    private static readonly string WATER = "\uD83D\uDCA7"; // 💧

    private static readonly string NITRO = "<:nitro:1091272926881390634>";
    private static readonly string PING = "<:ping:1091272927737036953>";
    private static readonly string MATTBUCKS = "<:MattBucks:1091275681024970853>";

    [SlashCommand("explanation", "explains what MattBucks are")]
    public async Task ExplanationAsync()
    {
        await RespondAsync($"{Context.User.Mention}, MattBucks are a virtual currency that can be earned and redeemed!\n\n" +
            $"Commands:\n" +
            $"`/matt bucks` - shows how many MattBucks you have\n" +
            $"`/matt richest` - shows the richest users\n" +
            $"`/matt shop` - shows the available redeemables\n" +
            $"`/matt purchase <item>` - buys an item from the shop\n\n" +
            $"Ways to earn MattBucks:\n" +
            $"`1.` __Nitro Boosting__ ~ Every 6 hours, anyone boosting any of the following servers gets 1 MattBuck\n" +
            $"\t\t\\- *CyberPatriot* (<{CYBERPATRIOT_SERVER_INVITE}>)\n" +
            $"\t\t\\- *CCDC* (<{CCDC_SERVER_INVITE}>)\n" +
            $"`2.` __Twitch__ ~ Every 6 hours, anyone subscribed to matt's Twitch channel gets 1 MattBuck\n" +
            $"`3.` __Giveaways__ ~ Sometimes matt will just give away MattBucks randomly\n" +
            $"Note: All the above methods stack; if you're boosting in Discord and subscribed on Twitch, you'll earn 2 MattBucks every 6 hours.", ephemeral: true);
    }

    [SlashCommand("shop", "shows the available redeemables")]
    public async Task GetShopAsync()
    {
        // Filters out any roles that are default color
        IEnumerable<SocketRole> filterOutDefault = Context.Guild.CurrentUser.Roles.Where(r => r.Color != Color.Default);

        // Grabs the highest role and the color for it
        Color botHighestRoleColor = Color.Default;
        if (!filterOutDefault.Count().Equals(0))
            botHighestRoleColor = filterOutDefault.MaxBy(r => r.Position).Color;

        EmbedBuilder eb = new EmbedBuilder().WithTitle("MattBucks Shop Inventory")
                                    .WithColor(botHighestRoleColor)
                                    .AddField($"{SWORD} 1v1 Matt", "MattBucks Price: `100`\nPurchase Code: `1v1`\n1v1 matt in Valorant, League, TFT, Minecraft, Tetris or Chess", true)
                                    .AddField($"{NITRO} 1 Month Discord Nitro", "MattBucks Price: `1500`\nPurchase Code: `nitro`\nOne month of Discord Nitro, delivered via Discord", true)
                                    .AddField($"{WATER} 1 Bottle of AFA Water", "MattBucks Price: `50000`\nPurchase Code: `water`\nOne bottle of AFA water, shipped directly to you", true)
                                    .AddField($"{PING} Custom @everyone", "MattBucks Price: `100000`\nPurchase Code: `ping`\nCustomized @everyone message in #announcements", true)
                                    .WithFooter("For full help, use /matt explanation");

        await RespondAsync(embed: eb.Build(), allowedMentions: AllowedMentions.None, ephemeral: true);
    }

    [SlashCommand("bucks", "shows how many MattBucks you have")]
    public async Task CheckCoinsAsync()
    {
        await RespondAsync($"{Context.User.Mention}, you have `1` MattBucks! {MATTBUCKS}", ephemeral: true);
    }

    [SlashCommand("purchase", "buys an item from the shop")]
    public async Task PurchaseAsync()
    {
        await RespondAsync($"{CONSTRUCTION} Uh oh, we're still getting stuff set up here, check back later! {CONSTRUCTION}", ephemeral: true);
    }

    [SlashCommand("richest", "shows the richest users")]
    public async Task CheckRichestAsync()
    {
        await RespondAsync($"{CONSTRUCTION} Uh oh, we're still getting stuff set up here, check back later! {CONSTRUCTION}", ephemeral: true);
    }

    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("modify", "modify a user's MattBucks")]
    public async Task ModifyAsync()
    {
        await RespondAsync($"Adding 1 mattbucks...\n@user: 1");
    }
}
