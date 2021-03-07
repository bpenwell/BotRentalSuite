using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RewardingRentals.Server
{
    public sealed class DiscordConnection
    {
        private DiscordConnection() { }

        public static DiscordConnection Instance { get; } = new DiscordConnection();

        public bool Connected { get; private set; }

        public string Token { get; private set; }

        private ulong m_serverId = 767916439600103515;

        private DiscordSocketClient m_client;

        private CommandService m_commands;

        private IServiceProvider m_services;

        public async Task StartupBot(string token)
        {
            Token = token;
            if (Token.Length == 0)
            {
                Console.WriteLine("Failed StartupBot: m_token is empty");
                return;
            }

            DiscordSocketConfig config = new DiscordSocketConfig();
            config.AlwaysDownloadUsers = true;

            m_client = new DiscordSocketClient(config);
            m_commands = new CommandService();
            m_services = new ServiceCollection()
                .AddSingleton(m_client)
                .AddSingleton(m_commands)
                .BuildServiceProvider();

            m_client.Log += LogClient;
            m_client.Connected += BotConnected;
            m_client.Disconnected += BotDisconnected;

            await RegisterCommandsAsync();
            await m_client.LoginAsync(TokenType.Bot, Token);
            await m_client.StartAsync();
        }

        public async Task SendMessageToChannel(string channelName, string value, bool isKey = false)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            var channels = m_client.GetGuild(m_serverId).TextChannels;

            try
            {
                var applicableChannel = channels.Single(c => c.Name == channelName);

                if (isKey)
                {
                    EmbedBuilder builder = new EmbedBuilder();
                    builder.WithTitle("Delivery Time!")
                        .AddField($"Key",
                        $"{value}\n");
                    await applicableChannel.SendMessageAsync("", false, builder.Build());
                }
                else
                {
                    await applicableChannel.SendMessageAsync(value);
                }
            }
            catch (Exception e)
            {
                var message = "\nMost likely found 0 or multiple channels with the same name\n" + e.Message;
                Console.WriteLine(message);
                throw new Exception(message);
            }

        }

        private Task BotConnected()
        {
            Connected = true;
            return Task.CompletedTask;
        }

        private Task BotDisconnected(Exception exception)
        {
            Connected = false;
            return Task.CompletedTask;
        }

        private Task LogClient(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            m_client.MessageReceived += HandleCommandAsync;
            var assembly = Assembly.LoadFrom("Server");
            var modules = await m_commands.AddModulesAsync(assembly, m_services);
            var modulesList = modules as IList;
            if (modulesList.Count == 0)
            {
                throw new Exception("No modules loaded. Make sure you are loading up the correct assembly");
            }
        }

        public async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message.Author.IsBot)
            {
                return;
            }

            int argPos = 0;
            if (message.HasStringPrefix("rental!", ref argPos))
            {
                var context = new SocketCommandContext(m_client, message);
                var result = await m_commands.ExecuteAsync(context, argPos, m_services);
                if (!result.IsSuccess)
                {
                    var errorMessage = "Error Message:```" + result.ErrorReason + "```" + "**Type rental!help for more information about commands.**\n";
                    Console.WriteLine(result.ErrorReason);
                    await message.Channel.SendMessageAsync(errorMessage);
                }
            }
        }
    }
}
