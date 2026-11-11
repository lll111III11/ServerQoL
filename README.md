# ServerQoL

Three small quality-of-life features for **SCP: Secret Laboratory** servers running **LabAPI 1.1.7**.

| Feature | What it does |
|---|---|
| **Backpack keycard** | Opens permission doors using a keycard stored in the inventory, even when your hands are empty or holding a weapon. Locked doors are never bypassed. |
| **Infinite ammo** | One reload refills the reserve to exactly what the magazine needs; afterwards every ammo count is set back to 1. Ammo is also set to 1 on spawn. |
| **Infinite stamina** | Refills the stamina bar on a short interval so players can sprint indefinitely. |

Everything can be turned on or off, or removed entirely, from the config file.

## Install

1. Put `ServerQoL.dll` into `LabAPI/plugins/global/`.
2. Restart the server (or reload plugins).
3. Edit `LabAPI/configs/<port>/ServerQoL/config.yml` and restart again if you change anything.

Nothing else is required. No dependencies besides LabAPI.

## Configuration

```yaml
is_enabled: true                 # master switch
auto_keycard: true               # backpack keycard auto-open
infinite_ammo: true              # infinite ammo
ammo_after_reload: 1             # reserve amount after reload / on spawn
infinite_stamina: true           # infinite stamina
stamina_refill_interval: 0.1     # seconds between stamina refills
stamina_value: 1                 # stamina value to restore (0.1 - 1)
debug: false                     # verbose logs
```

Set a feature to `false` to disable just that part, or set `is_enabled: false` to disable everything.

## Behaviour details

- **Keycards**: the plugin reads `player.Items`, so the card never has to be in your hand. The door's own `RequireAll` / any-permission rules are respected by calling the game's `DoorPermissionsPolicy.CheckPermissions`.
- **Locked doors** (`ActiveLocks > 0`, admin locks, warhead locks) are skipped; normal unlocking still applies.
- **Elevator doors** are button-only and unaffected.
- **Ammo**: the reserve is topped up when the reload request arrives, then clamped back to `ammo_after_reload` when the reload finishes. Switching to a firearm with zero reserve tops it up to 1 so the reload can start.
- **Stamina**: written through `Player.StaminaRemaining`, so it uses the vanilla stat sync.

## Build

```bash
dotnet build -c Release
```

The project expects the SCP:SL dedicated server `Managed` folder; override it with `-p:ManagedDir=...` if needed.

## License

MIT. See [LICENSE](LICENSE).

---

# ServerQoL（中文）

面向 **SCP: Secret Laboratory** 服务器、基于 **LabAPI 1.1.7** 的三个基础体验功能：

- **背包权限卡自动开门**：空手或持枪时也能用背包里权限足够的卡开门，上锁的门不会被绕过。
- **无限子弹**：按一次换弹就补足弹匣所需弹药，换完后所有备弹归为 1；出生时也设为 1。
- **无限体力**：周期性补满体力，可以一直奔跑。

## 安装

1. 把 `ServerQoL.dll` 放进 `LabAPI/plugins/global/`。
2. 重启服务器或重载插件。
3. 配置文件位于 `LabAPI/configs/<端口>/ServerQoL/config.yml`，三个功能都可单独开关。

## 说明

- 判断权限时直接调用游戏自己的 `DoorPermissionsPolicy.CheckPermissions`，所以 `RequireAll` 等规则与原版一致。
- 上锁的门（管理员锁、门禁锁、核弹锁）不处理。
- 电梯门是按钮式，不受影响。
- 换弹补弹与归 1 都走原版 `ServerSetAmmo`，客户端显示同步。
