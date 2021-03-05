using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class RentalInformation
    {
        public DateTime DeliveryTime;
        public DateTime CompletionTime;
        public DateTime ChannelDeletionTime;

        public bool IsDurationRental = false;
        public long Quantity;
        public decimal RentalPrice;
        public string Key;
        public string Region;
        public string DiscordID;
        public string BotName;
        public string ChannelName;

        public RentalInformation()
        {

        }

        /// <summary>
        /// Doesn't care about quantity number.
        /// </summary>
        /// <param name="comparableRental"></param>
        /// <returns></returns>
        public bool IsTheSameDrop(RentalInformation comparableRental)
        {
            bool retValue = false;
            if (Region == comparableRental.Region)
            {
                return true;
            }

            if (BotName == comparableRental.BotName)
            {
                return true;
            }

            retValue &= DeliveryTime.DayOfYear == comparableRental.DeliveryTime.DayOfYear;
            retValue &= DeliveryTime.TimeOfDay == comparableRental.DeliveryTime.TimeOfDay;

            return retValue;
        }
    };

}
