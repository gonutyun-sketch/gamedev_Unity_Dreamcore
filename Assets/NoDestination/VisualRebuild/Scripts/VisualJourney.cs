using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace NoDestination.VisualRebuild
{
    public sealed class VisualJourney : MonoBehaviour
    {
        public Camera eyes;
        public Transform slidingDoor;
        public Vector3 doorOffset=new Vector3(0,0,1.5f);
        public GameObject changedSeat;
        public TrainDreamMotion train;
        public AudioClip radioClip;
        public JourneySoundscape soundscape;
        public FieldDeadline deadline;
        public HouseMemoryPuzzle housePuzzle;
        public bool MapRevealed {get;private set;}
        public float MapTitleAge {get;private set;}
        public string MapName {get;private set;}="THE FIELD";
        public string MapCaption {get;private set;}="첫 번째 기억  /  돌아오지 않는 오후";
        public void ShowMapTitle(string name,string caption){MapName=name;MapCaption=caption;MapRevealed=true;MapTitleAge=0;}
        public bool ReadyForMission => TicketRead||introLine>=2;
        public bool SubtitlesBusy => subtitleLine.Length>0;
        public bool TicketRead {get;private set;}
        public bool GameStarted {get;private set;}
        public string FocusLabel => focusedDetail?focusedDetail.label:"";
        public void BeginGame(){GameStarted=true;SetPaused(false);}
        readonly Queue<string> subtitleLines=new Queue<string>();
        string subtitleLine="";float subtitleAge;int lastSubtitleLetters;
        JourneyDetail focusedDetail;
        public string VisibleSubtitle {get;private set;}="";
        public int stage;
        public bool paused;
        public bool IsWaking => elapsed<8;
        public float WakeProgress => Mathf.Clamp01(elapsed/8);
        [NonSerialized] public Vector2? scriptedMove;
        [NonSerialized] public bool? scriptedZoom;
        [NonSerialized] public bool scriptedHurry;
        public bool zooming {get;private set;}
        public string[] objectives={"좌석 옆에 놓인 빨간 승차권을 확인하세요.","THE FIELD에 정차하면 옆문으로 내려, 집을 찾아가세요.","집 안의 라디오에서 들리는 목소리를 확인하세요.","라디오가 말한 가족사진을 조사하세요.","찾은 기억을 가지고 열차로 돌아가세요.","첫 번째 기억을 찾았습니다. 열차는 다음 역으로 향합니다."};
        CharacterController controller;
        float targetYaw,yaw,pitch,targetPitch,noteUntil,bobPhase,footDistance,fovVelocity,elapsed;
        Vector3 movement,moveDamp,eyeBase;
        string note="",hint="";
        GUIStyle label,center,heading,subtitleStyle;

        int introLine;
        bool arrivalSaid;
        void Start()
        {
            controller=GetComponent<CharacterController>();yaw=targetYaw=transform.eulerAngles.y;eyeBase=eyes.transform.localPosition;
            Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;
            if(GetComponent<JourneyInterface>()&&!GameStarted)SetPaused(true);else GameStarted=true;
        }
        public void SetPaused(bool value){paused=value;Time.timeScale=value?0:1;AudioListener.pause=value;Cursor.lockState=value?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=value;}
        public void ResetPose(Vector3 position,float heading=0)
        {
            controller.enabled=false;transform.position=position;controller.enabled=true;targetYaw=yaw=heading;targetPitch=pitch=0;movement=Vector3.zero;eyes.transform.localPosition=eyeBase;
        }
        public void AimAt(Vector3 point)
        {
            var direction=(point-eyes.transform.position).normalized;targetYaw=yaw=Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg;targetPitch=pitch=-Mathf.Asin(direction.y)*Mathf.Rad2Deg;
            transform.rotation=Quaternion.Euler(0,yaw,0);eyes.transform.localRotation=Quaternion.Euler(pitch,0,0);
        }
        void Update()
        {
            if(GameStarted&&Input.GetKeyDown(KeyCode.Escape))SetPaused(!paused);
            if(paused)return;
            if(MapRevealed)MapTitleAge+=Time.deltaTime;
            UpdateSubtitle();
            if(deadline&&deadline.missedTrain)return;
            float dt=Mathf.Min(Time.deltaTime,.04f);elapsed+=Time.deltaTime;
            if(IsWaking)
            {
                float rise=Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-3.5f)/4.5f));
                eyes.transform.localPosition=eyeBase+new Vector3(-.52f*(1-rise),-.45f*(1-rise),0);
                eyes.transform.localRotation=Quaternion.Euler(Mathf.Lerp(24,0,rise),Mathf.Lerp(-18,0,rise),Mathf.Lerp(-9,0,rise));
                eyes.fieldOfView=Mathf.Lerp(54,63,rise);return;
            }
            if(introLine==0&&elapsed>12&&stage==0){introLine=1;Show("집으로 가는 열차를 기다리고 있었다.\n기억나는 것은 거기까지다.",7);}
            if(introLine==1&&elapsed>26&&stage==0&&!SubtitlesBusy){introLine=2;Show("객차에는 아무도 없다.\n좌석 옆에 빨간 승차권이 놓여 있다.",6);}
            if(train&&train.arrived&&!arrivalSaid){arrivalSaid=true;Show("THE FIELD 역에 도착했다.\n저 집은 어디선가 본 것 같다.",8);}
            if(!scriptedMove.HasValue){targetYaw+=Input.GetAxisRaw("Mouse X")*JourneyPreferences.Sensitivity;targetPitch=Mathf.Clamp(targetPitch-Input.GetAxisRaw("Mouse Y")*JourneyPreferences.Sensitivity,-77,77);}
            yaw=Mathf.LerpAngle(yaw,targetYaw,1-Mathf.Exp(-22*dt));pitch=Mathf.Lerp(pitch,targetPitch,1-Mathf.Exp(-22*dt));transform.rotation=Quaternion.Euler(0,yaw,0);
            Vector2 input=scriptedMove??new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));input=Vector2.ClampMagnitude(input,1);
            bool hurry=scriptedHurry||Input.GetKey(KeyCode.LeftShift);float walkSpeed=hurry?3.8f:2.15f;
            zooming=scriptedZoom??(Input.GetMouseButton(0)&&!focusedDetail);if(zooming)walkSpeed*=.65f;
            movement=Vector3.SmoothDamp(movement,new Vector3(input.x,0,input.y)*walkSpeed,ref moveDamp,.13f,20,dt);
            Vector3 beforeWalk=transform.position;
            controller.Move((transform.TransformDirection(movement)+Vector3.down*4)*dt);
            Vector3 walked=transform.position-beforeWalk;walked.y=0;float speed=walked.magnitude/Mathf.Max(.001f,dt);
            if(speed>.12f&&controller.isGrounded){bobPhase+=speed*dt*(Mathf.PI/1.12f);}
            float amount=Mathf.Clamp01(speed/2.5f)*(zooming?.3f:1);
            float trainAmount=train?train.speed/13:0;
            Vector3 bob=new Vector3(Mathf.Sin(bobPhase)*.027f,-Mathf.Cos(bobPhase*2)*.046f,0)*amount;
            bool inside=Mathf.Abs(transform.position.x)<2.4f&&Mathf.Abs(transform.position.z)<28.5f;
            if(inside)bob+=new Vector3(Mathf.Sin(Time.time*2.7f)*.003f,Mathf.Sin(Time.time*10.3f)*.0015f,0)*trainAmount;
            eyes.transform.localPosition=Vector3.Lerp(eyes.transform.localPosition,eyeBase+bob,1-Mathf.Exp(-14*dt));
            float roll=Mathf.Sin(bobPhase)*.72f*amount+(inside?Mathf.Sin(Time.time*1.8f)*.13f*trainAmount:0);
            eyes.transform.localRotation=Quaternion.Euler(pitch,0,roll);eyes.fieldOfView=Mathf.SmoothDamp(eyes.fieldOfView,zooming?38:63,ref fovVelocity,.18f,150,dt);
            if(!MapRevealed&&stage>=1&&train&&train.arrived&&transform.position.x>2.8f){MapRevealed=true;MapTitleAge=0;soundscape.EnterField();}
            if(stage==1&&transform.position.x>8)stage=2;
            if(stage==4&&inside&&Mathf.Abs(transform.position.x)<1.6f){stage=5;Show("사진 속 사람들은 누구였을까.\n열차가 다시 움직이기 시작한다.",8);if(train)train.Depart();}
            focusedDetail=FindInteraction();hint=focusedDetail?"조사하기 · "+focusedDetail.label:"";
            if(focusedDetail&&(Input.GetKeyDown(KeyCode.E)||Input.GetMouseButtonDown(0)))Examine(focusedDetail);
            bool mayExit=(!deadline||!deadline.missedTrain)&&stage>0&&(!train||train.arrived&&!train.departing);
            if(soundscape)soundscape.SetDoor(mayExit);
            if(slidingDoor)slidingDoor.localPosition=Vector3.Lerp(slidingDoor.localPosition,mayExit?doorOffset:Vector3.zero,1-Mathf.Exp(-2.5f*dt));
            if(transform.position.y< -4)ResetPose(new Vector3(0,.15f,-7.7f));
        }
        public void Examine(JourneyDetail detail)
        {
            if(detail.key=="coach-door"||detail.key=="coach-return-door"){GetComponent<CarriageConnection>().Toggle();return;}
            if(housePuzzle&&housePuzzle.Handle(detail))return;
            Show(detail.text,9);
            if(detail.key=="ticket"&&stage==0){TicketRead=true;stage=1;introLine=2;Show("UNKNOWN → HOME\nHOME 위로 검은 선이 그어져 있다.\n어디로 가는 표였을까.",11);}
            if(detail.key=="radio"&&stage>=2&&stage<4){stage=3;if(soundscape)soundscape.PlayRadio();Show("잡음이 잠깐 끊겼다.\n문득, 위층 벽에 걸린 사진이 떠오른다.\n왜 그 사진이 생각났지?",10);}
            if(detail.key=="photo"&&stage==3){stage=4;Show("얼굴은 기억나지 않는다. 그런데 이 집의 냄새는 기억난다.\n사진 속 아이의 손에도 빨간 승차권이 있다.\n이 기억을 가지고 열차로 돌아가야겠다.",11);}
        }
        public void PresentSubtitle(string text,float seconds=7){Show(text,seconds);}
        public JourneyDetail FindInteraction()
        {
            JourneyDetail best=null;float bestScore=-1;
            foreach(var d in FindObjectsByType<JourneyDetail>(FindObjectsSortMode.None))
            {
                if(!d.gameObject.activeInHierarchy)continue;
                var collider=d.GetComponent<Collider>();Vector3 target=collider?collider.bounds.center:d.transform.position;
                Vector3 offset=target-eyes.transform.position;float distance=offset.magnitude;
                if(distance>2.4f||distance<.01f||!collider)continue;
                float alignment=Vector3.Dot(eyes.transform.forward,offset/distance);if(alignment<.985f)continue;
                // A small assist around the actual shape, not a broad cone around every nearby object.
                var ray=new Ray(eyes.transform.position,eyes.transform.forward);var aimBounds=collider.bounds;aimBounds.Expand(.07f);
                if(!aimBounds.IntersectRay(ray,out float aimDistance)||aimDistance>2.4f)continue;
                bool blocked=false;
                foreach(var hit in Physics.RaycastAll(eyes.transform.position,offset.normalized,distance-.12f))
                {
                    if(hit.collider.GetComponentInParent<JourneyDetail>()==d||hit.collider.transform.IsChildOf(transform))continue;
                    if(hit.distance<distance-.35f){blocked=true;break;}
                }
                if(!blocked&&alignment-distance*.018f>bestScore){best=d;bestScore=alignment-distance*.018f;}
            }
            return best;
        }
        void Show(string text,float seconds=7)
        {
            subtitleLines.Clear();
            foreach(var paragraph in text.Split('\n'))
            {
                string line="";
                foreach(var word in paragraph.Split(' '))
                {
                    if(line.Length+word.Length>34&&line.Length>0){subtitleLines.Enqueue(line);line="";}
                    line+=(line.Length>0?" ":"")+word;
                }
                if(line.Length>0)subtitleLines.Enqueue(line);
            }
            NextSubtitle();
        }
        void NextSubtitle()
        {
            subtitleLine=subtitleLines.Count>0?subtitleLines.Dequeue():"";subtitleAge=0;lastSubtitleLetters=0;VisibleSubtitle="";
        }
        void UpdateSubtitle()
        {
            if(subtitleLine.Length==0)return;subtitleAge+=Time.deltaTime;
            int letters=Mathf.Clamp(Mathf.FloorToInt(subtitleAge*12),0,subtitleLine.Length);
            VisibleSubtitle=subtitleLine.Substring(0,letters);
            if(letters/3>lastSubtitleLetters/3&&soundscape)soundscape.SubtitleTick();lastSubtitleLetters=letters;
            if(subtitleAge>subtitleLine.Length/12f+3.0f)NextSubtitle();
        }
        void OnGUI()
        {
            if(GetComponent<JourneyInterface>())return;
            float scale=Mathf.Clamp(Screen.height/900f,.7f,1.6f);GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);float w=Screen.width/scale,h=Screen.height/scale;
            if(label==null){var font=Font.CreateDynamicFontFromOSFont("Malgun Gothic",18);label=new GUIStyle(GUI.skin.label){font=font,fontSize=15,wordWrap=true,normal={textColor=new Color(.96f,.92f,.80f)}};center=new GUIStyle(label){alignment=TextAnchor.MiddleCenter};heading=new GUIStyle(center){fontSize=27};subtitleStyle=new GUIStyle(center){fontSize=20};}
            string goal=objectives[Mathf.Clamp(stage,0,objectives.Length-1)];
            if(stage==1&&train&&!train.arrived)goal="승차권에 그려진 집은 다음 역에 있다. 정차하기를 기다리세요.";
            if(!IsWaking)GUI.Label(new Rect(28,25,w-56,50),goal,label);
            if(!IsWaking){GUI.Label(new Rect(w/2-10,h/2-10,20,20),focusedDetail?"◇":"·",center);GUI.Label(new Rect(w/2-250,h/2+27,500,28),hint,center);}
            if(!string.IsNullOrEmpty(VisibleSubtitle))
            {
                var old=GUI.color;GUI.color=new Color(0,0,0,.35f);GUI.DrawTexture(new Rect(w/2-390,h-158,780,105),Texture2D.whiteTexture);GUI.color=old;GUI.Label(new Rect(w/2-375,h-153,750,95),VisibleSubtitle,subtitleStyle);
            }
            if(paused)
            {
                GUI.Box(new Rect(w/2-300,h/2-185,600,370),"");GUI.Label(new Rect(w/2-285,h/2-160,570,45),"NO DESTINATION",heading);
                GUI.Label(new Rect(w/2-270,h/2-105,540,90),"집으로 가던 기억은 있지만, 승차권의 목적지는 지워져 있다.\n각 역에서 잃어버린 기억을 찾아 HOME이 어디인지 알아내야 한다.\n\n"+goal,center);
                if(GUI.Button(new Rect(w/2-120,h/2+22,240,40),"계속하기"))SetPaused(false);
                if(GUI.Button(new Rect(w/2-120,h/2+78,240,40),"처음부터"))
                {
                    Time.timeScale=1;AudioListener.pause=false;
#if UNITY_EDITOR
                    UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(SceneManager.GetActiveScene().path,new LoadSceneParameters(LoadSceneMode.Single));
#else
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#endif
                }
            }
            if(IsWaking)
            {
                float opening=Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-1.2f)/2.1f));
                float blinkClose=Mathf.Exp(-Mathf.Pow((elapsed-3.5f)/.22f,2))*.68f;
                float lid=Mathf.Clamp01(1-opening+blinkClose)*h*.5f;
                var old=GUI.color;GUI.color=Color.black;
                for(int column=0;column<256;column++){float u=(column+.5f)/128-1;float curved=Mathf.Min(h*.5f,lid*(1+.32f*u*u));GUI.DrawTexture(new Rect(column*w/256,0,w/256+1,curved),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(column*w/256,h-curved,w/256+1,curved),Texture2D.whiteTexture);}GUI.color=old;
            }
            if(train&&train.blink>0){var old=GUI.color;GUI.color=new Color(0,0,0,train.blink);GUI.DrawTexture(new Rect(0,0,w,h),Texture2D.whiteTexture);GUI.color=old;}
            if(MapRevealed&&MapTitleAge<8)
            {
                float alpha=Mathf.SmoothStep(0,1,Mathf.Clamp01(MapTitleAge/1.5f))*Mathf.Clamp01((8-MapTitleAge)/2.5f);
                var old=GUI.color;GUI.color=new Color(.04f,.055f,.045f,alpha*.23f);GUI.DrawTexture(new Rect(0,0,w,h),Texture2D.whiteTexture);
                GUI.color=new Color(1,.96f,.82f,alpha);var title=new GUIStyle(center){fontSize=Mathf.RoundToInt(w*.11f),fontStyle=FontStyle.Normal};
                GUI.Label(new Rect(0,h*.29f,w,h*.30f),"THE FIELD",title);
                GUI.Label(new Rect(0,h*.58f,w,45),"첫 번째 기억  ·  돌아오지 않는 오후",new GUIStyle(center){fontSize=23});GUI.color=old;
            }
        }
        void OnDestroy(){Time.timeScale=1;AudioListener.pause=false;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
    }
}


