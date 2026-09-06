using UnityEngine;
namespace Sanlaq {
public class Arena : MonoBehaviour {
    public const float HalfWidth=13, HalfHeight=8;
    public static readonly Rect Water=new Rect(-7,-3.7f,4.8f,2.5f), Hide=new Rect(5,2.5f,3.3f,2.8f);
    public void Build() {
        Art.Shape(transform,"Steppe",Vector2.up*.6f,new Vector2(30,22),new Color32(192,179,130,255),-100,false);
        Art.Shape(transform,"Arena sand",Vector2.zero,new Vector2(25.7f,15.7f),new Color32(224,210,164,255),-99,false);
        Art.Shape(transform,"Central clearing",Vector2.zero,new Vector2(16,11),new Color32(231,217,175,255),-98);
        Art.Shape(transform,"Pond shore",Water.center,Water.size+Vector2.one*.4f,new Color32(139,166,145,255),-95,false);
        Art.Shape(transform,"Water",Water.center,Water.size,new Color32(76,147,168,255),-94,false);
        for(int i=0;i<8;i++)Art.Shape(transform,"Water glint",Water.center+new Vector2(-1.8f+i*.48f,Mathf.Sin(i*2)*.8f),new Vector2(.4f,.045f),new Color32(150,209,211,255),-93);
        Solid("North fence",new Vector2(0,8.2f),new Vector2(26.8f,.5f),Art.Navy);
        Solid("South fence",new Vector2(0,-8.2f),new Vector2(26.8f,.5f),Art.Navy);
        Solid("West fence",new Vector2(-13.2f,0),new Vector2(.5f,16),Art.Navy);
        Solid("East fence",new Vector2(13.2f,0),new Vector2(.5f,16),Art.Navy);
        Rock(new Vector2(-3,2.4f),new Vector2(2.1f,1.4f)); Rock(new Vector2(3,-2),new Vector2(1.8f,1.4f)); Rock(new Vector2(-9,4.7f),new Vector2(1.4f,1.2f));
        Solid("Fallen log",new Vector2(1.2f,4.7f),new Vector2(3,.65f),new Color32(121,86,57,255));
        Art.Shape(transform,"Log grain",new Vector2(1.2f,4.8f),new Vector2(2.6f,.08f),new Color32(184,142,91,255),-5,false);
        Art.Shape(transform,"Yurt foundation",Hide.center,new Vector2(3.7f,3.1f),new Color32(164,145,102,255),-90);
        Art.Shape(transform,"Yurt felt",Hide.center+Vector2.up*.15f,new Vector2(3.4f,2.8f),Art.Cream,-89);
        Art.Shape(transform,"Yurt gold band",Hide.center-Vector2.up*.5f,new Vector2(3.1f,.16f),Art.Gold,-88,false);
        Art.Shape(transform,"Yurt roof crown",Hide.center+Vector2.up*.8f,new Vector2(.7f,.38f),Art.Gold,-87);
        Art.Shape(transform,"Yurt doorway",Hide.center-Vector2.up*.95f,new Vector2(.85f,.6f),Art.Navy,-87,false);
        var rng=new System.Random(41);
        for(int i=0;i<65;i++) { var p=new Vector2((float)rng.NextDouble()*24-12,(float)rng.NextDouble()*14-7); if(Water.Contains(p)||Hide.Contains(p)||p.sqrMagnitude<7)continue;
            Art.Shape(transform,"Steppe grass",p,new Vector2(.13f,.25f),new Color32(155,157,107,255),-96);
            Art.Shape(transform,"Grass tip",p+Vector2.right*.11f,new Vector2(.09f,.16f),new Color32(167,165,113,255),-96); }
    }
    void Rock(Vector2 p,Vector2 size){Solid("Rock",p,size,new Color32(112,125,125,255),true);Art.Shape(transform,"Rock highlight",p+new Vector2(-.15f,.2f),size*.6f,new Color32(153,161,151,255),-4);}
    void Solid(string label,Vector2 p,Vector2 size,Color c,bool round=false){var s=Art.Shape(transform,label,p,size,c,-5,round);s.gameObject.layer=8;
        if(round){var polygon=s.gameObject.AddComponent<PolygonCollider2D>();var points=new Vector2[20];for(int i=0;i<20;i++){float a=i*Mathf.PI*2/20;points[i]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.48f;}polygon.points=points;}
        else s.gameObject.AddComponent<BoxCollider2D>();}
}
}
