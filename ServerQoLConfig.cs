using System.ComponentModel;

namespace ServerQoL;

/// <summary>
/// 三合一基础体验插件配置。首次运行会生成在
/// %APPDATA%\SCP Secret Laboratory\LabAPI\configs\&lt;端口&gt;\ServerQoL\config.yml
/// </summary>
public sealed class ServerQoLConfig
{
    [Description("总开关。关闭后三个功能全部失效。")]
    public bool IsEnabled { get; set; } = true;

    [Description("背包权限卡自动开门：手里没卡（空手或持枪）时，用背包里权限足够的卡开门。")]
    public bool AutoKeycard { get; set; } = true;

    [Description("无限子弹：换弹时补满该枪所需弹药，换完后所有弹药数量归为 1。")]
    public bool InfiniteAmmo { get; set; } = true;

    [Description("换弹完成 / 出生后，把每种子弹的备弹数量设为多少。默认 1。")]
    public int AmmoAfterReload { get; set; } = 1;

    [Description("无限体力：周期性把体力补满，让玩家可以一直奔跑。")]
    public bool InfiniteStamina { get; set; } = true;

    [Description("体力补充间隔（秒）。越小越顺滑，但同步越频繁。默认 0.1。")]
    public float StaminaRefillInterval { get; set; } = 0.1f;

    [Description("体力补到多少。1 为满体力，可填 0.1~1 之间的值。")]
    public float StaminaValue { get; set; } = 1f;

    [Description("是否输出调试日志（自动开门、补弹等）。")]
    public bool Debug { get; set; } = false;
}
