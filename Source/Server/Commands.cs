using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Server;
using Utilities;
using Utilities.Type;

namespace RewardingRentals.Server
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle($"Help Menu")
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .AddField("Admin Commands",
                "**schedule <bot name> <quantity> <region> <@discord user> ** | Add a bot rental to the schedule if available\n"
                )
                .AddField("Non-Admin Commands",
                "**availability <Bot>** | Checks availability of a bot\n"
                )
                .WithColor(Color.Gold);

            await ReplyAsync("", false, builder.Build());
        }

        [Command("rent")]
        //This will allow supports to schedule with the bot
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task AddToSchedule(string botName, DateTime dropDate, long quantity, string region, string discordID, decimal totalPrice, string timeBeforeDrop, bool firmDelivery)
        {
            var idParts = discordID.Split('!', '<', '>');
            if (idParts.Length == 3)
            {
                throw new Exception("Cannot add a bot to the rentals schedule");
            }

            var id = idParts[2];
            var serverUser = Context.Client.GetUser(Convert.ToUInt64(id));
            var user = serverUser.Username;
            var ticketChannelName = AppHelpers.GetNextTicketName(user, botName).ToLower();

            //Backend will determine if this scheduling is possible
            var result = ScheduleManager.Instance.TryAddToSchedule(botName, dropDate, quantity, region, discordID, ticketChannelName, totalPrice, AppHelpers.GetRentalDeliveryTime(timeBeforeDrop), firmDelivery);

            if (result.Code == ResultEnum.Unavailable)
            {
                await ReplyAsync($"Scheduling failed for the following reason: {result.Message}");
                return;
            }

            //Make a private channel, deny all permissions unless you are explicitly the owner or the client
            var ticketChannel = await Context.Guild.CreateTextChannelAsync($"{ticketChannelName}");
            await ticketChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, AppHelpers.GetEveryonePermissionOverrides());
            await ticketChannel.AddPermissionOverwriteAsync(Context.Guild.Owner, AppHelpers.GetClientPermissionOverrides());
            await ticketChannel.AddPermissionOverwriteAsync(serverUser, AppHelpers.GetClientPermissionOverrides());
            // Confirm success via in the current channel
            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Scheduler")
                .AddField("Success",
                $"Your delivery channel is {ticketChannel.Mention}. The key will be delivered there.\n");

            await ReplyAsync("", false, builder.Build());

            await ticketChannel.SendMessageAsync(result.Message);
        }


        [Command("testOpenToolTab")]
        //This will allow supports to schedule with the bot
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task TestOpenToolTab()
        {
            await DeliveryManager.Instance.GetToBurnerPage();
        }


        [Command("deliver")]
        //This will allow supports to schedule with the bot
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task Deliver(long number)
        {
            await DeliveryManager.Instance.GetBotKey(number);
        }


        [Command("register")]
        public async Task RegisterKey(long number, string key)
        {
            DeliveryManager.Instance.RegisterNewKey(number, key);
            await Task.CompletedTask;
        }
    }
}