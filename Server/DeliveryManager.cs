using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Utilities;

namespace Server
{
    public class DeliveryManager : ISaveable
    {
        public static DeliveryManager Instance => new DeliveryManager();

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
            RentableBots = new Dictionary<string, long>();
            if (!File.Exists(m_absoluteSavePath))
            {
                File.Create(m_absoluteSavePath);
                return;
            }
        }

        public string SaveFileName => @"DeliveryManager.sv";
        private string m_absoluteSavePath => Path.Combine(Assembly.GetExecutingAssembly() + SaveFileName);

        public void Save()
        {
            File.Delete(m_absoluteSavePath);

            using (FileStream fileStream = File.OpenWrite(m_absoluteSavePath))
            {
                //Output currently scheduled stuff
            }
        }
    }
}
