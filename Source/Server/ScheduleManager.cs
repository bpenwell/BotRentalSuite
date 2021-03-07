using Server;
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
            m_deliveriesInProgress = new SortedList<DateTime, RentalInformation>();
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
        /// DateTime should be the delivery time
        /// </summary>
        private SortedList<DateTime, RentalInformation> m_deliveriesInProgress;

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

            var maxKeysOwned = DeliveryManager.Instance.MaximumBotKeys(newRental.BotName);
            var availableKeyMap = new List<bool>();
            for (var index = 0; index < maxKeysOwned; index++)
            {
                availableKeyMap.Add(true);
            }

            //First check undelivered rentals
            long copiesAlreadyTaken = 0;
            foreach (var rentalKVP in m_undeliveredRentals)
            {
                var rental = rentalKVP.Value;
                if (newRental.IsTheSameDrop(rental))
                {
                    copiesAlreadyTaken += rental.Quantity;

                    foreach (var key in rental.InternalKeyNumbers)
                    {
                        availableKeyMap[Convert.ToInt32(key - 1)] = false;
                    }
                }
            }

            //Now compare that to already delivered rentals
            foreach (var rentalKVP in m_deliveredRentals)
            {
                var rental = rentalKVP.Value;
                if (newRental.IsTheSameDrop(rental))
                {
                    copiesAlreadyTaken += rental.Quantity;

                    foreach (var key in rental.InternalKeyNumbers)
                    {
                        availableKeyMap[Convert.ToInt32(key - 1)] = false;
                    }
                }
            }

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

                    var keysWanted = newRental.Quantity;
                    var keyNumber = 1;
                    foreach (var key in availableKeyMap)
                    {
                        if (keysWanted == 0)
                        {
                            break;
                        }

                        //Key is available
                        if (key)
                        {
                            newRental.InternalKeyNumbers.Add(keyNumber);
                            keysWanted--;
                        }

                        keyNumber++;
                    }

                    //Only add when successful
                    m_undeliveredRentals.Add(newRental.DeliveryTime, newRental);
                }
            }

            return schedulerResult;
        }

        private async Task BeginDeliveryIfNecessary()
        {

            int index = 0;
            var unDeliveredRentals = new SortedList<DateTime, RentalInformation>(m_undeliveredRentals);
            foreach (var deliveryKVP in unDeliveredRentals)
            {
                var delivery = deliveryKVP.Value;
                if (DateTime.Now > delivery.DeliveryTime)
                {
                    m_deliveriesInProgress.Add(deliveryKVP.Key, deliveryKVP.Value);

                    foreach (var bot in delivery.InternalKeyNumbers)
                    {
                        Console.WriteLine("Working..."); 
                        await DeliveryManager.Instance.GetBotKey(bot);
                    }

                    m_undeliveredRentals.RemoveAt(index);
                }
                index++;
            }
        }

        public RentalInformation NextAvailableKey()
        {
            var currentAvailableKeys = DeliveryManager.Instance.RegisteredKeys;
            var deliveriesInProgress = new SortedList<DateTime, RentalInformation>(m_deliveriesInProgress);
            foreach (var deliveryKVP in deliveriesInProgress)
            {
                var deliveryInfo = deliveryKVP.Value;

                foreach (var undeliveredBotNumber in deliveryInfo.InternalKeyNumbers)
                {
                    if (currentAvailableKeys.ContainsKey(undeliveredBotNumber))
                    {
                        RentalInformation rentalInformation = deliveryInfo;
                        var key = currentAvailableKeys[undeliveredBotNumber];
                        rentalInformation.Key = key;
                        DeliveryManager.Instance.RegisteredKeys.Remove(undeliveredBotNumber);

                        m_deliveredRentals.Add(rentalInformation.DeliveryTime, rentalInformation);
                        m_deliveriesInProgress.RemoveAt(0);
                        return rentalInformation;
                    }
                }
            }

            throw new Exception("Couldn't deliver next key");
        }

        public async Task Update()
        {
            await BeginDeliveryIfNecessary();
            if (DeliveryManager.Instance.RegisteredKeys.Count == 0)
            {
                return;
            }

            var keyToDeliver = NextAvailableKey();
            DiscordConnection.Instance.DeliverKey(keyToDeliver.Key, keyToDeliver.ChannelName);
        }
    }
}
