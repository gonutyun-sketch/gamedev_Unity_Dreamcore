using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
namespace NoDestination.VisualRebuild.Editor
{
    [InitializeOnLoad]
    public static class VisualWorkshop
    {
        const string Root="Assets/NoDestination/VisualRebuild";
        const string ScenePath=Root+"/TheGoldenField.unity";
        static readonly Dictionary<string,Material> mats=new();
        static double captureAt=-1;
        static VisualWorkshop(){EditorApplication.update+=Tick;}
        static void Tick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            try
            {
                if(File.Exists("Artifacts/VisualRebuild/polish.request")&&!EditorApplication.isPlaying)
                {
                    File.Delete("Artifacts/VisualRebuild/polish.request");
                    foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                        if(r.name.StartsWith("Front /")&&!r.GetComponent<Collider>())r.gameObject.AddComponent<BoxCollider>();
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                }
                if(File.Exists("Artifacts/VisualRebuild/ui-capture.request")&&EditorApplication.isPlaying)
                {
                    File.Delete("Artifacts/VisualRebuild/ui-capture.request");ScreenCapture.CaptureScreenshot("Artifacts/VisualRebuild/05-objective-ui.png");
                }
                if(File.Exists("Artifacts/VisualRebuild/inspect.request"))
                {
                    File.Delete("Artifacts/VisualRebuild/inspect.request");
                    var lines=new List<string>();
                    foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(r.name.StartsWith("Ticket /")||r.name.StartsWith("Radio / rounded")||r.name.StartsWith("Photograph / walnut")||r.name.StartsWith("House / back")||r.name.StartsWith("Door / inset"))lines.Add(r.name+" "+r.bounds.center+" parent="+r.transform.parent.name);
                    File.WriteAllLines("Artifacts/VisualRebuild/landmarks.txt",lines);
                }
                if(File.Exists("Artifacts/VisualRebuild/create.request")&&!EditorApplication.isPlaying)
                {
                    if(!File.Exists(Root+"/Models/CarriagePrintedDetails.fbx")||!File.Exists(Root+"/Models/FieldHouse.fbx")||!File.Exists(Root+"/Models/VestibuleDoor.fbx")||!File.Exists(Root+"/Textures/WheatStand.png"))return;
                    File.Delete("Artifacts/VisualRebuild/create.request");Create();
                }
                if(File.Exists("Artifacts/VisualRebuild/capture.request"))
                {File.Delete("Artifacts/VisualRebuild/capture.request");captureAt=EditorApplication.timeSinceStartup+5;}
                if(captureAt>0&&EditorApplication.timeSinceStartup>captureAt){captureAt=-1;CaptureViews();}
                if(File.Exists("Artifacts/VisualRebuild/play.request")&&!EditorApplication.isPlaying)
                {File.Delete("Artifacts/VisualRebuild/play.request");PreserveUnsaved();EditorSceneManager.OpenScene(ScenePath);EditorApplication.isPlaying=true;}
                if(File.Exists("Artifacts/VisualRebuild/stop.request")&&EditorApplication.isPlaying)
                {File.Delete("Artifacts/VisualRebuild/stop.request");EditorApplication.isPlaying=false;}
            }
            catch(Exception ex){File.WriteAllText("Artifacts/VisualRebuild/editor-error.txt",ex.ToString());Debug.LogException(ex);}
        }
        static void PreserveUnsaved()
        {
            for(int i=0;i<SceneManager.sceneCount;i++){var s=SceneManager.GetSceneAt(i);if(s.isDirty)EditorSceneManager.SaveScene(s,Root+"/Preserved-"+DateTime.Now.ToString("HHmmss")+"-"+i+".unity",true);}
        }
        internal static Material GetMaterial(string name)=>mats[name];
        static Material Material(string name,Color color,float metallic=0,float smooth=.25f)
        {
            var path=Root+"/Materials/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smooth);
            if(name is "Walnut" or "Panel" or "Velvet" or "Curtain" or "Carpet" or "Leather" or "Path" or "Earth")
            {
                string tp=Root+"/Textures/"+name+".png";
                if(!File.Exists(tp))
                {
                    var t=new Texture2D(256,256,TextureFormat.RGB24,false);var px=new Color[65536];var rng=new System.Random(317);
                    for(int y=0;y<256;y++)for(int x=0;x<256;x++)
                    {
                        float v;
                        if(name is "Walnut" or "Panel")v=.82f+.09f*Mathf.Sin(x*.34f+Mathf.Sin(y*.045f)*2)+.045f*Mathf.Sin(x*1.7f+y*.015f)+(float)rng.NextDouble()*.035f;
                        else if(name is "Path" or "Earth")v=.66f+Mathf.PerlinNoise(x*.07f,y*.07f)*.28f+(float)rng.NextDouble()*.16f; else v=.87f+((x+y)%2)*.035f+(float)rng.NextDouble()*.045f;
                        px[y*256+x]=new Color(v,v,v);
                    }
                    t.SetPixels(px);t.Apply();File.WriteAllBytes(tp,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(tp);
                }
                var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(tp);m.SetTexture("_BaseMap",tex);if(name is "Path" or "Earth")m.SetTextureScale("_BaseMap",new Vector2(3,55));
            }
            mats[name]=m;return m;
        }
        static void Materials()
        {
            Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Textures");Directory.CreateDirectory(Root+"/Meshes");
            Material("Walnut",new Color(.24f,.10f,.037f),0,.38f);Material("Panel",new Color(.36f,.17f,.075f),0,.36f);
            Material("Brass",new Color(.70f,.51f,.25f),.45f,.48f);Material("Velvet",new Color(.10f,.32f,.22f),0,.18f);
            Material("Piping",new Color(.29f,.37f,.2f),0,.18f);Material("Ivory",new Color(.76f,.70f,.53f),0,.32f);
            Material("Carpet",new Color(.22f,.115f,.075f),0,.08f);Material("Curtain",new Color(.24f,.33f,.23f),0,.1f);
            var glow=Material("MilkGlass",new Color(.94f,.86f,.68f),0,.3f);glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",new Color(1,.78f,.50f)*2.2f);
            Material("Leather",new Color(.36f,.15f,.055f),0,.38f);Material("Paper",new Color(.88f,.82f,.64f),0,.13f);
            Material("Ceramic",new Color(.69f,.80f,.70f),0,.64f);Material("Ink",new Color(.023f,.032f,.026f),0,.3f);
            Material("Earth",new Color(.75f,.59f,.30f),0,.08f);Material("Path",new Color(.79f,.68f,.47f),0,.1f);
            Material("TicketRed",new Color(.62f,.055f,.033f),0,.15f);
            var glass=Material("WindowGlass",new Color(.61f,.78f,.81f,.065f),.1f,.92f);
            glass.SetFloat("_Surface",1);glass.SetFloat("_ZWrite",0);glass.SetInt("_SrcBlend",5);glass.SetInt("_DstBlend",10);glass.SetInt("_Cull",0);glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glass.renderQueue=3000;glass.SetFloat("_SpecularHighlights",0);glass.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");glass.SetFloat("_EnvironmentReflections",0);glass.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
        }
        internal static GameObject Cube(string name,Vector3 pos,Vector3 scale,Material mat,Transform parent,bool collider=true)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;
            if(!collider)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        internal static GameObject Import(string path,Vector3 position,Transform parent)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!prefab)throw new Exception("Model is not imported: "+path);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab);PrefabUtility.UnpackPrefabInstance(go,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            go.transform.SetParent(parent,false);go.transform.position=position;go.transform.rotation=Quaternion.Euler(0,180,0);
            foreach(var r in go.GetComponentsInChildren<Renderer>())
            {
                var mm=r.sharedMaterials;
                for(int i=0;i<mm.Length;i++)
                {
                    string name=mm[i]?mm[i].name:"Walnut";
                    if(name.Contains(" ("))name=name.Substring(0,name.IndexOf(" (",StringComparison.Ordinal));
                    mm[i]=mats.TryGetValue(name,out var m)?m:mats["Walnut"];
                }
                r.sharedMaterials=mm;
                if(r.name.StartsWith("Ticket /"))r.sharedMaterial=mats["TicketRed"];
            }
            var bounds=new Bounds(position,Vector3.zero);foreach(var r in go.GetComponentsInChildren<Renderer>())bounds.Encapsulate(r.bounds);
            if(bounds.size.y>bounds.size.z*2)go.transform.rotation=Quaternion.Euler(-90,0,0);
            return go;
        }
        static JourneyDetail Detail(string name,string key,string title,string body,Vector3 p,Vector3 scale,Transform parent)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=p;
            var box=go.AddComponent<BoxCollider>();box.size=scale;
            var d=go.AddComponent<JourneyDetail>();d.key=key;d.label=title;d.text=body;return d;
        }
        [MenuItem("NO DESTINATION/Visual Rebuild/Create Golden Field")]
        public static void Create()
        {
            PreserveUnsaved();Materials();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("NO DESTINATION | Golden Field").transform;
            var carriage=Import(Root+"/Models/NoDestination_Carriage.fbx",Vector3.zero,root);carriage.name="01 / Blender carriage";
            var bounds=new Bounds(Vector3.zero,Vector3.zero);foreach(var r in carriage.GetComponentsInChildren<Renderer>())bounds.Encapsulate(r.bounds);
            File.WriteAllText("Artifacts/VisualRebuild/model-bounds.txt",bounds.ToString());
            var door=new GameObject("Sliding vestibule assembly").transform;door.SetParent(root,false);
            foreach(var r in carriage.GetComponentsInChildren<Renderer>())
            {
                string n=r.name;
                if(n.StartsWith("Door /")&&r.bounds.center.z>8){r.transform.SetParent(door,true);if(n.StartsWith("Door / inset"))r.gameObject.AddComponent<BoxCollider>();}
                else if(n.StartsWith("Wall / lower")||n.StartsWith("Bulkhead /")||n.StartsWith("Seat / spring")||n.StartsWith("Seat / upholstered")||n.StartsWith("Table / rounded"))r.gameObject.AddComponent<BoxCollider>();
            }
            Cube("Carriage / physical floor",new Vector3(0,-.08f,0),new Vector3(4.42f,.12f,18),mats["Walnut"],root);
            // Invisible safety surfaces coincide with the actual glazing.
            for(int side=-1;side<=1;side+=2)
            {
                var wall=new GameObject("Window collision");wall.transform.SetParent(root,false);wall.transform.localPosition=new Vector3(side*2.22f,1.7f,0);wall.AddComponent<BoxCollider>().size=new Vector3(.06f,1.5f,18);
                for(int i=0;i<7;i++)Cube("Glazing / faint blue reflection",new Vector3(side*2.15f,1.72f,-7.5f+i*2.5f),new Vector3(.007f,1.24f,2.12f),mats["WindowGlass"],root,false);
                Cube("Exterior / green enamel skirting",new Vector3(side*2.32f,.47f,0),new Vector3(.1f,.92f,18),mats["Velvet"],root);
                Cube("Exterior / brass pinstripe",new Vector3(side*2.38f,.8f,0),new Vector3(.015f,.026f,18),mats["Brass"],root,false);
                foreach(float z in new[]{-6.2f,-4.8f,4.8f,6.2f})
                {
                    var wheel=GameObject.CreatePrimitive(PrimitiveType.Cylinder);wheel.name="Bogie / steel wheel";wheel.transform.SetParent(root,false);wheel.transform.localPosition=new Vector3(side*1.22f,-.32f,z);wheel.transform.localRotation=Quaternion.Euler(0,0,90);wheel.transform.localScale=new Vector3(.38f,.09f,.38f);wheel.GetComponent<Renderer>().sharedMaterial=mats["Ink"];UnityEngine.Object.DestroyImmediate(wheel.GetComponent<Collider>());
                }
            }
            // Lamps use warm local light; the outside remains clean blue/gold.
            for(int i=0;i<7;i++)Point("Pendant light",new Vector3(0,2.48f,-7.5f+i*2.5f),new Color(1,.82f,.59f),5.5f,4.1f,root);
            RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.44f,.51f,.56f);RenderSettings.ambientEquatorColor=new Color(.42f,.38f,.30f);RenderSettings.ambientGroundColor=new Color(.22f,.19f,.14f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Exponential;RenderSettings.fogColor=new Color(.71f,.79f,.69f);RenderSettings.fogDensity=.0038f;
            var sky=new Material(Shader.Find("NoDestination/ClearSummerSky"));sky.SetColor("_Zenith",new Color(.065f,.34f,.72f));sky.SetColor("_Horizon",new Color(.50f,.76f,.87f));sky=SaveAsset(sky,Root+"/Materials/ClearSummerSky.mat");RenderSettings.skybox=sky;
            var sunGo=new GameObject("Sun / the afternoon never moves");sunGo.transform.SetParent(root,false);sunGo.transform.rotation=Quaternion.Euler(31,-52,0);var sun=sunGo.AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.90f,.70f);sun.intensity=1.75f;sun.shadows=LightShadows.Soft;sun.shadowBias=.035f;RenderSettings.sun=sun;
            var volume=new GameObject("Quiet colour grade").AddComponent<Volume>();volume.transform.SetParent(root);volume.isGlobal=true;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();profile=SaveAsset(profile,Root+"/GoldenFieldVolume.asset");volume.sharedProfile=profile;
            var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);
            var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.16f);bloom.threshold.Override(1.1f);bloom.scatter.Override(.45f);
            var grade=profile.Add<ColorAdjustments>(true);grade.contrast.Override(8);grade.saturation.Override(5);
            var vignette=profile.Add<Vignette>(true);vignette.intensity.Override(.13f);vignette.smoothness.Override(.55f);
            foreach(var component in profile.components)AssetDatabase.AddObjectToAsset(component,profile);
            var field=new GameObject("02 / Gold beneath a blue sky").transform;field.SetParent(root,false);
            Cube("Field earth",new Vector3(0,-.2f,55),new Vector3(300,.35f,320),mats["Earth"],field);
            Cube("Platform / worn stone",new Vector3(0,-.045f,12.5f),new Vector3(7,.08f,7),mats["Ivory"],field);
            for(int i=0;i<18;i++)Cube("Platform / edge tile",new Vector3(-3.3f+i*.39f,.005f,15.7f),new Vector3(.36f,.022f,.3f),mats["Paper"],field,false);
            Cube("The path to the house",new Vector3(0,-.012f,37.5f),new Vector3(2.7f,.04f,45),mats["Path"],field);
            Wheat(field);
            var house=Import(Root+"/Models/FieldHouse.fbx",new Vector3(0,0,65),root);house.name="03 / House 317";
            foreach(var r in house.GetComponentsInChildren<Renderer>())
            {
                string n=r.name;
                if(n.StartsWith("Front /")||n.StartsWith("House / front")||n.StartsWith("House / door")||n.StartsWith("House / side")||n.StartsWith("House / back")||n.StartsWith("House / beneath")||n.StartsWith("Table / top")||n.StartsWith("Porch / step"))r.gameObject.AddComponent<BoxCollider>();
            }
            Cube("House / physical floor",new Vector3(0,.25f,65),new Vector3(8.65f,.1f,8.15f),mats["Walnut"],root);
            Point("House / opaline lamp",new Vector3(0,2.63f,65),new Color(1,.75f,.43f),5,10,root);
            Detail("Red ticket interaction","ticket","승차권 확인","출발역: UNKNOWN\n목적지는 지워져 있다. 밀밭의 집으로 가보자.",new Vector3(-1.07f,.95f,-7.35f),new Vector3(.19f,.08f,.32f),root);
            Detail("Suitcase interaction","case","여행 가방 조사","누군가 곧 돌아올 것처럼 놓여 있다.",new Vector3(1.3f,.92f,-3.9f),new Vector3(.8f,.48f,.34f),root);
            Detail("Radio interaction","radio","라디오 맞추기","스피커에서 희미한 잡음이 들린다.",new Vector3(-2,1.48f,66.3f),new Vector3(1.02f,.68f,.40f),root);
            Detail("Photo interaction","photo","가족사진 조사","기억나지 않는 가족. 그런데 낯설지 않다.",new Vector3(1.4f,1.9f,68.86f),new Vector3(1.17f,.94f,.18f),root);
            var player=new GameObject("Player / first person");player.transform.SetParent(root);player.transform.position=new Vector3(-.05f,.12f,-7.7f);
            var cc=player.AddComponent<CharacterController>();cc.height=1.72f;cc.radius=.22f;cc.center=new Vector3(0,.86f,0);cc.stepOffset=.30f;
            var cameraGo=new GameObject("Eyes");cameraGo.transform.SetParent(player.transform,false);cameraGo.transform.localPosition=new Vector3(0,1.59f,0);
            var camera=cameraGo.AddComponent<Camera>();camera.tag="MainCamera";camera.fieldOfView=63;camera.nearClipPlane=.045f;camera.farClipPlane=350;camera.allowHDR=true;camera.clearFlags=CameraClearFlags.Skybox;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;camera.GetUniversalAdditionalCameraData().antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;cameraGo.AddComponent<AudioListener>();
            var journey=player.AddComponent<VisualJourney>();journey.eyes=camera;journey.slidingDoor=door;player.AddComponent<JourneyInterface>();
            foreach(var t in carriage.GetComponentsInChildren<Transform>())if(t.name.StartsWith("Seat bay L 03"))
            {
                // Reserved for a later, separately authored carriage distortion; never obstruct the opening aisle.
                journey.changedSeat=null;break;
            }
            TrainWorldBuilder.Enhance(root,carriage,journey);CraftAndAudio.Apply(root,journey);HouseChapterBuilder.Apply(root,journey);FieldChapterPolish.Apply(root,journey);FinishMaterials.Apply(root,journey);CarriageExpansion.Apply(root,journey);SeaChapterBuilder.Apply(root,journey);
            DynamicGI.UpdateEnvironment();
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);
            if(SceneView.lastActiveSceneView)SceneView.lastActiveSceneView.LookAt(new Vector3(0,1.5f,0),Quaternion.Euler(9,12,0),10);
            File.WriteAllText("Artifacts/VisualRebuild/scene-created.txt","Created "+ScenePath+"\nBlender models, persistent materials, static scene geometry, runtime first-person objectives.");
            captureAt=EditorApplication.timeSinceStartup+8;Debug.Log("ND VISUAL: Scene created");
        }
        static T SaveAsset<T>(T obj,string path) where T : UnityEngine.Object
        {
            var old=AssetDatabase.LoadAssetAtPath<T>(path);
            if(old)
            {
                if(old is Mesh destination && obj is Mesh source)
                {
                    destination.Clear();destination.indexFormat=source.indexFormat;destination.vertices=source.vertices;destination.colors=source.colors;destination.uv=source.uv;destination.triangles=source.triangles;destination.bounds=source.bounds;destination.UploadMeshData(false);
                }
                else EditorUtility.CopySerialized(obj,old);
                EditorUtility.SetDirty(old);UnityEngine.Object.DestroyImmediate(obj);return old;
            }
            else AssetDatabase.CreateAsset(obj,path);return obj;
        }
        static void Point(string name,Vector3 position,Color color,float power,float range,Transform parent)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;var l=go.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.intensity=power;l.range=range;l.shadows=LightShadows.None;
        }
        static void Wheat(Transform parent)
        {
            string texturePath=Root+"/Textures/WheatStand.png";
            var importer=AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if(importer){importer.alphaIsTransparency=true;importer.mipmapEnabled=true;importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=4;importer.SaveAndReimport();}
            var material=new Material(Shader.Find("NoDestination/LivingWheat"));material.SetColor("_BaseColor",new Color(1,.94f,.69f));material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));material.SetFloat("_Cutoff",.3f);material=SaveAsset(material,Root+"/Materials/LivingWheat.mat");
            var rng=new System.Random(317);
            for(int bx=-5;bx<=4;bx++)for(int bz=-2;bz<=7;bz++)
            {
                var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();var uv=new List<Vector2>();
                Vector3 origin=new Vector3(bx*20+10,-.75f,bz*20+10);
                for(int j=0;j<1550;j++)
                {
                    float x=bx*20+(float)rng.NextDouble()*20,z=bz*20+(float)rng.NextDouble()*20;
                    float t=Mathf.Clamp01((z-12.7f)/47.5f);float pathX=Mathf.Lerp(9.2f,22,t);
                    if(Mathf.Abs(x)<3.8f||x>2.2f&&x<9.5f&&z> -5&&z<15)continue;
                    float height=1.4f+(float)rng.NextDouble()*.42f,width=height*.75f;float tint=.84f+(float)rng.NextDouble()*.23f;float angle=(float)rng.NextDouble()*Mathf.PI;
                    for(int plane=0;plane<2;plane++)
                    {
                        float a=angle+plane*Mathf.PI*.5f;Vector3 side=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*width*.5f;Vector3 center=new Vector3(x-origin.x,0,z-origin.z);int k=vertices.Count;
                        vertices.Add(center-side);vertices.Add(center+side);vertices.Add(center+side+Vector3.up*height);vertices.Add(center-side+Vector3.up*height);
                        uv.AddRange(new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)});
                        for(int c=0;c<4;c++)colors.Add(new Color(tint,tint,tint,1));triangles.AddRange(new[]{k,k+2,k+1,k,k+3,k+2});
                    }
                }
                var mesh=new Mesh();mesh.name="Dense wheat stand "+bx+" "+bz;mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetColors(colors);mesh.SetUVs(0,uv);mesh.RecalculateBounds();mesh=SaveAsset(mesh,Root+"/Meshes/Wheat_"+bx+"_"+bz+".asset");
                var go=new GameObject("Wheat / wind patch "+bx+" "+bz);go.transform.SetParent(parent,false);go.transform.localPosition=origin;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
            }
        }
        [MenuItem("NO DESTINATION/Visual Rebuild/Capture Views")]
        public static void CaptureViews()
        {
            var camera=UnityEngine.Object.FindFirstObjectByType<VisualJourney>()?.eyes;if(!camera)throw new Exception("Open the visual rebuild scene first.");
            var position=camera.transform.position;var rotation=camera.transform.rotation;float aspect=camera.aspect;
            Capture(camera,new Vector3(.6f,1.40f,-7.7f),Quaternion.LookRotation(new Vector3(2,1.35f,-7.5f)-new Vector3(.6f,1.40f,-7.7f)),"08-window-craft");
            Capture(camera,new Vector3(1.45f,1.75f,-7.70f),Quaternion.Euler(0,180,0),"09-printed-route");
            Capture(camera,new Vector3(4.7f,1.92f,2),Quaternion.Euler(0,90,0),"10-station-lettering");
            Capture(camera,new Vector3(.05f,1.66f,-7.8f),Quaternion.Euler(1,0,0),"01-carriage");
            Capture(camera,new Vector3(.12f,1.47f,-6.65f),Quaternion.Euler(6,-63,0),"02-seat-detail");
            Capture(camera,new Vector3(11.6f,.92f,21),Quaternion.Euler(-1,14,0),"03-golden-field");
            Capture(camera,new Vector3(30,.92f,54.5f),Quaternion.Euler(-5,-37,0),"04-house");
            Capture(camera,new Vector3(58,7,-34),Quaternion.LookRotation(new Vector3(0,1,8)-new Vector3(58,7,-34)),"06-complete-train");
            Capture(camera,new Vector3(23.4f,4.32f,67.2f),Quaternion.identity,"07-family-memory");
            Capture(camera,new Vector3(22.6f,1.15f,61.5f),Quaternion.LookRotation(new Vector3(25.2f,2.4f,65.5f)-new Vector3(22.6f,1.15f,61.5f)),"11-house-staircase");
            Capture(camera,new Vector3(23.6f,4.32f,62.2f),Quaternion.LookRotation(new Vector3(20,3.8f,66.5f)-new Vector3(23.6f,4.32f,62.2f)),"12-upstairs-memory-room");
            Capture(camera,new Vector3(21.9f,4.25f,66.6f),Quaternion.LookRotation(new Vector3(21.9f,4.03f,68.6f)-new Vector3(21.9f,4.25f,66.6f)),"13-power-puzzle");
            Capture(camera,new Vector3(0,1.63f,11.4f),Quaternion.identity,"14-dining-carriage");
            camera.transform.SetPositionAndRotation(position,rotation);camera.aspect=aspect;
            File.WriteAllText("Artifacts/VisualRebuild/render-complete.txt",DateTime.Now.ToString("O"));
        }
        internal static void CaptureEvidence(Camera camera,Vector3 position,Quaternion rotation,string filename)
        {
            var before=camera.transform.position;var angle=camera.transform.rotation;float aspect=camera.aspect;
            Capture(camera,position,rotation,filename);camera.transform.SetPositionAndRotation(before,angle);camera.aspect=aspect;
        }
        static void Capture(Camera camera,Vector3 position,Quaternion rotation,string filename)
        {
            camera.transform.SetPositionAndRotation(position,rotation);camera.aspect=16f/9;
            var rt=new RenderTexture(1600,900,24,RenderTextureFormat.ARGB32);rt.Create();
            var request=new UniversalRenderPipeline.SingleCameraRequest{destination=rt};RenderPipeline.SubmitRenderRequest(camera,request);
            var prior=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1600,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();File.WriteAllBytes("Artifacts/VisualRebuild/"+filename+".png",image.EncodeToPNG());RenderTexture.active=prior;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);
        }
    }
}







