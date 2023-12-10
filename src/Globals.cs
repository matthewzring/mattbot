global using Discord;
global using System.Text;
global using System.Text.RegularExpressions;
global using System.Reflection;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using mattbot.logging;
global using static mattbot.Attributes;
global using static mattbot.Globals;

namespace mattbot
{
    public class Globals
    {
        // Links
        public const string CYBERPATRIOT_SERVER_INVITE = "https://discord.gg/cyberpatriot";
        public const string CCDC_SERVER_INVITE = "https://discord.gg/fFX7fJy6Vj";

        // Strings
        public const string ERROR_MESSAGE = "An error occured: Please contact **matthewzring** for help.";

        // People
        public const ulong OWNER_ID = 349007194768801792;

        // Servers
        public const ulong CYBERPATRIOT_ID = 301768361136750592;
        public const ulong FINALISTS_ID = 546405227092508683;
        public const ulong TESTING_ID = 372483060769357824;
        public const ulong CCDC_ID = 1093372273295101992;
        public const ulong CYBERDISCORD_ID = 1105972904711176262;

        // Emoji
        public const string SUCCESS = "\u2611\uFE0F"; // ☑️
        public const string ERROR = "\u274C"; // ❌
    }
}
