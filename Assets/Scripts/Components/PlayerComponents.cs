using Unity.Entities;
using Unity.Mathematics;

public struct PlayerMovementComponent : IComponentData
{
    public float speed;
    public bool isGrounded;

    public double JumpStartTime;
    public bool isRunning;
    public bool isWalking;
    public bool IsAttacking;
}

public struct PlayerTargetPosition : IComponentData
{
    public float3 targetClickPosition;

    public float3 targetMousePosition;
    public bool isWaitingForClick;
    public Entity targetPrefab;
}