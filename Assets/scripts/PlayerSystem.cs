using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct PlayerSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }
        float3? clickPosition = null;
        if (Input.GetMouseButtonDown(0)) // 0 is for the left button
        {
            // Convert the mouse position to world space 2D coordinates
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            // Cast a ray from the mouse position straight down in the Z-axis, since it's 2D
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            
            if (hit.collider != null)
            {
                // Store the new target position
                clickPosition = new float3(hit.point.x, hit.point.y, 0);
            }
        }
        foreach ((RefRW<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed, RefRW<PlayerTargetPosition> targetPosition) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>, RefRW<PlayerTargetPosition>>())
        {
            if (clickPosition.HasValue)
            {
                targetPosition.ValueRW.targetClickPosition = clickPosition.Value;
            }
            float3 currentPosition = localTransform.ValueRW.Position; // Assuming LocalTransform has a Value with a Position
            float3 direction = math.normalize(targetPosition.ValueRO.targetClickPosition - currentPosition);

            // Update the position only if the target is not reached
            if (!math.all(math.abs(currentPosition - targetPosition.ValueRO.targetClickPosition) < new float3(0.05f, 0.05f, 0.05f)))
            {
                localTransform.ValueRW.Position += direction * playerSpeed.ValueRO.speed * SystemAPI.Time.DeltaTime;
            }
        }
    }
}

