using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class PlayerAuthoring : MonoBehaviour
{
    public float initialSpeed = 5f;
    public float3 targetClickPosition;
    private class Baker : Baker<PlayerAuthoring>{
      public override void Bake(PlayerAuthoring authoring)
        {
          Entity entity = GetEntity(TransformUsageFlags.Dynamic);
          AddComponent(entity ,new PlayerMovementComponent{
              speed = authoring.initialSpeed
          });
          AddComponent(entity ,new PlayerTargetPosition{
              targetClickPosition = authoring.targetClickPosition
          });
        }
    }
}
public struct PlayerMovementComponent : IComponentData
{
    public float speed;
}

public struct PlayerTargetPosition : IComponentData
{
    public float3 targetClickPosition;
}