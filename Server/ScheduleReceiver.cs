using System;
using System.Threading.Tasks;

namespace RewardingRentals.Server
{
    public class ScheduleReceiver
    {
        private static readonly ScheduleReceiver m_instance = new ScheduleReceiver();

        private ScheduleReceiver()
        {
        }

        public static ScheduleReceiver Instance
        {
            get
            {
                return m_instance;
            }
        }

        public async Task Update()
        {
            if (ScheduleManager.Instance.NeedToDeliverKeys())
            {
                var keyInfo = await ScheduleManager.Instance.DeliverNextKey();
                var discordConnection = DiscordConnection.Instance;
                await discordConnection.DeliverKeyAndDie(keyInfo.Key, keyInfo.ChannelName);
            }
            Console.WriteLine(DateTime.Now);
        }
    }
}
