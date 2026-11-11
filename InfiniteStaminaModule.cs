using System;
using LabApi.Features.Wrappers;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace ServerQoL;

/// <summary>
/// 无限体力：按固定间隔把存活玩家的体力条补回配置值。
/// 体力是 0~1 的归一化值，直接写 Player.StaminaRemaining 会走原版状态同步。
/// </summary>
internal static class InfiniteStaminaModule
{
    private static int errorCount;

    internal static void Refill(ServerQoLConfig config)
    {
        float value = Mathf.Clamp(config.StaminaValue, 0.1f, 1f);

        foreach (Player player in Player.List)
        {
            if (player == null || !player.IsAlive)
            {
                continue;
            }

            try
            {
                player.StaminaRemaining = value;
            }
            catch (Exception exception)
            {
                if (errorCount++ < 3)
                {
                    Logger.Warn("[ServerQoL] 补充体力失败: " + exception.Message);
                }
            }
        }
    }
}
