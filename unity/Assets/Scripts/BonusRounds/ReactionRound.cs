using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ReactionRound : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float minRedLightTime = 2f;
    [SerializeField] private float maxRedLightTime = 5f;
    [SerializeField] private float greenLightDuration = 2f; // How long to tap after GO

    private Dictionary<string, long> playerReactionTimes = new Dictionary<string, long>();
    private HashSet<string> eliminatedDuringRed = new HashSet<string>();
    private long goSignalTime;
    private bool waitingForGo = false;
    private bool greenLightActive = false;
    private float timer;
    private int cycleCount = 0;

    void Start()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnTapReceived += OnTapReceived;
        }
    }

    public void StartReactionRound()
    {
        cycleCount = 0;

        Debug.Log("⚡ FINAL ROUND: Reaction Round - Red Light Green Light!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRoundTitle("FINAL ROUND\nRED LIGHT GREEN LIGHT");
        }

        // Display ALL remaining masks on screen
        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();
        var allMaskIds = alivePlayers.Select(p => p.MaskId).ToList();

        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.DisplayMasks(allMaskIds);
        }

        Debug.Log($"⚡ Showing all {allMaskIds.Count} remaining masks");

        // Start first cycle after delay
        Invoke(nameof(StartRedLight), 3f);
    }

    void StartRedLight()
    {
        cycleCount++;
        int aliveCount = PlayerManager.Instance.GetAliveCount();

        Debug.Log($"⚡ Cycle {cycleCount}: {aliveCount} players remaining");

        // Check if only 1 player left - they're the winner!
        if (aliveCount <= 1)
        {
            ShowWinner();
            return;
        }

        waitingForGo = true;
        greenLightActive = false;
        playerReactionTimes.Clear();
        eliminatedDuringRed.Clear();

        Debug.Log("🔴 RED LIGHT - Don't tap!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRoundTitle("RED LIGHT\n\nDON'T TAP!");
            UIManager.Instance.ShowRedIndicator();
        }

        // Wait random time before GO
        float waitTime = Random.Range(minRedLightTime, maxRedLightTime);
        Invoke(nameof(StartGreenLight), waitTime);
    }

    void StartGreenLight()
    {
        waitingForGo = false;
        greenLightActive = true;
        goSignalTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        timer = greenLightDuration;

        Debug.Log("🟢 GREEN LIGHT - TAP NOW!");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRoundTitle("GREEN LIGHT\n\nGO!!!");
            UIManager.Instance.ShowGreenIndicator();
        }
    }

    void Update()
    {
        if (!greenLightActive) return;

        timer -= Time.deltaTime;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateRoundTimer(timer);
        }

        if (timer <= 0)
        {
            EvaluateCycle();
        }
    }

    void OnTapReceived(TapData tapData)
    {
        var player = PlayerManager.Instance.GetPlayer(tapData.PlayerId);
        if (player == null || !player.IsAlive) return;

        if (waitingForGo)
        {
            // Tapped during RED - eliminate immediately!
            Debug.Log($"❌ Player {tapData.PlayerId} (Mask #{player.MaskId}) tapped during RED - ELIMINATED!");
            eliminatedDuringRed.Add(tapData.PlayerId);
            PlayerManager.Instance.EliminatePlayer(tapData.PlayerId, "too_early");

            // Remove their mask from display immediately with animation
            if (MaskManager.Instance != null)
            {
                MaskManager.Instance.AnimateEliminatedMask(player.MaskId);
            }

            return;
        }

        if (!greenLightActive) return;

        // Record reaction time (only first tap counts)
        if (!playerReactionTimes.ContainsKey(tapData.PlayerId))
        {
            long reactionTime = tapData.Timestamp - goSignalTime;
            playerReactionTimes[tapData.PlayerId] = reactionTime;

            Debug.Log($"✓ Player {tapData.PlayerId} (Mask #{player.MaskId}) reaction: {reactionTime}ms");
        }
    }

    void EvaluateCycle()
    {
        greenLightActive = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideReactionIndicators();
        }

        var alivePlayers = PlayerManager.Instance.GetAlivePlayers();

        // Find slowest player who didn't tap OR who was slowest
        string slowestPlayerId = null;
        long slowestTime = -1;

        // First priority: Anyone who didn't tap at all
        foreach (var player in alivePlayers)
        {
            if (!playerReactionTimes.ContainsKey(player.Id) && !eliminatedDuringRed.Contains(player.Id))
            {
                slowestPlayerId = player.Id;
                break; // Found non-tapper
            }
        }

        // If everyone tapped, eliminate the slowest
        if (slowestPlayerId == null && playerReactionTimes.Count > 0)
        {
            var slowest = playerReactionTimes.OrderByDescending(kvp => kvp.Value).First();
            slowestPlayerId = slowest.Key;
            slowestTime = slowest.Value;
        }

        // Eliminate the slowest/missing player
        if (slowestPlayerId != null)
        {
            var player = PlayerManager.Instance.GetPlayer(slowestPlayerId);
            if (player != null && player.IsAlive)
            {
                Debug.Log($"💀 Eliminating slowest: Mask #{player.MaskId} ({(slowestTime >= 0 ? slowestTime + "ms" : "didn't tap")})");
                PlayerManager.Instance.EliminatePlayer(slowestPlayerId, "too_slow");

                // Animate mask removal
                if (MaskManager.Instance != null)
                {
                    MaskManager.Instance.AnimateEliminatedMask(player.MaskId);
                }
            }
        }

        // Check if we're down to 1 player
        int remaining = PlayerManager.Instance.GetAliveCount();
        Debug.Log($"⚡ Cycle {cycleCount} complete. {remaining} players remaining.");

        // Short pause, then continue to next cycle (or show winner if only 1 left)
        Invoke(nameof(StartRedLight), 1.5f);
    }

    void ShowWinner()
    {
        Debug.Log("🏆 Only 1 player remaining - showing winner!");

        if (MaskManager.Instance != null)
        {
            MaskManager.Instance.ClearMasks();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRoundTitle();
            UIManager.Instance.HideReactionIndicators();
        }

        // The final alive player is the winner!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundComplete(); // Will trigger EndGame which shows the winner
        }
    }
}
