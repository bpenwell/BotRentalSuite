using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace BotHelper
{
    public class DiscordConnection
    {
        private DiscordSocketClient m_client;
        private CommandService m_commands;
        private IServiceProvider m_services;
        private string m_token;
        public DiscordConnection(string token)
        {
            m_token = token;
        }

        public async Task StartupBot()
        {
            DiscordSocketConfig config = new DiscordSocketConfig();
            config.AlwaysDownloadUsers = true;

            m_client = new DiscordSocketClient(config);
            m_commands = new CommandService();
            m_services = new ServiceCollection()
                .AddSingleton(m_client)
                .AddSingleton(m_commands)
                .BuildServiceProvider();

            m_client.Log += LogClient;

            await RegisterCommandsAsync();
            await m_client.LoginAsync(TokenType.Bot, m_token);
            await m_client.StartAsync();

            await Task.Delay(-1);
        }

        private Task LogClient(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            m_client.MessageReceived += HandleCommandAsync;
            var assembly = Assembly.LoadFrom("BotHelper");
            var modules = await m_commands.AddModulesAsync(assembly, m_services);
            var modulesList = modules as IList;
            if (modulesList.Count == 0)
            {
                throw new Exception("No modules loaded. Make sure you are loading up the correct assembly");
            }
        }

        private async Task HandleCommandAsync(SocketMessage arg)
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
                    var errorMessage = "Error Message:```" + result.ErrorReason + "```" + "\n**Type rental!help for more information about commands.**\n";
                    Console.WriteLine(result.ErrorReason);
                    await message.Channel.SendMessageAsync(errorMessage);
                }
            }
        }
    }
}
