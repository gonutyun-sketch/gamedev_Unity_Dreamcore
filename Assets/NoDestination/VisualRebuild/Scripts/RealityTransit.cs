using UnityEngine;
namespace NoDestination.VisualRebuild
{
    [DefaultExecutionOrder(50)]
    public sealed class RealityTransit : MonoBehaviour
    {
        public VisualJourney player;public GameObject seaStation;public Collider seaFloor;
        public GameObject[] fieldOnly;public Renderer[] impossibleWindows;
        public Light[] carriageLights;float[] lampStrengths;
        public float Age {get;private set;}
        public float Blend {get;private set;}
        public float Whiteout {get;private set;}
        public bool SeaArrived {get;private set;}
        public bool Begun {get;private set;}
        bool seaTitle;Color fieldZenith,fieldHorizon,fieldSun,fieldFog;
        Material sky;
        void Start()
        {
            sky=RenderSettings.skybox;lampStrengths=new float[carriageLights.Length];for(int i=0;i<carriageLights.Length;i++)lampStrengths[i]=carriageLights[i].intensity;
            seaStation.SetActive(false);seaFloor.enabled=false;ResetGlobals();
        }
        static void ResetGlobals(){Shader.SetGlobalFloat("_RealityBlend",0);Shader.SetGlobalFloat("_WindowFracture",0);Shader.SetGlobalFloat("_PuddleReveal",0);Shader.SetGlobalFloat("_HorizonFracture",0);}
        void LateUpdate()
        {
            if(player.paused)return;
            if(!Begun&&player.stage==5&&player.train.departing)
            {
                Begun=true;fieldZenith=sky.GetColor("_Zenith");fieldHorizon=sky.GetColor("_Horizon");fieldSun=RenderSettings.sun.color;fieldFog=RenderSettings.fogColor;
            }
            if(!Begun)return;
            if(player.stage<5)return;
            Age+=Time.deltaTime;
            Blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(16,40,Age));
            Shader.SetGlobalFloat("_RealityBlend",Blend);
            Shader.SetGlobalFloat("_WindowFracture",SeaArrived?0:Mathf.SmoothStep(0,1,Mathf.InverseLerp(9,20,Age)));
            Shader.SetGlobalFloat("_PuddleReveal",Mathf.SmoothStep(0,1,Mathf.InverseLerp(19,33,Age)));
            Shader.SetGlobalFloat("_HorizonFracture",Mathf.Sin(Mathf.InverseLerp(20,40,Age)*Mathf.PI)*.8f);
            sky.SetColor("_Zenith",Color.Lerp(fieldZenith,new Color(.75f,.69f,.50f),Blend));sky.SetColor("_Horizon",Color.Lerp(fieldHorizon,new Color(.82f,.78f,.64f),Blend));
            RenderSettings.sun.color=Color.Lerp(fieldSun,new Color(1,.91f,.72f),Blend);RenderSettings.fogColor=Color.Lerp(fieldFog,new Color(.82f,.78f,.64f),Blend);RenderSettings.fogDensity=Mathf.Lerp(.0038f,.0018f,Blend);
            if(!SeaArrived&&Age>12)for(int i=0;i<carriageLights.Length;i++)carriageLights[i].intensity=lampStrengths[i]*(Mathf.Sin(Age*8.2f+i*.7f)>.94f?.50f:1);
            Whiteout=Age<41?Mathf.SmoothStep(0,1,Mathf.InverseLerp(39,41,Age)):Mathf.SmoothStep(1,0,Mathf.InverseLerp(43,46,Age));
            if(Age>=42&&!SeaArrived)
            {
                SeaArrived=true;player.train.StopAtSea();player.deadline.enabled=false;
                foreach(var o in fieldOnly)if(o)o.SetActive(false);
                foreach(var w in impossibleWindows)if(w)w.enabled=false;
                seaStation.SetActive(true);seaFloor.enabled=true;player.stage=6;player.soundscape.EnterSea();
                for(int i=0;i<carriageLights.Length;i++)carriageLights[i].intensity=lampStrengths[i];
            }
            if(SeaArrived&&Age>46&&player.transform.position.x>2.8f&&!seaTitle){seaTitle=true;player.ShowMapTitle("THE SEA","두 번째 기억  /  수평선의 반대편");}
            if(SeaArrived&&player.stage==6&&player.transform.position.x>8)player.stage=7;
        }
        void OnDestroy(){ResetGlobals();}
    }
}
