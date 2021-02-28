using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace BotHelper.Modules
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

        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }

        [Command("schedule")]
        //This will allow supports to schedule with the bot
        [RequireUserPermission(ChannelPermission.ManageRoles)]
        public async Task AddToSchedule(string botName, string quantity, string region, string discordID)
        {
            //Backend will determine if this scheduling is possible
            bool failure = false;
            if (failure)
            {
                var reason = "U suck kid";
                await ReplyAsync($"Scheduling failed for the following reason: {reason}");
                return;
            }

            //Make a private channel, deny all permissions unless you are explicitly the owner or the client
            var idParts = discordID.Split('!', '<', '>');
            var serverUser = Context.Client.GetUser(Convert.ToUInt64(idParts[2]));
            var user = serverUser.Username;
            var ticketChannelName = AppHelpers.GetNextTicketName(user).ToLower();
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
