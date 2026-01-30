using System.Collections;
using UnityEngine;

public class GameModeRunner : MonoBehaviour
{
    [Header("Scene Rig")]
    public Transform startPoint;
    public Transform playerPoint;
    public Transform endPoint;
    public Transform spawnParent;

    [Header("Dependencies")]
    public MonoBehaviour poseJudgeBehaviour; // assign something that implements IPoseJudge
    //private IPoseJudge PoseJudge => (IPoseJudge)poseJudgeBehaviour;

    private GameModeDefinition _mode;
    private int _score;
    private int _index;

    public void Run(GameModeDefinition mode)
    {
        _mode = mode;
        _score = 0;
        _index = 0;

        StopAllCoroutines();
        StartCoroutine(RunLoop());
    }

    private IEnumerator RunLoop()
    {
        for (int round = 0; round < _mode.rounds; round++)
        {
            var def = PickMaskDefinition();
            SpawnMask(def);

            yield return new WaitForSeconds(_mode.timeBetweenMasks);
        }

        Debug.Log($"Mode finished. Score: {_score}");
    }

    private MaskDefinition PickMaskDefinition()
    {
        if (_mode.masks == null || _mode.masks.Count == 0)
        {
            Debug.LogError("GameMode has no masks assigned.");
            return null;
        }

        switch (_mode.selectionStyle)
        {
            case MaskSelectionStyle.InOrder:
                var def = _mode.masks[_index % _mode.masks.Count];
                _index++;
                return def;

            case MaskSelectionStyle.Random:
                return _mode.masks[Random.Range(0, _mode.masks.Count)];

            case MaskSelectionStyle.WeightedRandom:
                // If you add weights to MaskDefinition, implement here.
                return _mode.masks[Random.Range(0, _mode.masks.Count)];

            default:
                return _mode.masks[0];
        }
    }

    private void SpawnMask(MaskDefinition def)
    {
        if (def == null || def.maskPrefab == null) return;

        var go = Instantiate(def.maskPrefab,
                             startPoint.position,
                             startPoint.rotation,
                             spawnParent ? spawnParent : null);

        var instance = go.GetComponent<MaskInstance>();
        if (!instance) instance = go.AddComponent<MaskInstance>();

        instance.Initialize(startPoint, playerPoint, endPoint,
                           def.travelDuration, def.travelCurve, def.evaluateWindowSeconds);

        instance.ReachedPlayerZone += OnMaskAtPlayer;
        instance.Finished += OnMaskFinished;
    }

    private void OnMaskAtPlayer(MaskInstance mask)
    {

        Debug.Log($"Mask at Player position)");
        /*
        float fit = PoseJudge.EvaluateFit(mask); // 0..1
        int gained = Mathf.RoundToInt(fit * 100f);
        _score += gained;

        Debug.Log($"Pose fit: {fit:0.00} => +{gained} (Total {_score})");
        */
    }

    private void OnMaskFinished(MaskInstance mask)
    {
        mask.ReachedPlayerZone -= OnMaskAtPlayer;
        mask.Finished -= OnMaskFinished;
    }
}
