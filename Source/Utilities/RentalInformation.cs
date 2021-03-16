using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

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
        public DateTime DeliveryTime { get; set; }
        public DateTime DropTime { get; set; }
        public DateTime CompletionTime { get; set; }
        public DateTime ChannelDeletionTime { get; set; }

        public long InternalKeyNumber { get; set; }

        public bool FirmDelivery { get; set; }
        public bool IsDurationRental { get; set; }
        public decimal RentalPrice { get; set; }
        public string Key { get; set; }
        public Region Region { get; set; }
        public string DiscordID { get; set; }
        public string BotName { get; set; }
        public string ChannelName { get; set; }

        public RentalInformation()
        {
            FirmDelivery = false;
            IsDurationRental = false;
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