using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


    //[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
    public partial struct CameraSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerMovementComponent>();
            state.RequireForUpdate<GamePlayingTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            //var tornadoPosition = PlayerMouvementSystem.Position((float)SystemAPI.Time.ElapsedTime);
            float3 playerPosition = new float3(0, 0, 0);
            foreach ((RefRW<LocalTransform> localTransform, RefRO<PlayerMovementComponent> playerSpeed) 
                in SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovementComponent>>())
            {
                playerPosition = localTransform.ValueRO.Position; 
            }

            var cam = Camera.main.transform;
            // Adjust the camera's position to be behind and slightly above the player
            cam.position = playerPosition + new float3(0, 6, -10); // You can adjust these values to your preference
            cam.LookAt(playerPosition); 

        }
    }
