using JetBrains.Annotations;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct PlayerInputSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
        state.RequireForUpdate<SkillsConfig>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var skillsConfig = SystemAPI.GetSingleton<SkillsConfig>();
        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("Main camera not found!");
            return;
        }

       
        var targetClickPosition = SystemAPI.GetSingletonRW<PlayerTargetPosition>();
   
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0)) // 0 is for the left button
        {
           
            if (new Plane(Vector3.up, 0f).Raycast(ray, out var dist))
            {
                // Store the new target position in 3D
                targetClickPosition.ValueRW.targetClickPosition = ray.GetPoint(dist);
            }
        }
             
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (new Plane(Vector3.up, 0f).Raycast(ray2, out var dist2))
            {
                // Store the new target position in 3D
                targetClickPosition.ValueRW.targetMousePosition = ray2.GetPoint(dist2);
            }
            
        }
      
      
    }
}