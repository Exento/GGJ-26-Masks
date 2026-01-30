using System.Collections.Generic;
using UnityEngine;

public enum MaskSelectionStyle
{
    InOrder,
    Random,
    WeightedRandom
}

[CreateAssetMenu(menuName = "HoleInWall/GameMode Definition")]
public class GameModeDefinition : ScriptableObject
{
    public string modeName;

    [Header("Mask Set")]
    public MaskSelectionStyle selectionStyle = MaskSelectionStyle.InOrder;
    public List<MaskDefinition> masks = new();

    [Header("Rules")]
    public int rounds = 10;
    public float timeBetweenMasks = 0.75f;

    // Optional: add lives, miss penalty, combo rules, etc.
}
