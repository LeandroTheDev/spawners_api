﻿using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SpawnersAPI;

public class Initialization : ModSystem
{
    public override void AssetsFinalize(ICoreAPI api)
    {
        base.AssetsFinalize(api);
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    public override double ExecuteOrder()
    {
        return base.ExecuteOrder();
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        Debug.LoadLogger(api.Logger);
        Debug.Log($"Running on Version: {Mod.Info.Version}");
        #region block-register
        api.RegisterBlockEntityClass("spawner", typeof(Spawner));
        #endregion
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
    }

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
    }
}

public class Debug
{
    private static readonly OperatingSystem system = Environment.OSVersion;
    static private ILogger loggerForNonTerminalUsers;

    static public void LoadLogger(ILogger logger) => loggerForNonTerminalUsers = logger;
    static public void Log(string message)
    {
        // Check if is linux or other based system and if the terminal is active for the logs to be show
        if ((system.Platform == PlatformID.Unix || system.Platform == PlatformID.Other) && Environment.UserInteractive)
            // Based terminal users
            Console.WriteLine($"{DateTime.Now:d.M.yyyy HH:mm:ss} [SpawnersAPI] {message}");
        else
            // Unbased non terminal users
            loggerForNonTerminalUsers?.Log(EnumLogType.Notification, $"[SpawnersAPI] {message}");
    }
}