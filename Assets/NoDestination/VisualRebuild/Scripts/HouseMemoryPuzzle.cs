using UnityEngine;
namespace NoDestination.VisualRebuild
{
    public sealed class HouseMemoryPuzzle : MonoBehaviour
    {
        public VisualJourney player;
        public GameObject[] segments;
        public Light[] lamps;
        public Renderer powerLamp;
        public Material litMaterial;
        public int[] digits=new int[3];
        public bool Powered {get;private set;}
        public bool ClockRead {get;private set;}
        static readonly int[] masks={63,6,91,79,102,109,125,7,127,111};
        void Start(){Refresh();foreach(var lamp in lamps)lamp.enabled=false;}
        void Refresh(){for(int d=0;d<3;d++)for(int s=0;s<7;s++)segments[d*7+s].SetActive((masks[digits[d]]&(1<<s))!=0);}
        public bool Handle(JourneyDetail detail)
        {
            if(detail.key=="house-clock"){ClockRead=true;player.PresentSubtitle("시계는 세 시 십칠 분에 멈춰 있다.\n승차권에도 같은 시간이 적혀 있었다.\n이층 전원함의 세 숫자는 3, 1, 7일까.");return true;}
            if(detail.key.StartsWith("house-dial-"))
            {
                if(Powered){player.PresentSubtitle("전원이 돌아왔다. 아래층 라디오를 확인하자.");return true;}
                int d=int.Parse(detail.key.Substring(11));digits[d]=(digits[d]+1)%10;Refresh();player.soundscape.PlaySwitch();
                player.PresentSubtitle("전원함  "+digits[0]+" · "+digits[1]+" · "+digits[2]);return true;
            }
            if(detail.key=="house-power")
            {
                player.soundscape.PlaySwitch();
                if(Powered){player.PresentSubtitle("전원은 켜져 있다. 라디오는 아래층에 있다.");return true;}
                if(digits[0]!=3||digits[1]!=1||digits[2]!=7){player.PresentSubtitle("스위치가 다시 내려갔다.\n아래층 시계가 멈춘 시간을 떠올려 보자.");return true;}
                Powered=true;foreach(var lamp in lamps)lamp.enabled=true;powerLamp.sharedMaterial=litMaterial;player.soundscape.PowerRadio();
                player.objectives[2]="전원이 돌아왔습니다. 아래층 라디오를 맞춰 보세요.";
                player.PresentSubtitle("따뜻한 불빛이 켜졌다.\n아래층 라디오에서 잡음이 들린다.");return true;
            }
            if(detail.key=="radio"&&!Powered){player.PresentSubtitle("전원이 들어오지 않는다.\n벽시계와 이층 전원함을 살펴봐야겠다.");return true;}
            if(detail.key=="photo"&&player.stage<3){player.PresentSubtitle("익숙한 사진이지만 기억이 흐릿하다.\n먼저 아래층 라디오를 켜 보자.");return true;}
            return false;
        }
    }
}
