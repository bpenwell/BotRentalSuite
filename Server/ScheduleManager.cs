using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Utilities;

namespace RewardingRentals.Server
{
    public enum ResultEnum
    {
        Success,
        PartiallyUnavailable,
        Unavailable
    }

    public struct SchedulerResult
    {
        public ResultEnum Code;
        public string Result;
    };

    public struct RentalInformation
    {
        public string Key;

        public DateTime DeliveryTime;
        public DateTime CompletionTime;
        public string DiscordID;
        public string BotName;
        public string ChannelName;
        public long Quantity;
        public decimal RentalPrice;
    };

    public class ScheduleManager : ISaveable
    {
        private static readonly ScheduleManager m_instance = new ScheduleManager();

        /// <summary>
        /// Loads up saved data
        /// </summary>
        private ScheduleManager()
        {
            m_undeliveredRentals = new SortedList<DateTime, RentalInformation>();
            m_deliveredRentals = new SortedList<DateTime, RentalInformation>();
            if (!File.Exists(m_absoluteSavePath))
            {
                File.Create(m_absoluteSavePath);
                return;
            }
        }

        public static ScheduleManager Instance
        {
            get
            {
                return m_instance;
            }
        }

        public string SaveFileName => @"ScheduleManager.sv";
        private string m_absoluteSavePath => Path.Combine(Assembly.GetExecutingAssembly() + SaveFileName);

        /// <summary>
        /// DateTime should be the delivery time
        /// </summary>
        private SortedList<DateTime, RentalInformation> m_undeliveredRentals;

        /// <summary>
        /// DateTime should be the completion time
        /// </summary>
        private SortedList<DateTime, RentalInformation> m_deliveredRentals;

        public void Save()
        {
            File.Delete(m_absoluteSavePath);

            using (FileStream fileStream = File.OpenWrite(m_absoluteSavePath))
            {
                //Output currently scheduled stuff
            }
        }

        public SchedulerResult TryAddToSchedule(string botName, DateTime dropDate, long quantity, string region, string discordID, string channelName, decimal totalPrice, TimeSpan deliveryTimeBeforeDrop)
        {
            var schedulerResult = new SchedulerResult();

            var actualDropTime = AppHelpers.GetDropDateTime(region, dropDate);
            var newRental = new RentalInformation
            {
                BotName = botName,
                Quantity = quantity,
                DiscordID = discordID,
                ChannelName = channelName,
                DeliveryTime = actualDropTime - deliveryTimeBeforeDrop,
                CompletionTime = actualDropTime + AppHelpers.GetRentalCompletionTime(),
                RentalPrice = totalPrice
            };

            m_undeliveredRentals.Add(newRental.DeliveryTime, newRental);

            return schedulerResult;
        }

        public bool NeedToDeliverKeys()
        {
            return m_undeliveredRentals.Count != 0;
        }

        public async Task<RentalInformation> DeliverNextKey()
        {
            RentalInformation rentalInformation;
            lock (m_undeliveredRentals)
            {
                rentalInformation = m_undeliveredRentals.Values[0];
                m_undeliveredRentals.RemoveAt(0);
            }

            await Task.Delay(1000);

            rentalInformation.Key = "KeYyYyY";

            return rentalInformation;
        }
    }
}
