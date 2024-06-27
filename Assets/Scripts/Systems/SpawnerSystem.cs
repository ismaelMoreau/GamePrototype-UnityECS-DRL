using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;
using UnityEngine.UIElements;
using Unity.Rendering;
using Random = Unity.Mathematics.Random;
using NUnit.Framework;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct OptimizedSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GamePlayingTag>();
    }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);

        new ProcessSpawnerJob
        {
            ElapsedTime = SystemAPI.Time.ElapsedTime,
            Ecb = ecb,
        }.ScheduleParallel();

        state.Dependency.Complete();
    }

    private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        return ecb.AsParallelWriter();
    }
}
[BurstCompile]
public partial struct ProcessSpawnerJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    public double ElapsedTime;

    private void Execute(Entity e, [ChunkIndexInQuery] int chunkIndex, ref Spawner spawner)
    {
        // Check if it's time to upgrade
        if (ElapsedTime >= spawner.NextUpgradeTime)
        {
            // Increase spawn rate (decrease time between spawns)
            spawner.CurrentSpawnRate = math.max(spawner.CurrentSpawnRate * 0.9f, spawner.MinSpawnRate);
            
            // Increase number of monsters spawned each time
            spawner.CurrentSpawnCount = math.min(spawner.CurrentSpawnCount + 1, spawner.MaxSpawnCount);
            
            spawner.NextUpgradeTime = (float)ElapsedTime + spawner.UpgradeInterval;
        }

        // Check if it's time to spawn
        if (ElapsedTime >= spawner.NextSpawnTime)
        {
            for (int i = 0; i < spawner.CurrentSpawnCount; i++)
            {
                Entity newEntity = Ecb.Instantiate(chunkIndex, spawner.Prefab);
                Ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPosition(spawner.SpawnPosition));
            }

            // Set the next spawn time
            spawner.NextSpawnTime = (float)ElapsedTime + spawner.CurrentSpawnRate;
        }
    }
}