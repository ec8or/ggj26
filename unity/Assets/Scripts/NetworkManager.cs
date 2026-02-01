using UnityEngine;
using SocketIOClient;
using System.Collections.Generic;
using System;
using System.Text.Json;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [SerializeField] private string localUrl = "http://localhost:3000";
    [SerializeField] private string serverUrl = "http://localhost:3000";
    [SerializeField] private bool useServer = false;

    private SocketIOUnity socket;
    public bool IsConnected => socket != null && socket.Connected;

    // Events for other scripts to subscribe to
    public event Action<string> OnPlayerJoined;
    public event Action<string> OnPlayerLeft;
    public event Action<TapData> OnTapReceived;

    // Queues for main thread processing
    private Queue<string> playerJoinedQueue = new Queue<string>();
    private Queue<string> playerLeftQueue = new Queue<string>();
    private Queue<TapData> tapReceivedQueue = new Queue<TapData>();
    private object queueLock = new object();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        ConnectToServer();
    }

    void Update()
    {
        // Process queued events on main thread
        lock (queueLock)
        {
            while (playerJoinedQueue.Count > 0)
            {
                string playerId = playerJoinedQueue.Dequeue();
                OnPlayerJoined?.Invoke(playerId);
            }

            while (playerLeftQueue.Count > 0)
            {
                string playerId = playerLeftQueue.Dequeue();
                OnPlayerLeft?.Invoke(playerId);
            }

            while (tapReceivedQueue.Count > 0)
            {
                TapData tapData = tapReceivedQueue.Dequeue();
                OnTapReceived?.Invoke(tapData);
            }
        }
    }

    void ConnectToServer()
    {
        socket = new SocketIOUnity(GetServerUrl());

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("✅ Connected to server");
            socket.Emit("identify", new { type = "unity" });
        };

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.LogWarning("❌ Disconnected from server");
        };

        socket.OnError += (sender, e) =>
        {
            Debug.LogError($"Socket error: {e}");
        };

        // Listen for player joins
        socket.On("player_joined", response =>
        {
            try
            {
                var data = response.GetValue<JsonElement>();
                string playerId = data.GetProperty("playerId").GetString();

                if (!string.IsNullOrEmpty(playerId))
                {
                    Debug.Log($"👤 Player joined: {playerId}");
                    lock (queueLock)
                    {
                        playerJoinedQueue.Enqueue(playerId);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing player_joined: {ex.Message}");
            }
        });

        // Listen for player leaves
        socket.On("player_left", response =>
        {
            try
            {
                var data = response.GetValue<JsonElement>();
                string playerId = data.GetProperty("playerId").GetString();

                if (!string.IsNullOrEmpty(playerId))
                {
                    Debug.Log($"👋 Player left: {playerId}");
                    lock (queueLock)
                    {
                        playerLeftQueue.Enqueue(playerId);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing player_left: {ex.Message}");
            }
        });

        // Listen for tap events
        socket.On("tap", response =>
        {
            try
            {
                var data = response.GetValue<JsonElement>();
                string playerId = data.GetProperty("playerId").GetString();
                long timestamp = data.GetProperty("timestamp").GetInt64();

                if (!string.IsNullOrEmpty(playerId))
                {
                    Debug.Log($"👆 Tap from {playerId} at {timestamp}");
                    lock (queueLock)
                    {
                        tapReceivedQueue.Enqueue(new TapData {
                            PlayerId = playerId,
                            Timestamp = timestamp,
                            TapCount = 0 // Not used - SprintRound multiplies each tap by 5
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error parsing tap: {ex.Message}");
            }
        });

        socket.Connect();
        Debug.Log($"🔌 Connecting to server at {GetServerUrl()}...");
    }

    // Send game state to all clients
    public void EmitGameState(string phase, int playersAlive, int totalPlayers, int roundNumber, string roundType)
    {
        if (!IsConnected) return;

        socket.Emit("game_state", new
        {
            type = "game_state",
            phase,
            playersAlive,
            totalPlayers,
            roundNumber,
            roundType
        });
    }

    // Send elimination to specific player
    public void EmitEliminated(string playerId, string reason, int maskId, int playersRemaining)
    {
        if (!IsConnected) return;

        socket.Emit("eliminated", new
        {
            type = "eliminated",
            playerId,
            reason,
            maskId,
            playersRemaining
        });
    }

    // Assign mask to player
    public void EmitMaskAssigned(string playerId, int maskId)
    {
        if (!IsConnected) return;

        socket.Emit("mask_assigned", new
        {
            type = "mask_assigned",
            playerId,
            maskId
        });
    }

    void OnDestroy()
    {
        socket?.Disconnect();
    }

    void OnApplicationQuit()
    {
        socket?.Disconnect();
    }

    // Get server URL for QR code generation
    public string GetServerUrl()
    {
        return useServer ? serverUrl : localUrl;
    }
}

[Serializable]
public class TapData
{
    public string PlayerId;
    public long Timestamp;
    public int TapCount; // For Sprint rounds: total tap count from mobile
}
