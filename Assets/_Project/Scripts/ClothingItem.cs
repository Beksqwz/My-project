using UnityEngine;
namespace Sanlaq {
public enum PlayerRole { Runner, Sokyrteke }
public enum ClothingSlot { Head, Torso, Pants, Shoes }
[CreateAssetMenu(menuName="SAÑLAQ/Clothing")]
public class ClothingItem : ScriptableObject {
    public string id, displayName;
    public ClothingSlot slot;
    public Sprite icon, visual;
    public Color color = Color.white;
    public int shape;
}
}
