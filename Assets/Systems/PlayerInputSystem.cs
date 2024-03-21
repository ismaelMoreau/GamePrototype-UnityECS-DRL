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

       
        float3? clickPosition = null;
   
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0)) // 0 is for the left button
        {
           
            if (new Plane(Vector3.up, 0f).Raycast(ray, out var dist))
            {
                // Store the new target position in 3D
                clickPosition = ray.GetPoint(dist);
            }
        }
      
        foreach ((RefRW<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed, RefRW<PlayerTargetPosition> targetPosition,Entity e) 
                 in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>, RefRW<PlayerTargetPosition>>().WithEntityAccess())
        {
            if (clickPosition.HasValue)
            {
                targetPosition.ValueRW.targetClickPosition = clickPosition.Value;
            }
            
            float3 currentPosition = localTransform.ValueRW.Position; // Assuming LocalTransform has a Value with a Position
            float3 direction = math.normalize(targetPosition.ValueRO.targetClickPosition - currentPosition);

            // Update the position only if the target is not reached
            // Check for all components including Z to ensure we're working in 3D
            if (!math.all(math.abs(currentPosition - targetPosition.ValueRO.targetClickPosition) < new float3(0.05f, 0.05f, 0.05f)))
            {
                localTransform.ValueRW.Position += direction * playerSpeed.ValueRO.speed * SystemAPI.Time.DeltaTime;
            }
        }
    }
}
