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

namespace Server
{
    public class DeliveryManager : ISaveable
    {
        public static DeliveryManager Instance { get; } = new DeliveryManager();

        /// <summary>
        /// Should be loaded from a config file
        /// </summary>
        public Dictionary<string, long> RentableBots;

        public Dictionary<long, string> RegisteredKeys;

        public long MaximumBotKeys(string botName)
        {
            if (!RentableBots.TryGetValue(botName, out var keys))
            {
                throw new Exception($"Cannot find any rentable bots under the name: {botName}");
            }

            return keys;
        }

        private DeliveryManager()
        {
            RegisteredKeys = new Dictionary<long, string>();
            RentableBots = SettingsFile.Instance.RentableBots;
            //Load();
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
                    AppHelpers.Deserialize(fileStream, RentableBots);
                    AppHelpers.Deserialize(fileStream, RegisteredKeys);
                }
            }
        }

        public void Save()
        {
            File.Delete(m_absoluteSavePath);

            if (RentableBots.Count == 0 && RegisteredKeys.Count == 0)
            {
                return;
            }

            Console.WriteLine($"Saved latest {m_absoluteSavePath}");
            using (FileStream fileStream = File.OpenWrite(m_absoluteSavePath))
            {
                AppHelpers.Serialize(fileStream, RentableBots);
                AppHelpers.Serialize(fileStream, RegisteredKeys);
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
            if (RegisteredKeys.ContainsKey(number) || RegisteredKeys.ContainsValue(key))
            {
                throw new Exception("Registering new key broke.");
            }

            RegisteredKeys.Add(number, key);
        }
    }
}
