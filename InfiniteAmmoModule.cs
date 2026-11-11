using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using Logger = LabApi.Features.Console.Logger;

namespace ServerQoL;

/// <summary>
/// 无限子弹。
/// 原版换弹时从 ReserveAmmo 里扣除弹匣容量，所以：
/// 1. 收到换弹请求时，把该枪所需弹药补到“刚好装满一个弹匣”；
/// 2. 换弹结束后，把所有弹药数量压回配置值（默认 1）；
/// 3. 出生与切枪时保证备弹至少有 1，否则原版会直接拒绝换弹。
/// </summary>
internal static class InfiniteAmmoModule
{
    internal static void OnReloadingWeapon(PlayerReloadingWeaponEventArgs ev, ServerQoLConfig config)
    {
        if (!config.InfiniteAmmo || ev.Player == null || ev.FirearmItem == null)
        {
            return;
        }

        FirearmItem firearm = ev.FirearmItem;
        ItemType ammoType = firearm.AmmoType;

        if (ammoType == ItemType.None)
        {
            return;
        }

        int needed = firearm.MaxAmmo - firearm.StoredAmmo;

        if (needed <= 0)
        {
            needed = firearm.ChamberMax - firearm.ChamberedAmmo;
        }

        if (needed <= 0)
        {
            return;
        }

        if (ev.Player.GetAmmo(ammoType) >= needed)
        {
            return;
        }

        ev.Player.SetAmmo(ammoType, (ushort)Math.Min(needed, ushort.MaxValue));

        if (config.Debug)
        {
            Logger.Info($"[ServerQoL] {ev.Player.Nickname} 换弹前补足 {ammoType} x{needed}");
        }
    }

    internal static void OnReloadedWeapon(PlayerReloadedWeaponEventArgs ev, ServerQoLConfig config)
    {
        if (!config.InfiniteAmmo || ev.Player == null)
        {
            return;
        }

        ApplyConfiguredAmount(ev.Player, config);
    }

    internal static void OnSpawned(PlayerSpawnedEventArgs ev, ServerQoLConfig config)
    {
        if (!config.InfiniteAmmo || ev.Player == null)
        {
            return;
        }

        ApplyConfiguredAmount(ev.Player, config);
        EnsureFirearmsHaveReserve(ev.Player, config);
    }

    internal static void OnChangedItem(PlayerChangedItemEventArgs ev, ServerQoLConfig config)
    {
        if (!config.InfiniteAmmo || ev.Player == null || ev.NewItem is not FirearmItem firearm)
        {
            return;
        }

        ItemType ammoType = firearm.AmmoType;

        if (ammoType == ItemType.None || ev.Player.GetAmmo(ammoType) > 0)
        {
            return;
        }

        ev.Player.SetAmmo(ammoType, (ushort)Clamp(config.AmmoAfterReload));
    }

    /// <summary>把所有已有子弹条目统一设为配置值，做到“换完弹全部为 1”。</summary>
    private static void ApplyConfiguredAmount(Player player, ServerQoLConfig config)
    {
        if (player.Ammo.Count == 0)
        {
            return;
        }

        ushort amount = (ushort)Clamp(config.AmmoAfterReload);
        List<ItemType> keys = new(player.Ammo.Keys);

        foreach (ItemType ammoType in keys)
        {
            player.SetAmmo(ammoType, amount);
        }
    }

    /// <summary>出生时背包里的枪如果连备弹条目都没有，补一条，保证第一次换弹能触发。</summary>
    private static void EnsureFirearmsHaveReserve(Player player, ServerQoLConfig config)
    {
        ushort amount = (ushort)Clamp(config.AmmoAfterReload);

        foreach (Item item in player.Items)
        {
            if (item is not FirearmItem firearm)
            {
                continue;
            }

            ItemType ammoType = firearm.AmmoType;

            if (ammoType != ItemType.None && player.GetAmmo(ammoType) == 0)
            {
                player.SetAmmo(ammoType, amount);
            }
        }
    }

    private static int Clamp(int value)
    {
        if (value < 1)
        {
            return 1;
        }

        return value > ushort.MaxValue ? ushort.MaxValue : value;
    }
}
