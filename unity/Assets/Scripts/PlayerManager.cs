using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    // Events for UI updates
    public event System.Action<Player> OnPlayerAdded;
    public event System.Action<string> OnPlayerRemoved;

    private Dictionary<string, Player> players = new Dictionary<string, Player>();
    private HashSet<int> usedMaskIds = new HashSet<int>();

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
        // Subscribe to network events
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnPlayerJoined += AddPlayer;
            NetworkManager.Instance.OnPlayerLeft += RemovePlayer;
        }
    }

    void AddPlayer(string playerId)
    {
        if (players.ContainsKey(playerId))
        {
            Debug.LogWarning($"Player {playerId} already exists!");
            return;
        }

        // Assign unique mask ID (1-60)
        int maskId = GetAvailableMaskId();
        if (maskId == -1)
        {
            Debug.LogWarning("⚠️ No more mask IDs available (max 60 players)!");
            return;
        }

        var player = new Player
        {
            Id = playerId,
            MaskId = maskId,
            IsAlive = true
        };

        players[playerId] = player;
        usedMaskIds.Add(maskId);

        // Notify mobile client
        NetworkManager.Instance.EmitMaskAssigned(playerId, maskId);

        // Notify UI
        OnPlayerAdded?.Invoke(player);

        Debug.Log($"✅ Player {playerId} assigned Mask #{maskId}. Total players: {players.Count}");
    }

    void RemovePlayer(string playerId)
    {
        if (!players.ContainsKey(playerId))
        {
            return;
        }

        int maskId = players[playerId].MaskId;
        usedMaskIds.Remove(maskId);
        players.Remove(playerId);

        // Notify UI
        OnPlayerRemoved?.Invoke(playerId);

        Debug.Log($"❌ Player {playerId} removed. Total players: {players.Count}");
    }

    int GetAvailableMaskId()
    {
        for (int i = 1; i <= 60; i++)
        {
            if (!usedMaskIds.Contains(i))
                return i;
        }
        return -1;
    }

    public void EliminatePlayer(string playerId, string reason)
    {
        if (!players.ContainsKey(playerId))
        {
            Debug.LogWarning($"Cannot eliminate player {playerId} - not found");
            return;
        }

        if (!players[playerId].IsAlive)
        {
            Debug.LogWarning($"Player {playerId} is already eliminated");
            return;
        }

        players[playerId].IsAlive = false;
        int playersRemaining = GetAliveCount();

        NetworkManager.Instance.EmitEliminated(playerId, reason, players[playerId].MaskId, playersRemaining);

        Debug.Log($"💀 Player {playerId} (Mask #{players[playerId].MaskId}) eliminated: {reason}. {playersRemaining} remaining.");
    }

    public List<Player> GetAlivePlayers()
    {
        return players.Values.Where(p => p.IsAlive).ToList();
    }

    public Player GetPlayer(string playerId)
    {
        return players.ContainsKey(playerId) ? players[playerId] : null;
    }

    public Player GetPlayerByMaskId(int maskId)
    {
        return players.Values.FirstOrDefault(p => p.MaskId == maskId);
    }

    public int GetAliveCount() => players.Values.Count(p => p.IsAlive);
    public int GetTotalCount() => players.Count;

    public void ResetTapCounts()
    {
        foreach (var player in players.Values)
        {
            player.TapCount = 0;
        }
    }
}

[System.Serializable]
public class Player
{
    public string Id;
    public int MaskId; // 1-60
    public bool IsAlive;
    public int TapCount; // For sprint bonus
    public long LastTapTime; // For reaction bonus
}
