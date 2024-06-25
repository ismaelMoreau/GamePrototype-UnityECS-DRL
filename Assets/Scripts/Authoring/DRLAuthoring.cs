
using Unity.Entities;
using UnityEngine;

public class ReadOnlyAttribute : PropertyAttribute { }
public class DRLAuthoring : MonoBehaviour
{

    [ReadOnly] public int inputSize = 11;
    public int hiddenSize = 9;
    [ReadOnly] public int outputSize = 9;



    private class Baker : Baker<DRLAuthoring>
    {
        public override void Bake(DRLAuthoring authoring)
        {

            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new NeuralNetworkComponent
            {
                inputSize = authoring.inputSize,
                hiddenSize = authoring.hiddenSize,
                outputSize = authoring.outputSize
            });
            // AddComponent(entity,new NeuralNetworkParametersComponent{       
            // });
            AddComponent(entity, new DrlConfigComponent
            {
                learningRate = 0.1f,
                discountFactor = 0.9f
            });
            // AddComponent(entity,new QvalueComponent{
            //     forward = 0,
            //     backward = 0,
            //     stepRight = 0,
            //     stepLeft = 0,
            //     dash = 0,
            //     block = 0,
            //     heal = 0,
            //     jump = 0,
            //     stay = 0
            // });
            AddBuffer<NeuralNetworkReplayBufferElement>(entity);
            // AddComponent(entity,new stateComponent{
            //     playerDistance = 0,
            //     playerHealth = 0,
            //     firstNearestEnemyDistance = 0,
            //     secondNearestEnemyDistance = 0,
            //     firstEnemyHealth = 0,
            //     secondEnemyHealth = 0,
            //     firstEnemyAction = 0,
            //     secondEnemyAction = 0,
            //     enemiesSharedReward = 0
            // });
        }


    }
}
