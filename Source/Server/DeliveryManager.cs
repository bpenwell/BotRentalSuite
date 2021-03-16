using RewardingRentals.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Threading.Tasks;
using Utilities;
using Utilities.Type;
using static Utilities.MouseOperations;
using System.Xml.Serialization;

namespace Server
{
    public class DeliveryManager : ISaveable
    {
        public static DeliveryManager Instance { get; } = new DeliveryManager();

        /// <summary>
        /// Should be loaded from a config file
        /// </summary>
        public RentableBotsContainer RentableContainer;

        public RegisteredKeysContainer RegisteredContainer;

        public long MaximumBotKeys(string botName)
        {
            var data = RentableContainer.RentableBots.Find(b => b.Bot == botName);
            return data.Quantity;
        }

        private DeliveryManager()
        {
            RegisteredContainer = new RegisteredKeysContainer();
            RentableContainer = new RentableBotsContainer
            {
                RentableBots = SettingsFile.Instance.RentableBots
            };
            Load();
        }

        ~DeliveryManager()
        {
            Save();
        }

        public string SaveFileName => @"DeliveryManager.xml";
        private string m_absoluteSavePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + SaveFileName;

        public void Load()
        {
            if (File.Exists(m_absoluteSavePath))
            {
                Console.WriteLine($"Loading latest {m_absoluteSavePath}");
                using (FileStream fileStream = File.OpenWrite(m_absoluteSavePath))
                {
                    Type[] extraTypes1 = new Type[1];
                    extraTypes1[0] = typeof(BotNumberPair);

                    Type[] extraTypes2 = new Type[1];
                    extraTypes2[0] = typeof(BotKeyPair);

                    XmlSerializer serializer1 = new XmlSerializer(typeof(RentableBotsContainer), extraTypes1);
                    XmlSerializer serializer2 = new XmlSerializer(typeof(RegisteredKeysContainer), extraTypes2);
                    serializer1.Serialize(fileStream, RentableContainer);
                    serializer2.Serialize(fileStream, RegisteredContainer);
                }
            }
        }

        public void Save()
        {
            File.Delete(m_absoluteSavePath);

            if (RentableContainer.RentableBots.Count == 0 && RegisteredContainer.RegisteredKeys.Count == 0)
            {
                return;
            }

            Console.WriteLine($"Saved latest {m_absoluteSavePath}");
            using (FileStream fileStream = File.OpenWrite(m_absoluteSavePath))
            {
                Type[] extraTypes1 = new Type[1];
                extraTypes1[0] = typeof(BotNumberPair);

                Type[] extraTypes2 = new Type[1];
                extraTypes2[0] = typeof(BotKeyPair);

                XmlSerializer serializer1 = new XmlSerializer(typeof(RentableBotsContainer), extraTypes1);
                XmlSerializer serializer2 = new XmlSerializer(typeof(RegisteredKeysContainer), extraTypes2);
                RentableContainer = serializer1.Deserialize(fileStream) as RentableBotsContainer;
                RegisteredContainer = serializer2.Deserialize(fileStream) as RegisteredKeysContainer;
            }
        }

        public async Task GetToBurnerPage()
        {
            MousePoint toolButtonCoords = new MousePoint(437, 525);
            await LeftClick(toolButtonCoords);

            MousePoint burnerTab = new MousePoint(950, 190);
            await LeftClick(burnerTab);
        }

        public async Task GetBotKey(long number)
        {
            var handler = new InteractionHandler();
            await handler.LoginToAccount(number);
            await handler.GoToDMsMenu();
            await handler.GoToUserDM("TSB-Bot");
            await handler.ResetKey("!reset ");
            await Task.Delay(TimeSpan.FromSeconds(5));
            await handler.CopyLastMessage();
            await handler.GoToUserDM("Rewarding Rentals#9446");
            await handler.RegisterNewKeyToBot(number);
            await handler.CloseBurner();
        }

        public void RegisterNewKey(long number, string key)
        {
            var exists = RegisteredContainer.RegisteredKeys.Exists(k => k.BotNumber == number || k.Key == key);
            if (exists)
            {
                throw new Exception("Registering new key broke.");
            }

            RegisteredContainer.RegisteredKeys.Add(new BotKeyPair
            {
                Key = key,
                BotNumber = number
            });
        }
    }
}
