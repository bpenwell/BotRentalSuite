﻿using Server;
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

    public class ScheduleManager : ISaveable
    {
        public static ScheduleManager Instance { get; } = new ScheduleManager();

        /// <summary>
        /// Loads up saved data
        /// </summary>
        private ScheduleManager()
        {
            m_undeliveredRentals = new SortedList<DateTime, RentalInformation>();
            m_deliveredRentals = new SortedList<DateTime, RentalInformation>();
            if (!File.Exists(m_absoluteSavePath))
            {
                Console.WriteLine($"Created {m_absoluteSavePath}");
                File.Create(m_absoluteSavePath);
                return;
            }
        }

        public string SaveFileName => @"ScheduleManager.sv";
        private string m_absoluteSavePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + SaveFileName;

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

            Console.WriteLine($"Saved latest {m_absoluteSavePath}");
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
                Region = region,
                DeliveryTime = actualDropTime - deliveryTimeBeforeDrop,
                CompletionTime = actualDropTime + AppHelpers.GetRentalCompletionTime(),
                ChannelDeletionTime = actualDropTime + AppHelpers.GetRentalCompletionTime(),
                RentalPrice = totalPrice
            };

            //First check undelivered rentals
            long copiesAlreadyTaken = 0;
            foreach (var rentalKVP in m_undeliveredRentals)
            {
                var rental = rentalKVP.Value;
                if (newRental.IsTheSameDrop(rental))
                {
                    copiesAlreadyTaken += rental.Quantity;
                }
            }

            //Now compare that to already delivered rentals
            foreach (var rentalKVP in m_deliveredRentals)
            {
                var rental = rentalKVP.Value;
                if (newRental.IsTheSameDrop(rental))
                {
                    copiesAlreadyTaken += rental.Quantity;
                }
            }

            var maxKeysOwned = DeliveryManager.Instance.MaximumBotKeys(newRental.BotName);
            if (copiesAlreadyTaken > maxKeysOwned)
            {
                throw new Exception("Congrats on breaking the system. Somehow, more copies are already taken than are available.");
            }
            else if (copiesAlreadyTaken == maxKeysOwned)
            {
                schedulerResult.Code = ResultEnum.Unavailable;
                schedulerResult.Result = "Sorry! All of our keys are already rented out for this drop.";
            }
            else
            {
                var copiesAvailable = maxKeysOwned - copiesAlreadyTaken;

                if (newRental.Quantity > copiesAvailable)
                {
                    schedulerResult.Code = ResultEnum.PartiallyUnavailable;
                    schedulerResult.Result = $"So we have a bit of a situation. We only have {copiesAvailable} copies of {newRental.BotName} available. " +
                        $"How many copies would you like to secure?";
                }
                else
                {
                    schedulerResult.Code = ResultEnum.Success;
                    //Customize display to show delivery time in their regional time.
                    schedulerResult.Result = $"Success! We have added you to the schedule. Expect key delivery at {newRental.DeliveryTime.ToLongDateString()} {newRental.DeliveryTime.ToLongTimeString()}!";

                    //Only add when successful
                    m_undeliveredRentals.Add(newRental.DeliveryTime, newRental);
                }
            }

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
                m_deliveredRentals.Add(rentalInformation.DeliveryTime, rentalInformation);
                m_undeliveredRentals.RemoveAt(0);
            }

            await Task.Delay(1000);

            rentalInformation.Key = "KeYyYyY";

            return rentalInformation;
        }
    }
}