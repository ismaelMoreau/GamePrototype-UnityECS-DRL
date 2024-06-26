using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Collections;
using Unity.Physics.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(TransformSystemGroup))]
public partial struct EnemyMovementSystem2 : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyMovementComponent>();
          state.RequireForUpdate<GamePlayingTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        float3 playerPosition = new float3(0, 0, 0);
        quaternion playerRotation = quaternion.identity;

        foreach ((RefRO<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed)
            in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerMovementComponent>>())
        {
            playerPosition = localTransform.ValueRO.Position;
            playerRotation = localTransform.ValueRO.Rotation;
        }

        new EnemyMovementJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            PlayerPosition = playerPosition,
            PlayerRotation = playerRotation,
        }.ScheduleParallel();

        state.Dependency.Complete();



        // foreach (var (localTransform,
        //     enemyMovementComponent,
        //     enemyActionComponent,
        //     enemyActionTimerComponent,
        //     physicsVelocity,
        //     physicsMass

        //     )
        //     in SystemAPI.Query<RefRW<LocalTransform>,
        //     RefRW<EnemyMovementComponent>,
        //     RefRW<EnemyActionComponent>,
        //     RefRW<EnemyActionTimerComponent>,
        //     RefRW<PhysicsVelocity>,
        //     RefRW<PhysicsMass>>()

        //     )
        // {

        //     float3 playerDirection = math.normalize(playerPosition - localTransform.ValueRW.Position);
        //     float3 flatDirection = new float3(playerDirection.x, 0, playerDirection.z);
        //     localTransform.ValueRW.Rotation = quaternion.LookRotationSafe(flatDirection, math.up());


        //     if (enemyActionComponent.ValueRO.isDoingAction && enemyMovementComponent.ValueRO.isGrounded)
        //     {
        //         if (enemyActionComponent.ValueRO.chosenAction != 7)
        //         {
        //         physicsVelocity.ValueRW.Linear = 0f;
        //         physicsVelocity.ValueRW.Angular = 0f;
        //         }


        //         enemyActionTimerComponent.ValueRW.actionTimer += SystemAPI.Time.DeltaTime;

        //         if (enemyActionTimerComponent.ValueRO.actionTimer >= enemyActionTimerComponent.ValueRO.actionDuration)
        //         {
        //             enemyActionComponent.ValueRW.isDoingAction = false;
        //             enemyActionTimerComponent.ValueRW.actionTimer = 0f;
        //         }
        //         else
        //         {
        //             float3 moveDirection = float3.zero;
        //             float speedMultiplier = 1.0f;

        //             switch (enemyActionComponent.ValueRO.chosenAction)
        //             {
        //                 case 0: // Move forward
        //                     moveDirection = playerDirection;
        //                     speedMultiplier = 1.1f;
        //                     break;
        //                 case 1: // Move backward
        //                     moveDirection = -playerDirection;
        //                     speedMultiplier = 0.9f;
        //                     break;
        //                 case 2: // Move left
        //                     moveDirection = math.cross(playerDirection, math.up());
        //                     break;
        //                 case 3: // Move right
        //                     moveDirection = math.cross(math.up(), playerDirection);
        //                     break;
        //                 case 4: // Dash 
        //                     if (!enemyMovementComponent.ValueRO.isCooldownDashActive)
        //                     {
        //                         enemyMovementComponent.ValueRW.isCooldownDashActive = true;
        //                         moveDirection = playerDirection;
        //                         speedMultiplier = 2f;

        //                     }
        //                     break;
        //                 case 5: // Block 
        //                     if (!enemyMovementComponent.ValueRO.isCooldownBlockActive)
        //                     {
        //                         enemyMovementComponent.ValueRW.isCooldownBlockActive = true;
        //                         // Block logic here

        //                     }
        //                     break;
        //                 case 6: // Heal 
        //                     if (!enemyMovementComponent.ValueRO.isCooldownHealActive)
        //                     {
        //                         enemyMovementComponent.ValueRW.isCooldownHealActive = true;
        //                         // Heal logic here

        //                     }
        //                     break;
        //                 case 7: // Jump

        //                     if (!enemyMovementComponent.ValueRO.isCooldownJumpActive)
        //                     {
        //                         // float3 jumpImpulse = new float3(0, enemyMovementComponent.jumpForce, 0);

        //                         // physicsVelocity.Linear += jumpImpulse;
        //                         enemyMovementComponent.ValueRW.isCooldownJumpActive = true;
        //                         enemyMovementComponent.ValueRW.isJumping = true;

        //                         float3 jumpDirection = math.normalize(new float3(flatDirection.x, 1f, flatDirection.z));
        //                         var impulse = jumpDirection * enemyMovementComponent.ValueRO.jumpForce;
        //                         //enemyMovementComponent.ValueRW.JumpImpulse = jumpDirection * enemyMovementComponent.ValueRO.jumpForce;
                                
        //                         // PhysicsComponentExtensions.ApplyImpulse(
        //                         //     ref physicsVelocity.ValueRW,
        //                         //     physicsMass.ValueRW,
        //                         //     localTransform.ValueRO.Position,
        //                         //     localTransform.ValueRO.Rotation,
        //                         //     impulse,
        //                         //     localTransform.ValueRO.Position-new float3(0.2f,0.2f,0.2f)  // Apply impulse at the center of mass
        //                         // );
        //                         PhysicsComponentExtensions.ApplyLinearImpulse(ref physicsVelocity.ValueRW,
        //                             physicsMass.ValueRW,impulse);
        //                         //UnityEngine.Debug.Log($"Jump applied. Impulse: {enemyMovementComponent.ValueRO.JumpImpulse}, New Velocity: {physicsVelocity.ValueRO.Linear}"); 

        //                         enemyMovementComponent.ValueRW.isGrounded = false;
        //                     }  // Jump logic here
        //                     break;
        //                 case 8: // Stay
        //                     break;
        //             }

        //             if (math.lengthsq(moveDirection) > 0.01f && enemyActionComponent.ValueRO.chosenAction != 7)
        //             {
        //                 //moveDirection = math.normalize(moveDirection);
        //                 physicsVelocity.ValueRW.Linear = moveDirection * enemyMovementComponent.ValueRO.speed * speedMultiplier;
        //             }
        //         }
            // }
        // }

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
        ref EnemyMovementComponent enemyMovementComponent,
        ref EnemyActionComponent EnemyActionComponent,
        ref EnemyActionTimerComponent enemyActionTimerComponent,
        ref PhysicsVelocity physicsVelocity,
        ref PhysicsMass physicsMass
        )
    {

        float3 playerDirection = math.normalize(PlayerPosition - localTransform.Position);
        float3 flatDirection = new float3(playerDirection.x, 0, playerDirection.z);
        localTransform.Rotation = quaternion.LookRotationSafe(flatDirection, math.up());


        if (EnemyActionComponent.isDoingAction && enemyMovementComponent.isGrounded)
        {
            if (EnemyActionComponent.chosenAction != 7)
            {
                physicsVelocity.Linear = 0f;
                physicsVelocity.Angular = 0f;
            }


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
                        if (!enemyMovementComponent.isCooldownDashActive)
                        {
                            enemyMovementComponent.isCooldownDashActive = true;
                            moveDirection = playerDirection;
                            speedMultiplier = 2f;

                        }
                        break;
                    case 5: // Block 
                        if (!enemyMovementComponent.isCooldownBlockActive)
                        {
                            enemyMovementComponent.isCooldownBlockActive = true;
                            // Block logic here

                        }
                        break;
                    case 6: // Heal 
                        if (!enemyMovementComponent.isCooldownHealActive)
                        {
                            enemyMovementComponent.isCooldownHealActive = true;
                            // Heal logic here

                        }
                        break;
                    case 7: // Jump

                        if (!enemyMovementComponent.isCooldownJumpActive)
                        {
                            // float3 jumpImpulse = new float3(0, enemyMovementComponent.jumpForce, 0);

                            // physicsVelocity.Linear += jumpImpulse;
                            //enemyMovementComponent.isCooldownJumpActive = true;

                            float3 jumpDirection = math.normalize(new float3(flatDirection.x, 1f, flatDirection.z));
                            enemyMovementComponent.JumpImpulse = jumpDirection ;
                            enemyMovementComponent.isJumping = true;
                            // PhysicsComponentExtensions.ApplyImpulse(
                            //     ref physicsVelocity,
                            //     physicsMass,
                            //     localTransform.Position,
                            //     localTransform.Rotation,
                            //     impulse,
                            //     localTransform.Position  // Apply impulse at the center of mass
                            // );
                           
                            enemyMovementComponent.isGrounded = false;
                        }  // Jump logic here
                        break;
                    case 8: // Stay
                        if (!enemyMovementComponent.isCooldownStayActive)
                        {
                            enemyMovementComponent.isCooldownStayActive = true;
                        }
                        break;
                }

                if (math.lengthsq(moveDirection) > 0.01f && EnemyActionComponent.chosenAction != 7)
                {
                    //moveDirection = math.normalize(moveDirection);
                    physicsVelocity.Linear = moveDirection * enemyMovementComponent.speed * speedMultiplier;
                }
            }
        }
    }

}

