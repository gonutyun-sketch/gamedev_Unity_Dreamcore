using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace NoDestination.VisualRebuild.Editor
{
    [InitializeOnLoad] public static class TargetedPolish
    {
        static TargetedPolish(){EditorApplication.update+=Tick;}
        static void Tick()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists("Artifacts/TargetedPolish/apply.request"))return;
            File.Delete("Artifacts/TargetedPolish/apply.request");
            try
            {
                var game=UnityEngine.Object.FindFirstObjectByType<VisualJourney>();if(!game)throw new Exception("Open the existing GoldenField scene first.");
                var all=game.transform.root.GetComponentsInChildren<Transform>(true);
                foreach(var entry in new[]{("House / clock minute hand",102f,.22f),("House / clock hour hand",98.5f,.14f)})
                {
                    var hand=all.Single(t=>t.name==entry.Item1);float angle=entry.Item2*Mathf.Deg2Rad;var pivot=new Vector3(19.3f,1.5f,61.19f);var direction=new Vector3(-Mathf.Sin(angle),Mathf.Cos(angle),0);
                    hand.position=pivot+direction*entry.Item3*.5f;hand.rotation=Quaternion.LookRotation(direction);var scale=hand.localScale;scale.z=entry.Item3;hand.localScale=scale;
                }
                const string p="Assets/NoDestination/VisualRebuild/Materials/WornFootpath.mat";
                var mat=AssetDatabase.LoadAssetAtPath<Material>(p);if(!mat){mat=new Material(Shader.Find("NoDestination/WornFootpath"));AssetDatabase.CreateAsset(mat,p);}mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/NoDestination/VisualRebuild/Textures/FieldSoil.png"));EditorUtility.SetDirty(mat);
                all.Single(t=>t.name=="Field / footpath").GetComponent<Renderer>().sharedMaterial=mat;
                string audit="Renderers intruding into the central walking envelope (bounds candidates):\n";var aisle=new Bounds(new Vector3(0,1,0),new Vector3(.55f,1.65f,16.4f));
                foreach(var r in game.transform.root.GetComponentsInChildren<Renderer>(true))if(r.enabled&&r.bounds.Intersects(aisle))audit+=r.name+" | "+r.bounds+" | "+r.transform.parent.name+"\n";
                File.WriteAllText("Artifacts/TargetedPolish/aisle-audit.txt",audit);
                EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);AssetDatabase.SaveAssets();
                VisualWorkshop.CaptureEvidence(game.eyes,new Vector3(19.3f,1.5f,62.05f),Quaternion.LookRotation(Vector3.back),"21-clock-0317");
                File.WriteAllText("Artifacts/TargetedPolish/applied.txt",DateTime.Now+" existing scene updated; story/puzzle components untouched");
            }
            catch(Exception e){File.WriteAllText("Artifacts/TargetedPolish/error.txt",e.ToString());Debug.LogException(e);}
        }
    }
}
