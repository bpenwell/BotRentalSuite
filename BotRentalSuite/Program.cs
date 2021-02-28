using BotHelper;
using System.Threading.Tasks;

namespace BotRentalSuite
{
    public class Program
    {
        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            var connection = new DiscordConnection("NzYzOTc5MTY0NjkyMTE5NjEy.X3_lCg.H8wKbecgvaKBjbwJmZAvzBxfsCo");
            await connection.StartupBot();
        }
    }
}
