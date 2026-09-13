using UnityEngine;
namespace NoDestination.VisualRebuild
{
    public sealed class FieldSpatialRules : MonoBehaviour
    {
        public VisualJourney player;
        public Transform[] houseParts;
        public Transform path;public Transform[] pathPebbles;
        public bool Extended {get;private set;}
        public float HouseZ => (Extended?110:65)-player.train.DepartureDistance;
        public float BoundaryFade {get;private set;}
        float originalFog,returnAge;bool returning,saidLost;
        Vector3 returnPoint;
        void Start(){originalFog=RenderSettings.fogDensity;UpdateExclusions();}
        void UpdateExclusions(){Shader.SetGlobalVector("_FieldHouseCentre",new Vector4(22,HouseZ,5.5f,5.1f));Shader.SetGlobalVector("_FieldPath",new Vector4(9.2f,12.7f-player.train.DepartureDistance,22,HouseZ-4.8f));}
        public void ExtendReturnPath()
        {
            if(Extended)return;Extended=true;Vector3 shift=Vector3.forward*45;
            foreach(var part in houseParts)if(part)part.position+=shift;
            var cc=player.GetComponent<CharacterController>();cc.enabled=false;player.transform.position+=shift;cc.enabled=true;
            player.soundscape.MoveHouseAudio(shift);
            var a=new Vector3(9.2f,-.736f,12.7f);var b=new Vector3(22,-.736f,105.2f);path.position=(a+b)*.5f;path.rotation=Quaternion.LookRotation(b-a);path.localScale=new Vector3(2.6f,.026f,Vector3.Distance(a,b));
            foreach(var pebble in pathPebbles)if(pebble)pebble.position+=Vector3.forward*45*Mathf.InverseLerp(12.7f,60.2f,pebble.position.z);
            UpdateExclusions();
        }
        void Update()
        {
            if(player.paused||player.stage<2)return;
            if(player.stage==5){UpdateExclusions();return;}
            var p=player.transform.position;
            // The playable field repeats before even the distant terrain edge can be approached.
            float distance=player.stage>=6?new Vector2(p.x,p.z).magnitude/180:new Vector2((p.x-14)/72,(p.z-60)/115).magnitude;
            if(!returning&&distance>1){returning=true;returnAge=0;returnPoint=new Vector3(12,-.65f,26);}
            if(returning)
            {
                returnAge+=Time.deltaTime;BoundaryFade=returnAge<1.5f?Mathf.SmoothStep(0,1,returnAge/1.5f):Mathf.Clamp01((3-returnAge)/1.5f);
                if(returnAge>=1.5f&&returnAge-Time.deltaTime<1.5f){player.ResetPose(returnPoint,20);if(!saidLost){saidLost=true;player.PresentSubtitle("...다시 같은 길이다.");}}
                if(returnAge>3){returning=false;BoundaryFade=0;}
            }
            else BoundaryFade=Mathf.InverseLerp(.85f,1,distance)*.25f;
            if(player.stage<5)RenderSettings.fogDensity=Mathf.Lerp(originalFog,.024f,BoundaryFade);
        }
        void OnDestroy(){RenderSettings.fogDensity=originalFog;Shader.SetGlobalVector("_FieldHouseCentre",Vector4.zero);Shader.SetGlobalVector("_FieldPath",Vector4.zero);}
    }
}
