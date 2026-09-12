using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    internal static class TrainWorldBuilder
    {
        const string Root="Assets/NoDestination/VisualRebuild";
        static Material M(string n)=>VisualWorkshop.GetMaterial(n);
        static GameObject Box(string n,Vector3 p,Vector3 s,string m,Transform parent,bool collider=false)=>VisualWorkshop.Cube(n,p,s,M(m),parent,collider);
        static Transform Find(Transform root,string name)=>root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==name);
        static void Text(string text,Vector3 position,float size,Transform parent,Quaternion rotation)
        {
            var go=new GameObject("Lettering / "+text);go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localRotation=rotation;var tm=go.AddComponent<TextMesh>();tm.text=text;tm.fontSize=64;tm.characterSize=size;tm.anchor=TextAnchor.MiddleCenter;tm.color=text.StartsWith("UNKNOWN —")?new Color(.10f,.14f,.11f):new Color(.84f,.78f,.58f);
        }
        public static void Enhance(Transform root,GameObject carriage,VisualJourney journey)
        {
            var station=new List<GameObject>();
            // The right vestibule now opens onto a lateral platform.
            foreach(var t in root.GetComponentsInChildren<Transform>(true).ToArray())
            {
                if(t.name=="Window collision"&&t.localPosition.x>0)UnityEngine.Object.DestroyImmediate(t.gameObject);
                else if(t.name=="Glazing / faint blue reflection"&&t.localPosition.x>0&&t.localPosition.z>6.5f)UnityEngine.Object.DestroyImmediate(t.gameObject);
                else if((t.name=="Exterior / green enamel skirting"||t.name=="Exterior / brass pinstripe")&&t.localPosition.x>0)UnityEngine.Object.DestroyImmediate(t.gameObject);
                else if(t.name=="Platform / edge tile")UnityEngine.Object.DestroyImmediate(t.gameObject);
            }
            foreach(var segment in new[]{new Vector2(-9,6.76f),new Vector2(8.24f,9)})
            {
                float z=(segment.x+segment.y)*.5f,length=segment.y-segment.x;
                Box("Right side / enamel",new Vector3(2.32f,.47f,z),new Vector3(.10f,.92f,length),"Velvet",root,true);
                Box("Right side / pinstripe",new Vector3(2.38f,.80f,z),new Vector3(.015f,.025f,length),"Brass",root);
                var wall=new GameObject("Vestibule-side collision");wall.transform.SetParent(root,false);wall.transform.localPosition=new Vector3(2.22f,1.7f,z);wall.AddComponent<BoxCollider>().size=new Vector3(.06f,1.5f,length);
            }
            var sideDoor=VisualWorkshop.Import(Root+"/Models/VestibuleDoor.fbx",Vector3.zero,root);sideDoor.name="The side door / opens after arrival";
            foreach(var r in sideDoor.GetComponentsInChildren<Renderer>())if(r.name.Contains("lower door panel")||r.name.Contains("upper glass"))r.gameObject.AddComponent<BoxCollider>();
            journey.slidingDoor=sideDoor.transform;journey.doorOffset=new Vector3(0,0,1.52f);
            // Complete train silhouette with contiguous couplers and bogies.
            var consist=new GameObject("Train / complete consist").transform;consist.SetParent(root,false);
            var rear=VisualWorkshop.Import(Root+"/Models/PassengerCarExterior.fbx",new Vector3(0,0,-19.55f),consist);rear.name="Car 03 / sleeping car";
            var front=VisualWorkshop.Import(Root+"/Models/PassengerCarExterior.fbx",new Vector3(0,0,19.55f),consist);front.name="Car 01 / dining car";
            var locomotive=VisualWorkshop.Import(Root+"/Models/Locomotive317.fbx",new Vector3(0,0,35.9f),consist);locomotive.name="Locomotive 317";
            Box("Coupling / forward connection",new Vector3(0,-.15f,29.9f),new Vector3(.25f,.2f,1.5f),"Ink",consist);
            // Rail profile, sleepers and clips remain visible under the whole train.
            var track=new GameObject("Railway / continuous line").transform;track.SetParent(root,false);
            Box("Track / ballast bed",new Vector3(0,-.72f,50),new Vector3(4.0f,.12f,420),"Path",track);
            for(int side=-1;side<=1;side+=2)
            {
                Box("Rail / head",new Vector3(side*1.22f,-.50f,50),new Vector3(.085f,.07f,420),"Brass",track);
                Box("Rail / web",new Vector3(side*1.22f,-.565f,50),new Vector3(.036f,.07f,420),"Ink",track);
                Box("Rail / foot",new Vector3(side*1.22f,-.615f,50),new Vector3(.15f,.035f,420),"Ink",track);
            }
            var sleepers=new GameObject("Railway / moving sleepers").transform;sleepers.SetParent(track,false);
            for(int i=0;i<620;i++)
            {
                float z=-150+i*.65f;Box("Sleeper",new Vector3(0,-.66f,z),new Vector3(3.2f,.09f,.20f),"Walnut",sleepers);
                for(int side=-1;side<=1;side+=2)Box("Rail fastening",new Vector3(side*1.22f,-.596f,z),new Vector3(.24f,.018f,.23f),"Ink",sleepers);
            }
            StaticBatchingUtility.Combine(sleepers.gameObject);
            // Lower the field to reveal wheels, suspension and the height of the platform.
            var earth=Find(root,"Field earth");earth.localPosition+=Vector3.down*.75f;
            var platform=Find(root,"Platform / worn stone");platform.localPosition=new Vector3(5,-.045f,5);platform.localScale=new Vector3(5.5f,.08f,18);station.Add(platform.gameObject);
            for(int i=0;i<47;i++)station.Add(Box("Platform / individual edge tile",new Vector3(2.44f,.015f,-3.8f+i*.38f),new Vector3(.27f,.025f,.35f),"Paper",root));
            for(int i=0;i<4;i++)station.Add(Box("Platform / exit step",new Vector3(7.94f+i*.37f,-.1f-i*.18f,12.7f),new Vector3(.4f,.2f,2.5f),"Ivory",root,true));
            var oldPath=Find(root,"The path to the house");UnityEngine.Object.DestroyImmediate(oldPath.gameObject);
            Vector3 begin=new Vector3(9.2f,-.736f,12.7f),end=new Vector3(22,-.736f,60.2f);var path=Box("Field / footpath",(begin+end)*.5f,new Vector3(2.6f,.026f,Vector3.Distance(begin,end)),"Path",root,true);path.transform.rotation=Quaternion.LookRotation(end-begin);station.Add(path);
            var house=Find(root,"03 / House 317");house.position+=new Vector3(22,-.75f,0);station.Add(house.gameObject);
            foreach(string n in new[]{"House / physical floor","House / opaline lamp","Radio interaction","Photo interaction"})
            {var t=Find(root,n);t.position+=new Vector3(22,-.75f,0);station.Add(t.gameObject);}
            // Properly aged photograph texture replaces the temporary geometric silhouettes.
            foreach(var r in house.GetComponentsInChildren<Renderer>())if(r.name.StartsWith("Photograph / figure")||r.name.StartsWith("Photograph / unfocused")||r.name.StartsWith("Photograph / faded"))r.gameObject.SetActive(false);
            var photoMat=new Material(Shader.Find("Universal Render Pipeline/Lit"));photoMat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/FamilyMemory.png"));photoMat.SetFloat("_Smoothness",.14f);
            string photoPath=Root+"/Materials/FamilyMemory.mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(photoPath);if(existing){EditorUtility.CopySerialized(photoMat,existing);UnityEngine.Object.DestroyImmediate(photoMat);photoMat=existing;EditorUtility.SetDirty(existing);}else AssetDatabase.CreateAsset(photoMat,photoPath);
            var photograph=GameObject.CreatePrimitive(PrimitiveType.Quad);photograph.name="Memory / aged family print";photograph.transform.SetParent(house,true);photograph.transform.position=new Vector3(23.4f,1.15f,68.815f);photograph.transform.rotation=Quaternion.identity;photograph.transform.localScale=new Vector3(.82f,.615f,1);photograph.GetComponent<Renderer>().sharedMaterial=photoMat;UnityEngine.Object.DestroyImmediate(photograph.GetComponent<Collider>());
            // Station sign and bench make the stop a place, rather than an isolated slab.
            var sign=new GameObject("Station / THE FIELD").transform;sign.SetParent(root,false);station.Add(sign.gameObject);
            for(int side=-1;side<=1;side+=2)Box("Sign / post",new Vector3(6.6f,1.05f,side*.85f+2),new Vector3(.10f,2.1f,.10f),"Walnut",sign);
            Box("Sign / enamel board",new Vector3(6.6f,1.92f,2),new Vector3(.10f,.45f,2.3f),"Velvet",sign);
            Text("THE FIELD",new Vector3(6.535f,1.92f,2),.11f,sign,Quaternion.Euler(0,90,0));
            for(int i=0;i<4;i++)Box("Bench / wooden slat",new Vector3(6.6f,.54f,-1.2f+i*.16f),new Vector3(.48f,.07f,.12f),"Walnut",sign);
            // Physical route map and printed ticket details invite closer inspection.
            Box("Route map / frame",new Vector3(1.45f,1.75f,-8.85f),new Vector3(1.12f,.64f,.045f),"Brass",root);
            Box("Route map / paper",new Vector3(1.45f,1.75f,-8.82f),new Vector3(1.06f,.58f,.012f),"Paper",root);
            Text("UNKNOWN — THE FIELD\nSUMMER — THE SEA\nTHE HOUSE — YESTERDAY\nNOWHERE — HOME",new Vector3(1.45f,1.75f,-8.807f),.031f,root,Quaternion.Euler(0,180,0));
            Text("UNKNOWN\nHOME\n03 : 17",new Vector3(-1.07f,.94f,-7.35f),.013f,root,Quaternion.Euler(90,0,0));
            Box("Ticket / cancelled destination",new Vector3(-1.07f,.942f,-7.35f),new Vector3(.063f,.001f,.002f),"Ink",root);
            // Repeating countryside moves while the player's carriage remains the reference frame.
            var scenery=new List<Transform>();
            for(int i=0;i<12;i++)
            {
                var pole=new GameObject("Passing countryside / telegraph pole").transform;pole.SetParent(root,false);pole.localPosition=new Vector3(9,0,-80+i*25);
                Box("Telegraph / timber",new Vector3(0,2,0),new Vector3(.17f,5.4f,.17f),"Walnut",pole);
                Box("Telegraph / crossarm",new Vector3(0,4.2f,0),new Vector3(2.4f,.10f,.10f),"Walnut",pole);
                for(int j=-1;j<=1;j++)Box("Telegraph / insulator",new Vector3(j*.82f,4.35f,0),new Vector3(.11f,.2f,.11f),"Ceramic",pole);
                scenery.Add(pole);
            }
            foreach(var p in new[]{new Vector3(-38,-.75f,40),new Vector3(46,-.75f,130),new Vector3(-43,-.75f,215)})
            {
                var repeat=VisualWorkshop.Import(Root+"/Models/FieldHouse.fbx",p,root);repeat.name="Passing countryside / the same house";scenery.Add(repeat.transform);
            }
            var motion=root.gameObject.AddComponent<TrainDreamMotion>();motion.wheatPatches=root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Wheat / wind patch")).ToArray();motion.passingScenery=scenery.ToArray();motion.stationObjects=station.ToArray();motion.sleeperRoot=sleepers;motion.announcement=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/FieldAnnouncement.wav");journey.train=motion;journey.radioClip=motion.announcement;
            journey.objectives=new[]{"좌석 옆에 놓인 빨간 승차권을 확인하세요.","THE FIELD에 정차하면 옆문으로 내려, 집을 찾아가세요.","집 안의 라디오에서 들리는 목소리를 확인하세요.","라디오가 말한 가족사진을 조사하세요.","찾은 기억을 가지고 열차로 돌아가세요.","첫 번째 기억을 찾았습니다. 열차는 다음 역으로 향합니다."};
        }
    }
}

