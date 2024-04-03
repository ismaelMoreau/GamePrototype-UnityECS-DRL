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

public partial struct AOESkillSystem : ISystem
{
   
    //PlayerTargetPosition priorMousePositionAfterButtonPress;
    
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
        Target target = state.EntityManager.GetComponentData<Target>(targetPrefab);
        if (!target.isAoeTarget){
            return;
        }      
          
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
        
        
       
        // Entity aoePrefab  = state.EntityManager.Instantiate(skillsConfig.PrefabAoe);
        
        // state.EntityManager.SetComponentData(aoePrefab, new LocalTransform
        // {
        //     Position = playerTargetPosition.targetMousePosition,
        //     Scale = 1,  // If we didn't set Scale and Rotation, they would default to zero (which is bad!)
        //     Rotation = quaternion.identity
        // });
        ApplyAOEAction(ref state, playerTargetPosition.targetMousePosition, skillsConfig.EffectRadius, AOEAction.Destroy);
        
        
        //EntityQuery query = SystemAPI.QueryBuilder().WithAll<Target>().Build();
        //EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        state.EntityManager.DestroyEntity(targetPrefab);
        //ecb.Playback(state.EntityManager);
        //state.EntityManager.DestroyEntity(query);
        // You are responsible for disposing of any ECB you create.
        //ecb.Dispose();
        
        //state.EntityManager.DestroyEntity(aoePrefab);
        //state.EntityManager.DestroyEntity(targetPrefab);
        
      
    }
    
    [BurstCompile]
    public void ApplyAOEAction(ref SystemState state, float3 aoePosition, float effectRadius, AOEAction action)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        foreach ((RefRO<LocalTransform> localTransform, RefRW<EnemyComponent> enemy ,RefRW<URPMaterialPropertyBaseColor> color, Entity entity)
                    in SystemAPI.Query<RefRO<LocalTransform>, RefRW<EnemyComponent>,RefRW<URPMaterialPropertyBaseColor>>().WithEntityAccess())
        {
            float distance = math.distance(localTransform.ValueRO.Position, aoePosition);
          
            if (distance <= effectRadius)
            {
                switch (action)
                {
                    case AOEAction.Destroy:

                        state.EntityManager.SetComponentEnabled<DestroyTag>(entity,true);
                        
                        break;
                    case AOEAction.ChangeColor:
                        // Assuming you have a component for color, e.g., EnemyColor
                       
                        color.ValueRW.Value = (Vector4)Color.red;
                        break;
                }
            }else{
                if(action == AOEAction.ChangeColor ){
                    color.ValueRW.Value = (Vector4)Color.black;
                }
            }
        }

    }
    public enum AOEAction
    {
        Destroy,
        ChangeColor
    }
   
}