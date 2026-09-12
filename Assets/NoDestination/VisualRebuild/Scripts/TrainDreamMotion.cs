using System;
using UnityEngine;
namespace NoDestination.VisualRebuild
{
    public sealed class TrainDreamMotion : MonoBehaviour
    {
        public Transform[] wheatPatches;
        public Transform[] passingScenery;
        public GameObject[] stationObjects;
        public Transform sleeperRoot;
        public AudioClip announcement;
        public float journeySeconds=42;
        public float speed;
        public bool arrived;
        public bool departing;
        public float elapsed;
        public float blink;
        Vector3[] wheatStart,sceneryStart;
        public JourneySoundscape soundscape;
        float travelled,departAt;
        bool announced;
        void Start()
        {
            wheatStart=new Vector3[wheatPatches.Length];for(int i=0;i<wheatStart.Length;i++)wheatStart[i]=wheatPatches[i].localPosition;
            sceneryStart=new Vector3[passingScenery.Length];for(int i=0;i<sceneryStart.Length;i++)sceneryStart[i]=passingScenery[i].localPosition;
            foreach(var o in stationObjects)if(o)o.SetActive(false);

        }
        public void Depart(){if(departing)return;departing=true;departAt=elapsed;arrived=false;foreach(var o in stationObjects)if(o)o.SetActive(false);}
        public void PlayAnnouncement(){if(soundscape)soundscape.PlayArrival();}
        void Update()
        {
            elapsed+=Time.deltaTime;
            if(departing)speed=Mathf.SmoothStep(0,13,Mathf.Clamp01((elapsed-departAt)/9));
            else if(!arrived)speed=13*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-(journeySeconds-11))/11)));
            else speed=0;
            if(!announced&&elapsed>journeySeconds-16){announced=true;PlayAnnouncement();}
            blink=0;
            if(!departing&&elapsed>journeySeconds-.35f&&elapsed<journeySeconds+.35f){blink=1-Mathf.Abs((elapsed-journeySeconds)/.35f);blink=Mathf.Clamp01(blink);}
            if(!arrived&&!departing&&elapsed>=journeySeconds)
            {
                arrived=true;speed=0;
                for(int i=0;i<wheatPatches.Length;i++)wheatPatches[i].localPosition=wheatStart[i];
                foreach(var o in stationObjects)if(o)o.SetActive(true);
                foreach(var p in passingScenery)if(p)p.gameObject.SetActive(false);
            }
            if(departing)foreach(var p in passingScenery)if(p&&!p.gameObject.activeSelf)p.gameObject.SetActive(true);
            if(speed>.01f)
            {
                travelled+=speed*Time.deltaTime;
                foreach(var patch in wheatPatches){var p=patch.localPosition;p.z-=speed*Time.deltaTime;if(p.z< -50)p.z+=200;patch.localPosition=p;}
                foreach(var prop in passingScenery){var p=prop.localPosition;p.z-=speed*Time.deltaTime;if(p.z< -90)p.z+=300;prop.localPosition=p;}
                if(sleeperRoot)sleeperRoot.localPosition=new Vector3(0,0,-Mathf.Repeat(travelled,.65f));
            }
}
    }
}

