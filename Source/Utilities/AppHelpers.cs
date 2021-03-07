using Discord;
using System;
using System.Collections.Generic;

namespace Utilities
{
    public static class AppHelpers
    {
        private static long m_ticketNumber = 1;

        /// <summary>
        /// Only call this once per ticket
        /// </summary>
        /// <param name="discordUser">Shouldn't contain the # or ending numbers</param>
        /// <returns></returns>
        public static string GetNextTicketName(string discordUser, string botName)
        {
            var nameParts = discordUser.Split(' ');
            var ticketName = $"ticket-{m_ticketNumber}-{botName}";
            m_ticketNumber++;

            foreach (var part in nameParts)
            {
                ticketName += $"-{part}";
            }

            return ticketName;
        }

        public static OverwritePermissions GetEveryonePermissionOverrides()
        {
            return new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);
        }

        public static OverwritePermissions GetClientPermissionOverrides()
        {
            return new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);
        }

        public static DateTime GetDropDateTime(string region, DateTime date)
        {
            if (region == "JP" || region == "Asia")
            {
                date = date.Subtract(TimeSpan.FromDays(1));
            }

            Dictionary<string, DateTime> regionMap = new Dictionary<string, DateTime>();
            regionMap.Add("EU", new DateTime(date.Year, date.Month, date.Day, 0, 0, 0));
            regionMap.Add("RU", new DateTime(date.Year, date.Month, date.Day, 3, 0, 0));
            regionMap.Add("CA", new DateTime(date.Year, date.Month, date.Day, 3, 0, 0));
            regionMap.Add("US", new DateTime(date.Year, date.Month, date.Day, 7, 0, 0));
            regionMap.Add("JP", new DateTime(date.Year, date.Month, date.Day, 16, 0, 0));
            regionMap.Add("Asia", new DateTime(date.Year, date.Month, date.Day, 18, 0, 0));

            bool success = regionMap.TryGetValue(region, out var spanBeforeDrop);
            if (!success)
            {
                throw new Exception("Unhandled delivery time");
            }

            return spanBeforeDrop;
        }

        public static TimeSpan GetRentalDeliveryTime(string timeBeforeDrop)
        {
            if (TimeSpan.TryParse(timeBeforeDrop, out var span))
            {
                return span;
            }
            else
            {
                throw new Exception("Unhandled delivery time. Enter time in the following format: HH:MM");
            }
        }

        public static TimeSpan GetRentalCompletionTime()
        {
            return new TimeSpan(1, 0, 0);
        }

        public static TimeSpan GetRentalChannelDeletionTime()
        {
            return new TimeSpan(1, 5, 0);
        }
    }
}
