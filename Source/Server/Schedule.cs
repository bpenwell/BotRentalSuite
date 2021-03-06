using System;
using System.Threading.Tasks;

namespace RewardingRentals.Server
{
    public class Schedule
    {
        public Schedule()
        {
        }

        public void Update()
        {
            if (ScheduleManager.Instance.NeedToDeliverKeys())
            {
                var keyInfo = ScheduleManager.Instance.DeliverNextKey();
                var discordConnection = DiscordConnection.Instance;
                discordConnection.DeliverKey(keyInfo.Key, keyInfo.ChannelName);
            }
            Console.WriteLine(DateTime.Now);
        }
    }
}
