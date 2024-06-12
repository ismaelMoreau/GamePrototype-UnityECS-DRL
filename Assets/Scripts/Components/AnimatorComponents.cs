using Unity.Entities;
using UnityEngine;
public struct AnimatorInstantiationData : IComponentData
{
    public UnityObjectRef<GameObject> AnimatorGameObject;
}

public class AnimatorCleanup : ICleanupComponentData
{
    public Animator DestroyThisAnimator;
}
