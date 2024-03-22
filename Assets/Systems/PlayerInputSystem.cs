using JetBrains.Annotations;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct PlayerInputSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
        state.RequireForUpdate<SkillsConfig>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var skillsConfig = SystemAPI.GetSingleton<SkillsConfig>();
        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }

       
        var playerTargetPosition = SystemAPI.GetSingletonRW<PlayerTargetPosition>();
   
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (new Plane(Vector3.up, 0f).Raycast(ray, out var dist))
        {
            if (Input.GetMouseButtonDown(1)) // 0 is for the left button
            {
                // Store the new target position in 3D
                playerTargetPosition.ValueRW.targetClickPosition = ray.GetPoint(dist);
                
            }
            playerTargetPosition.ValueRW.targetMousePosition = ray.GetPoint(dist);
           
        }
        
        if(playerTargetPosition.ValueRO.isWaitingForClick){
            if (Input.GetMouseButtonDown(0)) {
               playerTargetPosition.ValueRW.isWaitingForClick = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playerTargetPosition.ValueRW.isWaitingForClick = true;
            
            if(!SystemAPI.TryGetSingletonEntity<Target>(out Entity entitytarget))
            {
                Entity targetPrefab  = state.EntityManager.Instantiate(playerTargetPosition.ValueRW.targetPrefab);
                //if (new Plane(Vector3.up, 0f).Raycast(ray2, out var dist2)){  
                state.EntityManager.SetComponentData(targetPrefab, new LocalTransform
                {
                    Position = playerTargetPosition.ValueRW.targetMousePosition,
                    Scale = 1,  // If we didn't set Scale and Rotation, they would default to zero (which is bad!)
                    Rotation = quaternion.identity
                });
            }
        }
           
    }
    
}
