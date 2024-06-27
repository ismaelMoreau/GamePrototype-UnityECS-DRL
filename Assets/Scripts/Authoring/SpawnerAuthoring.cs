using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public float3 SpawnPosition;
    
    public float CurrentSpawnRate = 20f;
    public int CurrentSpawnCount = 1;
    public float UpgradeInterval = 30f;
    
    public float MinSpawnRate = 0.5f;
    public int MaxSpawnCount = 10;
    class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Spawner
            {
                // By default, each authoring GameObject turns into an Entity.
                // Given a GameObject (or authoring component), GetEntity looks up the resulting Entity.
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                SpawnPosition = authoring.transform.position,
                NextSpawnTime = 0.0f,
                CurrentSpawnRate = authoring.CurrentSpawnRate, 
                CurrentSpawnCount = authoring.CurrentSpawnCount,
                UpgradeInterval = authoring.UpgradeInterval,
                NextUpgradeTime = 0f,
                MinSpawnRate = authoring.MinSpawnRate,
                MaxSpawnCount = authoring.MaxSpawnCount
            });
        }
    }
}