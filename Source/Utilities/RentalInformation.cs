using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public enum Region
    {
        EU,
        CA,
        RU,
        MX,
        US,
        JP,
        Asia,
    }
    public class RentalInformation
    {
        public DateTime DeliveryTime;
        public DateTime DropTime;
        public DateTime CompletionTime;
        public DateTime ChannelDeletionTime;

        public long InternalKeyNumber;

        public bool FirmDelivery = false;
        public bool IsDurationRental = false;
        public decimal RentalPrice;
        public string Key;
        public Region Region;
        public string DiscordID;
        public string BotName;
        public string ChannelName;

        public RentalInformation()
        {
            InternalKeyNumber = -1;
        }

        /// <summary>
        /// Doesn't care about quantity number.
        /// </summary>
        /// <param name="comparableRental"></param>
        /// <returns></returns>
        public bool IsTheSameDrop(RentalInformation comparableRental)
        {
            bool retValue = false;
            if (Region != comparableRental.Region)
            {
                return false;
            }

            if (BotName != comparableRental.BotName)
            {
                return false;
            }

            retValue &= DropTime.DayOfYear == comparableRental.DropTime.DayOfYear;
            retValue &= DropTime.TimeOfDay == comparableRental.DropTime.TimeOfDay;

            return retValue;
        }

        public bool KeyAssigned()
        {
            return InternalKeyNumber != -1;
        }
    };

}