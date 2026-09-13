using UnityEngine;
namespace NoDestination.VisualRebuild
{
    public sealed class HouseMemoryPuzzle : MonoBehaviour
    {
        public VisualJourney player;
        public Transform[] selectors;
        public GameObject fuseVisual;
        public Light[] lamps;
        public Renderer powerLamp;
        public Material litMaterial;
        public int[] digits=new int[3];
        public bool Powered {get;private set;}
        public bool ClockRead {get;private set;}
        public bool PanelSeen {get;private set;}
        public bool FuseFound {get;private set;}
        public bool FuseInstalled {get;private set;}
        public string Objective => Powered?"아래층 라디오를 맞춰 보세요.":!PanelSeen?"집 안을 살펴보세요.":!FuseFound?"전원함에 빠진 퓨즈를 집 안에서 찾아보세요.":!FuseInstalled?"찾은 퓨즈를 이층 전원함에 끼우세요.":"배선을 따라 회전 접점을 연결한 뒤 스위치를 올리세요.";
        void Start(){Refresh();foreach(var lamp in lamps)lamp.enabled=false;}
        void Refresh(){for(int d=0;d<3;d++)selectors[d].localRotation=Quaternion.Euler(0,0,-90*digits[d]);}
        public bool Handle(JourneyDetail detail)
        {
            if(detail.key=="house-clock"){ClockRead=true;player.PresentSubtitle("세 시 십칠 분.\n초침 소리는 나는데, 바늘은 움직이지 않는다.");return true;}
            if(detail.key=="house-fuse"){FuseFound=true;fuseVisual.SetActive(false);detail.gameObject.SetActive(false);player.soundscape.PlaySwitch();player.PresentSubtitle(PanelSeen?"작은 퓨즈다. 전원함의 빈 자리에 맞을 것 같다.":"서랍 안에 퓨즈가 하나 남아 있다.");return true;}
            if(detail.key.StartsWith("house-dial-"))
            {
                if(Powered){player.PresentSubtitle("전원이 돌아왔다. 아래층 라디오를 확인하자.");return true;}
                PanelSeen=true;int d=int.Parse(detail.key.Substring(11));digits[d]=(digits[d]+1)%3;Refresh();player.soundscape.PlaySwitch();
                player.PresentSubtitle("접점이 돌아갔다. 선이 이어지는 쪽은 어디지?");return true;
            }
            if(detail.key=="house-power")
            {
                player.soundscape.PlaySwitch();
                PanelSeen=true;
                if(!FuseInstalled){if(!FuseFound){player.PresentSubtitle("퓨즈 한 개가 빠져 있다.\n집 안에 남은 것이 있을까.");return true;}FuseInstalled=true;player.PresentSubtitle("퓨즈가 딱 맞는다.\n이제 끊어진 배선을 이어야 한다.");return true;}
                if(Powered){player.PresentSubtitle("전원은 켜져 있다. 라디오는 아래층에 있다.");return true;}
                if(digits[0]!=2||digits[1]!=0||digits[2]!=1){player.PresentSubtitle("불이 들어오지 않는다.\n회전 접점이 끊긴 선을 이어야 할 것 같다.");return true;}
                Powered=true;player.GetComponent<FieldSpatialRules>()?.ExtendReturnPath();foreach(var lamp in lamps)lamp.enabled=true;powerLamp.sharedMaterial=litMaterial;player.soundscape.PowerRadio();
                player.objectives[2]="전원이 돌아왔습니다. 아래층 라디오를 맞춰 보세요.";
                player.PresentSubtitle("따뜻한 불빛이 켜졌다.\n아래층 라디오에서 잡음이 들린다.");return true;
            }
            if(detail.key=="radio"&&!Powered){player.PresentSubtitle(PanelSeen?"아직 전원이 들어오지 않는다.":"켜지지 않는다.\n전원이 끊긴 것 같다.");return true;}
            if(detail.key=="photo"&&player.stage<3){player.PresentSubtitle("익숙한 사진이지만 기억이 흐릿하다.\n먼저 아래층 라디오를 켜 보자.");return true;}
            return false;
        }
    }
}
