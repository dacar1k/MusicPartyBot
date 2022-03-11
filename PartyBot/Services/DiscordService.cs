﻿using Discord;
using Discord.Commands;
using Infrastructure;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MusicStreaming.Handlers;
using System;
using System.Threading.Tasks;
using Victoria;

namespace MusicStreaming.Services
{
    public class DiscordService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandHandler _commandHandler;
        private readonly ServiceProvider _services;
        private readonly LavaNode _lavaNode;
        private readonly LavaLinkAudio _audioService;
        private readonly GlobalData _globalData;
        
        public DiscordService()
        {
            _services = ConfigureServices();
            _client = _services.GetRequiredService<DiscordSocketClient>();
            _commandHandler = _services.GetRequiredService<CommandHandler>();
            _lavaNode = _services.GetRequiredService<LavaNode>();
            _globalData = _services.GetRequiredService<GlobalData>();
            _audioService = _services.GetRequiredService<LavaLinkAudio>();

            SubscribeLavaLinkEvents();
            SubscribeDiscordEvents();
        }

        public async Task InitializeAsync()
        {
            await InitializeGlobalDataAsync();
            
            await _client.LoginAsync(TokenType.Bot, GlobalData.Config.DiscordToken);
            await _client.StartAsync();

            await _commandHandler.InitializeAsync();

            await Task.Delay(-1);
        }

        private void SubscribeLavaLinkEvents()
        {
            _lavaNode.OnLog += LogAsync;
            _lavaNode.OnTrackEnded += _audioService.TrackEnded;            
        }

        private void SubscribeDiscordEvents()
        {
            _client.Ready += ReadyAsync;
            _client.Log += LogAsync;
        }

        private async Task InitializeGlobalDataAsync()
        {
            await _globalData.InitializeAsync();
        }

        private async Task ReadyAsync()
        {
            try
            {
                await _lavaNode.ConnectAsync();
                //await _client.SetGameAsync(GlobalData.Config.GameStatus);
                //await _client.SetGameAsync();
            }
            catch (Exception ex)
            {
                await LoggingService.LogInformationAsync(ex.Source, ex.Message);
            }

        }
        private async Task LogAsync(LogMessage logMessage)
        {
                await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message);
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddDbContext<DataBaseContext>()
                .AddSingleton<Servers>()
                .AddSingleton<Tracks>()   //если что править тут
                .AddSingleton<PlayLists>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<LavaNode>()
                .AddSingleton(new LavaConfig())
                .AddSingleton<LavaLinkAudio>()
                .AddSingleton<ConfigurationService>()
                .AddSingleton<BotService>()
                .AddSingleton<GlobalData>()
                .BuildServiceProvider();
        }
    }
}
