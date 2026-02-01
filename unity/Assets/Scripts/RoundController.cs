using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoundController : MonoBehaviour
{
    [Header("Round Settings")]
    [SerializeField] private float roundDuration = 5f; // Seconds to tap
    [SerializeField] private int minMasks = 3;
    [SerializeField] private int maxMasks = 5;
    [SerializeField] private int snapsPerRound = 5; // How many snaps before moving to next round
    [SerializeField] private int defaultRoundEliminationCount = 1;
    [SerializeField] private float endSnapDelay = 2f;

    private List<int> currentActiveMasks = new List<int>();
    private HashSet<string> playersWhoTapped = new HashSet<string>();
    private HashSet<int> masksAlreadyAnimated = new HashSet<int>();
    private float roundTimer;
    private bool roundActive = false;
    private int currentSnap = 0;
    private int totalSnaps = 0;
    private int currentRoundEliminationCount = 1;

    void Start()
    {
        // Subscribe to tap events
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnTapReceived += OnTapReceived;
        }
    }

    public void StartRound()
    {
        currentSnap = 0;
        currentRoundEliminationCount = defaultRoundEliminationCount;
        totalSnaps = snapsPerRound;

        // Show title screen only once at the start
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRoundInfo($"SNAP ROUND", $"{totalSnaps} Snaps!", 2f);
        }

        Invoke(nameof(StartNextSnap), 2f); // Delay before first snap
    }

    private void StartNextSnap()
    {
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.ClearMasks();
        }
        
        currentSnap++;

        if (currentSnap > totalSnaps || PlayerManager.Instance.GetAlivePlayers().Count < 3)
        {
            // All snaps complete, move to next round
            CompleteRound();
            return;
        }

        currentActiveMasks.Clear();
        playersWhoTapped.Clear();
        masksAlreadyAnimated.Clear();
        roundActive = true;
        roundTimer = roundDuration;

        // Select 3-5 random masks from alive players
        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();

        if (alivePlayers.Count == 0)
        {
            Debug.LogError("No alive players to start round!");
            CompleteRound();
            return;
        }

        int maskCount = Random.Range(minMasks, maxMasks + 1);
        maskCount = Mathf.Min(maskCount, alivePlayers.Count);

        var selectedPlayers = alivePlayers.OrderBy(x => Random.value).Take(maskCount);
        currentActiveMasks = selectedPlayers.Select(p => p.MaskId).ToList();

        // Display masks on screen
        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.DisplayMasks(currentActiveMasks);
        }

        // Update UI with snap counter
        if (UIManager.Instance != null)
        {
            //UIManager.Instance.ShowRoundInfo($"SNAP {currentSnap}/{totalSnaps}", "", 2f);
            UIManager.Instance.ShowVisualTimer();
        }

        Debug.Log($"🎭 Snap {currentSnap}/{totalSnaps} active! Masks shown: {string.Join(", ", currentActiveMasks.Select(m => $"#{m}"))}");
    }

    void Update()
    {
        if (!roundActive) return;

        roundTimer -= Time.deltaTime;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateRoundTimer(roundTimer);
            UIManager.Instance.UpdateVisualTimer(roundTimer, roundDuration);
        }

        if (roundTimer <= 0)
        {
            EndSnapRound();
        }
    }

    void OnTapReceived(TapData tapData)
    {
        if (!roundActive) return;

        playersWhoTapped.Add(tapData.PlayerId);
        Debug.Log($"✓ Tap registered from {tapData.PlayerId}");

        // Animate mask if it's a correct tap (only once per round)
        var player = PlayerManager.Instance.GetPlayer(tapData.PlayerId);
        if (player != null)
        {
            if (currentActiveMasks.Contains(player.MaskId))
            {
                // Correct tap! Animate the mask (only if not already animated)
                if (!masksAlreadyAnimated.Contains(player.MaskId))
                {
                    masksAlreadyAnimated.Add(player.MaskId);
                    if (MaskManager.Instance != null)
                    {
                        MaskManager.Instance.AnimateSafeTap(player.MaskId);
                        MaskManager.Instance.ShowTickOverlay(player.MaskId, 20f);
                    }
                }
            }
            else
            {
                //invalid tap
                PlayerManager.Instance.EliminatePlayer(player.Id, "wrong_tap");
            }
        }

        CheckEndSnapRound();
    }

    void CheckEndSnapRound()
    {
        Debug.Log($"CheckEndSnapRound - playersWhoTapped.Count: {playersWhoTapped.Count}, currentActiveMasks.Count: {currentActiveMasks.Count}, currentRoundEliminationCount: {currentRoundEliminationCount}");

        var activePlayersLeft = currentActiveMasks.Count;
        foreach (var currentActiveMask in currentActiveMasks)
        {
            var player = PlayerManager.Instance.GetPlayerByMaskId(currentActiveMask);
            if (playersWhoTapped.Contains(player.Id))
            {
                activePlayersLeft--;
            }
        }
        if(activePlayersLeft <= currentRoundEliminationCount) {
            EndSnapRound();
        }

    }
    void EndSnapRound()
    {
        roundActive = false;

        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
        int eliminatedThisSnap = 0;

        // First pass: Show overlays and eliminate
        foreach (var player in alivePlayers)
        {
            bool playerTapped = playersWhoTapped.Contains(player.Id);
            bool playerMaskWasActive = currentActiveMasks.Contains(player.MaskId);

            // Show overlay feedback
            if (playerMaskWasActive)
            {
                // Their mask was shown
                if (playerTapped)
                {
                    // Correct! Show tick
                    MaskManager.Instance.ShowTickOverlay(player.MaskId, 2.0f);
                }
                else
                {
                    // Didn't tap - show cross and eliminate
                    MaskManager.Instance.ShowCrossOverlay(player.MaskId, 2.0f);
                    PlayerManager.Instance.EliminatePlayer(player.Id, "missed_tap");
                    eliminatedThisSnap++;
                }
            }
            else
            {
                // Their mask was NOT shown
                if (playerTapped)
                {
                    // Wrong tap! Show cross and eliminate
                    PlayerManager.Instance.EliminatePlayer(player.Id, "wrong_tap");
                    eliminatedThisSnap++;
                }
                // If they didn't tap and their mask wasn't shown = correct (do nothing)
            }
        }

        Debug.Log($"📊 Snap {currentSnap}/{totalSnaps} complete. {eliminatedThisSnap} players eliminated.");

        // Start next snap after delay
        Invoke(nameof(StartNextSnap), endSnapDelay);
    }

    private void CompleteRound()
    {
        Debug.Log($"📊 Snap Round complete! All {totalSnaps} snaps finished.");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRoundTitle();
            UIManager.Instance.HideVisualTimer();
        }

        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundComplete();
        }
    }
}
