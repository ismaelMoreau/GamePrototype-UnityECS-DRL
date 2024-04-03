using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;
using Unity.Transforms;

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct DebugToolSystem : ISystem
    {
  
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Initial setup
            
            state.RequireForUpdate<PlayerMovementComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float3 centerPosition = new float3(0, 0, 0); // Default center position
            foreach ((var transforms, var playerMovementComponent) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<PlayerMovementComponent>>())
            {
                centerPosition = transforms.ValueRO.Position;
            }


            // Grid size, spacing, and center position setup
            int gridSize = 20;
            float cellSize = 1.0f; // Assuming each cell in the grid is 1x1 units
            float3 bottomLeftPosition = centerPosition - new float3(gridSize * cellSize / 2, 0, gridSize * cellSize / 2);
            int y = 2; // To ensure visibility above ground

            // Line visibility duration
            float lineDuration = SystemAPI.Time.DeltaTime; // Adjust as needed

            // Drawing the grid, ensuring to include the closing lines
            for (int x = 0; x <= gridSize; x++)
            {
                for (int z = 0; z <= gridSize; z++)
                {
                    // Vertical lines (including the closing line at the last x position)
                    if (x <= gridSize) // Adjust condition to <= for closing line
                    {
                        Debug.DrawLine(
                            new float3(x * cellSize, y, z * cellSize) + bottomLeftPosition,
                            new float3(x * cellSize, y, (z < gridSize ? z + 1 : z) * cellSize) + bottomLeftPosition,
                            Color.white, lineDuration);
                    }
                    
                    // Horizontal lines (including the closing line at the last z position)
                    if (z <= gridSize) // Adjust condition to <= for closing line
                    {
                        Debug.DrawLine(
                            new float3(x * cellSize, y, z * cellSize) + bottomLeftPosition,
                            new float3((x < gridSize ? x + 1 : x) * cellSize, y, z * cellSize) + bottomLeftPosition,
                            Color.white, lineDuration);
                    }
                }
            }
            
            float3 gridOrigin = centerPosition - new float3(gridSize / 2 * cellSize, 0, gridSize / 2 * cellSize); // Adjust grid origin based on player position

            foreach ((RefRO<LocalToWorld> localTransform, RefRW<EnemyComponent> enemy, Entity entity)
                        in SystemAPI.Query<RefRO<LocalToWorld>, RefRW<EnemyComponent>>().WithEntityAccess())
            {
                float3 enemyPosition = localTransform.ValueRO.Position - gridOrigin; // Relative position to the moving grid origin
                
                // Calculate the grid cell index
                float2 gridPos = new float2(math.floor(enemyPosition.x / cellSize), math.floor(enemyPosition.z / cellSize));
                
                // Ensure the enemy is within the grid bounds
                if(gridPos.x >= 0 && gridPos.x < gridSize && gridPos.y >= 0 && gridPos.y < gridSize)
                {
                    // Calculate the world position of the grid cell's bottom-left corner
                    float3 cellBottomLeft = gridOrigin + new float3(gridPos.x * cellSize, 0, gridPos.y * cellSize);
                    
                    // Calculate other corners based on the bottom-left
                    float3 cellTopLeft = cellBottomLeft + new float3(0, 0, cellSize);
                    float3 cellTopRight = cellBottomLeft + new float3(cellSize, 0, cellSize);
                    float3 cellBottomRight = cellBottomLeft + new float3(cellSize, 0, 0);
                    
                    // Draw the highlighted square
                    Color highlightColor = Color.red; // Color for highlighting
                    Debug.DrawLine(cellBottomLeft, cellTopLeft, highlightColor, lineDuration);
                    Debug.DrawLine(cellTopLeft, cellTopRight, highlightColor, lineDuration);
                    Debug.DrawLine(cellTopRight, cellBottomRight, highlightColor, lineDuration);
                    Debug.DrawLine(cellBottomRight, cellBottomLeft, highlightColor, lineDuration);
                }
            }
        }
    }
    

