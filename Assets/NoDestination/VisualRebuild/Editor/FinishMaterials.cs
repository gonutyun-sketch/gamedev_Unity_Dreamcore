using System.Linq;
using UnityEditor;
using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    internal static class FinishMaterials
    {
        const string Root="Assets/NoDestination/VisualRebuild";
        public static void Apply(Transform root,VisualJourney player)
        {
            var extension=root.GetComponentsInChildren<Transform>(true).First(t=>t.name=="House / second floor and connected staircase");
            foreach(var r in extension.GetComponentsInChildren<Renderer>(true))if(r.name.StartsWith("House / bed ")||r.name=="House / folded blanket"||r.name=="House / pillow")r.enabled=false;
            var furniture=VisualWorkshop.Import(Root+"/Models/HouseFurnishings.fbx",new Vector3(22,-.75f,65),extension);furniture.name="House / Blender furniture and lived details";
            foreach(var r in furniture.GetComponentsInChildren<Renderer>())if(r.name=="House chair / seat"||r.name=="Bedroom / nightstand")r.gameObject.AddComponent<BoxCollider>();
            StaticBatchingUtility.Combine(furniture);
            string normalPath=Root+"/Textures/FieldSoilNormal.png";var importer=(TextureImporter)AssetImporter.GetAtPath(normalPath);if(importer.textureType!=TextureImporterType.NormalMap){importer.textureType=TextureImporterType.NormalMap;importer.SaveAndReimport();}
            var earth=VisualWorkshop.GetMaterial("Earth");earth.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/FieldSoil.png"));earth.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));earth.EnableKeyword("_NORMALMAP");earth.SetFloat("_BumpScale",.65f);earth.SetColor("_BaseColor",new Color(1,.97f,.87f));earth.SetTextureScale("_BaseMap",new Vector2(100,106));earth.SetTextureScale("_BumpMap",new Vector2(100,106));earth.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/FieldSoilSmoothness.png"));earth.EnableKeyword("_METALLICSPECGLOSSMAP");earth.SetFloat("_Smoothness",.24f);EditorUtility.SetDirty(earth);
            foreach(var r in root.GetComponentsInChildren<Renderer>())
            {
                if(r.name.StartsWith("Bogie / wheel")){float ratio=.38f/r.bounds.size.y;r.transform.localScale*=ratio;r.transform.position+=Vector3.up*(-.31f-r.bounds.center.y);}
                if(r.name.StartsWith("Bogie / hub")){r.transform.localScale*=.6f;r.transform.position+=Vector3.up*(-.31f-r.bounds.center.y);}
            }
            var pipe=GameObject.CreatePrimitive(PrimitiveType.Cylinder);pipe.name="Locomotive / short diesel exhaust";pipe.transform.SetParent(root,false);pipe.transform.position=new Vector3(.64f,3.1f,34.6f);pipe.transform.localScale=new Vector3(.19f,.25f,.19f);pipe.GetComponent<Renderer>().sharedMaterial=VisualWorkshop.GetMaterial("Ink");Object.DestroyImmediate(pipe.GetComponent<Collider>());
            var exhaust=new GameObject("Locomotive / warm exhaust in motion");exhaust.transform.SetParent(root,false);exhaust.transform.position=new Vector3(.64f,3.4f,34.6f);exhaust.transform.rotation=Quaternion.Euler(-90,0,0);var ps=exhaust.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.startLifetime=new ParticleSystem.MinMaxCurve(2,4);main.startSize=new ParticleSystem.MinMaxCurve(.25f,.55f);main.startSpeed=.3f;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=65;
            var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.radius=.065f;shape.angle=8;
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=0;velocity.y=.15f;velocity.z=-2;
            var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.3f),new Keyframe(1,2.5f)));
            var smoke=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/DieselSmoke.mat");if(!smoke){smoke=new Material(Shader.Find("NoDestination/SoftParticle"));AssetDatabase.CreateAsset(smoke,Root+"/Materials/DieselSmoke.mat");}smoke.SetColor("_BaseColor",new Color(.30f,.29f,.25f,.16f));ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=smoke;ps.Play();
            var motion=root.gameObject.AddComponent<TrainSurfaceMotion>();motion.train=player.train;motion.exhaust=ps;motion.earth=root.GetComponentsInChildren<Renderer>().First(r=>r.name=="Field earth");motion.ballast=root.GetComponentsInChildren<Renderer>().First(r=>r.name=="Track / ballast bed");
            var light=new GameObject("Locomotive / focused headlight");light.transform.SetParent(root,false);light.transform.position=new Vector3(0,1.62f,41.3f);var head=light.AddComponent<Light>();head.type=LightType.Spot;head.range=45;head.spotAngle=37;head.intensity=3;head.color=new Color(1,.85f,.57f);
        }
    }
}
