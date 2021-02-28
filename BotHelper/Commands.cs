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
            var idParts = discordID.Split('!', '<', '>');
            var serverUser = Context.Client.GetUser(Convert.ToUInt64(idParts[2]));
            var user = serverUser.Username;

            Random ran = new Random();
            ulong origin = (ulong)GuildPermission.Speak + (ulong)GuildPermission.SendTTSMessages + (ulong)GuildPermission.SendMessages + (ulong)GuildPermission.ViewChannel + (ulong)GuildPermission.EmbedLinks + (ulong)GuildPermission.Connect + (ulong)GuildPermission.AttachFiles + (ulong)GuildPermission.AddReactions;
            GuildPermissions perms = new GuildPermissions(origin);

            var ticketChannelName = AppHelpers.GetNextTicketName(user);
            TextChannelProperties props = new TextChannelProperties();

            var everyoneOverrides = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);
            var clientOverrides = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);
            var ticketChannel = await Context.Guild.CreateTextChannelAsync($"{ticketChannelName}");

            //Make a private channel
            await ticketChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, everyoneOverrides);
            await ticketChannel.AddPermissionOverwriteAsync(Context.Guild.Owner, clientOverrides);
            await ticketChannel.AddPermissionOverwriteAsync(serverUser, clientOverrides);

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("Scheduler")
                .AddField("Success",
                $"{builder.Author} scheduled you to rent {quantity} {botName} for the {region} region!\n" +
                $"Your delivery channel is #{ticketChannelName}. The key will be delivered to this channel. \n" +
                $" \n");
            await ReplyAsync("", false, builder.Build());
        }
    }
}
