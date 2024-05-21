using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Rendering;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct OptimizedSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state) { 
       
    }

    public void OnDestroy(ref SystemState state) { }
    

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
      
        var configQlearn =  SystemAPI.GetSingleton<ConfigQlearn>();
        foreach ((RefRW<LocalTransform> localTransform, RefRW<Spawner> spawner,Entity e) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRW<Spawner>>().WithEntityAccess())
        {
            spawner.ValueRW.SpawnPosition = SystemAPI.GetComponent<LocalToWorld>(e).Position;
           
        }
       
        EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);
        Random rnd = new Random(123);
        // Creates a new instance of the job, assigns the necessary data, and schedules the job in parallel.
        
        new ProcessSpawnerJob
        {
            ElapsedTime = SystemAPI.Time.ElapsedTime,
            Ecb = ecb,
            rnd = rnd,
            startingEpsilon = configQlearn.sartingEpsilon
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
    public float3 spawnerPosition;
    public EntityCommandBuffer.ParallelWriter Ecb;
    public double ElapsedTime;

    public Random rnd;
    public float startingEpsilon; 
    // IJobEntity generates a component data query based on the parameters of its `Execute` method.
    // This example queries for all Spawner components and uses `ref` to specify that the operation
    // requires read and write access. Unity processes `Execute` for each entity that matches the
    // component data query.
    
    private void Execute(Entity e, [ChunkIndexInQuery] int chunkIndex, ref Spawner spawner)
    {
        
        // If the next spawn time has passed.
        if (spawner.NextSpawnTime < ElapsedTime)
        {
            // Spawns a new entity and positions it at the spawner.
            Entity newEntity = Ecb.Instantiate(chunkIndex, spawner.Prefab);
            
            Ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPosition(spawner.SpawnPosition));
            //Ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPositionRotation(spawner.SpawnPosition,new quaternion(new float4(-90,0,90,0))));
            //Ecb.SetComponent(chunkIndex,newEntity , new URPMaterialPropertyBaseColor { Value = RandomColor(ref rnd) });
            Ecb.SetComponent(chunkIndex,newEntity , new EnemyEpsilonComponent{ epsilon = startingEpsilon });
            // Resets the next spawn time.
            spawner.NextSpawnTime = (float)ElapsedTime + spawner.SpawnRate;
        }
    }
    static float4 RandomColor(ref Random random)
    {
        // 0.618034005f is inverse of the golden ratio
        var hue = (random.NextFloat() + 0.618034005f) % 1;
        return (Vector4)Color.HSVToRGB(hue, 1.0f, 1.0f);
    }
}