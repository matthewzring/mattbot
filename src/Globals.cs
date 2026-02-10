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

global using Discord;
global using System.Text;
global using System.Reflection;
global using System.Text.RegularExpressions;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using mattbot.logging;
global using static mattbot.Attributes;
global using static mattbot.Globals;

namespace mattbot;

public class Globals
{
    // Links
    public static readonly string CYBERPATRIOT_SERVER_INVITE = "https://discord.com/invite/FqPbspP";
    public static readonly string VANITY_CYBERPATRIOT_SERVER_INVITE = "https://discord.gg/cyberpatriot";
    public static readonly string CCDC_SERVER_INVITE = "https://discord.gg/fFX7fJy6Vj";
    public static readonly string VANITY_CCDC_SERVER_INVITE = "https://discord.gg/ccdc";

    // Strings
    public static readonly string ERROR_MESSAGE = "An error occured: Please contact **matt** for help.";

    // People
    public static readonly ulong OWNER_ID = 349007194768801792;

    // Servers
    public static readonly ulong CCDC_ID = 1093372273295101992;
    public static readonly ulong CYBERPATRIOT_ID = 301768361136750592;
    public static readonly ulong ECITADEL_ID = 1105972904711176262;
    public static readonly ulong FINALISTS_ID = 546405227092508683;
    public static readonly ulong MATTLOUNGE_ID = 372483060769357824;

    // Emoji
    public static readonly string SUCCESS = "<a:mSuccess:1238370165054504970>";
    public static readonly string ERROR = "\u274C"; // ❌
    public static readonly string WARN = "⚠";

    public static readonly string LOADING = "<a:typing:1110293618222178324>";
}
