using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public class DamageUI : ICleanupComponentData
{
    public GameObject Value;
}
public struct damageUIOffset : IComponentData
{
    public float3 Value;
}
public struct DamageComponent : IComponentData 
{
    public float currentDamage;
    public float damageShowTimer;
    public float damageShowDuration;
}

public struct DamageShowUpdate : IComponentData, IEnableableComponent {}
