using System;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace ServerQoL;

/// <summary>
/// 基础体验三合一：
/// 1. 背包权限卡自动开门（空手 / 持枪时也能用背包里的卡）；
/// 2. 无限子弹（换弹补满、换完归 1、出生归 1）；
/// 3. 无限体力（持续补满，可以一直奔跑）。
/// </summary>
public sealed class ServerQoLPlugin : Plugin<ServerQoLConfig>
{
    private static GameObject? tickerHost;

    private float staminaTimer;

    internal static ServerQoLPlugin? Instance { get; private set; }

    public override string Name => "ServerQoL";

    public override string Description => "背包权限卡自动开门 / 无限子弹 / 无限体力";

    public override string Author => "LLL111III11";

    public override Version RequiredApiVersion => new(1, 1, 7);

    public override void Enable()
    {
        Instance = this;

        try
        {
            if (!Config.IsEnabled)
            {
                Logger.Info("[ServerQoL] 配置已关闭，插件不启用。");
                return;
            }

            PlayerEvents.InteractingDoor += OnInteractingDoor;
            PlayerEvents.ReloadingWeapon += OnReloadingWeapon;
            PlayerEvents.ReloadedWeapon += OnReloadedWeapon;
            PlayerEvents.ChangedItem += OnChangedItem;
            PlayerEvents.Spawned += OnSpawned;

            staminaTimer = 0f;
            CreateTicker();

            Logger.Info("[ServerQoL] 已启用：背包权限卡自动开门 / 无限子弹 / 无限体力");
        }
        catch (Exception exception)
        {
            Logger.Error("[ServerQoL] 初始化失败: " + exception);
        }
    }

    public override void Disable()
    {
        PlayerEvents.InteractingDoor -= OnInteractingDoor;
        PlayerEvents.ReloadingWeapon -= OnReloadingWeapon;
        PlayerEvents.ReloadedWeapon -= OnReloadedWeapon;
        PlayerEvents.ChangedItem -= OnChangedItem;
        PlayerEvents.Spawned -= OnSpawned;

        if (tickerHost != null)
        {
            UnityEngine.Object.Destroy(tickerHost);
            tickerHost = null;
        }

        Instance = null;
    }

    internal void Tick(float deltaTime)
    {
        if (!Config.InfiniteStamina)
        {
            return;
        }

        staminaTimer -= deltaTime;

        if (staminaTimer > 0f)
        {
            return;
        }

        // 间隔太短会让状态同步过于频繁，限制一个下限。
        staminaTimer = Mathf.Max(0.02f, Config.StaminaRefillInterval);
        InfiniteStaminaModule.Refill(Config);
    }

    private static void OnInteractingDoor(PlayerInteractingDoorEventArgs ev)
    {
        if (Instance != null)
        {
            AutoKeycardModule.OnInteractingDoor(ev, Instance.Config);
        }
    }

    private static void OnReloadingWeapon(PlayerReloadingWeaponEventArgs ev)
    {
        if (Instance != null)
        {
            InfiniteAmmoModule.OnReloadingWeapon(ev, Instance.Config);
        }
    }

    private static void OnReloadedWeapon(PlayerReloadedWeaponEventArgs ev)
    {
        if (Instance != null)
        {
            InfiniteAmmoModule.OnReloadedWeapon(ev, Instance.Config);
        }
    }

    private static void OnChangedItem(PlayerChangedItemEventArgs ev)
    {
        if (Instance != null)
        {
            InfiniteAmmoModule.OnChangedItem(ev, Instance.Config);
        }
    }

    private static void OnSpawned(PlayerSpawnedEventArgs ev)
    {
        if (Instance != null)
        {
            InfiniteAmmoModule.OnSpawned(ev, Instance.Config);
        }
    }

    private static void CreateTicker()
    {
        tickerHost = new GameObject("ServerQoL.Ticker");
        UnityEngine.Object.DontDestroyOnLoad(tickerHost);
        tickerHost.AddComponent<ServerQoLTicker>();
    }
}
