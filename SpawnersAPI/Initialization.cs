﻿using SpawnersAPI.Shared;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SpawnersAPI;

public class Initialization : ModSystem
{
    readonly Overwrite overwriter = new();

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

        // Register spawner block
        api.RegisterBlockEntityClass("spawner", typeof(Spawner));
        // Overwrite native functions
        overwriter.OverwriteNativeFunctions(api);
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
    static private ILogger loggerForNonTerminalUsers;

    static public void LoadLogger(ILogger logger) => loggerForNonTerminalUsers = logger;
    static public void Log(string message) => loggerForNonTerminalUsers?.Log(EnumLogType.Notification, $"[SpawnersAPI] {message}");
}