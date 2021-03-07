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
            Console.WriteLine(DateTime.Now);
            await ScheduleManager.Instance.Update();
        }
    }
}
