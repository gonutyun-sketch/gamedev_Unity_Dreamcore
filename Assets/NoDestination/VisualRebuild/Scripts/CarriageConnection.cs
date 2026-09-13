using UnityEngine;
namespace NoDestination.VisualRebuild
{
    public sealed class CarriageConnection : MonoBehaviour
    {
        public VisualJourney player;public Transform firstDoor,secondDoor;
        public bool Opened {get;private set;}
        Vector3 firstBase,secondBase;
        void Start(){firstBase=firstDoor.localPosition;secondBase=secondDoor.localPosition;}
        public void Toggle()
        {
            if(Opened&&player.transform.position.z>8.3f&&player.transform.position.z<11.2f){player.PresentSubtitle("연결 통로를 지나간 뒤 문을 닫자.");return;}
            Opened=!Opened;player.soundscape.PlayCoachDoor(Opened);
            if(Opened)player.PresentSubtitle("식당칸이다.\n식사가 나오기 직전처럼 차려져 있다.");
        }
        void Update(){if(player.paused)return;float t=1-Mathf.Exp(-3*Time.deltaTime);firstDoor.localPosition=Vector3.Lerp(firstDoor.localPosition,firstBase+(Opened?Vector3.right*1.4f:Vector3.zero),t);secondDoor.localPosition=Vector3.Lerp(secondDoor.localPosition,secondBase+(Opened?Vector3.right*1.4f:Vector3.zero),t);}
    }
}
