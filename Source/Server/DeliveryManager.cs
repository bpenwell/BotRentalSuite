﻿using RewardingRentals.Utilities;
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
    }
}