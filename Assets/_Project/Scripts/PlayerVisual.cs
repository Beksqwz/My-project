using UnityEngine;
using UnityEngine.Rendering;
namespace Sanlaq {
public class PlayerVisual:MonoBehaviour {
    PlayerController actor; Transform doll; SpriteRenderer[] parts; float punch;
    public void Build(PlayerController player){
        actor=player;var group=gameObject.AddComponent<SortingGroup>();group.sortingOrder=20;
        doll=new GameObject("Clothed character").transform;doll.SetParent(transform,false);
        var skin=new Color32(222,163,113,255);
        Art.Shape(doll,"Shadow",new Vector2(0,-.1f),new Vector2(.9f,.35f),new Color(0,0,0,.15f),0);
        if(actor.human){Art.Shape(doll,"You ring",new Vector2(0,-.08f),new Vector2(1.06f,.5f),Art.Cream,1);Art.Shape(doll,"Ring center",new Vector2(0,-.08f),new Vector2(.84f,.33f),new Color32(224,210,164,255),2);}
        for(int side=-1;side<=1;side+=2){
            Art.Shape(doll,"Shoe",new Vector2(side*.2f,.02f),new Vector2(.33f,.28f),actor.outfit[3].color,4,actor.outfit[3].shape==0);
            Art.Shape(doll,"Pants",new Vector2(side*.17f,.3f),new Vector2(.29f,.52f),actor.outfit[2].color,5,false);
            Art.Shape(doll,"Hand",new Vector2(side*.42f,.53f),new Vector2(.2f,.3f),skin,6);
            Art.Shape(doll,"Sleeve",new Vector2(side*.36f,.76f),new Vector2(.24f,.35f),actor.outfit[1].color,7);
        }
        Art.Shape(doll,"Torso",new Vector2(0,.68f),new Vector2(.7f,.71f),actor.outfit[1].color,8);
        if(actor.outfit[1].shape==1)Art.Shape(doll,"Tunic stripe",new Vector2(0,.7f),new Vector2(.09f,.55f),Art.Cream,9,false);
        if(actor.outfit[1].shape==2)Art.Shape(doll,"Vest inset",new Vector2(0,.76f),new Vector2(.27f,.48f),Art.Gold,9,false);
        Art.Shape(doll,"Hair outline",new Vector2(0,1.25f),new Vector2(.88f,.82f),Art.Navy,10);
        Art.Shape(doll,"Face",new Vector2(0,1.19f),new Vector2(.75f,.65f),skin,11);
        for(int side=-1;side<=1;side+=2)Art.Shape(doll,"Eye",new Vector2(side*.15f,1.21f),new Vector2(.055f,.085f),Art.Navy,12);
        var hat=actor.outfit[0];Art.Shape(doll,"Headwear",new Vector2(0,1.57f),new Vector2(.86f,hat.shape==2?.5f:.28f),hat.color,13,hat.shape!=2);
        if(hat.shape==1)Art.Shape(doll,"Hat brim",new Vector2(.1f,1.45f),new Vector2(1.02f,.12f),hat.color,14);
        if(actor.role==PlayerRole.Sokyrteke){
            Art.Shape(doll,"Blindfold",new Vector2(0,1.23f),new Vector2(.78f,.17f),Art.Navy,15,false);
            Art.Shape(doll,"Hunter badge",new Vector2(0,1.97f),new Vector2(.38f,.38f),Art.Gold,16);
            Art.Shape(doll,"Hunter cross vertical",new Vector2(0,1.97f),new Vector2(.055f,.23f),Art.Navy,17,false);
            Art.Shape(doll,"Hunter cross horizontal",new Vector2(0,1.97f),new Vector2(.23f,.055f),Art.Navy,17,false);
        }
        parts=GetComponentsInChildren<SpriteRenderer>();
    }
    public void Punch(){punch=.3f;}
    void LateUpdate(){if(!actor||actor.eliminated)return;var g=GameManager.Instance;
        bool hidden=g.human.role==PlayerRole.Sokyrteke&&actor.role==PlayerRole.Runner&&(actor.Hiding||Vector2.Distance(actor.transform.position,g.human.transform.position)>g.visionRadius);
        foreach(var sr in parts)sr.enabled=!hidden;
        GetComponent<SortingGroup>().sortingOrder=100-Mathf.RoundToInt(actor.transform.position.y*5);
        punch=Mathf.Max(0,punch-Time.deltaTime);float bob=actor.Body.linearVelocity.sqrMagnitude>.1f?Mathf.Sin(Time.time*16)*.035f:0;
        doll.localScale=new Vector3(1+punch*.55f,1-punch*.25f,1);doll.localPosition=Vector3.up*bob;
        if(actor.Hiding&&actor.human)foreach(var sr in parts){var c=sr.color;c.a=.55f;sr.color=c;}
        else foreach(var sr in parts){var c=sr.color;c.a=sr.name=="Shadow"?.15f:1;sr.color=c;}
    }
}
}
