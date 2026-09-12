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
        GameObject watcher;float nextKnock;
        bool warned; Font failureFont;
        void Start()
        {
            originalSky=RenderSettings.skybox;sky=new Material(originalSky);RenderSettings.skybox=sky;
            zenith=sky.GetColor("_Zenith");horizon=sky.GetColor("_Horizon");sunColor=RenderSettings.sun.color;fogColor=RenderSettings.fogColor;
            // Faceless, distant shape; no human voice, attack, or sudden full-screen jump scare.
            watcher=new GameObject("Field / figure beyond the wheat");
            var silhouette=new Material(Shader.Find("Universal Render Pipeline/Lit"));silhouette.color=new Color(.018f,.016f,.02f);
            foreach(var part in new[]{new Vector4(0,1.05f,.47f,1.1f),new Vector4(0,1.82f,.31f,.31f)})
            {
                var body=GameObject.CreatePrimitive(PrimitiveType.Capsule);body.transform.SetParent(watcher.transform,false);body.transform.localPosition=new Vector3(part.x,part.y,0);body.transform.localScale=new Vector3(part.z,part.w*.5f,part.z);body.GetComponent<Renderer>().sharedMaterial=silhouette;Destroy(body.GetComponent<Collider>());
            }
            watcher.transform.position=new Vector3(31,-.75f,57);watcher.SetActive(false);
        }
        void Update()
        {
            if(player.paused)return;
            if(missedTrain){failureAge+=Time.deltaTime;if(player.slidingDoor)player.slidingDoor.localPosition=Vector3.Lerp(player.slidingDoor.localPosition,Vector3.zero,Time.deltaTime*3);return;}
            bool aboard=Mathf.Abs(player.transform.position.x)<1.7f&&Mathf.Abs(player.transform.position.z)<9;
            if(!aboard&&player.stage>=2&&player.stage<5&&player.train.arrived){if(visitElapsed==0)player.PresentSubtitle("하늘이 붉어지기 전에 열차로 돌아와야 한다.",7);visitElapsed+=Time.deltaTime;}
            float danger=player.stage==5?0:Danger;
            sky.SetColor("_Zenith",Color.Lerp(zenith,new Color(.24f,.012f,.023f),danger));sky.SetColor("_Horizon",Color.Lerp(horizon,new Color(.92f,.13f,.035f),danger));
            RenderSettings.sun.color=Color.Lerp(sunColor,new Color(1,.22f,.09f),danger);RenderSettings.fogColor=Color.Lerp(fogColor,new Color(.48f,.09f,.055f),danger);
            if(!warned&&danger>.32f){warned=true;player.PresentSubtitle("하늘이 붉어지고 있다.\n열차로 돌아가야 한다.",9);}
            if(danger>.65f&&player.stage<5&&!aboard)
            {
                if(!HorrorSeen){HorrorSeen=true;nextKnock=Time.time+14;player.soundscape.Knock();player.PresentSubtitle("창밖에 누군가 서 있었던 것 같다.\n분명 이 집에는 나 혼자였는데.");}
                Vector3 toward=watcher.transform.position+Vector3.up-player.eyes.transform.position;
                watcher.SetActive(Vector3.Dot(player.eyes.transform.forward,toward.normalized)<.985f);
                if(Time.time>nextKnock){player.soundscape.Knock();nextKnock=Time.time+18;}
                if(player.housePuzzle&&player.housePuzzle.Powered)foreach(var light in player.housePuzzle.lamps)light.intensity=Mathf.Sin(Time.time*17)> .86f?.12f:2.2f;
            }
            else if(watcher)watcher.SetActive(false);
            if(aboard&&LastCall){LastCall=false;LastCallAge=0;visitElapsed=allowedSeconds*.85f;}
            if(!aboard&&visitElapsed>=allowedSeconds&&player.stage<5)
            {
                if(!LastCall){LastCall=true;player.soundscape.Knock();player.PresentSubtitle("집 안에서 문 두드리는 소리가 들린다.\n열차의 불빛이 깜빡인다. 지금 돌아가야 한다.");}
                LastCallAge+=Time.deltaTime;
                if(LastCallAge<15)return;
                missedTrain=true;player.scriptedMove=Vector2.zero;player.soundscape.SetDoor(false);
                player.soundscape.Knock();player.PresentSubtitle("열차 문이 닫혔다.\n집의 시계가 다시 움직이기 시작했다.\n이 오후는 끝나지 않는다.",12);
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
        void OnDestroy(){if(watcher){var r=watcher.GetComponentInChildren<Renderer>();if(r)Destroy(r.sharedMaterial);Destroy(watcher);}if(originalSky)RenderSettings.skybox=originalSky;if(RenderSettings.sun)RenderSettings.sun.color=sunColor;RenderSettings.fogColor=fogColor;if(sky)Destroy(sky);}
    }
}

