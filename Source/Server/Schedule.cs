using System;
using System.Threading.Tasks;

namespace RewardingRentals.Server
{
    public class Schedule
    {
        public Schedule()
        {
        }

        public async Task Update()
        {
            if (ScheduleManager.Instance.NeedToDeliverKeys())
            {
                var keyInfo = await ScheduleManager.Instance.DeliverNextKey();
                var discordConnection = DiscordConnection.Instance;
                await discordConnection.DeliverKey(keyInfo.Key, keyInfo.ChannelName);
            }
            Console.WriteLine(DateTime.Now);
        }
    }
}
