using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoundController : MonoBehaviour
{
    [SerializeField] private float roundDuration = 5f; // Seconds to tap
    [SerializeField] private int minMasks = 3;
    [SerializeField] private int maxMasks = 5;

    private List<int> currentActiveMasks = new List<int>();
    private HashSet<string> playersWhoTapped = new HashSet<string>();
    private HashSet<int> masksAlreadyAnimated = new HashSet<int>();
    private float roundTimer;
    private bool roundActive = false;

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

        Debug.Log($"🎭 Round active! Masks shown: {string.Join(", ", currentActiveMasks.Select(m => $"#{m}"))}");
    }

    void Update()
    {
        if (!roundActive) return;

        roundTimer -= Time.deltaTime;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateRoundTimer(roundTimer);
        }

        if (roundTimer <= 0)
        {
            EvaluateRound();
        }
    }

    void OnTapReceived(TapData tapData)
    {
        if (!roundActive) return;

        playersWhoTapped.Add(tapData.PlayerId);
        Debug.Log($"✓ Tap registered from {tapData.PlayerId}");

        // Animate mask if it's a correct tap (only once per round)
        var player = PlayerManager.Instance.GetPlayer(tapData.PlayerId);
        if (player != null && currentActiveMasks.Contains(player.MaskId))
        {
            // Correct tap! Animate the mask (only if not already animated)
            if (!masksAlreadyAnimated.Contains(player.MaskId))
            {
                masksAlreadyAnimated.Add(player.MaskId);
                if (MaskManager.Instance != null)
                {
                    MaskManager.Instance.AnimateSafeTap(player.MaskId);
                }
            }
        }
    }

    void EvaluateRound()
    {
        roundActive = false;

        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.ClearMasks();
        }

        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
        int eliminatedThisRound = 0;

        foreach (var player in alivePlayers)
        {
            bool playerTapped = playersWhoTapped.Contains(player.Id);
            bool playerMaskWasActive = currentActiveMasks.Contains(player.MaskId);

            // Eliminate if:
            // 1. Tapped when mask was NOT active (wrong tap)
            // 2. Didn't tap when mask WAS active (missed tap)
            if (playerTapped && !playerMaskWasActive)
            {
                PlayerManager.Instance.EliminatePlayer(player.Id, "wrong_tap");
                eliminatedThisRound++;
            }
            else if (!playerTapped && playerMaskWasActive)
            {
                PlayerManager.Instance.EliminatePlayer(player.Id, "missed_tap");
                eliminatedThisRound++;
            }
        }

        Debug.Log($"📊 Round complete. {eliminatedThisRound} players eliminated.");

        // Notify GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundComplete();
        }
    }
}
