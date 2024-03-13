/*
 * Copyright 2023-2024 Matthew Ring
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

namespace mattbot.utils;

/**
 * Modelled after jagrosh's LogUtil in Vortex
 */
public class LogUtil
{
    private static readonly string LOG_TIME = "`[{0}]`";
    private static readonly string EMOJI = " {1}";

    private static readonly string LOG_FORMAT = LOG_TIME + EMOJI + " {2}";

    public static string LogFormat(DateTimeOffset time, string emoji, string content)
    {
        return String.Format(LOG_FORMAT, TimeF(time), emoji, content);
    }

    private static string TimeF(DateTimeOffset time)
    {
        return time.ToLocalTime().ToString("HH:mm:ss");
    }
}
