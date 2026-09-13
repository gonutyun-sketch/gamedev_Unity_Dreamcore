using UnityEngine;
using UnityEngine.SceneManagement;
namespace NoDestination.VisualRebuild
{
    public static class JourneyPreferences
    {
        public static float Master=1,Music=.8f,Sfx=1,Sensitivity=1.45f;
        public static int ResolutionIndex=2,Graphics=2;public static bool Fullscreen=true;
        public static readonly Vector2Int[] Resolutions={new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080),new Vector2Int(2560,1440)};
        public static void Load(){Master=PlayerPrefs.GetFloat("nd.master",1);Music=PlayerPrefs.GetFloat("nd.music",.8f);Sfx=PlayerPrefs.GetFloat("nd.sfx",1);Sensitivity=PlayerPrefs.GetFloat("nd.mouse",1.45f);ResolutionIndex=PlayerPrefs.GetInt("nd.resolution",2);Graphics=PlayerPrefs.GetInt("nd.graphics",2);Fullscreen=PlayerPrefs.GetInt("nd.fullscreen",1)==1;AudioListener.volume=Master;ConfigureDisplay();}
        public static void Apply(){PlayerPrefs.SetFloat("nd.master",Master);PlayerPrefs.SetFloat("nd.music",Music);PlayerPrefs.SetFloat("nd.sfx",Sfx);PlayerPrefs.SetFloat("nd.mouse",Sensitivity);PlayerPrefs.SetInt("nd.resolution",ResolutionIndex);PlayerPrefs.SetInt("nd.graphics",Graphics);PlayerPrefs.SetInt("nd.fullscreen",Fullscreen?1:0);PlayerPrefs.Save();AudioListener.volume=Master;ConfigureDisplay();}
        static void ConfigureDisplay(){QualitySettings.SetQualityLevel(Mathf.Clamp(Graphics,0,QualitySettings.names.Length-1));
#if !UNITY_EDITOR
            var r=Resolutions[Mathf.Clamp(ResolutionIndex,0,Resolutions.Length-1)];Screen.SetResolution(r.x,r.y,Fullscreen?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
#endif
        }
    }
    public sealed class JourneyInterface : MonoBehaviour
    {
        VisualJourney game;GUIStyle small,body,title,button,subtitle,center;
        string displayedGoal="",pendingGoal="";float quietAge,goalAge;bool settings;
        public bool MissionVisible => displayedGoal.Length>0&&!game.SubtitlesBusy&&quietAge>1.2f;
        void Awake(){game=GetComponent<VisualJourney>();JourneyPreferences.Load();}
        public string Goal
        {
            get{
                if(!game.TicketRead)return "좌석 옆의 승차권을 확인하세요.";
                if(game.stage==1)return game.train.arrived?"FIELD 역에 내려 집을 살펴보세요.":"열차가 정차하기를 기다리세요.";
                if(game.stage==2&&game.housePuzzle)return game.housePuzzle.Objective;
                return game.objectives[Mathf.Clamp(game.stage,0,game.objectives.Length-1)];
            }
        }
        void Update()
        {
            if(!game.GameStarted||game.paused)return;
            if(game.SubtitlesBusy||game.IsWaking||!game.ReadyForMission){quietAge=0;return;}
            quietAge+=Time.deltaTime;pendingGoal=Goal;
            if(quietAge>1.2f&&pendingGoal!=displayedGoal){displayedGoal=pendingGoal;goalAge=0;game.soundscape.SubtitleTick();}
            goalAge+=Time.deltaTime;
        }
        void Styles()
        {
            if(body!=null)return;var font=Font.CreateDynamicFontFromOSFont("Malgun Gothic",20);
            body=new GUIStyle(GUI.skin.label){font=font,fontSize=17,wordWrap=true,normal={textColor=new Color(.89f,.88f,.79f)}};
            small=new GUIStyle(body){fontSize=13};title=new GUIStyle(body){fontSize=47,fontStyle=FontStyle.Normal};center=new GUIStyle(body){alignment=TextAnchor.MiddleCenter};subtitle=new GUIStyle(center){fontSize=21};
            button=new GUIStyle(body){alignment=TextAnchor.MiddleLeft,padding=new RectOffset(14,14,8,8),hover={textColor=Color.white},active={textColor=new Color(1,.84f,.5f)}};
        }
        static void Fill(Rect rect,Color color){var c=GUI.color;GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=c;}
        bool Button(Rect r,string text){Fill(r,new Color(.2f,.23f,.2f,.35f));Fill(new Rect(r.x,r.yMax-1,r.width,1),new Color(.8f,.8f,.7f,.18f));return GUI.Button(r,text,button);}
        void Slider(string label,ref float value,float min,float max,float x,float y,float width)
        {
            GUI.Label(new Rect(x,y,width,25),label,body);GUI.Label(new Rect(x+width-65,y,65,25),Mathf.RoundToInt(value/max*100)+"",small);value=GUI.HorizontalSlider(new Rect(x,y+32,width,20),value,min,max);
        }
        void OnGUI()
        {
            Styles();float scale=Mathf.Clamp(Screen.height/900f,.65f,2);GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);float w=Screen.width/scale,h=Screen.height/scale;
            if(!game.GameStarted||game.paused)
            {
                Fill(new Rect(0,0,w,h),new Color(.018f,.032f,.029f,.78f));float x=w*.12f;
                GUI.Label(new Rect(x,h*.16f,w*.8f,70),"NO DESTINATION",title);
                GUI.Label(new Rect(x,h*.16f+72,700,30),"목적지를 잃어버린 여행",small);
                if(settings)
                {
                    float y=h*.31f,width=Mathf.Min(480,w*.45f);
                    Slider("전체 음량",ref JourneyPreferences.Master,0,1,x,y,width);Slider("음악",ref JourneyPreferences.Music,0,1,x,y+65,width);Slider("효과음",ref JourneyPreferences.Sfx,0,1,x,y+130,width);Slider("마우스 감도",ref JourneyPreferences.Sensitivity,.35f,3,x,y+195,width);
                    AudioListener.volume=JourneyPreferences.Master;
                    float right=x+width+65;var r=JourneyPreferences.Resolutions[Mathf.Clamp(JourneyPreferences.ResolutionIndex,0,3)];
                    if(Button(new Rect(right,y,290,44),"해상도   "+r.x+" × "+r.y))JourneyPreferences.ResolutionIndex=(JourneyPreferences.ResolutionIndex+1)%4;
                    if(Button(new Rect(right,y+62,290,44),"전체 화면   "+(JourneyPreferences.Fullscreen?"켜짐":"꺼짐")))JourneyPreferences.Fullscreen=!JourneyPreferences.Fullscreen;
                    if(Button(new Rect(right,y+124,290,44),"그래픽   "+QualitySettings.names[Mathf.Clamp(JourneyPreferences.Graphics,0,QualitySettings.names.Length-1)]))JourneyPreferences.Graphics=(JourneyPreferences.Graphics+1)%QualitySettings.names.Length;
                    if(Button(new Rect(x,y+286,220,46),"적용하고 돌아가기")){JourneyPreferences.Apply();settings=false;}
                }
                else
                {
                    float y=h*.42f;if(Button(new Rect(x,y,300,48),game.GameStarted?"여행 계속하기":"여행 시작"))game.BeginGame();
                    if(Button(new Rect(x,y+68,300,48),"설정"))settings=true;
                    if(game.GameStarted&&Button(new Rect(x,y+136,300,48),"처음부터 다시 시작"))Restart();
                    GUI.Label(new Rect(x,h-92,680,35),"기억은 장소에 남아 있다. 목적지는 아직 지워져 있다.",small);
                }
                return;
            }
            if(MissionVisible&&!game.IsWaking)
            {
                float fade=Mathf.SmoothStep(0,1,Mathf.Clamp01(goalAge/1.2f));var c=GUI.color;GUI.color=new Color(1,1,1,fade);
                GUI.Label(new Rect(48-7*(1-fade),42,540,25),"기억의 실마리",small);Fill(new Rect(48,76,32,1),new Color(.9f,.87f,.7f,.6f));
                GUI.Label(new Rect(48-7*(1-fade),86,520,52),displayedGoal,body);GUI.color=c;
            }
            if(!game.IsWaking&&game.FocusLabel.Length>0){GUI.Label(new Rect(w/2-10,h/2-10,20,20),"◇",center);GUI.Label(new Rect(w/2-200,h/2+30,400,35),game.FocusLabel,new GUIStyle(center){fontSize=14});}
            if(game.VisibleSubtitle.Length>0){Fill(new Rect(w/2-390,h-120,780,70),new Color(.02f,.025f,.02f,.35f));GUI.Label(new Rect(w/2-370,h-112,740,58),game.VisibleSubtitle,subtitle);}
            if(game.MapRevealed&&game.MapTitleAge<8)
            {
                float a=Mathf.SmoothStep(0,1,Mathf.Clamp01(game.MapTitleAge/2))*Mathf.Clamp01((8-game.MapTitleAge)/2.5f);Fill(new Rect(0,0,w,h),new Color(.03f,.05f,.04f,a*.18f));var c=GUI.color;GUI.color=new Color(1,.96f,.82f,a);
                GUI.Label(new Rect(0,h*.28f,w,h*.30f),game.MapName,new GUIStyle(center){fontSize=Mathf.RoundToInt(w*.1f)});GUI.Label(new Rect(0,h*.59f,w,45),game.MapCaption,new GUIStyle(center){fontSize=18});GUI.color=c;
            }
            if(game.IsWaking)
            {
                float e=game.WakeProgress*8,opening=Mathf.SmoothStep(0,1,Mathf.Clamp01((e-1.2f)/2.1f)),blink=Mathf.Exp(-Mathf.Pow((e-3.5f)/.22f,2))*.68f,lid=Mathf.Clamp01(1-opening+blink)*h*.5f;
                for(int i=0;i<256;i++){float u=(i+.5f)/128-1,c=Mathf.Min(h*.5f,lid*(1+.32f*u*u));Fill(new Rect(i*w/256,0,w/256+1,c),Color.black);Fill(new Rect(i*w/256,h-c,w/256+1,c),Color.black);}
            }
            var spatial=game.GetComponent<FieldSpatialRules>();if(spatial&&spatial.BoundaryFade>0)Fill(new Rect(0,0,w,h),new Color(.78f,.79f,.69f,spatial.BoundaryFade));
            if(game.train.blink>0)Fill(new Rect(0,0,w,h),new Color(0,0,0,game.train.blink));
            var transit=game.GetComponent<RealityTransit>();if(transit&&transit.Whiteout>0)Fill(new Rect(0,0,w,h),new Color(.88f,.85f,.74f,transit.Whiteout));
        }
        static void Restart(){Time.timeScale=1;AudioListener.pause=false;
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(SceneManager.GetActiveScene().path,new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#endif
        }
    }
}
