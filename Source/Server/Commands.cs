using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Utilities;

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

        [Command("schedule")]
        //This will allow supports to schedule with the bot
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task AddToSchedule(string botName, DateTime dropDate, long quantity, string region, string discordID, decimal totalPrice, string timeBeforeDrop)
        {
            var idParts = discordID.Split('!', '<', '>');
            var serverUser = Context.Client.GetUser(Convert.ToUInt64(idParts[2]));
            var user = serverUser.Username;
            var ticketChannelName = AppHelpers.GetNextTicketName(user, botName).ToLower();

            //Backend will determine if this scheduling is possible
            var result = ScheduleManager.Instance.TryAddToSchedule(botName, dropDate, quantity, region, discordID, ticketChannelName, totalPrice, AppHelpers.GetRentalDeliveryTime(timeBeforeDrop));

            if (result.Code == ResultEnum.Unavailable)
            {
                await ReplyAsync($"Scheduling failed for the following reason: {result.Result}");
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
                $"{builder.Author} scheduled you to rent {quantity} {botName} for the {region} region!\n" +
                $"Your delivery channel is {ticketChannel.Mention}. The key will be delivered to this channel.\n");

            await ReplyAsync("", false, builder.Build());
        }
    }
}
