
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Physics.Extensions;
using UnityEngine;
using UnityEngine.AI;


[UpdateInGroup(typeof(TransformSystemGroup))]
public partial struct JumpsSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        float3 playerPosition = new float3(0, 0, 0);
        quaternion playerRotation = quaternion.identity;

        foreach ((RefRO<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed)
            in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PlayerMovementComponent>>())
        {
            playerPosition = localTransform.ValueRO.Position;
            playerRotation = localTransform.ValueRO.Rotation;
        }
        foreach (var (localTransform,physicsVelocity, enemyMovementComponent,physicsMass,enemyActionsCooldownComponent) in 
            SystemAPI.Query<RefRW<LocalTransform>, RefRW<PhysicsVelocity>, RefRW<EnemyMovementComponent>,RefRO<PhysicsMass>,RefRW<EnemyActionsCooldownComponent>>())
        {
            if (enemyMovementComponent.ValueRO.isJumping && !enemyMovementComponent.ValueRO.isCooldownJumpActive)
            {
                // Start the jump
                enemyMovementComponent.ValueRW.isCooldownJumpActive = true;
                enemyMovementComponent.ValueRW.isJumping = false;
                enemyMovementComponent.ValueRW.jumpTimeElapsed = 0f;

                // Calculate jump parameters
                float jumpHeight = 3f; // Adjust this value to change jump height
                float jumpDuration = 1f; // Adjust this value to change jump duration

                // Calculate initial vertical velocity for desired jump height
                float initialVerticalVelocity = 2f * jumpHeight / (jumpDuration / 2f);

                // Set initial jump velocity
                float3 jumpDirection = enemyMovementComponent.ValueRO.JumpImpulse;
                float3 initialVelocity = jumpDirection * initialVerticalVelocity;

                physicsVelocity.ValueRW.Linear = initialVelocity;

                //Debug.Log($"Jump started. Initial Velocity: {initialVelocity}");
            }
            else if (enemyMovementComponent.ValueRO.isCooldownJumpActive)
            {
                // Continue the jump
                enemyMovementComponent.ValueRW.jumpTimeElapsed += deltaTime;
                float jumpProgress = enemyMovementComponent.ValueRO.jumpTimeElapsed / 1f;
                if (enemyMovementComponent.ValueRO.jumpTimeElapsed <= 1.2f){
                // {
                //     // End the jump
                //     enemyMovementComponent.ValueRW.isCooldownJumpActive = false;
                //    // Debug.Log("Jump ended");
                // }
                // else
                // {
                    // Apply gravity during the jump
                    // Apply declining forward momentum
                     // Adjust this value for initial forward speed
                    float declineFactor = 1f - jumpProgress; // Linear decline, you can adjust this curve
                    float currentForwardSpeed = enemyMovementComponent.ValueRO.speed * declineFactor;

                    float3 forwardMomentum = enemyMovementComponent.ValueRO.JumpImpulse * currentForwardSpeed * deltaTime;
                    physicsVelocity.ValueRW.Linear.xz += forwardMomentum.xz;

                    //.Log($"Jump in progress. Velocity: {physicsVelocity.ValueRO.Linear}, Position: {localTransform.ValueRO.Position}");
                }else{
                    physicsVelocity.ValueRW.Linear = 0f;
                    physicsVelocity.ValueRW.Angular = 0f;
                }
            }
           
        }
    }
}