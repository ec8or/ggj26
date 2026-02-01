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

    [Header("Round Title")]
    [SerializeField] private GameObject roundTitlePanel;
    [SerializeField] private TextMeshProUGUI roundTitleText;

    [Header("Reaction Round Indicators")]
    [SerializeField] private GameObject redIndicator; // WAIT state
    [SerializeField] private GameObject greenIndicator; // GO state

    [Header("Game Over")]
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TextMeshProUGUI winnerText;

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

    // Round Timer
    public void UpdateRoundTimer(float seconds)
    {
        if (roundTimerText != null)
        {
            roundTimerText.text = $"Time: {Mathf.Ceil(seconds):F0}s";
        }
    }

    // Round Title
    public void ShowRoundTitle(string title)
    {
        if (roundTitlePanel != null)
        {
            roundTitlePanel.SetActive(true);
        }

        if (roundTitleText != null)
        {
            roundTitleText.text = title;
        }
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
        foreach (var kvp in lobbyMaskDisplayPlayerDict)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value);
            }
        }
        lobbyMaskDisplayPlayerDict.Clear();
        lobbyMaskDisplays.Clear();

        Debug.Log("🧹 Cleared all lobby masks");
    }

    // Winner
    public void ShowWinner(int maskId)
    {
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);
        }

        if (winnerText != null)
        {
            if (maskId > 0)
            {
                winnerText.text = $"🏆 WINNER 🏆\n\nMask #{maskId}";
            }
            else
            {
                winnerText.text = "GAME OVER\n\nNo Winner!";
            }
        }
    }

    public void HideWinner()
    {
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(false);
        }
    }
}
