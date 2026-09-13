using UnityEngine;
using UnityEngine.SceneManagement;
namespace NoDestination.VisualRebuild
{
    public sealed class FieldDeadline : MonoBehaviour
    {
        public VisualJourney player;
        public float allowedSeconds=180;
        public float visitElapsed;
        public bool missedTrain {get;private set;}
        public float Danger => Mathf.SmoothStep(0,1,Mathf.InverseLerp(allowedSeconds*.38f,allowedSeconds,visitElapsed));
        Material originalSky,sky;
        Color zenith,horizon,sunColor,fogColor;
        float failureAge;
        public bool LastCall {get;private set;}
        public float LastCallAge {get;private set;}
        public bool HorrorSeen {get;private set;}
        
        bool warned; Font failureFont;
        void Start()
        {
            originalSky=RenderSettings.skybox;sky=new Material(originalSky);RenderSettings.skybox=sky;
            zenith=sky.GetColor("_Zenith");horizon=sky.GetColor("_Horizon");sunColor=RenderSettings.sun.color;fogColor=RenderSettings.fogColor;

        }
        void Update()
        {
            if(player.paused)return;
            if(missedTrain){failureAge+=Time.deltaTime;if(player.slidingDoor)player.slidingDoor.localPosition=Vector3.Lerp(player.slidingDoor.localPosition,Vector3.zero,Time.deltaTime*3);return;}
            bool aboard=Mathf.Abs(player.transform.position.x)<1.7f&&Mathf.Abs(player.transform.position.z)<28.5f;
            if(!aboard&&player.stage>=2&&player.stage<5&&player.train.arrived)visitElapsed+=Time.deltaTime;
            float danger=Danger;
            sky.SetColor("_Zenith",Color.Lerp(zenith,new Color(.24f,.012f,.023f),danger));sky.SetColor("_Horizon",Color.Lerp(horizon,new Color(.92f,.13f,.035f),danger));
            RenderSettings.sun.color=Color.Lerp(sunColor,new Color(1,.22f,.09f),danger);RenderSettings.fogColor=Color.Lerp(fogColor,new Color(.48f,.09f,.055f),danger);
            if(!warned&&danger>.32f&&!player.SubtitlesBusy){warned=true;player.PresentSubtitle("뭐지...?\n하늘 색이 달라졌다.",9);}
            var crows=player.GetComponent<ScarecrowField>();
            if(crows&&crows.AnyMoved)HorrorSeen=true;
            if(aboard&&LastCall){LastCall=false;LastCallAge=0;visitElapsed=allowedSeconds*.85f;}
            if(!aboard&&visitElapsed>=allowedSeconds&&player.stage<5)
            {
                if(!LastCall){LastCall=true;player.PresentSubtitle("허수아비가... 아까도 저기에 있었나?");}
                LastCallAge+=Time.deltaTime;
                if(LastCallAge<15)return;
                missedTrain=true;player.scriptedMove=Vector2.zero;player.soundscape.SetDoor(false);
                player.PresentSubtitle("빛이... 사라졌다.\n지금 나는 어디에 있는 거지?",12);
                Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
            }
        }
        void OnGUI()
        {
            if(!missedTrain)return;
            var old=GUI.matrix;GUI.matrix=Matrix4x4.identity;
            GUI.color=new Color(.025f,0,0,Mathf.Clamp01((failureAge-4)/5)*.94f);GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);GUI.color=Color.white;
            if(failureAge>10)
            {
                if(!failureFont)failureFont=Font.CreateDynamicFontFromOSFont("Malgun Gothic",20);var style=new GUIStyle(GUI.skin.label){font=failureFont,alignment=TextAnchor.MiddleCenter,fontSize=20,wordWrap=true};
                GUI.Label(new Rect(Screen.width/2-220,Screen.height/2-90,440,80),"열차를 놓쳤습니다.\n이번 역에서 찾은 기억을 잃었습니다.",style);
                if(GUI.Button(new Rect(Screen.width/2-110,Screen.height/2+20,220,44),"다시 눈뜨기",new GUIStyle(GUI.skin.button){font=failureFont,fontSize=18}))
                {
#if UNITY_EDITOR
                    UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(SceneManager.GetActiveScene().path,new LoadSceneParameters(LoadSceneMode.Single));
#else
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#endif
                }
            }
            GUI.matrix=old;
        }
        void OnDestroy(){if(originalSky)RenderSettings.skybox=originalSky;if(RenderSettings.sun)RenderSettings.sun.color=sunColor;RenderSettings.fogColor=fogColor;if(sky)Destroy(sky);}
    }
}

