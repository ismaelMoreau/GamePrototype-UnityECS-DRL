using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

// [UpdateBefore(typeof(QlearningActionSelectionSystem))]
[UpdateAfter(typeof(QlearningActionSelectionSystem))]
public partial struct EnemyMovementSystem : ISystem
{
   

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ConfigQlearnGrid>();
        state.RequireForUpdate<EnemyMovementComponent>();
    }

    [BurstCompile] 
   
    public void OnUpdate(ref SystemState state)
    {
        
        var config = SystemAPI.GetSingleton<ConfigQlearnGrid>();
        float3 playerPosition = new float3(0, 0, 0);
        foreach ((RefRW<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed) 
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>>())
        {
            playerPosition = localTransform.ValueRO.Position; 
        }

        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.Dependency.Complete();

        new EnemyMoveTowardsPlayerJob
        {
            PlayerPosition = playerPosition,
            DeltaTime = SystemAPI.Time.DeltaTime,
            //CollisionRadius = 1.0f, // Define your collision radius
            ECB = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            width = config.width,
            height = config.height,
            cellSize = config.cellSize
        }.ScheduleParallel();
        
        new EnemyDisableInGridJob
        {
            PlayerPosition = playerPosition,
            DeltaTime = SystemAPI.Time.DeltaTime,
            width = config.width,
            height = config.height,
            cellSize = config.cellSize
        }.ScheduleParallel();

        // state.Dependency = job2.ScheduleParallel(state.Dependency);
        state.Dependency.Complete();

     
    }
    
}
// Job to move enemies towards the player
[WithNone(typeof(EnemyActionComponent))]
[BurstCompile] 
public partial struct EnemyMoveTowardsPlayerJob : IJobEntity
{
    public float3 PlayerPosition;
    public float DeltaTime;
    //public float CollisionRadius;

    public EntityCommandBuffer.ParallelWriter ECB;
    public float height;
    public float width;

    public float cellSize;

    public void Execute(Entity entity,[ChunkIndexInQuery]int index, ref LocalTransform localTransform, in EnemyMovementComponent enemy)
    {
        if(!IsPositionInSquare(localTransform.Position,PlayerPosition)){
            float3 direction = math.normalize(PlayerPosition - localTransform.Position);
            float distance = math.distance(PlayerPosition, localTransform.Position);
            localTransform.Position += direction * enemy.speed * DeltaTime;
        }
        else
        {
           ECB.SetComponentEnabled<EnemyActionComponent>(index, entity, true); 
        }
       
    }
    bool IsPositionInSquare(float3 position, float3 center)
    {
        // Define the half size of the square, 
        float halfx = width*cellSize/2;
        float halfz = height*cellSize/2;
        float diffX = math.abs(position.x - center.x);
        float diffZ = math.abs(position.z - center.z);
        return diffX <= halfx && diffZ <= halfz;
    }
} 
[BurstCompile] 
public partial struct EnemyDisableInGridJob : IJobEntity
{
    public float3 PlayerPosition;
    public float DeltaTime;
    //public float CollisionRadius;

    public int height;
    public int width;

    public float cellSize;
    

    public void Execute(Entity entity,[ChunkIndexInQuery]int index, ref LocalTransform localTransform, in EnemyMovementComponent enemy
        ,EnabledRefRW<EnemyActionComponent> enemyGridEnableRef,ref EnemyActionComponent EnemyActionComponent, ref EnemyActionTimerComponent enemyActionTimerComponent)
    {
        if(!IsPositionInSquare(localTransform.Position,PlayerPosition)){
           EnemyActionComponent.isDoingAction = false; 
           enemyGridEnableRef.ValueRW = false;
        }
        else
        {
            var actualPosition = CalculateFlattenedGridPosition(PlayerPosition,localTransform.Position,cellSize,width,height);
            
            // Calculate the direction to the player and ensure the enemy faces the player
            float3 playerDirection = math.normalize(PlayerPosition - localTransform.Position);
            localTransform.Rotation = quaternion.LookRotationSafe(playerDirection, math.up());

            EnemyActionComponent.gridFlatenPosition = actualPosition;

            if (EnemyActionComponent.isDoingAction)
            {
                enemyActionTimerComponent.actionTimer += DeltaTime;

                if (enemyActionTimerComponent.actionTimer >= enemyActionTimerComponent.actionDuration)
                {
                    // Action completed
                    EnemyActionComponent.isDoingAction = false;
                    // if player moved
                    if (actualPosition == EnemyActionComponent.nextActionGridFlatenPosition)
                        { EnemyActionComponent.IsReadyToUpdateQtable = true; }
                    
                    enemyActionTimerComponent.actionTimer = 0f;
                }
                else
                {
                    float3 moveDirection = new float3(0, 0, 0);
                    float speedMultiplier = 1.0f; // Default multiplier

                    switch (EnemyActionComponent.chosenAction)
                    {
                        case 0: // Move toward the player
                            moveDirection = playerDirection;
                            break;
                        case 1: // Move backward
                            moveDirection = -playerDirection;
                            break;
                        case 2: // Step to the right
                            moveDirection = math.cross(playerDirection, math.up());
                            break;
                        case 3: // Step to the left
                            moveDirection = math.cross(math.up(), playerDirection);
                            break;
                        case 4: // Dash forward
                            moveDirection = playerDirection;
                            speedMultiplier = 2.0f; // Adjust this multiplier for desired dash speed
                            break;
                    }

                    // Normalize the move direction and calculate the new position
                    if (math.lengthsq(moveDirection) > 0.01f)
                    {
                        moveDirection = math.normalize(moveDirection);
                        localTransform.Position += moveDirection * enemy.speed * speedMultiplier * DeltaTime;
                    }
                }
            }
        }
    }
    bool IsPositionInSquare(float3 position, float3 center)
    {
       // Define the half size of the square, 
        float halfx = width*cellSize/2;
        float halfz = height*cellSize/2;
        float diffX = math.abs(position.x - center.x);
        float diffZ = math.abs(position.z - center.z);
        return diffX <= halfx && diffZ <= halfz;
    }
    int CalculateFlattenedGridPosition(float3 playerPosition, float3 enemyPosition, float cellSize, int width, int height)
    {
        // Calculate the relative position of the enemy to the player
        float3 relativePosition = enemyPosition - playerPosition;

        // Convert the world space relative position to grid coordinates
        // Offset by half the grid dimensions to center the grid around the player
        int gridX = (int)(math.floor(relativePosition.x / cellSize) + width / 2);
        int gridZ = (int)(math.floor(relativePosition.z / cellSize) + height / 2);

        // // Clamp gridX and gridZ to the grid dimensions to handle positions outside the grid
        // gridX = math.clamp(gridX, 0, width - 1);
        // gridZ = math.clamp(gridZ, 0, height - 1);

        // Calculate the flattened grid position using the width for row-major order indexing
        int flattenedIndex = gridZ * width + gridX;

        return flattenedIndex;
    }


    // float3 ChosenEnemyDirection(int chosenAction,float speedMultiplier)
    // {
    //     float3 direction = new float3(0, 0, 0);

    //     switch (chosenAction)
    //     {
    //         case 0: // Up
    //             direction.z -= 1;
    //             break;
    //         case 1: // Down
    //             direction.z += 1;
    //             break;
    //         case 2: // Right
    //             direction.x += 1;
    //             break;
    //         case 3: // Left
    //             direction.x -= 1;
    //             break;
    //         case 4: // UpRight
    //             direction.z -= 1;
    //             direction.x += 1;
    //             break;
    //         case 5: // UpLeft
    //             direction.z -= 1;
    //             direction.x -= 1;
    //             break;
    //         case 6: // DownRight
    //             direction.z += 1;
    //             direction.x += 1;
    //             break;
    //         case 7: // DownLeft
    //             direction.z += 1;
    //             direction.x -= 1;
    //             break;
    //         case 8: // Stay
    //             // No change in direction, enemy stays in place
    //             break;
    //     }

    //     return direction;
    // }



   

} 



