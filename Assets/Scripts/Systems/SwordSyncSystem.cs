using Unity.Burst;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;


    //[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(AnimatorSystem))]
    public partial struct SwordSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerMovementComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            bool playerisAttacking = false;
            foreach (var player 
                in SystemAPI.Query<RefRO<PlayerMovementComponent>>())
            {
                playerisAttacking = player.ValueRO.IsAttacking; 
            }
            Transform swordTransform = null;
            Transform shieldTransform = null;
            // Sync the Animator's transform with the LocalToWorld
            foreach ( var animator in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<Animator>>())
            {
                foreach (Transform t in  animator.Value.gameObject.GetComponentsInChildren<Transform>())
                {
                    if (t.CompareTag("PlayerWeapon"))
                    {
                        swordTransform = t;
                        
                    }
                    else if (t.CompareTag("PlayerShield"))
                    {
                        shieldTransform = t;
                        
                    }

                }   
            }
            if (swordTransform == null)
            {
                Debug.LogError("Sword not found in the model.");
                return;
            }else if (shieldTransform == null)
            {
                Debug.LogError("Shield not found in the model.");
                return;
            }
            else{
                foreach (var (lt, playerWeaponTag,entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerWeaponTag>>().WithEntityAccess())
                {
                    if(playerWeaponTag.ValueRO.playerSword){
                        lt.ValueRW.Position = swordTransform.position;
                        Quaternion rotationAdjustment = Quaternion.Euler(0, 0, 180);
                        lt.ValueRW.Rotation = swordTransform.rotation * rotationAdjustment;
                        //lt.ValueRW.Rotation = swordTransform.rotation;
                        // if (!playerisAttacking){
                        //     var buffer = SystemAPI.GetBuffer<HitBufferElement>(entity);
                        //     buffer.Clear();
                        // }
                        // Debug.Log("Sword Rotation: " + swordTransform.rotation.eulerAngles);
                        // Debug.Log("Adjusted Rotation: " + (swordTransform.rotation * rotationAdjustment).eulerAngles);
                    }
                    if(playerWeaponTag.ValueRO.playerShield){
                        lt.ValueRW.Position = shieldTransform.position;
                        Quaternion rotationAdjustment = Quaternion.Euler(180, 0, 180);
                        lt.ValueRW.Rotation = shieldTransform.rotation * rotationAdjustment;
                    }
                    
                }
            }
            
        }
    }
