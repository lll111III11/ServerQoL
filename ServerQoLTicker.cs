using UnityEngine;

namespace ServerQoL;

/// <summary>
/// 每帧驱动体力补充的宿主组件。
/// </summary>
public sealed class ServerQoLTicker : MonoBehaviour
{
    private void Update()
    {
        ServerQoLPlugin.Instance?.Tick(Time.deltaTime);
    }
}
