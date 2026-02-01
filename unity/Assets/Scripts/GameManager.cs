using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Lobby, RoundIntro, Playing, RoundOver, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Lobby;

    public enum RoundType { Snap, Sprint, Reaction, Precision, Advanced, Chaos }

    public int CurrentRound { get; private set; } = 0;
    private RoundType currentRoundType = RoundType.Snap;

    [Header("Game Settings")]
    [SerializeField] private int minPlayersToStart = 2;
    [SerializeField] private int startingRound = 1; // Debug: Set to start at specific round

    [Header("Round Sequence (Edit This!)")]
    [SerializeField] private RoundType[] roundSequence = new RoundType[]
    {
        RoundType.Snap,      // Round 1
        RoundType.Snap,      // Round 2
        RoundType.Sprint,    // Round 3
        RoundType.Snap,      // Round 4
        RoundType.Chaos,     // Round 5 - Randomize all masks!
        RoundType.Advanced,  // Round 6 - Hard mode!
        RoundType.Reaction,  // Round 7
        RoundType.Snap,      // Round 8
        RoundType.Snap,      // Round 9
        RoundType.Precision, // Round 10
        RoundType.Snap,      // Round 11
        RoundType.Advanced,  // Round 12 - Hard mode again!
        RoundType.Sprint,    // Round 13
        RoundType.Snap,      // Round 14
        RoundType.Snap,      // Round 15
        RoundType.Reaction,  // Round 16
        // Repeats after this...
    };

    private RoundController roundController;
    private SprintRound sprintRound;
    private ReactionRound reactionRound;
    private PrecisionRound precisionRound;
    private AdvancedRound advancedRound;
    private ChaosRound chaosRound;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        roundController = GetComponent<RoundController>();
        sprintRound = GetComponent<SprintRound>();
        reactionRound = GetComponent<ReactionRound>();
        precisionRound = GetComponent<PrecisionRound>();
        advancedRound = GetComponent<AdvancedRound>();
        chaosRound = GetComponent<ChaosRound>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (CurrentState == GameState.Lobby)
            {
                StartGame();
            }else if (CurrentState == GameState.Playing)
            {
                EndRound();
            }else if (CurrentState == GameState.RoundOver)
            { 
                NextRound();
            }
        }else if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            if (CurrentState == GameState.Lobby || CurrentState == GameState.RoundOver)
            {
                NextRound(RoundType.Snap);
            }
        }else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            if (CurrentState == GameState.Lobby || CurrentState == GameState.RoundOver)
            {
                NextRound(RoundType.Sprint);
            }
        }else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            if (CurrentState == GameState.Lobby || CurrentState == GameState.RoundOver)
            {
                NextRound(RoundType.Reaction);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            if (CurrentState == GameState.Lobby || CurrentState == GameState.RoundOver)
            {
                NextRound(RoundType.Precision);
            }
        }else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            if (CurrentState == GameState.Lobby || CurrentState == GameState.RoundOver)
            {
                NextRound(RoundType.Advanced);
            }
        }else if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
        {
            if (CurrentState == GameState.Lobby || CurrentState == GameState.RoundOver)
            {
                NextRound(RoundType.Chaos);
            }
        }


        // Debug: Skip to next round with N key
        if (Input.GetKeyDown(KeyCode.N) && CurrentState != GameState.Lobby)
        {
            Debug.Log("⏭️ Skipping to next round (debug)");
            NextRound();
        }

        // Debug: Press 'C' key to manually trigger chaos for testing
        if (Input.GetKeyDown(KeyCode.C) && CurrentState != GameState.Lobby)
        {
            Debug.Log("🃏 Manual Chaos Round triggered");
            if (chaosRound != null)
            {
                chaosRound.StartChaosRound();
            }
            else
            {
                Debug.LogError("❌ ChaosRound component is missing! Add it to GameManager GameObject.");
            }
        }
    }

    void StartGame(bool isAutoNextRound = true)
    {
        if (PlayerManager.Instance.GetTotalCount() < minPlayersToStart)
        {
            Debug.LogWarning($"⚠️ Need at least {minPlayersToStart} players to start");
            return;
        }

        CurrentState = GameState.Playing;
        CurrentRound = startingRound - 1; // Will increment to startingRound in NextRound()

        Debug.Log($"🎮 GAME STARTING! (Starting at Round {startingRound})");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideQRCode();
            UIManager.Instance.ClearLobbyMasks(); // Hide lobby masks when game starts
        }

        if(isAutoNextRound) NextRound();
    }

    public void NextRound()
    {
        // Get round type from sequence (loop if we exceed array length)
        int sequenceIndex = CurrentRound % roundSequence.Length;
        var nextRoundType = roundSequence[sequenceIndex];
        
        NextRound(nextRoundType);
    }
    public void NextRound(RoundType nextRoundType)
    {
        if (CurrentState == GameState.Lobby)
        {
            StartGame(false);
        }
        
        int aliveCount = PlayerManager.Instance.GetAliveCount();

        // Check for game over
        if (aliveCount <= 1)
        {
            EndGame();
            return;
        }

        CurrentRound++;
        currentRoundType = nextRoundType;
        
        PlayerManager.Instance.ResetEliminations();
        
        Debug.Log($"\n🎯 === ROUND {CurrentRound} ({currentRoundType}) ===");

        // Start the appropriate round type
        switch (currentRoundType)
        {
            case RoundType.Snap:
                CurrentState = GameState.Playing;
                if (roundController != null)
                {
                    roundController.StartRound();
                }
                break;

            case RoundType.Sprint:
                CurrentState = GameState.Playing;
                Debug.Log("🏃 Sprint Round!");
                if (sprintRound != null) sprintRound.StartSprintRound();
                break;

            case RoundType.Reaction:
                CurrentState = GameState.Playing;
                Debug.Log("⚡ Reaction Round!");
                if (reactionRound != null) reactionRound.StartReactionRound();
                break;

            case RoundType.Precision:
                CurrentState = GameState.Playing;
                Debug.Log("🎯 Precision Round!");
                if (precisionRound != null) precisionRound.StartPrecisionRound();
                break;

            case RoundType.Advanced:
                CurrentState = GameState.Playing;
                Debug.Log("💀 Advanced Round!");
                if (advancedRound != null) advancedRound.StartAdvancedRound();
                break;

            case RoundType.Chaos:
                CurrentState = GameState.Playing;
                Debug.Log("🃏 Chaos Round!");
                if (chaosRound != null)
                {
                    chaosRound.StartChaosRound();
                }
                else
                {
                    Debug.LogError("❌ ChaosRound component is missing! Add it to GameManager GameObject.");
                    OnRoundComplete(); // Skip to next round
                }
                break;
        }

        // Update UI and send state to clients
        BroadcastGameState();
    }

    void EndRound()
    {
        
    }

    void EndGame()
    {
        CurrentState = GameState.GameOver;

        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
        if (alivePlayers.Count == 1)
        {
            Debug.Log($"🏆 WINNER: Mask #{alivePlayers[0].MaskId} (Player {alivePlayers[0].Id})");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWinner(alivePlayers[0].MaskId);
            }
        }
        else if (alivePlayers.Count == 0)
        {
            Debug.Log("💀 No winner - all players eliminated!");
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWinner(-1); // No winner
            }
        }

        BroadcastGameState();
    }

    void BroadcastGameState()
    {
        NetworkManager.Instance.EmitGameState(
            CurrentState.ToString().ToLower(),
            PlayerManager.Instance.GetAliveCount(),
            PlayerManager.Instance.GetTotalCount(),
            CurrentRound,
            currentRoundType.ToString().ToLower()
        );
    }

    // Public method for rounds to call when they complete
    public void OnRoundComplete()
    {
        Debug.Log($"=== OnRoundComplete ROUND: {CurrentRound} ===");
        
        //Invoke(nameof(NextRound), 2f); // 2 second delay between rounds

        CurrentState = GameState.RoundOver;
    }
}
