using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private Dictionary<string, MaskDisplayGrid> lobbyMaskDisplayPlayerDict = new Dictionary<string, MaskDisplayGrid>();
    private List<MaskDisplayGrid> lobbyMaskDisplays = new List<MaskDisplayGrid>();
    
    private List<MaskDisplayGrid> interRoundMaskDisplays = new List<MaskDisplayGrid>();

    [Header("Lobby")]
    [SerializeField] private GameObject qrCodePanel;
    [SerializeField] private QRCodeFromWeb qrCodeGenerator;
    [SerializeField] private TextMeshProUGUI urlText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private int lobbyMaskContainerCount;
    [SerializeField] private RectTransform lobbyMaskContainer1; // Container for showing joined players
    [SerializeField] private RectTransform lobbyMaskContainer2; // Container for showing joined players
    [FormerlySerializedAs("lobbyMaskPrefab")] [SerializeField] private MaskDisplayGrid gridMaskPrefab; // Small mask prefab for lobby

    [Header("Game HUD")]
    
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI roundNumberText;
    [SerializeField] private TextMeshProUGUI roundTimerText;
    
    [Header("Interround Grid")]
    [SerializeField] private GameObject interRoundSummaryGrp; // Container for showing joined players
    [SerializeField] private TextMeshProUGUI interRoundCountTxt; // Container for showing joined players
    [SerializeField] private RectTransform interRoundGridContainer; // Container for showing joined players
    
    [Header("Round Title")]
    [SerializeField] private GameObject roundTitlePanel;
    [SerializeField] private TextMeshProUGUI roundTitleText;
    [SerializeField] private TextMeshProUGUI roundMessageText;

    [Header("Visual Timer")]
    [SerializeField] private GameObject visualTimerPanel; // Container for visual timer
    [SerializeField] private Image timerFillImage; // Radial fill image
    [SerializeField] private TextMeshProUGUI timerSecondsText; // Optional seconds text

    [Header("Reaction Round Indicators")]
    [SerializeField] private GameObject redIndicator; // WAIT state
    [SerializeField] private GameObject greenIndicator; // GO state

    [Header("Game Over")]
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private GameObject winnerGrp;
    [SerializeField] private GameObject noWinnerGrp;
    [SerializeField] private Image winnerImg;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        ShowQRCode();
        HideWinner();
        HideRoundTitle();
        HideReactionIndicators();
        HideVisualTimer(); // Hide timer during lobby
        interRoundSummaryGrp.gameObject.SetActive(false);

        CreateLobbyMasks();

        // Subscribe to player join/leave for lobby display
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnPlayerAdded += AddLobbyMask;
            PlayerManager.Instance.OnPlayerRemoved += RemoveLobbyMask;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.OnPlayerAdded -= AddLobbyMask;
            PlayerManager.Instance.OnPlayerRemoved -= RemoveLobbyMask;
        }
    }

    void Update()
    {
        UpdatePlayerCount();
        UpdateRoundNumber();
        
        var checkState = GameManager.Instance.CurrentState;
        if (checkState != currentGameState)
        {
            OnCurrentStateChanged(checkState);
        }
    }

    GameManager.GameState currentGameState = GameManager.GameState.Lobby;
    void OnCurrentStateChanged(GameManager.GameState state)
    {
        currentGameState = state;

        interRoundSummaryGrp.SetActive(false);
        interRoundGridContainer.gameObject.SetActive(false);
        ClearInterRoundMasks();
        
        if (currentGameState == GameManager.GameState.RoundOver)
        {
            interRoundGridContainer.gameObject.SetActive(true);
            ShowInterRoundSummary();
            RefreshInterRoundMasks();
        }
    }

    // Lobby
    public void ShowQRCode()
    {
        if (qrCodePanel != null)
        {
            qrCodePanel.SetActive(true);
        }

        // Generate QR code with server URL
        if (NetworkManager.Instance != null)
        {
            string serverUrl = NetworkManager.Instance.GetServerUrl();

            if (qrCodeGenerator != null)
            {
                qrCodeGenerator.GenerateQRCode(serverUrl);
            }

            if (urlText != null)
            {
                urlText.text = $"Or visit:\n{serverUrl}";
            }
        }

        if (instructionsText != null)
        {
            instructionsText.text = "Press SPACE to start game\nWaiting for players...";
        }
    }

    public void HideQRCode()
    {
        if (qrCodePanel != null)
        {
            qrCodePanel.SetActive(false);
        }
    }

    // Player Count
    void UpdatePlayerCount()
    {
        var playerCountStr = GameManager.Instance.CurrentState switch
        {
            GameManager.GameState.Lobby => "{0} Players Ready",
            _ => "{0} Players Left"
        };
        
        if (playerCountText != null && PlayerManager.Instance != null)
        {
            playerCountText.text = string.Format(playerCountStr, PlayerManager.Instance.GetAliveCount());
        }
    }

    // Round Number
    void UpdateRoundNumber()
    {
        if (roundNumberText != null && GameManager.Instance != null)
        {
            roundNumberText.text = $"Round {GameManager.Instance.CurrentRound}";
        }
    }

    // Round Timer (text-only, legacy)
    public void UpdateRoundTimer(float seconds)
    {
        if (roundTimerText != null)
        {
            roundTimerText.text = $"Time: {Mathf.Ceil(seconds):F0}s";
        }
    }

    // Visual Timer (radial fill with color changes)
    private int lastSecondDisplay;
    private float lastSecondChangeTime;
    public void UpdateVisualTimer(float timeRemaining, float totalTime)
    {
        if (timerFillImage == null) return;

        float fillAmount = timeRemaining / totalTime;
        //timerFillImage.fillAmount = fillAmount;
        
        timerFillImage.fillAmount = fillAmount;
        if (fillAmount > 0.5f)
            timerFillImage.color = Color.green;
        else if (fillAmount > 0.25f)
            timerFillImage.color = Color.yellow;
        else
            timerFillImage.color = Color.red;

        // Color shift based on remaining tim

        // Optional: Show seconds inside timer
        if (timerSecondsText != null)
        {
            var checkSecondsDisplay = Mathf.CeilToInt(timeRemaining);
            if (checkSecondsDisplay != lastSecondDisplay)
            {
                lastSecondDisplay = checkSecondsDisplay;
                
                timerSecondsText.text = lastSecondDisplay.ToString();

                lastSecondChangeTime = Time.time;
            }
            
            var timeSinceSecondChange = Time.time - lastSecondChangeTime;

            var scale = Vector3.one * (1.2f - (timeSinceSecondChange * 0.3f));
            var scale2 = scale;
            scale2.x *= 1.1f;
            timerSecondsText.transform.localScale = scale;
        }
    }

    public void ShowVisualTimer()
    {
        if (visualTimerPanel != null)
            visualTimerPanel.SetActive(true);
    }

    public void HideVisualTimer()
    {
        if (visualTimerPanel != null)
            visualTimerPanel.SetActive(false);
    }

    // Round Title
    public void ShowRoundInfo(string title, string message, float autoTimeout = 2f)
    {
        Debug.Log($"ShowRoundInfo - title: {title}, autoTimeout: {autoTimeout}");
        
        if (roundTitlePanel != null)
        {
            roundTitlePanel.SetActive(true);
        }

        if (roundTitleText != null)
        {
            roundTitleText.text = title;
            roundMessageText.text = message;
        }

        if (autoTimeout > 0f)
        {
            StartCoroutine(AutoTimeOutRoundInfoPanel(autoTimeout));
        }
    }

    IEnumerator AutoTimeOutRoundInfoPanel(float time)
    {
        yield return new WaitForSeconds(time);
        
        HideRoundTitle();
    }
    

    public void HideRoundTitle()
    {
        if (roundTitlePanel != null)
        {
            roundTitlePanel.SetActive(false);
        }
    }

    // Reaction Round Indicators
    public void ShowRedIndicator()
    {
        if (redIndicator != null)
        {
            redIndicator.SetActive(true);
        }
        if (greenIndicator != null)
        {
            greenIndicator.SetActive(false);
        }
    }

    public void ShowGreenIndicator()
    {
        if (redIndicator != null)
        {
            redIndicator.SetActive(false);
        }
        if (greenIndicator != null)
        {
            greenIndicator.SetActive(true);
        }
    }

    public void HideReactionIndicators()
    {
        if (redIndicator != null)
        {
            redIndicator.SetActive(false);
        }
        if (greenIndicator != null)
        {
            greenIndicator.SetActive(false);
        }
    }

    // Lobby Mask Display
    void AddLobbyMask(Player player)
    {

        RefreshLobbyMasks();
        return;
        
        if (gridMaskPrefab == null)
        {
            Debug.LogWarning("⚠️ Lobby mask container or prefab not assigned!");
            return;
        }

        // Don't add if already exists
        if (lobbyMaskDisplayPlayerDict.ContainsKey(player.Id))
        {
            return;
        }

        var targetLobbyMaskContainer = lobbyMaskDisplayPlayerDict.Count > lobbyMaskContainerCount ? lobbyMaskContainer1 : lobbyMaskContainer2;

        // Create mask display
        MaskDisplayGrid maskObj = Instantiate<MaskDisplayGrid>(gridMaskPrefab, targetLobbyMaskContainer);

        if (MaskManager.Instance != null)
        {
            Sprite maskSprite = MaskManager.Instance.GetMaskSprite(player.MaskId);
            if (maskSprite != null)
            {
                maskObj.mainImg.sprite = maskSprite;
            }
            else
            {
                // Fallback: colored circle
                Color color = Color.HSVToRGB((player.MaskId * 6f) / 360f, 0.7f, 0.9f);
                maskObj.mainImg.color = color;
            }
        }

        lobbyMaskDisplayPlayerDict[player.Id] = maskObj;

        Debug.Log($"✅ Added lobby mask for player {player.Id} (Mask #{player.MaskId})");
        // Grid Layout Group will automatically arrange masks!
    }

    void CreateLobbyMasks()
    {
        ClearLobbyMasks();
        
        var playerCount = PlayerManager.Instance.maxPlayerCount;

        for (int i = 0; i < playerCount; i++)
        {
            var targetLobbyMaskContainer = lobbyMaskDisplays.Count < lobbyMaskContainerCount ? lobbyMaskContainer1 : lobbyMaskContainer2;
            
            MaskDisplayGrid maskObj = Instantiate(gridMaskPrefab, targetLobbyMaskContainer);
            
            lobbyMaskDisplays.Add(maskObj);
            
        }
        
        RefreshLobbyMasks();
    }

    void RefreshLobbyMasks()
    {
        var allPlayers = PlayerManager.Instance.GetAllPlayers();
        
        Debug.Log($"allPlayers: {allPlayers.Count}");

        for (int i = 0; i < lobbyMaskDisplays.Count; i++)
        {
            lobbyMaskDisplays[i].Reset();
        }
        lobbyMaskDisplayPlayerDict.Clear();
        

        for (int i = 0; i < lobbyMaskDisplays.Count; i++)
        {
            if (i < allPlayers.Count)
            {
                var player = allPlayers[i];
                
                lobbyMaskDisplays[i].SetPlayer(player);
                lobbyMaskDisplayPlayerDict[player.Id] =  lobbyMaskDisplays[i];
            }
        }
    }
    

    void RemoveLobbyMask(string playerId)
    {
        RefreshLobbyMasks();
        return;
        
        if (lobbyMaskDisplayPlayerDict.ContainsKey(playerId))
        {
            Destroy(lobbyMaskDisplayPlayerDict[playerId]);
            lobbyMaskDisplayPlayerDict.Remove(playerId);

            Debug.Log($"❌ Removed lobby mask for player {playerId}");
            // Grid Layout Group will automatically rearrange!
        }
    }

    public void ClearLobbyMasks()
    {
        foreach (var kvp in lobbyMaskDisplays)
        {
            if (kvp != null)
            {
                Destroy(kvp.gameObject);
            }
        }
        lobbyMaskDisplayPlayerDict.Clear();
        lobbyMaskDisplays.Clear();

        Debug.Log("🧹 Cleared all lobby masks");
    }
    
    
    public void ClearInterRoundMasks()
    {
        foreach (var maskDisplayGrid in interRoundMaskDisplays)
        {
            if (maskDisplayGrid != null)
            {
                Destroy(maskDisplayGrid.gameObject);
            }
        }
        interRoundMaskDisplays.Clear();
    }
    void CreateInterRoundMasks()
    {
        ClearInterRoundMasks();
        
        var playerCount = PlayerManager.Instance.maxPlayerCount;

        for (int i = 0; i < playerCount; i++)
        {
            MaskDisplayGrid maskObj = Instantiate(gridMaskPrefab, interRoundGridContainer);
            
            interRoundMaskDisplays.Add(maskObj);
        }
    }

    public void ShowInterRoundSummary()
    {
        interRoundSummaryGrp.SetActive(true);
        interRoundCountTxt.text = PlayerManager.Instance.numPlayersEliminatedThisRound.ToString();

        StartCoroutine(HideInterRoundSummary());
    }

    IEnumerator HideInterRoundSummary()
    {
        yield return new WaitForSeconds(3f);
        interRoundSummaryGrp.SetActive(false);
    }
    
    void RefreshInterRoundMasks()
    {
        if (interRoundMaskDisplays.Count == 0)
        {
            CreateInterRoundMasks();
        }
        
        var allPlayers = PlayerManager.Instance.GetAllPlayers();

        for (int i = 0; i < interRoundMaskDisplays.Count; i++)
        {
            interRoundMaskDisplays[i].Reset();
        }
        

        for (int i = 0; i < interRoundMaskDisplays.Count; i++)
        {
            if (i < allPlayers.Count)
            {
                var player = allPlayers[i];
                
                interRoundMaskDisplays[i].SetPlayer(player);
            }
        }
    }
    
    

    // Winner
    public void ShowWinner(int maskId)
    {
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);

            if (maskId < 0)
            {
                noWinnerGrp.gameObject.SetActive(true);
                winnerGrp.gameObject.SetActive(false);
                winnerImg.gameObject.SetActive(false);
            }
            else
            {
                noWinnerGrp.gameObject.SetActive(false);
                winnerGrp.gameObject.SetActive(true);
                winnerImg.gameObject.SetActive(true);
                winnerImg.sprite = MaskManager.Instance.GetMaskSprite(maskId);
            }
        }

        // if (winnerText != null)
        // {
        //     if (maskId > 0)
        //     {
        //         winnerText.text = $"🏆 WINNER 🏆\n\nMask #{maskId}";
        //     }
        //     else
        //     {
        //         winnerText.text = "GAME OVER\n\nNo Winner!";
        //     }
        // }
    }

    public void HideWinner()
    {
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(false);
        }
    }
}
