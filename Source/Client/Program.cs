using RewardingRentals.Server;
using RewardingRentals.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace RewardingRentals.Client
{
    public class Program
    {
        public static void Main(string[] args) =>   new Program().MainAsync().GetAwaiter().GetResult();

        public Schedule BotSchedule = new Schedule();
        public async Task MainAsync()
        {
            var settings = SettingsFile.Instance;
            settings.Parse();

            await DiscordConnection.Instance.StartupBot("NzYzOTc5MTY0NjkyMTE5NjEy.X3_lCg.H8wKbecgvaKBjbwJmZAvzBxfsCo");

            while (true)
            {
                if (DiscordConnection.Instance.Connected)
                {
                    await Update();
                }
                else
                {
                    Console.WriteLine("Waiting for internet connection");
                }

                await Task.Delay(1000);
            }
        }

        public async Task Update()
        {
            await BotSchedule.Update();
        }
    }
}
