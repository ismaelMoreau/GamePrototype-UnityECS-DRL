using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class QlearningAuthoring : MonoBehaviour
{   
  
    private class Baker : Baker<QlearningAuthoring>{
        public override void Bake(QlearningAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
        }
    }
}

public struct QtableComponent: IComponentData
{

    public float up, down, left, right;

}
public struct QtableRewardComponent: IComponentData
{

    public float reward;
}