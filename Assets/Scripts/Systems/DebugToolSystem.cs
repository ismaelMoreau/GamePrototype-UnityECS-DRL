using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine;
using Unity.Transforms;
using System.Resources;
using Unity.Collections;
using Unity.VisualScripting;

[UpdateAfter(typeof(EnemyMovementSystem))]
[CreateAfter(typeof(EnemyMovementSystem))]
    public partial struct DebugToolSystem : ISystem
    {
        private bool initialized;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
            state.RequireForUpdate<PlayerMovementComponent>();
            state.RequireForUpdate<QtableComponent>();
            state.RequireForUpdate<ConfigQlearnGrid>();
            initialized = false;
        }

      
        public void OnUpdate(ref SystemState state)
        {   
            var config = SystemAPI.GetSingleton<ConfigQlearnGrid>(); 
            var configEntity = SystemAPI.GetSingletonEntity<ConfigQlearnGrid>();
            var configManaged = state.EntityManager.GetComponentObject<ConfigManaged>(configEntity);
            if (Input.GetKey(KeyCode.A)) {
            
               
                if (!initialized)
                {
                    initialized = true;
               
                
                    configManaged.UIController = GameObject.FindObjectOfType<UIController>();
                    configManaged.UIController.GenerateTable();
                    int index = 0;
                    foreach(var Qtable in SystemAPI.Query<RefRO<QtableComponent>>()){
                        configManaged.UIController.SetCellContentFlat(index,index.ToString()+": "+ Qtable.ValueRO.up.ToString("F2")+"/"+
                        Qtable.ValueRO.down.ToString("F2")+"/"+
                        Qtable.ValueRO.right.ToString("F2")+"/"+
                        Qtable.ValueRO.left.ToString("F2"));
                        index++;
                    }
                }
            }
            else if (Input.GetKey(KeyCode.Q)){
                
                if (!initialized)
                {
                    initialized = true;
               
                
                    configManaged.UIController = GameObject.FindObjectOfType<UIController>();
                    configManaged.UIController.GenerateTable();
                    int index = 0;
                    foreach(var Qtable in SystemAPI.Query<RefRO<QtableRewardComponent>>()){
                        configManaged.UIController.SetCellContentFlat(index, Qtable.ValueRO.reward.ToString());
                        index++;
                    }
                }
                
            }
            else if (Input.GetKey(KeyCode.D)){

                float3 centerPosition = new float3(0, 0, 0); // Default center position
                foreach ((var transforms, var playerMovementComponent) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<PlayerMovementComponent>>())
                {
                    centerPosition = transforms.ValueRO.Position;
                }

                // Use config values for grid size, spacing, and center position setup
                int gridSizeX = config.width;
                int gridSizeZ = config.height;
                float cellSize = config.cellSize; // Assuming each cell in the grid is defined by cellSize units
                float3 bottomLeftPosition = centerPosition - new float3(gridSizeX * cellSize / 2, 0, gridSizeZ * cellSize / 2);
                int y = 1; // To ensure visibility above ground

                // Line visibility duration
                float lineDuration = SystemAPI.Time.DeltaTime; // Adjust as needed

                // Drawing the grid, ensuring to include the closing lines
                for (int x = 0; x <= gridSizeX; x++)
                {
                    for (int z = 0; z <= gridSizeZ; z++)
                    {
                        // Vertical lines
                        Debug.DrawLine(
                            new float3(x * cellSize, y, 0) + bottomLeftPosition,
                            new float3(x * cellSize, y, gridSizeZ * cellSize) + bottomLeftPosition,
                            Color.white, lineDuration);

                        // Horizontal lines
                        if (z < gridSizeZ) // To avoid drawing an extra line at the end
                        {
                            Debug.DrawLine(
                                new float3(0, y, z * cellSize) + bottomLeftPosition,
                                new float3(gridSizeX * cellSize, y, z * cellSize) + bottomLeftPosition,
                                Color.white, lineDuration);
                        }
                    }
                }

                float3 gridOrigin = centerPosition - new float3(gridSizeX / 2 * cellSize, 0, gridSizeZ / 2 * cellSize); // Adjust grid origin based on player position
            
                foreach ((RefRO<LocalToWorld> localTransform, RefRW<EnemyMovementComponent> enemy, Entity entity)
                            in SystemAPI.Query<RefRO<LocalToWorld>, RefRW<EnemyMovementComponent>>().WithEntityAccess())
                {
                    float3 enemyPosition = localTransform.ValueRO.Position - gridOrigin; // Relative position to the moving grid origin
                    
                    // Calculate the grid cell index
                    float2 gridPos = new float2(math.floor(enemyPosition.x / cellSize), math.floor(enemyPosition.z / cellSize));
                    
                    // Ensure the enemy is within the grid bounds
                    if(gridPos.x >= 0 && gridPos.x < gridSizeX && gridPos.y >= 0 && gridPos.y < gridSizeZ)
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
            else{
                if(initialized){
                    configManaged.UIController.ClearTable();
                    initialized = false;
                }
            }
        }
    }
    

