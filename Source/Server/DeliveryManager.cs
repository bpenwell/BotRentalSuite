using RewardingRentals.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
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
            RentableBots = SettingsFile.Instance.RentableBots;
            if (!File.Exists(m_absoluteSavePath))
            {
                Console.WriteLine($"Created {m_absoluteSavePath}");
                File.Create(m_absoluteSavePath);
                return;
            }
        }

        public string SaveFileName => @"DeliveryManager.sv";
        private string m_absoluteSavePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + SaveFileName;

        public void Save()
        {
            File.Delete(m_absoluteSavePath);

            Console.WriteLine($"Saved latest {m_absoluteSavePath}");
            using (FileStream fileStream = File.OpenWrite(m_absoluteSavePath))
            {
                //Output currently scheduled stuff
            }
        }

        public async Task GoToToolsPage()
        {
            MousePoint toolButtonCoords = new MousePoint(437, 525);
            await LeftClick(toolButtonCoords);
        }

        public async Task<string> GetBotKey(long number)
        {
            MousePoint toolsButtonCoords = new MousePoint(437, 349);
            await GoToToolsPage();
            Dictionary<long, MousePoint> openBotButtonMap = new Dictionary<long, MousePoint>();
            openBotButtonMap.Add(1, new MousePoint(930, 390));
            openBotButtonMap.Add(2, new MousePoint(1150, 390));
            openBotButtonMap.Add(3, new MousePoint(1390, 390));
            openBotButtonMap.Add(4, new MousePoint(700, 540));
            openBotButtonMap.Add(5, new MousePoint(930, 540));
            openBotButtonMap.Add(6, new MousePoint(1150, 540));
            openBotButtonMap.Add(7, new MousePoint(1390, 540));
            openBotButtonMap.Add(8, new MousePoint(700, 680));
            openBotButtonMap.Add(9, new MousePoint(930, 680));
            openBotButtonMap.Add(10, new MousePoint(1150, 680));
            openBotButtonMap.Add(11, new MousePoint(1390, 680));
            openBotButtonMap.Add(12, new MousePoint(700, 825));
            openBotButtonMap.Add(13, new MousePoint(930, 825));
            openBotButtonMap.Add(14, new MousePoint(1150, 825));
            openBotButtonMap.Add(15, new MousePoint(1390, 825));

            if (!openBotButtonMap.ContainsKey(number))
            {
                throw new Exception("Invalid bot number selection");
            }

            var buttonCoord = openBotButtonMap[number];
            await LeftClick(buttonCoord);


            //MousePoint toolsButtonCoords = new MousePoint(1340, 349);

            return "";
        }
    }
}
