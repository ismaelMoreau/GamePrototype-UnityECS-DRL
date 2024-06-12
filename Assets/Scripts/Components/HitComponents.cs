using Unity.Entities;
using Unity.Mathematics;

public struct HitBufferElement : IBufferElementData
{
    public bool IsHandled;
    public float3 Position;
    public float3 Normal;
    public Entity HitEntity;
    public Entity triggerEntity;
    public float cooldownTimer ;
    public float cooldownDuration ;
}
public struct BackwardEffect : IComponentData
{
    public bool haveAbackwardEffect;
}
public struct HitBackwardEffectComponent : IComponentData, IEnableableComponent
{
    public bool animationHasPlayed;
    public float goingBackHitSpeed;
    public float3 direction;
    public float goingBackHitTimer;
    public float goingBackHitDuration;
}