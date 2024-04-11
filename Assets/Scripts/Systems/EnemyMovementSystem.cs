using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

[UpdateBefore(typeof(QlearningSystem))]
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
[WithNone(typeof(EnemyGridPositionComponent))]
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
           ECB.SetComponentEnabled<EnemyGridPositionComponent>(index, entity, true); 
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

    public float height;
    public float width;

    public float cellSize;
    

    public void Execute(Entity entity,[ChunkIndexInQuery]int index, ref LocalTransform localTransform, in EnemyMovementComponent enemy
        ,EnabledRefRW<EnemyGridPositionComponent> enemyGridEnableRef,ref EnemyGridPositionComponent enemyGridPositionComponent)
    {
        if(!IsPositionInSquare(localTransform.Position,PlayerPosition)){
           enemyGridPositionComponent.isDoingAction = false; 
           enemyGridEnableRef.ValueRW = false;
        }
        else
        {
            var actualPosition = CalculateFlattenedGridPosition(PlayerPosition,localTransform.Position,cellSize,width,height);
            
            if (actualPosition != enemyGridPositionComponent.gridFlatenPosition){
                enemyGridPositionComponent.isDoingAction = false; 
            }
            
            enemyGridPositionComponent.gridFlatenPosition = actualPosition;

            if(enemyGridPositionComponent.isDoingAction){
                float3 direction = new float3(0,0,0);
                switch (enemyGridPositionComponent.chosenAction)
                {
                    case 0: 
                        direction.z-=1;
                        break;
                    case 1:
                        direction.z+=1;
                        break;
                    case 2:
                        direction.x+=1;
                        break;
                    case 3:
                        direction.x-=1;
                        break;
                }
                localTransform.Position += direction * enemy.speed * DeltaTime;
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
    float CalculateFlattenedGridPosition(float3 playerPosition, float3 enemyPosition, float cellSize, float width, float height)
    {
        // Calculate the relative position of the enemy to the player
        float3 relativePosition = enemyPosition - playerPosition;

        // Convert the world space relative position to grid coordinates
        // Offset by half the grid dimensions to center the grid around the player
        float gridX = math.floor((relativePosition.x / cellSize) + (width / 2.0f));
        float gridZ = math.floor((relativePosition.z / cellSize) + (height / 2.0f));

        // // Clamp gridX and gridZ to the grid dimensions to handle positions outside the grid
        // gridX = math.clamp(gridX, 0, width - 1);
        // gridZ = math.clamp(gridZ, 0, height - 1);

        // Calculate the flattened grid position using the width for row-major order indexing
        float flattenedIndex = gridZ * width + gridX;

        return flattenedIndex;
    }
} 



