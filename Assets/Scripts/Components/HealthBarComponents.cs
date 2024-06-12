using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class HealthBarUI : ICleanupComponentData
{
    public GameObject Value;
}
public struct HealthBarOffset : IComponentData
{
    public float3 Value;
}

public struct UpdateHealthBarUI : IComponentData, IEnableableComponent {}
