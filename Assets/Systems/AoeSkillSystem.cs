using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst.Intrinsics;
using UnityEngine;

// [BurstCompile]
// public struct AOEDamageJob : IJobChunk
// {
//     public float Damage;
//     public float EffectRadius;
//     public double CurrentTime;
//     public EntityCommandBuffer.ParallelWriter CommandBuffer;
//     [ReadOnly] public ComponentTypeHandle<LocalTransform> TranslationTypeHandle;
//     public ComponentTypeHandle<EnemyComponent> enemiesTypeHandle;
//     [ReadOnly] public float3 AOESkillPosition;
    
//     [BurstCompile]
//     public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
//      in v128 chunkEnabledMask)
//     {
    
//         var translations = chunk.GetNativeArray(ref TranslationTypeHandle);
//         var enemies = chunk.GetNativeArray(ref enemiesTypeHandle);

//         var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
//         while(enumerator.NextEntityIndex(out var i))
//         {
//             float distance = math.distance(enemies[i].position, AOESkillPosition);
//             if (distance <= EffectRadius)
//             {
//                 // var health = enemiesHealths[i];
//                 // enemiesHealths. -= Damage;
//                 // enemiesHealths[i] = health;
//                 // ecb needed
//                 CommandBuffer.AddComponent<DestroyTag>(unfilteredChunkIndex,chunk.);
//             }
//         }
//     }
// }

public partial struct AOESkillSystem : ISystem
{
   

    [BurstCompile]
    public void OnCreate(ref SystemState state){
        //state.RequireForUpdate<AOESkill>();
    }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var skillsConfig = SystemAPI.GetSingleton<SkillsConfig>();
        float3? aoePosition = null;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (new Plane(Vector3.up, 0f).Raycast(ray2, out var dist2))
            {
                // Store the new target position in 3D
                aoePosition = ray2.GetPoint(dist2);
            }
            if (aoePosition.HasValue){
                Entity target  = state.EntityManager.Instantiate(skillsConfig.PrefabAoe);
                //if (new Plane(Vector3.up, 0f).Raycast(ray2, out var dist2)){  
                state.EntityManager.SetComponentData(target, new LocalTransform
                {
                    Position = (float3)aoePosition,
                    Scale = 1,  // If we didn't set Scale and Rotation, they would default to zero (which is bad!)
                    Rotation = quaternion.identity
                });
            
        
            // EntityQuery query = SystemAPI.QueryBuilder().WithAll<LocalTransform,EnemyComponent>().Build();
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        

            // //double currentTime = SystemAPI.Time.ElapsedTime;
            // // var translationTypeHandle = state.GetComponentTypeHandle<LocalTransform>(true);
            // // var enemiesTypeHandle = state.GetComponentTypeHandle<Health>();
        
           
        
        

            foreach ( (RefRO<LocalTransform> localTransform,RefRW<EnemyComponent> enemy,Entity e)
                in SystemAPI.Query<RefRO<LocalTransform>, RefRW<EnemyComponent>>().WithEntityAccess())
                {
                    float distance = math.distance(localTransform.ValueRO.Position,state.EntityManager.GetComponentData<LocalTransform>(target).Position );
                    if (distance <= state.EntityManager.GetComponentData<AOESkill>(target).EffectRadius)
                    {
                        // var health = enemiesHealths[i];
                        // enemiesHealths. -= Damage;
                        // enemiesHealths[i] = health;
                        // ecb needed
                        
                        ecb.AddComponent<DestroyTag>(e);
                    }
                }
        
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            // EntityQuery querySkill = SystemAPI.QueryBuilder().WithAll<AOESkill>().Build();
            // state.EntityManager.DestroyEntity(querySkill);
            }
        }
    }
}
