using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Sanlaq;
[InitializeOnLoad]
public static class ProjectBuilder {
    const string Root="Assets/_Project/";
    const string Scene=Root+"Scenes/Sanlaq.unity";
    static ProjectBuilder(){EditorApplication.update+=Poll;}
    static void Poll(){
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        string command="Temp/sanlaq-command";if(!File.Exists(command))return;
        var action=File.ReadAllText(command).Trim();File.Delete(command);
        try{
            if(action=="setup")Build();
            if(action=="smoke"){File.WriteAllText("Temp/sanlaq-smoke","");EditorApplication.isPlaying=true;}
            if(action=="stop")EditorApplication.isPlaying=false;
            if(action=="build")BuildPlayer();
        }catch(System.Exception e){Debug.LogException(e);File.WriteAllText("Logs/sanlaq-editor-error.txt",e.ToString());}
    }
    [MenuItem("SAÑLAQ/Build or refresh MVP scene")]
    public static void Build(){
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play before rebuilding.");
        foreach(var d in new[]{"Scenes","Scripts","Prefabs","Art","UI","Audio","Data","Resources/Shapes"})Directory.CreateDirectory(Root+d);
        Sprite disc=ShapeAsset("Disc",true),square=ShapeAsset("Square",false);
        var items=new System.Collections.Generic.List<ClothingItem>();
        string[][] names={new[]{"Sand cap","Blue brim hat","Red felt hat"},new[]{"Teal shirt","Cream tunic","Coral vest"},new[]{"Navy pants","Sand pants","Rust pants"},new[]{"Brown shoes","Cream boots","Blue shoes"}};
        Color[][] colors={new[]{Art.Gold,new Color(.25f,.52f,.69f),new Color(.75f,.32f,.25f)},new[]{new Color(.18f,.52f,.5f),Art.Cream,new Color(.8f,.4f,.32f)},new[]{Art.Navy,new Color(.72f,.59f,.38f),new Color(.6f,.32f,.23f)},new[]{new Color(.35f,.24f,.18f),Art.Cream,new Color(.24f,.42f,.6f)}};
        for(int s=0;s<4;s++)for(int i=0;i<3;i++){
            string id=((ClothingSlot)s).ToString().ToLower()+"_"+i;string path=Root+"Data/"+id+".asset";
            var item=AssetDatabase.LoadAssetAtPath<ClothingItem>(path);if(!item){item=ScriptableObject.CreateInstance<ClothingItem>();AssetDatabase.CreateAsset(item,path);}
            item.id=id;item.displayName=names[s][i];item.slot=(ClothingSlot)s;item.color=colors[s][i];item.shape=i;item.visual=s==0?disc:square;item.icon=IconAsset(id,s,i,item.color);EditorUtility.SetDirty(item);items.Add(item);
        }
        var go=new GameObject("Player");go.AddComponent<Rigidbody2D>();go.AddComponent<CircleCollider2D>();var player=go.AddComponent<PlayerController>();go.AddComponent<BotController>();
        var prefab=PrefabUtility.SaveAsPrefabAsset(go,Root+"Prefabs/Player.prefab");Object.DestroyImmediate(go);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Main Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=6.4f;camera.transform.position=new Vector3(0,0,-10);camera.backgroundColor=Art.Navy;camera.gameObject.AddComponent<AudioListener>();
        var arena=new GameObject("Steppe Arena").AddComponent<Arena>();arena.Build();
        var manager=new GameObject("SAÑLAQ Match").AddComponent<GameManager>();manager.playerPrefab=prefab.GetComponent<PlayerController>();manager.wardrobe=items.ToArray();
        EditorSceneManager.SaveScene(scene,Scene);EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Scene,true)};
        PlayerSettings.productName="SAÑLAQ";PlayerSettings.companyName="Sanlaq";PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;PlayerSettings.defaultScreenWidth=1280;PlayerSettings.defaultScreenHeight=720;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
        AssetDatabase.SaveAssets();File.WriteAllText("Logs/sanlaq-setup.txt","Scene, player prefab and 12 clothing assets generated successfully.");
        Debug.Log("SAÑLAQ scene ready. Press Play.");
    }
    static Sprite ShapeAsset(string name,bool round){string path=Root+"Resources/Shapes/"+name+".png";var t=new Texture2D(64,64);for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),Vector2.one*32);t.SetPixel(x,y,new Color(1,1,1,round?Mathf.Clamp01(32-d):1));}t.Apply();File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);return Import(path);}
    static Sprite Import(string path){AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=TextureImporterType.Sprite;ti.spritePixelsPerUnit=64;ti.alphaIsTransparency=true;ti.mipmapEnabled=false;ti.filterMode=FilterMode.Bilinear;ti.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);}
    static Sprite IconAsset(string id,int slot,int shape,Color color){
        var t=new Texture2D(96,96);for(int y=0;y<96;y++)for(int x=0;x<96;x++){float xx=(x-48)/48f,yy=(y-48)/48f;bool inside=false;
            if(slot==0)inside=shape==2?Mathf.Abs(xx)<.47f&&yy>-.28f&&yy<.65f:(xx*xx/.49f+(yy-.1f)*(yy-.1f)/.22f<1)||(shape==1&&Mathf.Abs(xx)<.86f&&yy>-.33f&&yy<-.19f);
            if(slot==1)inside=Mathf.Abs(xx)<.44f&&Mathf.Abs(yy)<.65f||Mathf.Abs(xx)<.78f&&yy>.12f&&yy<.52f;
            if(slot==2)inside=Mathf.Abs(xx)<.55f&&Mathf.Abs(yy)<.7f&&(yy>.2f||Mathf.Abs(xx)>.09f);
            if(slot==3)inside=Mathf.Abs(xx)<.68f&&yy>-.45f&&yy<.15f||Mathf.Abs(xx)<.43f&&yy>.05f&&yy<(shape==1?.7f:.37f);
            Color c=inside?color:Color.clear;if(slot==1&&inside&&shape==1&&Mathf.Abs(xx)<.08f)c=Art.Gold;if(slot==1&&inside&&shape==2&&Mathf.Abs(xx)<.2f)c=Art.Gold;t.SetPixel(x,y,c);
        }t.Apply();string path=Root+"Art/"+id+".png";File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);return Import(path);
    }
    [MenuItem("SAÑLAQ/Build Windows MVP")]
    public static void BuildPlayer(){Directory.CreateDirectory("Builds/Windows");var r=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName="Builds/Windows/Sanlaq.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});File.WriteAllText("Logs/sanlaq-build.txt",r.summary.result+" errors="+r.summary.totalErrors);}
    [MenuItem("SAÑLAQ/Run gameplay validation")]
    public static void Smoke(){if(EditorApplication.isPlaying)return;EditorSceneManager.OpenScene(Scene);File.WriteAllText("Temp/sanlaq-smoke","");EditorApplication.isPlaying=true;}
}
