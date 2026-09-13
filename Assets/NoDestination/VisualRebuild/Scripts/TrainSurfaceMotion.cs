using UnityEngine;
namespace NoDestination.VisualRebuild
{
    public sealed class TrainSurfaceMotion : MonoBehaviour
    {
        public TrainDreamMotion train;public Renderer earth,ballast;public ParticleSystem exhaust;
        Material soil,stones;float travel;
        void Start(){soil=earth.material;stones=ballast.material;}
        void LateUpdate()
        {
            travel=train.WorldTravel;
            var offset=new Vector2(0,-travel/(320f/106f));soil.SetTextureOffset("_BaseMap",offset);soil.SetTextureOffset("_BumpMap",offset);soil.SetTextureOffset("_MetallicGlossMap",offset);stones.SetTextureOffset("_BaseMap",new Vector2(0,-travel/(420f/55)));
            Shader.SetGlobalFloat("_WorldTravel",travel);
            var emission=exhaust.emission;emission.rateOverTime=2+train.speed*.65f;var velocity=exhaust.velocityOverLifetime;velocity.z=-train.speed*.5f;
        }
        void OnDestroy(){if(soil)Destroy(soil);if(stones)Destroy(stones);Shader.SetGlobalFloat("_WorldTravel",0);}
    }
}
