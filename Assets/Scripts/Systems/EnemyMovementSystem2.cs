using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;
using Unity.Physics.Extensions;

[UpdateInGroup(typeof(TransformSystemGroup))]
public partial struct EnemyMovementSystem2 : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyMovementComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        float3 playerPosition = new float3(0, 0, 0);
        quaternion playerRotation = quaternion.identity;

        foreach ((RefRW<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed)
            in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>>())
        {
            playerPosition = localTransform.ValueRO.Position;
            playerRotation = localTransform.ValueRO.Rotation;
        }

        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.Dependency.Complete();

        new EnemyMovementJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            PlayerPosition = playerPosition,
            PlayerRotation = playerRotation,
        }.ScheduleParallel();

        state.Dependency.Complete();
    }


}

[BurstCompile]
[WithNone(typeof(HitBackwardEffectComponent))]
public partial struct EnemyMovementJob : IJobEntity
{

    public float DeltaTime;
    public float3 PlayerPosition;
    public quaternion PlayerRotation;

    //public PhysicsWorld physicsWorld;

    public void Execute(ref LocalTransform localTransform,
    in EnemyMovementComponent enemy,
    ref EnemyActionComponent EnemyActionComponent,
    ref EnemyActionTimerComponent enemyActionTimerComponent,
    ref PhysicsVelocity physicsVelocity,
    ref EnemyActionsCooldownComponent enemyActionsCooldownComponent,
    ref Grounded grounded,
    ref PhysicsMass physicsMass
    )
    {
       

        float3 playerDirection = math.normalize(PlayerPosition - localTransform.Position);
        float3 flatDirection = new float3(playerDirection.x, 0, playerDirection.z);
        localTransform.Rotation = quaternion.LookRotationSafe(flatDirection, math.up());

        // Update cooldown timers
        enemyActionsCooldownComponent.cooldownHealTimer = math.max(enemyActionsCooldownComponent.cooldownHealTimer - DeltaTime, 0f);
        enemyActionsCooldownComponent.cooldownBlockTimer = math.max(enemyActionsCooldownComponent.cooldownBlockTimer - DeltaTime, 0f);
        enemyActionsCooldownComponent.cooldownDashTimer = math.max(enemyActionsCooldownComponent.cooldownDashTimer - DeltaTime, 0f);
        enemyActionsCooldownComponent.cooldownJumpTimer = math.max(enemyActionsCooldownComponent.cooldownJumpTimer - DeltaTime, 0f);

        if (EnemyActionComponent.isDoingAction && grounded.IsGrounded)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            enemyActionTimerComponent.actionTimer += DeltaTime;

            if (enemyActionTimerComponent.actionTimer >= enemyActionTimerComponent.actionDuration)
            {
                EnemyActionComponent.isDoingAction = false;
                enemyActionTimerComponent.actionTimer = 0f;
            }
            else
            {
                float3 moveDirection = float3.zero;
                float speedMultiplier = 1.0f;

                switch (EnemyActionComponent.chosenAction)
                {
                    case 0: // Move forward
                        moveDirection = playerDirection;
                        speedMultiplier = 1.1f;
                        break;
                    case 1: // Move backward
                        moveDirection = -playerDirection;
                        speedMultiplier = 0.9f;
                        break;
                    case 2: // Move left
                        moveDirection = math.cross(playerDirection, math.up());
                        break;
                    case 3: // Move right
                        moveDirection = math.cross(math.up(), playerDirection);
                        break;
                    case 4: // Dash 
                        if (enemyActionsCooldownComponent.cooldownDashTimer <= 0f)
                        {
                            moveDirection = playerDirection;
                            speedMultiplier = 2f;
                            enemyActionsCooldownComponent.cooldownDashTimer = enemyActionsCooldownComponent.cooldownDashDuration;
                        }
                        break;
                    case 5: // Block 
                        if (enemyActionsCooldownComponent.cooldownBlockTimer <= 0f)
                        {
                            // Block logic here
                            enemyActionsCooldownComponent.cooldownBlockTimer = enemyActionsCooldownComponent.cooldownBlockDuration;
                        }
                        break;
                    case 6: // Heal 
                        if (enemyActionsCooldownComponent.cooldownHealTimer <= 0f)
                        {
                            // Heal logic here
                            enemyActionsCooldownComponent.cooldownHealTimer = enemyActionsCooldownComponent.cooldownHealDuration;
                        }
                        break;
                    case 7: // Jump
                        if (enemyActionsCooldownComponent.cooldownJumpTimer <= 0f)
                        {
                            float3 jumpDirection = math.normalize(flatDirection+ new float3(0,2f,0));
                            float3 impulse =  jumpDirection * enemy.jumpForce;
                
                            PhysicsComponentExtensions.ApplyImpulse(
                                ref physicsVelocity, 
                                physicsMass,
                                localTransform.Position,
                                localTransform.Rotation,
                                impulse,
                                localTransform.Position);
                            // Heal logic here
                            enemyActionsCooldownComponent.cooldownJumpTimer = enemyActionsCooldownComponent.cooldownJumpDuration;
                        }  // Jump logic here
                        break;
                    case 8: // Stay
                        break;
                }

                if (math.lengthsq(moveDirection) > 0.01f)
                {
                    moveDirection = math.normalize(moveDirection);
                    localTransform.Position += moveDirection * enemy.speed * speedMultiplier * DeltaTime;
                }
            }
        }
    }

}

