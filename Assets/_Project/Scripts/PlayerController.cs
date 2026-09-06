using UnityEngine;
using UnityEngine.InputSystem;
namespace Sanlaq {
[RequireComponent(typeof(Rigidbody2D),typeof(CircleCollider2D))]
public class PlayerController:MonoBehaviour {
    public bool human, eliminated, frozen, sprint;
    public PlayerRole role;
    public string displayName;
    public ClothingItem[] outfit=new ClothingItem[4];
    public float slowUntil, immuneUntil;
    public Vector2 input;
    public Rigidbody2D Body {get;private set;}
    public bool InWater => Arena.Water.Contains(transform.position);
    public bool Hiding => role==PlayerRole.Runner && Arena.Hide.Contains(transform.position);
    public bool Slowed => Time.time<slowUntil;
    public float Speed => (sprint?6.2f:4.5f)*(InWater?.65f:1)*(Slowed?.5f:1);
    public PlayerVisual visual;
    float dust;bool wasSprinting;
    void Awake(){Body=GetComponent<Rigidbody2D>();Body.gravityScale=0;Body.freezeRotation=true;Body.interpolation=RigidbodyInterpolation2D.Interpolate;Body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;GetComponent<CircleCollider2D>().radius=.32f;}
    void Update(){
        if(human && !GameManager.Instance.testing) {
            var k=Keyboard.current; input=Vector2.zero; sprint=false;
            if(k!=null){input=new Vector2((k.dKey.isPressed||k.rightArrowKey.isPressed?1:0)-(k.aKey.isPressed||k.leftArrowKey.isPressed?1:0),(k.wKey.isPressed||k.upArrowKey.isPressed?1:0)-(k.sKey.isPressed||k.downArrowKey.isPressed?1:0));sprint=k.leftShiftKey.isPressed||k.rightShiftKey.isPressed;}
        }
        if(!CanMove){input=Vector2.zero;return;}
        if(human&&sprint&&!wasSprinting&&input.sqrMagnitude>.1f)GameManager.Instance.audioFx.Play(SoundCue.Sprint,.25f);
        wasSprinting=sprint;
        dust-=Time.deltaTime;
        if(input.sqrMagnitude>.1f && dust<=0 && (sprint||InWater)){dust=InWater?.28f:.18f;Art.Burst(transform.position,InWater?new Color(.6f,.86f,.9f):Art.Cream,2);if(InWater)GameManager.Instance.audioFx.Play(SoundCue.Water,.12f);}
    }
    public bool CanMove => !eliminated&&!frozen&&GameManager.Instance&&GameManager.Instance.IsLive;
    void FixedUpdate(){Body.linearVelocity=CanMove?Vector2.ClampMagnitude(input,1)*Speed:Vector2.zero;}
    public void Eliminate(){if(eliminated)return; eliminated=true;frozen=false;Body.linearVelocity=Vector2.zero;GetComponent<Collider2D>().enabled=false;Art.Burst(transform.position,Art.Gold,16);visual.gameObject.SetActive(false);}
}
}
