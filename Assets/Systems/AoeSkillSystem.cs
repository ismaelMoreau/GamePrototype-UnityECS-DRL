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
   
    PlayerTargetPosition priorMousePositionAfterButtonPress;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state){
         state.RequireForUpdate<SkillsConfig>();
         state.RequireForUpdate<PlayerTargetPosition>();
         state.RequireForUpdate<Target>();
    }

    public void OnDestroy(ref SystemState state) { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var skillsConfig = SystemAPI.GetSingleton<SkillsConfig>();   
        var playerTargetPosition = SystemAPI.GetSingleton<PlayerTargetPosition>();
        Entity targetPrefab = SystemAPI.GetSingletonEntity<Target>();
        
        if (playerTargetPosition.isWaitingForClick){
          
            ApplyAOEAction(ref state, playerTargetPosition.targetMousePosition, skillsConfig.EffectRadius, AOEAction.ChangeColor);
            state.EntityManager.SetComponentData(targetPrefab, new LocalTransform
            {
                Position = playerTargetPosition.targetMousePosition,
                Scale = 1,  // If we didn't set Scale and Rotation, they would default to zero (which is bad!)
                Rotation = quaternion.identity
            });
           return;
        }

        // if (PositionMouseCheck(playerTargetPosition,priorMousePositionAfterButtonPress) )
        // {
        //     return;
        // }
        // priorMousePositionAfterButtonPress.targetMousePosition = playerTargetPosition.targetMousePosition;
        
        
       
        Entity aoePrefab  = state.EntityManager.Instantiate(skillsConfig.PrefabAoe);
        
        state.EntityManager.SetComponentData(aoePrefab, new LocalTransform
        {
            Position = playerTargetPosition.targetMousePosition,
            Scale = 1,  // If we didn't set Scale and Rotation, they would default to zero (which is bad!)
            Rotation = quaternion.identity
        });
        ApplyAOEAction(ref state, playerTargetPosition.targetMousePosition, skillsConfig.EffectRadius, AOEAction.Destroy);
       
        // EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        
        // //todo make it jobs 
        // foreach ( (RefRO<LocalTransform> localTransform,RefRW<EnemyComponent> enemy,Entity e)
        //     in SystemAPI.Query<RefRO<LocalTransform>, RefRW<EnemyComponent>>().WithEntityAccess())
        //     {
        //         float distance = math.distance(localTransform.ValueRO.Position,state.EntityManager.GetComponentData<LocalTransform>(target).Position );
        //         if (distance <= state.EntityManager.GetComponentData<AOESkill>(target).EffectRadius)
        //         {
        //             // var health = enemiesHealths[i];
        //             // enemiesHealths. -= Damage;
        //             // enemiesHealths[i] = health;
        //             // ecb needed
                    
        //             ecb.AddComponent<DestroyTag>(e);
        //         }
        //     }
    
        // ecb.Playback(state.EntityManager);
        // ecb.Dispose();
        //EntityQuery querySkill = SystemAPI.QueryBuilder().WithAll<AOESkill>().Build();
        state.EntityManager.DestroyEntity(aoePrefab);
        state.EntityManager.DestroyEntity(targetPrefab);
        
      
    }
    public void ApplyAOEAction(ref SystemState state, float3 aoePosition, float effectRadius, AOEAction action)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        foreach ((RefRO<LocalTransform> localTransform, RefRW<EnemyComponent> enemy, Entity entity)
                    in SystemAPI.Query<RefRO<LocalTransform>, RefRW<EnemyComponent>>().WithEntityAccess())
        {
            float distance = math.distance(localTransform.ValueRO.Position, aoePosition);
            if (distance <= effectRadius)
            {
                switch (action)
                {
                    case AOEAction.Destroy:
                        ecb.AddComponent<DestroyTag>(entity);
                        break;
                    case AOEAction.ChangeColor:
                        // Assuming you have a component for color, e.g., EnemyColor
                       
                        ecb.SetComponent(entity , new URPMaterialPropertyBaseColor { Value = (Vector4)Color.red });
                        break;
                }
            }else{
                if(action == AOEAction.ChangeColor ){
                    ecb.SetComponent(entity , new URPMaterialPropertyBaseColor { Value = (Vector4)Color.black });
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    public enum AOEAction
    {
        Destroy,
        ChangeColor
    }
    static float4 RandomColor(ref Random random)
    {
        // 0.618034005f is inverse of the golden ratio
        var hue = (random.NextFloat() + 0.618034005f) % 1;
        return (Vector4)Color.HSVToRGB(hue, 1.0f, 1.0f);
    }
}