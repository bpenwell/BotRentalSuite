using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RewardingRentals.Utilities
{
    public class SettingsFile
    {
        public static SettingsFile Instance => new SettingsFile();

        public string FileName = "Settings.txt";

        public Dictionary<string, long> RentableBots;

        private SettingsFile()
        {
            RentableBots = new Dictionary<string, long>();
        }

        /// <summary>
        /// If it doesn't exist, this function will kill the program and open a file explorer where the settings file is made.
        /// </summary>
        private void CreateDefault(string path)
        {
            var fileStream = File.Create(path);

            var line = "#Anything with '#' doesn't get read in the program.\n";
            byte[] line1 = Encoding.UTF8.GetBytes(line);
            fileStream.Write(line1, 0, line1.Length);

            line = "#List of bots you want to rent. YOU WILL NEED TO USE THE EXACT NAME WHEN ADDING TO THE SCHEDULE.\n";
            byte[] line2 = Encoding.UTF8.GetBytes(line);
            fileStream.Write(line2, 0, line2.Length);

            line = "#Another note: the bot name must be 1 word. I.E. TSB (works), The Shit Bot (does not work).\n";
            byte[] line3 = Encoding.UTF8.GetBytes(line);
            fileStream.Write(line3, 0, line3.Length);

            line = "#Example seen below:\n";
            byte[] line4 = Encoding.UTF8.GetBytes(line);
            fileStream.Write(line4, 0, line4.Length);

            line = "TSB: 9\n";
            byte[] line5 = Encoding.UTF8.GetBytes(line);
            fileStream.Write(line5, 0, line5.Length);

            line = "ECB: 1\n";
            byte[] line6 = Encoding.UTF8.GetBytes(line);
            fileStream.Write(line6, 0, line6.Length);

            fileStream.Close();
        }

        public void Parse()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + FileName;

            if (!File.Exists(path))
            {
                CreateDefault(path);
                //Kill the program and open a file explorer to allow the user to set up the file.
                Process.Start("explorer.exe", path);
                Environment.Exit(0);
                return;
            }

            var stream = new StreamReader(path);
            string currentLine = stream.ReadLine();
            while (currentLine.StartsWith('#'))
            {
                currentLine = stream.ReadLine();
                if (currentLine == null)
                {
                    stream.Close();
                    return;
                }
            }

            //Read the bots being rented
            while (!currentLine.StartsWith('#'))
            {
                var parts = currentLine.Split(':');
                var botName = parts[0];
                var numOwned = parts[1];
                string botNameTrimmed = String.Concat(botName.Where(c => !Char.IsWhiteSpace(c)));
                string numOwnedTrimmed = String.Concat(numOwned.Where(c => !Char.IsWhiteSpace(c)));

                RentableBots.Add(botNameTrimmed, Convert.ToInt64(numOwnedTrimmed));

                currentLine = stream.ReadLine();
                if (currentLine == null)
                {
                    stream.Close();
                    return;
                }
            }

            stream.Close();

            if (RentableBots.Count == 0)
            {
                throw new Exception("You don't have any bots to rent out");
            }
        }
    }
}
