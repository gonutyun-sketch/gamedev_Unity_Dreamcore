using System;
using UnityEngine;
namespace NoDestination.VisualRebuild
{
    public sealed class TrainDreamMotion : MonoBehaviour
    {
        public Transform[] wheatPatches,passingScenery;
        public GameObject[] stationObjects;
        public Transform sleeperRoot;
        public AudioClip announcement;
        public float journeySeconds=42,speed,elapsed,blink;
        public bool arrived,departing;
        public float DepartureDistance {get;private set;}
        public bool AtSea {get;private set;}
        public float ApproachDistance {get;private set;}
        public float WorldTravel {get;private set;}
        public JourneySoundscape soundscape;
        Vector3[] wheatStart,stationStart;float departAt;bool announced;
        // Integral of the existing 11-second smooth braking curve. Zero at the established stop.
        public float RemainingDistance(float time)
        {
            float brake=Mathf.Min(11,journeySeconds),begin=journeySeconds-brake;
            if(time<=begin)return 13*(begin-time+brake*.5f);
            float u=Mathf.Clamp01((time-begin)/brake);
            return 13*brake*(.5f-u+u*u*u-.5f*u*u*u*u);
        }
        void Start()
        {
            wheatStart=new Vector3[wheatPatches.Length];for(int i=0;i<wheatStart.Length;i++)wheatStart[i]=wheatPatches[i].localPosition;
            stationStart=new Vector3[stationObjects.Length];for(int i=0;i<stationObjects.Length;i++)if(stationObjects[i]){stationStart[i]=stationObjects[i].transform.position;stationObjects[i].SetActive(true);}
            ApproachDistance=RemainingDistance(0);WorldTravel=-ApproachDistance;PlaceApproachingStation();MoveWheat();
        }
        void PlaceApproachingStation()
        {
            for(int i=0;i<stationObjects.Length;i++)if(stationObjects[i])stationObjects[i].transform.position=stationStart[i]+Vector3.forward*ApproachDistance;
            Shader.SetGlobalVector("_FieldHouseCentre",new Vector4(22,65+ApproachDistance,5.5f,5.1f));
            Shader.SetGlobalVector("_FieldPath",new Vector4(9.2f,12.7f+ApproachDistance,22,60.2f+ApproachDistance));
        }
        void MoveWheat()
        {
            for(int i=0;i<wheatPatches.Length;i++){var p=wheatStart[i];p.z=Mathf.Repeat(p.z-WorldTravel+40,200)-40;wheatPatches[i].localPosition=p;}
            if(sleeperRoot)sleeperRoot.localPosition=new Vector3(0,0,-Mathf.Repeat(WorldTravel,.65f));
        }
        public void Depart(){if(departing)return;departing=true;departAt=elapsed;DepartureDistance=0;arrived=false;}
        public void StopAtSea(){AtSea=true;arrived=true;departing=false;speed=0;blink=0;foreach(var o in stationObjects)if(o)o.SetActive(false);foreach(var p in passingScenery)if(p)p.gameObject.SetActive(false);}
        public void PlayAnnouncement(){if(soundscape)soundscape.PlayArrival();}
        void Update()
        {
            if(AtSea)return;
            elapsed+=Time.deltaTime;float distance=0;
            if(departing)
            {
                speed=Mathf.SmoothStep(0,13,Mathf.Clamp01((elapsed-departAt)/9));distance=speed*Time.deltaTime;DepartureDistance+=distance;WorldTravel+=distance;
                foreach(var o in stationObjects)if(o)o.transform.position+=Vector3.back*distance;
                foreach(var p in passingScenery)if(p&&!p.gameObject.activeSelf)p.gameObject.SetActive(true);
            }
            else if(!arrived)
            {
                float remaining=RemainingDistance(elapsed);distance=ApproachDistance-remaining;ApproachDistance=remaining;WorldTravel=-remaining;
                speed=13*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-(journeySeconds-11))/11)));
                PlaceApproachingStation();
            }
            else speed=0;
            if(!announced&&elapsed>journeySeconds-16){announced=true;PlayAnnouncement();}
            blink=0;
            if(!arrived&&!departing&&elapsed>=journeySeconds)
            {
                arrived=true;speed=0;
                // Countryside stays in place as the train brakes; no background disappearance.
            }
            MoveWheat();
            if(distance>0)foreach(var prop in passingScenery){var p=prop.localPosition;p.z-=distance;if(p.z< -90)p.z+=300;prop.localPosition=p;}
        }
    }
}
