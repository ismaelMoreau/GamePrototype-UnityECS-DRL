using JetBrains.Annotations;
using Unity.Burst;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
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
               playerTargetPosition.ValueRW.targetClickPosition = ray.GetPoint(dist);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
           
            
            if(!SystemAPI.TryGetSingletonEntity<Target>(out Entity entitytarget))
            {
                 playerTargetPosition.ValueRW.isWaitingForClick = true;
                Entity targetPrefab  = state.EntityManager.Instantiate(playerTargetPosition.ValueRW.targetPrefab);
                //if (new Plane(Vector3.up, 0f).Raycast(ray2, out var dist2)){  
                
                state.EntityManager.SetComponentData(targetPrefab, new Target{ isAoeTarget = true}); 
                state.EntityManager.SetComponentData(targetPrefab, new LocalTransform
                {
                    Position = playerTargetPosition.ValueRW.targetMousePosition,
                    Scale = 1,  // If we didn't set Scale and Rotation, they would default to zero (which is bad!)
                    Rotation = quaternion.identity
                });
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            
            if(!SystemAPI.TryGetSingletonEntity<Target>(out Entity entitytarget))
            {
                playerTargetPosition.ValueRW.isWaitingForClick = true;
                Entity targetPrefab  = state.EntityManager.Instantiate(playerTargetPosition.ValueRW.targetPrefab);
                state.EntityManager.SetComponentData(targetPrefab, new Target{ isJumpTarget = true});
                //if (new Plane(Vector3.up, 0f).Raycast(ray2, out var dist2)){  
                state.EntityManager.SetComponentData(targetPrefab, new LocalTransform
                {
                    Position = playerTargetPosition.ValueRW.targetMousePosition,
                    Scale = 1,  // If we didn't set Scale and Rotation, they would default to zero (which is bad!)
                    Rotation = quaternion.identity
                });
            }
            // // Assuming the player entity has a PhysicsVelocity component and you're using Unity Physics
            // Entities.ForEach((ref PhysicsVelocity velocity, in LocalTransform localTransform) => {
            //     // Calculate direction towards the mouse point (you might want to adjust the Y component)
            //     float3 direction = math.normalize(new float3(mousePoint.x, localTransform.Position.y, mousePoint.z) - localTransform.Position);
            //     float leapStrength = 10f; // Customize this value as needed
            //     velocity.Linear += direction * leapStrength; // Apply the leap
            // }).Schedule();
        }
        

           
    }
    
}
