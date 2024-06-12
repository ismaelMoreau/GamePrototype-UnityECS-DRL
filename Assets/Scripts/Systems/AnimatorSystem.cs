using Unity.Burst;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public interface IAddMonoBehaviourToEntityOnAnimatorInstantiation {}

[UpdateAfter(typeof(TransformSystemGroup))]
partial struct AnimatorSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     var playerEntity = SystemAPI.GetSingletonEntity<AnimatorInstantiationData>();
        //     state.EntityManager.DestroyEntity(playerEntity);
        // }

        // Instantiate the Animator if it doesn't exist
        foreach (var entity in SystemAPI.QueryBuilder()
                     .WithAll<AnimatorInstantiationData>().WithNone<Animator>()
                     .Build().ToEntityArray(state.WorldUpdateAllocator))
        {
            var data = SystemAPI.GetComponent<AnimatorInstantiationData>(entity);
            var spawnedGameObject = Object.Instantiate(data.AnimatorGameObject.Value);

            var spawnedAnimator = spawnedGameObject.GetComponent<Animator>();
            state.EntityManager.AddComponentObject(entity, spawnedAnimator);
            state.EntityManager.AddComponentData(entity, new AnimatorCleanup
            {
                DestroyThisAnimator = spawnedAnimator
            });

            foreach (var mb in spawnedGameObject.GetComponents<IAddMonoBehaviourToEntityOnAnimatorInstantiation>())
            {
                if (mb is MonoBehaviour monoBehaviour)
                    state.EntityManager.AddComponentObject(entity, monoBehaviour);
            }
        }

        // Sync the Animator's transform with the LocalToWorld
        foreach (var (ltw, animator) in SystemAPI.Query<LocalToWorld, SystemAPI.ManagedAPI.UnityEngineComponent<Animator>>())
        {
            animator.Value.transform.SetPositionAndRotation(ltw.Position, ltw.Rotation);
        }

        foreach (var (ltw, animator, enemyActionComponent, enemyActionTimerComponent) in SystemAPI.Query<LocalToWorld, 
            SystemAPI.ManagedAPI.UnityEngineComponent<Animator>,
            EnemyActionComponent,
            EnemyActionTimerComponent>())
        {
            AnimatorStateInfo stateInfo = animator.Value.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f)
            {
                // Reset all action-related Animator bools
                var walkFWD = Animator.StringToHash("WalkFWD");
                var walkBWD = Animator.StringToHash("WalkBWD");
                var walkRight = Animator.StringToHash("WalkRight");
                var walkLeft = Animator.StringToHash("WalkLeft");
                var runFWD = Animator.StringToHash("RunFWD");
                var block = Animator.StringToHash("Block");
                // var heal = Animator.StringToHash("Heal");
                // var jump = Animator.StringToHash("Jump");
                // var stay = Animator.StringToHash("Stay");

                animator.Value.SetBool(walkFWD, false);
                animator.Value.SetBool(walkBWD, false);
                animator.Value.SetBool(walkRight, false);
                animator.Value.SetBool(walkLeft, false);
                animator.Value.SetBool(runFWD, false);
                animator.Value.SetBool(block, false);
                // animator.Value.SetBool(heal, false);
                // animator.Value.SetBool(jump, false);
                // animator.Value.SetBool(stay, false);

                //if (enemyActionTimerComponent.actionTimer <= enemyActionTimerComponent.actionDuration)
                {
                    var id = 0;
                    switch (enemyActionComponent.chosenAction)
                    {
                        case 0: // Move toward the player
                            id = Animator.StringToHash("WalkFWD");
                            break;
                        case 1: // Move backward
                            id = Animator.StringToHash("WalkBWD");
                            break;
                        case 2: // Step to the right
                            id = Animator.StringToHash("WalkRight");
                            break;
                        case 3: // Step to the left
                            id = Animator.StringToHash("WalkLeft");
                            break;
                        case 4: // Dash forward
                            id = Animator.StringToHash("RunFWD");
                            break;
                        case 5: // Block
                            id = Animator.StringToHash("Block");
                            break;
                        case 6: // Heal
                            id = Animator.StringToHash("Heal");
                            break;
                        case 7: // Jump
                            id = Animator.StringToHash("Jump");
                            break;
                        case 8: // Stay
                            id = Animator.StringToHash("Stay");
                            break;
                    }
                    animator.Value.SetBool(id, true);
                }
            }
        }

        foreach (var (animator, hitBackwardEffectComponent, velocity) in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<Animator>, RefRW<HitBackwardEffectComponent>, RefRW<PhysicsVelocity>>())
        {
            AnimatorStateInfo stateInfo = animator.Value.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f)
            {
                // Reset all action-related Animator bools
                var walkFWD = Animator.StringToHash("WalkFWD");
                var walkBWD = Animator.StringToHash("WalkBWD");
                var walkRight = Animator.StringToHash("WalkRight");
                var walkLeft = Animator.StringToHash("WalkLeft");
                var runFWD = Animator.StringToHash("RunFWD");
                var block = Animator.StringToHash("Block");
                // var heal = Animator.StringToHash("Heal");
                // var jump = Animator.StringToHash("Jump");
                // var stay = Animator.StringToHash("Stay");

                animator.Value.SetBool(walkFWD, false);
                animator.Value.SetBool(walkBWD, false);
                animator.Value.SetBool(walkRight, false);
                animator.Value.SetBool(walkLeft, false);
                animator.Value.SetBool(runFWD, false);
                animator.Value.SetBool(block, false);
                // animator.Value.SetBool(heal, false);
                // animator.Value.SetBool(jump, false);
                // animator.Value.SetBool(stay, false);

                var id = Animator.StringToHash("GetHit");
                if (!hitBackwardEffectComponent.ValueRO.animationHasPlayed)
                {
                    velocity.ValueRW.Linear = float3.zero;
                    velocity.ValueRW.Angular = float3.zero;
                    hitBackwardEffectComponent.ValueRW.animationHasPlayed = true;
                    animator.Value.Play(id);
                }
            }
        }

        foreach (var (ltw, animator, playerMouvement) in SystemAPI.Query<LocalToWorld, SystemAPI.ManagedAPI.UnityEngineComponent<Animator>, RefRW<PlayerMovementComponent>>())
        {
            var runFWD = Animator.StringToHash("IsRunningFoward");
            if (playerMouvement.ValueRO.isWalking)
            {
                animator.Value.SetBool(runFWD, true);
            }
            else
            {
                animator.Value.SetBool(runFWD, false);
            }

            if (Input.GetMouseButtonDown(0))
            {
                var attack = Animator.StringToHash("IsAttacking");
                animator.Value.SetTrigger(attack);
                playerMouvement.ValueRW.IsAttacking = true;
            }

            AnimatorStateInfo stateInfo = animator.Value.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1.0f)
            {
                playerMouvement.ValueRW.IsAttacking = false;
            }
        }

        // If the Animator is destroyed, remove the AnimatorCleanup component
        foreach (var entity in SystemAPI.QueryBuilder()
                     .WithAll<AnimatorCleanup>().WithNone<AnimatorInstantiationData>()
                     .Build().ToEntityArray(state.WorldUpdateAllocator))
        {
            var data = SystemAPI.ManagedAPI.GetComponent<AnimatorCleanup>(entity);
            Object.Destroy(data.DestroyThisAnimator.gameObject);
            state.EntityManager.RemoveComponent<AnimatorCleanup>(entity);
        }
    }
}

#if UNITY_EDITOR
struct EditorAnimatorVisualEntityPrefab : IComponentData
{
    public Entity Prefab;
}

[WorldSystemFilter(WorldSystemFilterFlags.Editor)]
partial struct EditorAnimatorSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnCreate(ref SystemState state) => state.RequireForUpdate<EditorAnimatorVisualEntityPrefab>();

    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        var query = SystemAPI.QueryBuilder().WithAll<EditorAnimatorVisualEntityPrefab>().Build();
        foreach (var originalEntity in query.ToEntityArray(state.WorldUpdateAllocator))
        {
            var data = SystemAPI.GetComponent<EditorAnimatorVisualEntityPrefab>(originalEntity);
            var originalLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(originalEntity);
            var spawnedPrefab = state.EntityManager.Instantiate(data.Prefab);

            // Make original entity parent of spawned prefab - so it moves with the original entity
            if (SystemAPI.HasComponent<Parent>(spawnedPrefab))
                SystemAPI.SetComponent(spawnedPrefab, new Parent { Value = originalEntity });
            else
                state.EntityManager.AddComponentData(spawnedPrefab, new Parent { Value = originalEntity });

            // Ensure parent has a LocalTransform with correct values from the start
            if (SystemAPI.HasComponent<LocalTransform>(originalEntity))
                SystemAPI.SetComponent(originalEntity, LocalTransform.FromMatrix(originalLocalToWorld.Value));
            else
                state.EntityManager.AddComponentData(originalEntity, LocalTransform.FromMatrix(originalLocalToWorld.Value));
        }

        // Remove the EditorAnimatorVisualEntityPrefab component
        state.EntityManager.RemoveComponent<EditorAnimatorVisualEntityPrefab>(query);
    }

    public void OnStopRunning(ref SystemState state) {}
}
#endif
