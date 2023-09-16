using Discord.Interactions;

using Color = Discord.Color;

namespace MattBot.Modules.General
{
    [Ignore]
    [EnabledInDm(false)]
    [Group("matt", "matt coins commands")]
    public class MattCoinsModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("explanation", "explains what MattCoins are")]
        public async Task ExplanationAsync()
        {
            await RespondAsync($"{Context.User.Mention}, MattCoins are a virtual currency that can be earned and redeemed!\n\n" +
                $"Commands:\n" +
                $"`/matt coins` - shows how many MattCoins you have\n" +
                $"`/matt richest` - shows the richest users\n" +
                $"`/matt shop` - shows the available redeemables\n" +
                $"`/matt purchase <item>` - buys an item from the shop\n\n" +
                $"Ways to earn MattCoins:\n" +
                $"`1.` __Nitro Boosting__ ~ Every 6 hours, anyone boosting any of the following servers gets 1 MattBuck\n" +
                $"\t\t- *CyberPatriot* (<{CYBERPATRIOT_INVITE}>)\n" +
                $"\t\t- *CCDC* (<{CCDC_INVITE}>)\n" +
                $"`2.` __Twitch__ ~ Every 6 hours, anyone subscribed to matt's Twitch channel gets 1 MattBuck\n" +
                $"`3.` __Giveaways__ ~ Sometimes matt will just give away MattCoins randomly\n" +
                $"Note: All the above methods stack; if you're boosting in Discord and subscribed on Twitch, you'll earn 2 MattCoins every 6 hours.", ephemeral: true);
        }

        [SlashCommand("shop", "shows the available redeemables")]
        public async Task GetShopAsync()
        {
            // Filters out any roles that are default color
            var filterOutDefault = Context.Guild.CurrentUser.Roles.Where(r => r.Color != Color.Default);

            // Grabs the highest role and the color for it
            Color botHighestRoleColor = Color.Default;
            if (!filterOutDefault.Count().Equals(0))
                botHighestRoleColor = filterOutDefault.MaxBy(r => r.Position).Color;

            var eb = new EmbedBuilder().WithTitle("MattCoins Shop Inventory")
                                        .WithColor(botHighestRoleColor)
                                        .AddField($"{SWORD} 1v1 Matt", "MattCoins Price: `100`\nPurchase Code: `1v1`\n1v1 matt in Valorant, League, TFT, Minecraft, Tetris or Chess", true)
                                        .AddField($"{NITRO} 1 Month Discord Nitro", "MattCoins Price: `1500`\nPurchase Code: `nitro`\nOne month of Discord Nitro, delivered via Discord", true)
                                        .AddField($"{WATER} 1 Bottle of AFA Water", "MattCoins Price: `50000`\nPurchase Code: `water`\nOne bottle of AFA water, shipped directly to you", true)
                                        .AddField($"{PING} Custom @everyone", "MattCoins Price: `100000`\nPurchase Code: `ping`\nCustomized @everyone message in #announcements", true)
                                        .WithFooter("For full help, use /matt explanation");

            await RespondAsync(embed: eb.Build(), allowedMentions: AllowedMentions.None, ephemeral: true);
        }

        [SlashCommand("coins", "shows how many MattCoins you have")]
        public async Task CheckCoinsAsync()
        {
            await RespondAsync($"{CONSTRUCTION} Uh oh, we're still getting stuff set up here, check back later! {CONSTRUCTION}", ephemeral: true);
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
    }
}
