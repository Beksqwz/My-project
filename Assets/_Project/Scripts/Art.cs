using UnityEngine;
namespace Sanlaq {
public static class Art {
    public static readonly Color Navy = new Color32(25,39,57,255), Gold = new Color32(235,187,91,255), Cream = new Color32(250,241,216,255);
    static Sprite disc, square;
    public static Sprite Disc { get { if(!disc) disc=Resources.Load<Sprite>("Shapes/Disc"); if(!disc)disc=Make(true); return disc; } }
    public static Sprite Square { get { if(!square) square=Resources.Load<Sprite>("Shapes/Square"); if(!square)square=Make(false); return square; } }
    static Sprite Make(bool round) {
        var t=new Texture2D(64,64,TextureFormat.RGBA32,false); t.filterMode=FilterMode.Bilinear;
        for(int y=0;y<64;y++) for(int x=0;x<64;x++) { float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(32,32)); t.SetPixel(x,y,new Color(1,1,1,round?Mathf.Clamp01(32-d):1)); }
        t.Apply(); return Sprite.Create(t,new Rect(0,0,64,64),new Vector2(.5f,.5f),64);
    }
    public static SpriteRenderer Shape(Transform parent,string name, Vector2 pos,Vector2 size,Color color,int order=0,bool round=true) {
        var go=new GameObject(name); go.transform.SetParent(parent,false); go.transform.localPosition=pos; go.transform.localScale=new Vector3(size.x,size.y,1);
        var sr=go.AddComponent<SpriteRenderer>(); sr.sprite=round?Disc:Square; sr.color=color; sr.sortingOrder=order; return sr;
    }
    public static void Burst(Vector2 p,Color c,int count=9) {
        for(int i=0;i<count;i++) { var s=Shape(null,"Feedback",p,Vector2.one*.12f,c,200); var f=s.gameObject.AddComponent<Fx>(); f.velocity=Random.insideUnitCircle*2; }
    }
}
public class Fx:MonoBehaviour {
    public Vector2 velocity; float life=.55f;
    void Update(){life-=Time.deltaTime; transform.position+=(Vector3)(velocity*Time.deltaTime); transform.localScale*=Mathf.Pow(.12f,Time.deltaTime); if(life<=0)Destroy(gameObject);}
}
}
