global using Discord;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.Reflection;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using LiteDB;
global using MattBot.Logging;
global using static MattBot.Attributes;
global using static MattBot.Globals;

namespace MattBot
{
    public class Globals
    {
        // Links
        public const string CYBERPATRIOT_INVITE = "https://discord.gg/cyberpatriot";
        public const string CCDC_INVITE = "https://discord.gg/fFX7fJy6Vj";

        // Strings
        public const string ERROR_MESSAGE = "An error occured: Please contact **matthewzring** for help.";

        // People
        public const ulong OWNER_ID = 349007194768801792;

        // Webhooks
        public const ulong TOURN_NOTIF_ID = 1107915868207321098;

        // Servers
        public const ulong CYBERPATRIOT_ID = 301768361136750592;
        public const ulong FINALISTS_ID = 546405227092508683;
        public const ulong TESTING_ID = 372483060769357824;
        public const ulong CCDC_ID = 1093372273295101992;
        public const ulong CYBERDISCORD_ID = 1105972904711176262;

        // Emojis
        public const string CHECK = "\u2611\uFE0F"; // ☑️
        public const string X = "\u274C"; // ❌

        // MattCoin
        public const string CONSTRUCTION = "\uD83D\uDEA7"; // 🚧
        public const string SWORD = "\u2694\uFE0F"; // ⚔️
        public const string WATER = "\uD83D\uDCA7"; // 💧
        public const string NITRO = "<:nitro:1091272926881390634>";
        public const string PING = "<:ping:1091272927737036953>";
        public const string MATTCOIN = "<:mattcoin:1110649089122635776>";

        // Logs
        public const string VOICE_JOIN = "<:voicejoin:1110632369414742046>";
        public const string VOICE_LEAVE = "<:voiceleave:1110632368156463246>";
        public const string VOICE_CHANGE = "<:voicechange:1110632371495129098>";

        // Gems
        public const int GEM_THRESHOLD = 4;
        public const int GEM_DURATION = 2; // hours
        public const string GEM_EMOJI = "\uD83D\uDC8E"; // 💎

        // Message Reports
        public const int MESSAGE_REPORT_THRESHOLD = 3;
        public const int MESSAGE_REPORT_DURATION = 2; // hours
        public const string MESSAGE_REPORT_EMOJI = "\uD83D\uDDD1\uFE0F"; // 🗑️

        // Crowd Mute
        public const int CROWD_MUTE_THRESHOLD = 3;
        public const int CROWD_MUTE_DURATION = 10; // minutes
        public const string CROWD_MUTE_EMOJI = "<:1984:1025604468559061042>";
    }
}
