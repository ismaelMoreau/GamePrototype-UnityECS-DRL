using Unity.Entities;
using Unity.Mathematics;

public struct Spawner : IComponentData
{
    public Entity Prefab;
    public float3 SpawnPosition;
    public float NextSpawnTime;
    public float CurrentSpawnRate;
    public int CurrentSpawnCount;
    public float UpgradeInterval;
    public float NextUpgradeTime;
    public float MinSpawnRate;
    public int MaxSpawnCount;
}