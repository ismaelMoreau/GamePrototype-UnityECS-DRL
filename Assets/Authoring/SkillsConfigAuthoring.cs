using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class SkillConfigAuthoring : MonoBehaviour
{   

    public GameObject PrefabAoe;
    private class Baker : Baker<SkillConfigAuthoring>{
        public override void Bake(SkillConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity ,new SkillsConfig
            {
                PrefabAoe = GetEntity(authoring.PrefabAoe, TransformUsageFlags.Dynamic),
            });
        }
    }
}

public struct SkillsConfig: IComponentData
{
    public Entity PrefabAoe;
}