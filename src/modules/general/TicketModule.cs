/*
 * Copyright 2023-2026 Matthew Ring
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
using System.Text.Json;

namespace mattbot.modules.general;

internal static class TicketShared
{
    public const ulong TICKET_CHANNEL_ID = 1508801207202873435;
    public const ulong STAFF_MATT_ID = 349007194768801792;
    public const ulong STAFF_KALI_ID = 332967486540611585;

    public const string WELCOME_TEXT =
        "**👋 Welcome to your new ticket!**\n\n" +
        "**⏲️ We'll be here soon!** Typically we respond in a few minutes, but sometimes we might take a bit longer if the server is busy or if you have a particularly tricky issue.\n\n" +
        "**⏱️ We close idle tickets,** which makes them read-only. Once a ticket is closed it won't be reopened, but you can always create a new ticket if you have another issue.\n\n" +
        "**📝 Have more to share?** If you have not fully explained your issue, please do so now. You may add more details, screenshots, videos, etc. below.";

    public static async Task<JsonElement?> FetchPortalDataAsync(IConfiguration configuration)
    {
        try
        {
            HttpClient httpClient = new HttpClient();
            string apiUrl = "https://portal.ecitadel.org/auth/competitor/data/";
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", configuration["token"]);
            var response = await httpClient.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode)
                return null;

            var content = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(content).RootElement.GetProperty("data");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<(string teamId, List<string> competitors)> LookupTeamByUserAsync(IConfiguration configuration, ulong userId)
    {
        var data = await FetchPortalDataAsync(configuration);
        if (data == null) return (null, null);

        foreach (var team in data.Value.EnumerateArray())
        {
            var competitors = team.GetProperty("competitors")
                .EnumerateArray()
                .Select(c => c.GetString())
                .ToList();

            if (!competitors.Contains(userId.ToString())) continue;

            return (team.GetProperty("id").GetInt32().ToString(), competitors);
        }
        return (null, null);
    }

    public static async Task<(string teamId, List<string> competitors)> LookupTeamByIdAsync(IConfiguration configuration, int teamId)
    {
        var data = await FetchPortalDataAsync(configuration);
        if (data == null) return (null, null);

        foreach (var team in data.Value.EnumerateArray())
        {
            if (team.GetProperty("id").GetInt32() != teamId) continue;

            var competitors = team.GetProperty("competitors")
                .EnumerateArray()
                .Select(c => c.GetString())
                .ToList();
            return (teamId.ToString(), competitors);
        }
        return (null, null);
    }

    public static async Task<List<string>> LookupCompetitorsByTeamAsync(IConfiguration configuration, string teamId)
    {
        if (!int.TryParse(teamId, out var parsed)) return null;
        var (_, competitors) = await LookupTeamByIdAsync(configuration, parsed);
        return competitors;
    }

    public static ComponentBuilder BuildAddTeammatesComponents(string teamId)
        => new ComponentBuilder()
            .WithButton("Add teammates to ticket",
                        customId: $"add-teammates:{teamId}",
                        ButtonStyle.Primary,
                        new Emoji("👥"));
}

[ECitadel]
[CommandContextType(InteractionContextType.Guild)]
[DefaultMemberPermissions(GuildPermission.BanMembers)]
[Group("ticket", "Ticket management")]
public class TicketModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IConfiguration _configuration;
    public TicketModule(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [SlashCommand("message", "Post the ticket entry-point message")]
    public async Task TicketMessageAsync()
    {
        var components = new ComponentBuilder()
            .WithButton("Question",             customId: "ticket-question",     style: ButtonStyle.Primary, emote: new Emoji("❓"), row: 0)
            .WithButton("Team Registration",    customId: "ticket-registration", style: ButtonStyle.Primary, emote: new Emoji("📋"), row: 0)
            .WithButton("Competition Date",     customId: "ticket-date",         style: ButtonStyle.Primary, emote: new Emoji("📅"), row: 0)
            .WithButton("VMRC Console",         customId: "ticket-vmrc",         style: ButtonStyle.Primary, emote: new Emoji("⌨️"), row: 1)
            .WithButton("Reopen Closed Thread", customId: "ticket-reopen",       style: ButtonStyle.Primary, emote: new Emoji("📦"), row: 1);

        await ReplyAsync(
            "# Request help or technical assistance\n" +
            "Use this to open a private conversation with competition organizers.\n" +
            "Get started by selecting the option below that best describes your issue.",
            components: components.Build());
        await RespondAsync("Posted!", ephemeral: true);
    }

    [SlashCommand("create", "Create a ticket for a user or team")]
    public async Task CreateAsync(IUser user = null, int team = 0)
    {
        bool hasUser = user != null;
        bool hasTeam = team != 0;
        if (hasUser == hasTeam)
        {
            await RespondAsync("Provide either a user or a team id!", ephemeral: true);
            return;
        }

        await DeferAsync(ephemeral: true);

        var channel = Context.Guild.GetTextChannel(TicketShared.TICKET_CHANNEL_ID);
        if (channel == null)
        {
            await FollowupAsync("Ticket channel not found.", ephemeral: true);
            return;
        }

        string teamId;
        List<string> competitors;
        string threadName;

        if (hasUser)
        {
            (teamId, competitors) = await TicketShared.LookupTeamByUserAsync(_configuration, user.Id);
            threadName = teamId ?? user.Username;
        }
        else
        {
            (teamId, competitors) = await TicketShared.LookupTeamByIdAsync(_configuration, team);
            if (teamId == null)
            {
                await FollowupAsync($"Team `{team}` not found.", ephemeral: true);
                return;
            }
            threadName = teamId;
        }

        var existing = channel.Threads.FirstOrDefault(t => t.Name == threadName && !t.IsArchived && !t.IsLocked);
        if (existing != null)
        {
            if (hasUser)
            {
                var member = Context.Guild.GetUser(user.Id);
                if (member != null) await existing.AddUserAsync(member);
            }
            await FollowupAsync($"A ticket already exists: {existing.Mention}", ephemeral: true);
            return;
        }

        var thread = await channel.CreateThreadAsync(name: threadName, type: ThreadType.PrivateThread);

        string opening = hasUser
            ? $"{user.Mention} — a ticket was opened for you."
            : $"Ticket opened for team {team}.";
        await thread.SendMessageAsync(opening);

        var welcomeComponents = teamId != null ? TicketShared.BuildAddTeammatesComponents(teamId) : null;
        await thread.SendMessageAsync(TicketShared.WELCOME_TEXT, components: welcomeComponents?.Build());

        if (hasUser)
        {
            var member = Context.Guild.GetUser(user.Id);
            if (member != null) await thread.AddUserAsync(member);
        }
        else
        {
            var captainIdStr = competitors?.FirstOrDefault();
            if (captainIdStr != null && ulong.TryParse(captainIdStr, out var captainId))
            {
                var captain = Context.Guild.GetUser(captainId);
                if (captain != null) await thread.AddUserAsync(captain);
            }
        }

        await thread.AddUserAsync(Context.Guild.GetUser(TicketShared.STAFF_MATT_ID));
        await thread.AddUserAsync(Context.Guild.GetUser(TicketShared.STAFF_KALI_ID));

        await FollowupAsync($"Created {thread.Mention}", ephemeral: true);
    }

    [SlashCommand("close", "Close the current ticket thread")]
    public async Task CloseAsync()
    {
        if (Context.Channel is not SocketThreadChannel thread)
        {
            await RespondAsync("This command must be used inside a ticket thread.", ephemeral: true);
            return;
        }
        if (thread.ParentChannel?.Id != TicketShared.TICKET_CHANNEL_ID)
        {
            await RespondAsync("This command can only be used in a ticket thread.", ephemeral: true);
            return;
        }

        await RespondAsync("Ticket closed.");
        await thread.ModifyAsync(t =>
        {
            t.Locked = true;
            t.Archived = true;
        });
    }
}

[ECitadel]
[CommandContextType(InteractionContextType.Guild)]
public class TicketInteractionsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IConfiguration _configuration;
    public TicketInteractionsModule(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [ComponentInteraction("ticket-question")]
    public async Task HandleQuestionButton()
    {
        var (teamId, _) = await TicketShared.LookupTeamByUserAsync(_configuration, Context.User.Id);

        var channel = Context.Channel as SocketTextChannel;
        var name = teamId ?? Context.User.Username;

        var existing = channel?.Threads.FirstOrDefault(t => t.Name == name && !t.IsArchived && !t.IsLocked);
        if (existing != null)
        {
            if (Context.User is IGuildUser guildUser)
                await existing.AddUserAsync(guildUser);
            await RespondAsync($"Your team has an open ticket: {existing.Mention}", ephemeral: true);
            return;
        }

        var modal = new ModalBuilder()
            .WithTitle("❓ Question")
            .WithCustomId("submit-ticket")
            .AddTextInput("Your Question", "question", TextInputStyle.Paragraph,
                placeholder: "Provide as much detail as possible. You can add more info (screenshots, etc.) after submitting.");
        await RespondWithModalAsync(modal.Build());
    }

    [ComponentInteraction("ticket-registration")]
    public async Task HandleRegistrationButton()
    {
        var components = new ComponentBuilder()
            .WithButton("I am the team captain", customId: "ticket-registration-captain", style: ButtonStyle.Primary, emote: new Emoji("👨‍✈️"));

        await RespondAsync(
            "# Please have the team captain open a ticket\n" +
            "The team captain is the one who pays for the team. If you are not the captain, please ask the captain to open this ticket.",
            components: components.Build(),
            ephemeral: true);
    }

    [ComponentInteraction("ticket-registration-captain")]
    public async Task HandleRegistrationCaptainButton()
    {
        var components = new ComponentBuilder()
            .WithButton("I want to register a team late",   customId: "ticket-registration-late", style: ButtonStyle.Primary, emote: new Emoji("⏳"))
            .WithButton("I want to substitute team members", customId: "ticket-registration-sub",  style: ButtonStyle.Primary, emote: new Emoji("🔄"));

        var component = (SocketMessageComponent)Context.Interaction;
        await component.UpdateAsync(m =>
        {
            m.Content =
                "# The deadline to register has passed\n" +
                "If you have already paid for a team, you can request to add or substitute team members. Changes are not guaranteed.\n\n" +
                "If you do not have a team and would like to request late registration, open a ticket ASAP. Late registration is $40. Spots are limited and registration is not guaranteed.";
            m.Components = components.Build();
        });
    }

    [ComponentInteraction("ticket-registration-late")]
    public async Task HandleRegistrationLateButton()
    {
        var modal = new ModalBuilder()
            .WithTitle("⏳ Late Registration")
            .WithCustomId("submit-ticket-registration-late")
            .AddTextInput("Discord IDs", "ids", TextInputStyle.Paragraph,
                placeholder: "List the Discord IDs of all the members on the team");
        await RespondWithModalAsync(modal.Build());
    }

    [ComponentInteraction("ticket-registration-sub")]
    public async Task HandleRegistrationSubButton()
    {
        var modal = new ModalBuilder()
            .WithTitle("🔄 Substitute Members")
            .WithCustomId("submit-ticket-registration-sub")
            .AddTextInput("Discord IDs", "ids", TextInputStyle.Paragraph,
                placeholder: "List the Discord IDs of the teammates being substituted");
        await RespondWithModalAsync(modal.Build());
    }

    [ComponentInteraction("ticket-date")]
    public async Task HandleDateButton()
    {
        var components = new ComponentBuilder()
            .WithButton("I want to compete earlier",    customId: "ticket-date-earlier", style: ButtonStyle.Primary, emote: new Emoji("⏱️"))
            .WithButton("I want to compete later",      customId: "ticket-date-missed",  style: ButtonStyle.Primary, emote: new Emoji("⏰"))
            .WithButton("I missed my competition date", customId: "ticket-date-missed",  style: ButtonStyle.Primary, emote: new Emoji("🛟"));

        await RespondAsync(
            "# Use the portal to change your competition date\n" +
            "The team captain can select the preferred competition date on the portal. Choose the date you will start competing, even if the 6 hour competition window overlaps into the next morning.\n\n" +
            "Once you start competing you will have the full 6 hours to work on all virtual machines and challenges, but you must finish all your work by <t:1780977540:F>.",
            components: components.Build(), ephemeral: true);
    }

    [ComponentInteraction("ticket-date-missed")]
    public async Task HandleDateMissedButton()
    {
        var component = (SocketMessageComponent)Context.Interaction;
        await component.UpdateAsync(m =>
        {
            m.Content =
                "# Don't worry, you can compete anytime\n" +
                "If you weren't able to compete on your originally scheduled date, no need to make a ticket or request approval for a later date. Just login to the portal and start competing when you're ready.";
            m.Components = new ComponentBuilder().Build();
        });
    }

    [ComponentInteraction("ticket-date-earlier")]
    public async Task HandleDateEarlierButton()
    {
        var modal = new ModalBuilder()
            .WithTitle("⏱️ Change Date")
            .WithCustomId("submit-ticket-earlier")
            .AddTextInput("Date", "reason", TextInputStyle.Paragraph,
                placeholder: "When would you like to compete instead?");
        await RespondWithModalAsync(modal.Build());
    }

    [ComponentInteraction("ticket-vmrc")]
    public async Task HandleVmrcButton()
    {
        var embed = new EmbedBuilder()
            .WithColor(new Color(0x2b91d2))
            .WithImageUrl("https://i.imgur.com/vJHKgp8.png")
            .Build();
        await RespondAsync(
            "# The VMRC console is not officially supported\n" +
            "Usage of the VMRC console is only recommended for experienced competitors. No technical support or help will be provided for issues related to the VMRC console." +
            "\n\nYou will need VMWare Workstation Player (Workstation Pro is not supported). You may need to change your default VMRC app to Player in the registry:",
            embed: embed, ephemeral: true);
    }

    [ComponentInteraction("ticket-reopen")]
    public async Task HandleReopenButton()
        => await RespondAsync(
            "# Please start a new thread; we don't reopen closed threads\n" +
            "If you would like to view the contents of a closed ticket, click the 🧵 **Threads** icon at the top of this channel.",
            ephemeral: true);

    [ModalInteraction("submit-ticket")]
    public async Task CreateNewTicket(TicketModal modal)
        => await CreateTicketAsync("asked a question", "Question", modal.Question);

    [ModalInteraction("submit-ticket-earlier")]
    public async Task CreateEarlierTicket(TicketEarlierModal modal)
        => await CreateTicketAsync("requested an earlier competition date", "Date", modal.Reason);

    [ModalInteraction("submit-ticket-registration-late")]
    public async Task CreateLateRegistrationTicket(TicketRegistrationLateModal modal)
        => await CreateTicketAsync("requested late team registration", "Discord IDs", modal.Ids);

    [ModalInteraction("submit-ticket-registration-sub")]
    public async Task CreateSubstitutionTicket(TicketRegistrationSubModal modal)
        => await CreateTicketAsync("requested team member substitution", "Discord IDs", modal.Ids);

    private async Task CreateTicketAsync(string action, string fieldLabel, string fieldValue)
    {
        await DeferAsync(ephemeral: true);

        var (teamId, _) = await TicketShared.LookupTeamByUserAsync(_configuration, Context.User.Id);

        var channel = Context.Channel as SocketTextChannel;
        var name = teamId ?? Context.User.Username;

        var existing = channel.Threads.FirstOrDefault(t => t.Name == name && !t.IsArchived && !t.IsLocked);
        if (existing != null)
        {
            if (Context.User is IGuildUser guildUser)
                await existing.AddUserAsync(guildUser);
            await FollowupAsync($"Your team has an open ticket: {existing.Mention}", ephemeral: true);
            return;
        }

        var thread = await channel.CreateThreadAsync(name: name, type: ThreadType.PrivateThread);
        await thread.SendMessageAsync($"{Context.User.Mention} {action}:\n\n**{fieldLabel}**\n{fieldValue}");

        await thread.AddUserAsync(Context.Guild.GetUser(TicketShared.STAFF_MATT_ID));
        await thread.AddUserAsync(Context.Guild.GetUser(TicketShared.STAFF_KALI_ID));

        var welcomeComponents = teamId != null ? TicketShared.BuildAddTeammatesComponents(teamId) : null;
        await thread.SendMessageAsync(TicketShared.WELCOME_TEXT, components: welcomeComponents?.Build());

        await FollowupAsync($"New ticket created: {thread.Mention}", ephemeral: true);
    }

    [ComponentInteraction("add-teammates:*")]
    public async Task HandleAddTeammates(string teamId)
    {
        await DeferAsync(ephemeral: true);
        if (Context.Channel is not SocketThreadChannel thread)
        {
            await FollowupAsync("This button must be used inside a ticket thread.", ephemeral: true);
            return;
        }

        var competitors = await TicketShared.LookupCompetitorsByTeamAsync(_configuration, teamId);
        int added = 0;
        if (competitors != null)
        {
            foreach (var id in competitors)
            {
                if (!ulong.TryParse(id, out var uid)) continue;
                var user = Context.Guild.GetUser(uid);
                if (user != null)
                {
                    await thread.AddUserAsync(user);
                    added++;
                }
            }
        }

        if (Context.Interaction is SocketMessageComponent comp)
        {
            var disabled = new ComponentBuilder()
                .WithButton("Teammates added",
                            customId: $"add-teammates:{teamId}",
                            ButtonStyle.Secondary,
                            new Emoji("✅"),
                            disabled: true);
            await comp.Message.ModifyAsync(m => m.Components = disabled.Build());
        }

        await FollowupAsync($"Added {added} teammate(s)!", ephemeral: true);
    }
}

public class TicketModal : IModal
{
    public string Title => "❓ Question";

    [ModalTextInput("question")]
    public string Question { get; set; }
}

public class TicketEarlierModal : IModal
{
    public string Title => "⏱️ Change Date";

    [ModalTextInput("reason")]
    public string Reason { get; set; }
}

public class TicketRegistrationLateModal : IModal
{
    public string Title => "⏳ Late Registration";

    [ModalTextInput("ids")]
    public string Ids { get; set; }
}

public class TicketRegistrationSubModal : IModal
{
    public string Title => "🔄 Substitute Members";

    [ModalTextInput("ids")]
    public string Ids { get; set; }
}
