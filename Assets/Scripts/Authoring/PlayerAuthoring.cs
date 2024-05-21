using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;

public class PlayerAuthoring : MonoBehaviour
{
    public float initialSpeed = 5f;
    public GameObject targetPrefab;
    public bool isGrounded = true;
    public float currentHealth = 100;
    public float maxHealth = 100; 
    private class Baker : Baker<PlayerAuthoring>{
      public override void Bake(PlayerAuthoring authoring)
        {
          Entity entity = GetEntity(TransformUsageFlags.Dynamic);
          AddComponent(entity ,new PlayerMovementComponent{
            speed = authoring.initialSpeed,
            isGrounded = authoring.isGrounded
          });
          AddComponent(entity ,new PlayerTargetPosition{
            isWaitingForClick = false,
            targetPrefab = GetEntity(authoring.targetPrefab, TransformUsageFlags.Dynamic),
          });
          AddComponent(entity, new PlayerHealth{
            currentHealth = authoring.currentHealth,
            maxHealth = authoring.maxHealth
          });
          AddBuffer<HitBufferElement>(entity);
        }
    }
}
public struct PlayerMovementComponent : IComponentData
{
    public float speed;
    public bool isGrounded;

    public double JumpStartTime;
}

public struct PlayerTargetPosition : IComponentData
{
    public float3 targetClickPosition;

    public float3 targetMousePosition;
    public bool isWaitingForClick;
    public Entity targetPrefab;
}
public struct PlayerHealth : IComponentData 
{
    public float currentHealth;
    public float maxHealth; 
}
public struct HitBufferElement : IBufferElementData
{
    public bool IsHandled;
    public float3 Position;
    public float3 Normal;
    public Entity HitEntity;
}