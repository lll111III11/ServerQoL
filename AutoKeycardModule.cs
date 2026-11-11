using Interactables.Interobjects.DoorUtils;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using Logger = LabApi.Features.Console.Logger;

namespace ServerQoL;

/// <summary>
/// 背包权限卡自动开门。
/// 原版只在“当前手持物品”里找 IDoorPermissionProvider，空手或持枪时背包里的卡等于不存在；
/// 这里在门交互被判定为无权限时，遍历背包找出权限足够的卡，直接把 CanOpen 置为 true。
/// 已经上锁的门（管理员锁 / 门禁锁 / 核弹等）不处理，仍然需要正常解锁。
/// </summary>
internal static class AutoKeycardModule
{
    internal static void OnInteractingDoor(PlayerInteractingDoorEventArgs ev, ServerQoLConfig config)
    {
        if (!config.AutoKeycard || ev.Player == null || ev.Door == null)
        {
            return;
        }

        // 已经有权限（手持卡 / 角色自带权限 / 管理员）时不插手。
        if (ev.CanOpen || !ev.Player.IsAlive)
        {
            return;
        }

        Door door = ev.Door;

        if (door.Base == null || door.Base.ActiveLocks > 0)
        {
            return;
        }

        DoorPermissionFlags required = door.Permissions;

        if (required == DoorPermissionFlags.None)
        {
            return;
        }

        KeycardItem? usable = FindUsableCard(ev.Player, door, required);

        if (usable == null)
        {
            return;
        }

        ev.CanOpen = true;

        if (config.Debug)
        {
            Logger.Info($"[ServerQoL] {ev.Player.Nickname} 用背包里的 {usable.Type} 打开了 {door.NameTag}");
        }
    }

    private static KeycardItem? FindUsableCard(Player player, Door door, DoorPermissionFlags required)
    {
        foreach (Item item in player.Items)
        {
            if (item is not KeycardItem card)
            {
                continue;
            }

            // 钥匙卡等级用 CheckPermissions 判定，和原版一样支持 RequireAll / 任意匹配。
            if (door.Base.RequiredPermissions.CheckPermissions(card.Permissions))
            {
                return card;
            }
        }

        return null;
    }
}
