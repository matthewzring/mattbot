using MattBot.Services;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using MattBot.Utilities;

namespace MattBot.AutoMod
{
    public class UserVerification
    {
        private readonly Listener _listener;

        public UserVerification(Listener listener)
        {
            _listener = listener;
        }

        public async Task InitializeAsync()
        {
            _listener.UserJoined += OnUserJoinedAsync;
        }

        private async Task OnUserJoinedAsync(IGuildUser arg)
        {
            if (arg.Guild.Id == CYBERDISCORD_ID)
            {
                string[] scopes = { SheetsService.Scope.SpreadsheetsReadonly };
                GoogleCredential credential;
                using (var stream = new FileStream("mattbot-387506-f9b988a3302e.json", FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
                }

                string spreadsheetId = "18LFfUXsLTsXmljfkOb4qf33ptiNs_aPju09A87l-rTc";
                string[] ranges = { "Form Responses 1!F2:F200", "Form Responses 1!J2:J200", "Form Responses 1!N2:N200", "Form Responses 1!R2:R200" };

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "MattBot"
                });

                foreach (var range in ranges)
                {
                    var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
                    var response = request.Execute();
                    var values = response.Values;

                    if (values != null && values.Count > 0)
                    {
                        foreach (var row in values)
                        {
                            if (row.Count > 0 && row[0].ToString() == arg.Id.ToString())
                            {
                                var captain = arg.Guild.Roles.FirstOrDefault(x => x.Name == "2023 Captain");
                                if (captain == null)
                                    return;

                                var competitor = arg.Guild.Roles.FirstOrDefault(x => x.Name == "2023 Competitor");
                                if (competitor == null)
                                    return;

                                ITextChannel tc = (await arg.Guild.GetTextChannelsAsync()).FirstOrDefault(x => x.Name == "serverlog");
                                if (tc == null)
                                    return;

                                if (range.StartsWith("Form Responses 1!F"))
                                {
                                    await arg.AddRoleAsync(captain);
                                    await arg.AddRoleAsync(competitor);
                                    await tc.SendMessageAsync($"{CHECK} Verified {FormatUtil.formatFullUser(arg)} as a team captain!");
                                    break;
                                }
                                else
                                {
                                    await arg.AddRoleAsync(competitor);
                                    await tc.SendMessageAsync($"{CHECK} Verified {FormatUtil.formatFullUser(arg)} as a competitor!");
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (arg.Guild.Id == FINALISTS_ID)
            {
                var unverified = arg.Guild.Roles.FirstOrDefault(x => x.Name == "Unverified");
                if (unverified == null)
                    return;
                await arg.AddRoleAsync(unverified);

                ITextChannel tc = (await arg.Guild.GetTextChannelsAsync()).FirstOrDefault(x => x.Name == "serverlog");
                if (tc == null)
                    return;
                DateTimeOffset now = DateTimeOffset.UtcNow;
                await tc.SendMessageAsync($"{X} Could not verify {FormatUtil.formatFullUser(arg)}!");
            }
        }
    }
}
