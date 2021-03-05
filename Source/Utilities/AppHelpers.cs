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
            if (region == "EU" || region == "Asia")
            {
                date.AddDays(1);
            }

            Dictionary<string, DateTime> regionMap = new Dictionary<string, DateTime>();
            regionMap.Add("EU", new DateTime(date.Year, date.Month, date.Day, 0, 0, 0));
            regionMap.Add("CA", new DateTime(date.Year, date.Month, date.Day, 3, 0, 0));
            regionMap.Add("US", new DateTime(date.Year, date.Month, date.Day, 7, 0, 0));
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
            Dictionary<string, TimeSpan> deliveryMap = new Dictionary<string, TimeSpan>();
            deliveryMap.Add("default", new TimeSpan(2, 0, 0));
            deliveryMap.Add("1H", new TimeSpan(1, 0, 0));
            deliveryMap.Add("2H", new TimeSpan(2, 0, 0));
            deliveryMap.Add("3H", new TimeSpan(3, 0, 0));
            deliveryMap.Add("4H", new TimeSpan(4, 0, 0));
            deliveryMap.Add("5H", new TimeSpan(5, 0, 0));
            deliveryMap.Add("6H", new TimeSpan(6, 0, 0));
            deliveryMap.Add("7H", new TimeSpan(7, 0, 0));
            deliveryMap.Add("8H", new TimeSpan(8, 0, 0));
            deliveryMap.Add("9H", new TimeSpan(9, 0, 0));
            deliveryMap.Add("10H", new TimeSpan(10, 0, 0));
            deliveryMap.Add("11H", new TimeSpan(11, 0, 0));
            deliveryMap.Add("12H", new TimeSpan(12, 0, 0));
            deliveryMap.Add("13H", new TimeSpan(13, 0, 0));
            deliveryMap.Add("14H", new TimeSpan(14, 0, 0));
            deliveryMap.Add("15H", new TimeSpan(15, 0, 0));
            deliveryMap.Add("16H", new TimeSpan(16, 0, 0));
            deliveryMap.Add("17H", new TimeSpan(17, 0, 0));
            deliveryMap.Add("18H", new TimeSpan(18, 0, 0));
            deliveryMap.Add("19H", new TimeSpan(19, 0, 0));
            deliveryMap.Add("20H", new TimeSpan(20, 0, 0));
            deliveryMap.Add("21H", new TimeSpan(21, 0, 0));
            deliveryMap.Add("22H", new TimeSpan(22, 0, 0));
            deliveryMap.Add("23H", new TimeSpan(23, 0, 0));
            deliveryMap.Add("1D", new TimeSpan(1, 0, 0, 0));

            bool success = deliveryMap.TryGetValue(timeBeforeDrop, out var spanBeforeDrop);
            if (!success)
            {
                throw new Exception("Unhandled delivery time");
            }

            return spanBeforeDrop;
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
