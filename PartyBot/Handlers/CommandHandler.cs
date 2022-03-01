using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MusicStreaming.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly Servers _servers;

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _servers = services.GetRequiredService<Servers>();
            HookEvents();
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),services: _services);
        }

        public void HookEvents()
        {
            _commands.CommandExecuted += CommandExecutedAsync;
            _commands.Log += LogAsync;
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            //var argPos = 0;

            //if (!(socketMessage is SocketUserMessage message) || message.Author.IsBot)
            //    return Task.CompletedTask;

            //if (!message.HasStringPrefix(GlobalData.Config.DefaultPrefix, ref argPos))
            //    return Task.CompletedTask;

            //var context = new SocketCommandContext(_client, socketMessage as SocketUserMessage);
            //var result = _commands.ExecuteAsync(context, argPos, _services, MultiMatchHandling.Best);
            //return result;          
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            //var prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id) ?? "!";
            var prefix = "-";
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _commands.ExecuteAsync(context, argPos, _services, MultiMatchHandling.Best);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            /* command is unspecified when there was a search failure (command not found); we don't care about these errors */
            if (!command.IsSpecified) return;

            /* the command was succesful, we don't care about this result, unless we want to log that a command succeeded. */
            if (result.IsSuccess) return;
            await context.Channel.SendMessageAsync($"error: {result}");
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }
    }
}
