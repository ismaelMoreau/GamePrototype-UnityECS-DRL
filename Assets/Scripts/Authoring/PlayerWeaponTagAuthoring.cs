
using UnityEngine;
using Unity.Entities;
public class PlayerWeaponAuthoring : MonoBehaviour

    // Start is called before the first frame update
    {
            public bool playerShield = false;
            public bool playerSword = false;
            private class Baker : Baker<PlayerWeaponAuthoring>{
                public override void Bake(PlayerWeaponAuthoring authoring)
                {
                    var entity = GetEntity(TransformUsageFlags.Dynamic);
                    AddComponent(entity ,new PlayerWeaponTag
                    {        
                        playerShield = authoring.playerShield,
                        playerSword = authoring.playerSword
                    });
                }
            }
    }