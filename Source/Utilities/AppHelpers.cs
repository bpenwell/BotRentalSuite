using Discord;
using System;
using System.Collections.Generic;

namespace Utilities
{
    public static class AppHelpers
    {
        private static long m_ticketNumber = 1;

        private static TimeSpan m_latestDeliveryBuffer = new TimeSpan(1, 30, 0);

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

        public static string GetRegionString(Region region)
        {
            return region switch
            {
                Region.Asia => "Asia",
                Region.CA => "CA",
                Region.EU => "EU",
                Region.JP => "JP",
                Region.MX => "MX",
                Region.RU => "RU",
                Region.US => "US",
                _ => throw new Exception("Invalid region"),
            };
        }

        public static Region GetRegionEnum(string region)
        {
            return region switch
            {
                "Asia" => Region.Asia,
                "CA" => Region.CA,
                "EU" => Region.EU,
                "JP" => Region.JP,
                "MX" => Region.MX,
                "RU" => Region.RU,
                "US" => Region.US,
                _ => throw new Exception("Invalid region"),
            };
        }

        public static string GetRegionZoneString(Region region)
        {
            return region switch
            {
                Region.Asia => "Singapore Standard Time",
                Region.JP => "Tokyo Standard Time",
                Region.CA => "Eastern Standard Time",
                Region.MX => "Eastern Standard Time",
                Region.RU => "Russian Standard Time",
                Region.EU => "W. Europe Standard Time",
                Region.US => "Eastern Standard Time",
                _ => throw new NotImplementedException(),
            };
        }

        public static DateTime GetRegionalTime(Region region, DateTime date)
        {
            return region switch
            {
                Region.Asia => TimeZoneInfo.ConvertTime(date, TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time")),
                Region.JP => TimeZoneInfo.ConvertTime(date, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")),
                Region.CA => TimeZoneInfo.ConvertTime(date, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
                Region.MX => TimeZoneInfo.ConvertTime(date, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
                Region.RU => TimeZoneInfo.ConvertTime(date, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")),
                Region.EU => TimeZoneInfo.ConvertTime(date, TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")),
                Region.US => TimeZoneInfo.ConvertTime(date, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")),
                _ => throw new NotImplementedException(),
            };
        }

        public static DateTime GetDropDateTime(Region region, DateTime date)
        {
            if (region == Region.JP || region == Region.Asia)
            {
                date = date.Subtract(TimeSpan.FromDays(1));
            }

            Dictionary<Region, DateTime> regionMap = new Dictionary<Region, DateTime>();
            regionMap.Add(Region.EU, new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Local));
            regionMap.Add(Region.MX, new DateTime(date.Year, date.Month, date.Day, 3, 0, 0, DateTimeKind.Local));
            regionMap.Add(Region.CA, new DateTime(date.Year, date.Month, date.Day, 3, 0, 0, DateTimeKind.Local));
            regionMap.Add(Region.RU, new DateTime(date.Year, date.Month, date.Day, 3, 0, 0, DateTimeKind.Local));
            regionMap.Add(Region.US, new DateTime(date.Year, date.Month, date.Day, 7, 0, 0, DateTimeKind.Local));
            regionMap.Add(Region.JP, new DateTime(date.Year, date.Month, date.Day, 16, 0, 0, DateTimeKind.Local));
            regionMap.Add(Region.Asia, new DateTime(date.Year, date.Month, date.Day, 18, 0, 0, DateTimeKind.Local));

            bool success = regionMap.TryGetValue(region, out var spanBeforeDrop);
            if (!success)
            {
                throw new Exception("Unhandled delivery time");
            }

            return spanBeforeDrop;
        }

        public static bool CanRentalGetPushedBack(RentalInformation rental, out DateTime newDeliveryTime)
        {
            if (rental.FirmDelivery)
            {
                newDeliveryTime = rental.DeliveryTime;
                return false;
            }
            else
            {
                newDeliveryTime = rental.DropTime - m_latestDeliveryBuffer;
                return true;
            }
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
