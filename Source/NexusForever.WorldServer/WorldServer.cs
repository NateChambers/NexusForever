﻿using System;
using System.IO;
using System.Reflection;
using NLog;
using NexusForever.Shared;
using NexusForever.Shared.Configuration;
using NexusForever.Shared.Database;
using NexusForever.Shared.Game;
using NexusForever.Shared.GameTable;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Command;
using NexusForever.WorldServer.Game;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Entity.Network;
using NexusForever.WorldServer.Game.Map;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Command.Contexts;

namespace NexusForever.WorldServer
{
    internal static class WorldServer
    {
        #if DEBUG
        private const string Title = "NexusForever: World Server (DEBUG)";
        #else
        private const string Title = "NexusForever: World Server (RELEASE)";
        #endif

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private static void Main()
        {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            Console.Title = Title;
            log.Info("Initialising...");

            ConfigurationManager<WorldServerConfiguration>.Initialise("WorldServer.json");
            using (var webHost = WorldServerEmbeddedWebServer
                .Initialize(ConfigurationManager<WorldServerConfiguration>.Configuration)
                .Build())
            {
                // Expose ASP.NET Core DI outside of ASP.NET Core.
                DependencyInjection.Initialize(webHost.Services);

                DatabaseManager.Initialise(ConfigurationManager<WorldServerConfiguration>.Config.Database);

                GameTableManager.Initialise();

                EntityManager.Initialise();
                EntityCommandManager.Initialise();

                AssetManager.Initialise();
                ServerManager.Initialise();

                MessageManager.Initialise();
                CommandManager.Initialise();
                NetworkManager<WorldSession>.Initialise(ConfigurationManager<WorldServerConfiguration>.Config.Network);
                WorldManager.Initialise(lastTick =>
                {
                    NetworkManager<WorldSession>.Update(lastTick);
                    MapManager.Update(lastTick);
                });

                webHost.Start();
                log.Info("Ready!");

                while (true)
                {
                    Console.Write(">> ");
                    string line = Console.ReadLine();
                    if (!CommandManager.HandleCommand(new ConsoleCommandContext(), line, false))
                        Console.WriteLine("Invalid command");
                }
            }
        }
    }
}
