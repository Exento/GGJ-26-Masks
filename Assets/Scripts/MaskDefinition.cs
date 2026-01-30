using UnityEngine;

[CreateAssetMenu(menuName = "HoleInWall/Mask Definition")]
public class MaskDefinition : ScriptableObject
{
    public string id;
    public GameObject maskPrefab;

    [Header("Movement")]
    public float travelDuration = 2.5f;   // time from start->end
    public AnimationCurve travelCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Gameplay")]
    public float evaluateWindowSeconds = 0.35f; // how long around PlayerPoint to evaluate
    public int baseScore = 100;

    // You can add difficulty, required pose label, etc.
}