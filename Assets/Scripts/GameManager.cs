using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Modes")]
    public GameModeDefinition[] availableModes;
    public int startModeIndex = 0;

    [Header("Runner")]
    public GameModeRunner runner;

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // optional
    }

    private void Start()
    {
        StartMode(startModeIndex);
    }

    public void StartMode(int index)
    {
        if (availableModes == null || availableModes.Length == 0)
        {
            Debug.LogError("No modes assigned to GameManager.");
            return;
        }

        index = Mathf.Clamp(index, 0, availableModes.Length - 1);
        var mode = availableModes[index];

        runner.Run(mode);
        Debug.Log($"Started mode: {mode.modeName}");
    }
}
