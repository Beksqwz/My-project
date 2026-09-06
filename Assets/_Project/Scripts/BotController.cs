using UnityEngine;
namespace Sanlaq {
public class BotController:MonoBehaviour {
    PlayerController actor;Vector2 wander,last;float rethink,stuck;
    void Awake(){actor=GetComponent<PlayerController>();}
    void Update(){
        var g=GameManager.Instance;if(!g||actor.human||!actor.CanMove||g.testing)return;
        Vector2 p=transform.position,desired=Vector2.zero;rethink-=Time.deltaTime;
        if(rethink<=0){rethink=Random.Range(.9f,2.2f);wander=new Vector2(Random.Range(-11f,11f),Random.Range(-6f,6f));}
        actor.sprint=false;
        if(actor.role==PlayerRole.Sokyrteke){PlayerController nearest=null;float best=999;
            foreach(var r in g.players){if(r.eliminated||r.role!=PlayerRole.Runner)continue;float d=((Vector2)r.transform.position-p).sqrMagnitude;if(r.Hiding&&d>2.25f)continue;if(d<best){best=d;nearest=r;}}
            desired=nearest?(Vector2)nearest.transform.position-p:wander-p;actor.sprint=nearest&&best<36;
        }else{
            Vector2 away=p-(Vector2)g.hunter.transform.position;
            if(away.sqrMagnitude<30&&!actor.Hiding){desired=away.normalized+new Vector2(-away.y,away.x).normalized*.38f;actor.sprint=true;}
            else if(actor.Hiding&&away.sqrMagnitude<36){desired=Vector2.zero;}
            else desired=wander-p;
        }
        if(Mathf.Abs(p.x)>11.5f)desired.x-=Mathf.Sign(p.x)*3;
        if(Mathf.Abs(p.y)>6.5f)desired.y-=Mathf.Sign(p.y)*3;
        if(desired.sqrMagnitude>.02f)desired=Steer(p,desired.normalized);
        if(Vector2.Distance(p,last)<.018f && desired.sqrMagnitude>.1f)stuck+=Time.deltaTime;else stuck=0;
        if(stuck>.7f){rethink=0;desired=Quaternion.Euler(0,0,90)*desired;stuck=0;}
        last=p;actor.input=desired;
    }
    Vector2 Steer(Vector2 p,Vector2 direction){
        if(!Physics2D.CircleCast(p,.38f,direction,1.15f,1<<8))return direction;
        float best=-999;Vector2 chosen=Vector2.zero;
        for(int i=0;i<12;i++){Vector2 candidate=Quaternion.Euler(0,0,i*30)*direction;var hit=Physics2D.CircleCast(p,.38f,candidate,1.5f,1<<8);float distance=hit?hit.distance:1.5f;float score=Vector2.Dot(direction,candidate)*.8f+distance;if(distance>.38f&&score>best){best=score;chosen=candidate;}}
        return chosen;
    }
}
}
