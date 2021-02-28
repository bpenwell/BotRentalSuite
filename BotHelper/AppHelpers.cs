using System;
using System.Collections.Generic;
using System.Text;

namespace BotHelper
{
    public static class AppHelpers
    {
        private static long m_ticketNumber = 1;

        /// <summary>
        /// Only call this once per ticket
        /// </summary>
        /// <param name="discordUser">Shouldn't contain the # or ending numbers</param>
        /// <returns></returns>
        public static string GetNextTicketName(string discordUser)
        {
            var nameParts = discordUser.Split(' ');
            var ticketName = $"ticket-{m_ticketNumber}";
            m_ticketNumber++;

            foreach(var part in nameParts)
            {
                ticketName += $"-{part}";
            }

            return ticketName;
        }
    }
}
