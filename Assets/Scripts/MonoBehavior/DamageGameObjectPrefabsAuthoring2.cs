using Unity.Entities;
using UnityEngine;


public class DamageGameObjectPrefabsAuthoring2 : MonoBehaviour
    {
        public GameObject damageUIPrefab;

        public class DamageGameObjectPrefabsBaker : Baker<DamageGameObjectPrefabsAuthoring2>
        {
            public override void Bake(DamageGameObjectPrefabsAuthoring2 authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(entity, new DamageGameObjectPrefabs2
                {
                    damageUIPrefab = authoring.damageUIPrefab
                });
            }
        }
    }

public class DamageGameObjectPrefabs2 : IComponentData
    {
        public GameObject damageUIPrefab;
    }