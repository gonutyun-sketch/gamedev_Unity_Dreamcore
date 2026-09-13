using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    internal static class FieldChapterPolish
    {
        const string Root="Assets/NoDestination/VisualRebuild";static Transform scene;
        static GameObject Box(string n,Vector3 p,Vector3 s,string m,bool collision=false)=>VisualWorkshop.Cube(n,p,s,VisualWorkshop.GetMaterial(m),scene,collision);
        static Material AssetMaterial(string n,string shader,Color c)
        {
            string path=Root+"/Materials/"+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}m.shader=Shader.Find(shader);m.SetColor("_BaseColor",c);EditorUtility.SetDirty(m);return m;
        }
        public static void Apply(Transform root,VisualJourney player)
        {
            scene=root;var spatial=player.gameObject.AddComponent<FieldSpatialRules>();spatial.player=player;
            spatial.houseParts=root.GetComponentsInChildren<Transform>(true).Where(t=>new[]{"03 / House 317","House / physical floor","House / opaline lamp","Radio interaction","Photo interaction","House / second floor and connected staircase"}.Contains(t.name)).ToArray();
            spatial.path=root.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Field / footpath");
            var wornPath=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/WornFootpath.mat");if(wornPath)spatial.path.GetComponent<Renderer>().sharedMaterial=wornPath;
            var crowField=new GameObject("FIELD / patient scarecrows");crowField.transform.SetParent(root,false);
            var controller=player.gameObject.AddComponent<ScarecrowField>();controller.player=player;var crows=new List<Transform>();
            foreach(var p in new[]{new Vector3(-16,-.75f,28),new Vector3(34,-.75f,42),new Vector3(8,-.75f,86),new Vector3(40,-.75f,112),new Vector3(-24,-.75f,133)})
            {
                var crow=VisualWorkshop.Import(Root+"/Models/FieldScarecrow.fbx",p,crowField.transform);crow.name="Scarecrow / linen and straw";crow.transform.rotation=Quaternion.Euler(0,180,0);crows.Add(crow.transform);
            }
            controller.crows=crows.ToArray();player.train.stationObjects=player.train.stationObjects.Append(crowField).ToArray();
            // Continuous distant ground, not a fog curtain hiding a finite tile.
            var far=new GameObject("FIELD / distant wheat terrain to the horizon");far.transform.SetParent(root,false);
            var vertices=new List<Vector3>();var indices=new List<int>();const int N=96;const float span=2400;
            for(int z=0;z<=N;z++)for(int x=0;x<=N;x++)
            {
                float wx=-span/2+x*span/N,wz=-span/2+z*span/N+60;
                float height=-.79f+Mathf.SmoothStep(0,1,Mathf.InverseLerp(180,500,new Vector2(wx,wz-60).magnitude))*(Mathf.Sin(wx*.008f)*Mathf.Sin(wz*.005f)*1.6f);
                vertices.Add(new Vector3(wx,height,wz));
            }
            for(int z=0;z<N;z++)for(int x=0;x<N;x++)
            {
                float wx=-span/2+(x+.5f)*span/N,wz=-span/2+(z+.5f)*span/N+60;if(Mathf.Abs(wx)<88&&Mathf.Abs(wz-60)<88)continue;
                int a=z*(N+1)+x;indices.AddRange(new[]{a,a+N+1,a+1,a+1,a+N+1,a+N+2});
            }
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            string meshPath=Root+"/Meshes/FieldHorizon.asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(existing){existing.Clear();existing.vertices=mesh.vertices;existing.triangles=mesh.triangles;existing.normals=mesh.normals;existing.bounds=mesh.bounds;existing.UploadMeshData(false);Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(mesh);}else AssetDatabase.CreateAsset(mesh,meshPath);
            far.AddComponent<MeshFilter>().sharedMesh=mesh;far.AddComponent<MeshRenderer>().sharedMaterial=AssetMaterial("FieldHorizon","NoDestination/FieldContinuum",new Color(.70f,.56f,.29f));
            player.eyes.farClipPlane=1800;
            // Window glass closes the upper apertures without blue sky painted onto an opaque pane.
            var glass=VisualWorkshop.GetMaterial("WindowGlass");glass.SetColor("_BaseColor",new Color(.82f,.82f,.75f,.038f));
            foreach(int floor in new[]{0,1})foreach(int side in new[]{-1,1})
            {
                var pane=VisualWorkshop.Cube("House / real side glazing",new Vector3(22+side*4.4f,1.09f+floor*3.2f,65),new Vector3(.018f,1.5f,3.8f),glass,spatial.houseParts.First(t=>t.name.Contains("second floor")),true);
                // Parent has identity at creation; glass moves with the whole house on return-path change.
            }
            var station=new GameObject("Station / lived-in small details");station.transform.SetParent(root,false);scene=station.transform;
            foreach(float z in new[]{-2.8f,10.5f})
            {
                Box("Station / lamp foundation",new Vector3(7,.04f,z),new Vector3(.48f,.12f,.48f),"Ivory",true);
                Box("Station / cast iron lamp post",new Vector3(7,1.48f,z),new Vector3(.08f,2.8f,.08f),"Ink",true);
                Box("Station / lamp canopy",new Vector3(7,2.94f,z),new Vector3(.46f,.1f,.46f),"Velvet");
                Box("Station / opaline lamp",new Vector3(7,2.77f,z),new Vector3(.25f,.26f,.25f),"MilkGlass");
                for(int side=-1;side<=1;side+=2)Box("Station / lamp frame",new Vector3(7+side*.16f,2.78f,z),new Vector3(.02f,.31f,.02f),"Ink");
            }
            for(int i=0;i<6;i++)Box("Station / fence post",new Vector3(7.7f,.55f,-2+i*1.8f),new Vector3(.07f,1.1f,.07f),"Walnut",true);
            foreach(float y in new[]{.36f,.78f})Box("Station / fence rail",new Vector3(7.7f,y,2.5f),new Vector3(.065f,.06f,9.2f),"Panel");
            foreach(float z in new[]{-1.18f,-.72f})foreach(float x in new[]{6.43f,6.77f})Box("Station / bench cast leg",new Vector3(x,.27f,z),new Vector3(.07f,.50f,.07f),"Ink");
            Box("Station / maintenance cabinet",new Vector3(7.05f,.55f,5.4f),new Vector3(.70f,1.10f,.40f),"Velvet",true);
            for(int i=0;i<7;i++)Box("Station / cabinet ventilation",new Vector3(6.692f,.65f,5.18f+i*.068f),new Vector3(.015f,.018f,.042f),"Ink");
            Box("Station / enamel sign rim",new Vector3(6.645f,1.92f,2),new Vector3(.06f,.51f,2.36f),"Brass");
            foreach(float z in new[]{.96f,3.04f})Box("Station / sign fixing bolt",new Vector3(6.525f,1.92f,z),new Vector3(.025f,.045f,.045f),"Brass");
            var rng=new System.Random(317);
            for(int i=0;i<120;i++)
            {
                float u=(float)rng.NextDouble(),x=Mathf.Lerp(9.2f,22,u)+((float)rng.NextDouble()-.5f)*2.4f,z=Mathf.Lerp(12.7f,60.2f,u);float size=.025f+(float)rng.NextDouble()*.065f;
                Box("Field / scattered path pebble",new Vector3(x,-.71f,z),new Vector3(size,size*.4f,size*.7f),i%2==0?"Ivory":"Walnut");
            }
            player.train.stationObjects=player.train.stationObjects.Append(station).ToArray();
            spatial.pathPebbles=station.GetComponentsInChildren<Transform>().Where(t=>t.name=="Field / scattered path pebble").ToArray();
            Shader.SetGlobalVector("_FieldHouseCentre",new Vector4(22,65,5.5f,5.1f));Shader.SetGlobalVector("_FieldPath",new Vector4(9.2f,12.7f,22,60.2f));
        }
    }
}
