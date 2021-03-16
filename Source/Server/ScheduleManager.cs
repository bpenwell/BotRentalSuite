using Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Utilities;
using Utilities.Type;

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
        public string Message;
        public RentalInformation Rental;
    };

    public class ScheduleManager : ISaveable
    {
        public static ScheduleManager Instance { get; } = new ScheduleManager();

        /// <summary>
        /// Loads up saved data
        /// </summary>
        private ScheduleManager()
        {
            m_rentalMaster = new SchedulingRentalMaster();
            Load();
        }

        ~ScheduleManager()
        {
            Save();
        }

        public string SaveFileName => @"ScheduleManager.xml";
        private string m_absoluteSavePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + SaveFileName;

        private SchedulingRentalMaster m_rentalMaster;

        public void Load()
        {
            if (File.Exists(m_absoluteSavePath))
            {
                Console.WriteLine($"Loading latest {m_absoluteSavePath}");
                using (FileStream fileStream = File.OpenRead(m_absoluteSavePath))
                {
                    Type[] extraTypes = new Type[3];
                    extraTypes[0] = typeof(RentalContainer);
                    extraTypes[1] = typeof(DateTime);
                    extraTypes[2] = typeof(RentalInformation);

                    XmlSerializer serializer = new XmlSerializer(typeof(SchedulingRentalMaster), extraTypes);

                    try
                    {
                        m_rentalMaster = serializer.Deserialize(fileStream) as SchedulingRentalMaster;
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }

                }
            }
        }

        public void Save()
        {
            File.Delete(m_absoluteSavePath);

            if (m_rentalMaster.NoRentalsRecorded())
            {
                return;
            }

            Console.WriteLine($"Saved latest {m_absoluteSavePath}");
            using (FileStream fileStream = File.OpenWrite(m_absoluteSavePath))
            {
                Type[] extraTypes = new Type[3];
                extraTypes[0] = typeof(RentalContainer);
                extraTypes[1] = typeof(DateTime);
                extraTypes[2] = typeof(RentalInformation);
                XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
                xmlSerializerNamespaces.Add("", "");
                XmlSerializer serializer = new XmlSerializer(typeof(SchedulingRentalMaster), extraTypes);
                serializer.Serialize(fileStream, m_rentalMaster);
            }
        }

        public SchedulerResult TryAddToSchedule(string botName, DateTime dropDate, long quantity, string region, string discordID, string channelName, decimal totalPrice, TimeSpan deliveryTimeBeforeDrop, bool firmDelivery)
        {
            var schedulerResult = new SchedulerResult();
            var beforeUndeliveredCount = m_rentalMaster.UndeliveredRentals.RentalContainerList.Count;
            var quantityLeftToFulfill = quantity;

            var regionEnum = AppHelpers.GetRegionEnum(region);
            var actualDropTime = AppHelpers.GetDropDateTime(regionEnum, dropDate);
            var newRental = new RentalInformation
            {
                BotName = botName,
                DropTime = actualDropTime,
                FirmDelivery = firmDelivery,
                DiscordID = discordID,
                ChannelName = channelName,
                Region = regionEnum,
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
            foreach (var rentalContainer in m_rentalMaster.UndeliveredRentals.RentalContainerList)
            {
                var rental = rentalContainer.RentalInfo;
                if (newRental.IsTheSameDrop(rental))
                {
                    copiesAlreadyTaken++;
                    availableKeyMap[Convert.ToInt32(rental.InternalKeyNumber - 1)] = false;
                }
                else if (rental.DropTime > newRental.DropTime)
                {
                    if ((newRental.CompletionTime > rental.DeliveryTime) && rental.FirmDelivery)
                    {
                        copiesAlreadyTaken++;
                        availableKeyMap[Convert.ToInt32(rental.InternalKeyNumber - 1)] = false;
                    }
                }
                else if (rental.DropTime < newRental.DropTime)
                {
                    if ((rental.CompletionTime >= newRental.DeliveryTime) && newRental.FirmDelivery)
                    {
                        copiesAlreadyTaken++;
                        availableKeyMap[Convert.ToInt32(rental.InternalKeyNumber - 1)] = false;
                    }
                }
            }

            //Now compare that to already delivered rentals
            foreach (var rentalContainer in m_rentalMaster.DeliveredRentals.RentalContainerList)
            {
                var rental = rentalContainer.RentalInfo;
                if (newRental.IsTheSameDrop(rental))
                {
                    copiesAlreadyTaken++;
                    availableKeyMap[Convert.ToInt32(rental.InternalKeyNumber - 1)] = false;
                }
                else if (rental.DropTime > newRental.DropTime)
                {
                    if ((newRental.CompletionTime > rental.DeliveryTime) && rental.FirmDelivery)
                    {
                        copiesAlreadyTaken++;
                        availableKeyMap[Convert.ToInt32(rental.InternalKeyNumber - 1)] = false;
                    }
                }
                else if (rental.DropTime < newRental.DropTime)
                {
                    if ((rental.CompletionTime >= newRental.DeliveryTime) && newRental.FirmDelivery)
                    {
                        copiesAlreadyTaken++;
                        availableKeyMap[Convert.ToInt32(rental.InternalKeyNumber - 1)] = false;
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
                schedulerResult.Message = "Sorry! All of our keys are already rented out for this drop.";
            }
            else
            {
                var copiesAvailable = maxKeysOwned - copiesAlreadyTaken;

                if (copiesAvailable < quantity)
                {
                    schedulerResult.Code = ResultEnum.PartiallyUnavailable;
                    schedulerResult.Message = $"We only have {copiesAvailable} copies of {newRental.BotName} available. " +
                        $"How many copies would you like to secure?";
                }
                else
                {
                    var keyNumber = 1;

                    var prioritizedKeys = new List<bool>();
                    for (var index = 0; index < maxKeysOwned; index++)
                    {
                        prioritizedKeys.Add(true);
                    }

                    foreach (var key in availableKeyMap)
                    {
                        //Key is available
                        if (key)
                        {
                            //Prioritize keys that wont move a delivery back
                            foreach (var rentalContainer in m_rentalMaster.UndeliveredRentals.RentalContainerList)
                            {
                                var delivery = rentalContainer.RentalInfo;

                                if (newRental.DropTime < delivery.DropTime)
                                {
                                    if (newRental.CompletionTime > delivery.DeliveryTime)
                                    {
                                        prioritizedKeys[keyNumber - 1] = false;
                                    }
                                }
                                else
                                {
                                    if (newRental.CompletionTime < delivery.DeliveryTime)
                                    {
                                        prioritizedKeys[keyNumber - 1] = false;
                                    }
                                }
                            }
                        }

                        keyNumber++;
                    }

                    keyNumber = 1;
                    foreach (var key in prioritizedKeys)
                    {
                        //This key is prioritized, since adding a rental to this bot's schedule won't put a delivery out
                        if (key)
                        {
                            if (quantityLeftToFulfill == 0)
                            {
                                break;
                            }

                            newRental.InternalKeyNumber = keyNumber;
                            m_rentalMaster.UndeliveredRentals.RentalContainerList.Add(new RentalContainer(newRental.DeliveryTime, newRental));
                            m_rentalMaster.UndeliveredRentals.RentalContainerList.Sort();
                            quantityLeftToFulfill--;

                            var regionalTime = AppHelpers.GetRegionalTime(newRental.Region, newRental.DeliveryTime);
                            schedulerResult.Code = ResultEnum.Success;
                            schedulerResult.Rental = newRental;
                            schedulerResult.Message = $"Delivery time: ```{regionalTime.ToLongDateString()} {regionalTime.ToLongTimeString()} {AppHelpers.GetRegionZoneString(newRental.Region)}\n" +
                                $"{newRental.DeliveryTime.ToLongDateString()} {newRental.DeliveryTime.ToLongTimeString()} PST```";
                        }

                        keyNumber++;
                    }

                    if (!newRental.KeyAssigned())
                    {
                        keyNumber = 1;
                        //We need to push a delivery back in order to make this happen
                        foreach (var key in availableKeyMap)
                        {
                            //Key is available
                            if (key)
                            {
                                var originalList = new List<RentalContainer>(m_rentalMaster.UndeliveredRentals.RentalContainerList);
                                foreach (var rentalContainer in originalList)
                                {
                                    var delivery = rentalContainer.RentalInfo;

                                    if (delivery.FirmDelivery || quantityLeftToFulfill == 0)
                                    {
                                        continue;
                                    }

                                    newRental.InternalKeyNumber = keyNumber;
                                    m_rentalMaster.UndeliveredRentals.RentalContainerList.Add(new RentalContainer(newRental.DeliveryTime, newRental));
                                    m_rentalMaster.UndeliveredRentals.RentalContainerList.Sort();
                                    quantityLeftToFulfill--;

                                    delivery.DeliveryTime = newRental.CompletionTime;
                                    var regionalTime = AppHelpers.GetRegionalTime(delivery.Region, delivery.DeliveryTime);

                                    schedulerResult.Code = ResultEnum.Success;
                                    schedulerResult.Rental = newRental;
                                    schedulerResult.Message = $"Delivery time: ```{regionalTime.ToLongDateString()} {regionalTime.ToLongTimeString()} {AppHelpers.GetRegionZoneString(newRental.Region)}\n" +
                                        $"{newRental.DeliveryTime.ToLongDateString()} {newRental.DeliveryTime.ToLongTimeString()} PST```";

                                    var deliveryTimeUpdateMessage = "@here We have updated your delivery time to accommadate another region's rental.\n" +
                                        $"Updated delivery time: ```{regionalTime.ToLongDateString()} {regionalTime.ToLongTimeString()} {AppHelpers.GetRegionZoneString(delivery.Region)}\n" +
                                        $"{delivery.DeliveryTime.ToLongDateString()} {delivery.DeliveryTime.ToLongTimeString()} PST```" +
                                        $"If you have any issue with this, please contact server support.";
                                    _ = DiscordConnection.Instance.SendMessageToChannel(delivery.ChannelName, deliveryTimeUpdateMessage);
                                    return schedulerResult;
                                }
                            }

                            keyNumber++;
                        }

                    }
                }
            }

            if (schedulerResult.Code == ResultEnum.Success)
            {
                if (!newRental.KeyAssigned())
                {
                    throw new Exception("Sanity check failure: TryAddToSchedule has failed to assign rental a key, but claims success");
                }
                else if (m_rentalMaster.UndeliveredRentals.RentalContainerList.Count != (beforeUndeliveredCount + quantity))
                {
                    throw new Exception("Sanity check failure: TryAddToSchedule has failed add a success to the undelivered rental list");
                }
                else if (quantityLeftToFulfill != 0)
                {
                    throw new Exception("Sanity check failure: TryAddToSchedule has failed fulfill all the keys, but claims success");
                }

                Save();
            }

            return schedulerResult;
        }

        private async Task BeginDeliveryIfNecessary()
        {
            int index = 0;
            var undeliveredRentals = new List<RentalContainer>(m_rentalMaster.UndeliveredRentals.RentalContainerList);
            foreach (var rentalContainer in undeliveredRentals)
            {
                var delivery = rentalContainer.RentalInfo;
                if (DateTime.Now > delivery.DeliveryTime)
                {
                    m_rentalMaster.DeliveriesInProgress.RentalContainerList.Add(rentalContainer);
                    m_rentalMaster.DeliveriesInProgress.RentalContainerList.Sort();

                    Console.WriteLine("Retrieving key...");
                    await DiscordConnection.Instance.SendMessageToChannel(delivery.ChannelName, "Retrieving key... Please allow a minute for processing.");
                    await DeliveryManager.Instance.GetBotKey(delivery.InternalKeyNumber);

                    m_rentalMaster.UndeliveredRentals.RentalContainerList.RemoveAt(index);
                    Save();
                }
                index++;
            }
        }

        public RentalInformation NextAvailableKey()
        {
            var currentAvailableKeys = DeliveryManager.Instance.RegisteredContainer;
            var deliveriesInProgress = new List<RentalContainer>(m_rentalMaster.DeliveriesInProgress.RentalContainerList);
            foreach (var rentalContainer in deliveriesInProgress)
            {
                var delivery = rentalContainer.RentalInfo;

                if (currentAvailableKeys.RegisteredKeys.Exists(k => k.BotNumber == delivery.InternalKeyNumber))
                {
                    RentalInformation rentalInformation = delivery;
                    var key = currentAvailableKeys.RegisteredKeys.Find(k => k.BotNumber == delivery.InternalKeyNumber);
                    rentalInformation.Key = key.Key;
                    DeliveryManager.Instance.RegisteredContainer.RegisteredKeys.Remove(key);

                    m_rentalMaster.DeliveredRentals.RentalContainerList.Add(rentalContainer);
                    m_rentalMaster.DeliveredRentals.RentalContainerList.Sort();
                    m_rentalMaster.DeliveriesInProgress.RentalContainerList.RemoveAt(0);
                    Save();
                    return rentalInformation;
                }
            }

            throw new Exception("Couldn't deliver next key");
        }

        public async Task Update()
        {
            await BeginDeliveryIfNecessary();
            if (DeliveryManager.Instance.RegisteredContainer.RegisteredKeys.Count == 0)
            {
                return;
            }

            var keyToDeliver = NextAvailableKey();
            await DiscordConnection.Instance.SendMessageToChannel(keyToDeliver.Key, keyToDeliver.ChannelName, true);
        }
    }
}
