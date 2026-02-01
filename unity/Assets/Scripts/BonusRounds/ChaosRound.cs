using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChaosRound : MonoBehaviour
{
    [Header("Chaos Settings")]
    [SerializeField] private int numFlyingMasks = 60; // Number of masks to spawn
    [SerializeField] private float maskSpeed = 300f; // Speed of flying masks
    [SerializeField] private float spinSpeed = 180f; // Rotation speed

    [Header("References")]
    [SerializeField] private Transform flyingMaskContainer; // Parent for flying masks
    [SerializeField] private GameObject maskPrefab; // Same prefab as MaskManager uses

    private List<FlyingMask> flyingMasks = new List<FlyingMask>();
    private bool waitingForSpace = false;

    void Update()
    {
        // Wait for Space press to continue
        if (waitingForSpace && Input.GetKeyDown(KeyCode.Space))
        {
            waitingForSpace = false;
        }
    }

    public void StartChaosRound()
    {
        Debug.Log("🃏 === CHAOS CARD ACTIVATED ===");
        StartCoroutine(ChaosSequence());
    }

    IEnumerator ChaosSequence()
    {
        // Show title screen
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowRoundTitle("🃏 CHAOS CARD 🃏\n\nEveryone gets a NEW mask!\n\n<size=40>Press SPACE to continue</size>");
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager not found - can't show title");
        }

        // Spawn flying masks
        SpawnFlyingMasks();

        // Randomize all masks
        if (PlayerManager.Instance != null)
        {
            int aliveCount = PlayerManager.Instance.GetAliveCount();
            Debug.Log($"🔀 Randomizing masks for {aliveCount} alive players...");
            PlayerManager.Instance.RandomizeAllMasks();
        }
        else
        {
            Debug.LogError("❌ PlayerManager not found - can't randomize masks!");
        }

        // Wait for Space press
        waitingForSpace = true;
        Debug.Log("⌨️ Waiting for SPACE press to continue...");
        while (waitingForSpace)
        {
            yield return null;
        }

        // Clear flying masks
        ClearFlyingMasks();

        // Hide title
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideRoundTitle();
        }

        Debug.Log("🃏 Chaos Round complete - masks randomized!");

        // Signal completion to GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRoundComplete();
        }
        else
        {
            Debug.LogError("❌ GameManager not found - can't advance to next round!");
        }
    }

    void SpawnFlyingMasks()
    {
        if (MaskManager.Instance == null)
        {
            Debug.LogError("❌ MaskManager not found - can't spawn flying masks!");
            return;
        }

        if (flyingMaskContainer == null)
        {
            Debug.LogWarning("⚠️ Flying mask container not assigned - spawning at root");
        }

        if (maskPrefab == null)
        {
            Debug.LogError("❌ Mask prefab not assigned!");
            return;
        }

        // Get screen bounds
        RectTransform canvasRect = flyingMaskContainer?.GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
        float screenWidth = canvasRect != null ? canvasRect.rect.width : 1920f;
        float screenHeight = canvasRect != null ? canvasRect.rect.height : 1080f;

        // Spawn masks with random positions and velocities
        for (int i = 1; i <= numFlyingMasks; i++)
        {
            Sprite maskSprite = MaskManager.Instance.GetMaskSprite(i);
            if (maskSprite == null) continue;

            GameObject maskObj = Instantiate(maskPrefab, flyingMaskContainer);
            Image img = maskObj.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = maskSprite;
            }

            RectTransform rt = maskObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Random starting position
                rt.anchoredPosition = new Vector2(
                    Random.Range(-screenWidth / 2, screenWidth / 2),
                    Random.Range(-screenHeight / 2, screenHeight / 2)
                );

                // Random size
                float size = Random.Range(100f, 250f);
                rt.sizeDelta = new Vector2(size, size);
            }

            // Add flying behavior
            FlyingMask flyingMask = new FlyingMask
            {
                gameObject = maskObj,
                rectTransform = rt,
                velocity = new Vector2(
                    Random.Range(-maskSpeed, maskSpeed),
                    Random.Range(-maskSpeed, maskSpeed)
                ),
                spinSpeed = Random.Range(-spinSpeed, spinSpeed),
                screenWidth = screenWidth,
                screenHeight = screenHeight
            };

            flyingMasks.Add(flyingMask);
        }

        // Start animating
        StartCoroutine(AnimateFlyingMasks());

        Debug.Log($"✨ Spawned {flyingMasks.Count} flying masks!");
    }

    IEnumerator AnimateFlyingMasks()
    {
        while (flyingMasks.Count > 0)
        {
            foreach (var mask in flyingMasks)
            {
                if (mask.rectTransform == null) continue;

                // Move
                mask.rectTransform.anchoredPosition += mask.velocity * Time.deltaTime;

                // Spin
                mask.rectTransform.Rotate(0, 0, mask.spinSpeed * Time.deltaTime);

                // Bounce off edges
                Vector2 pos = mask.rectTransform.anchoredPosition;
                if (pos.x < -mask.screenWidth / 2 || pos.x > mask.screenWidth / 2)
                {
                    mask.velocity.x *= -1;
                }
                if (pos.y < -mask.screenHeight / 2 || pos.y > mask.screenHeight / 2)
                {
                    mask.velocity.y *= -1;
                }
            }

            yield return null;
        }
    }

    void ClearFlyingMasks()
    {
        foreach (var mask in flyingMasks)
        {
            if (mask.gameObject != null)
            {
                Destroy(mask.gameObject);
            }
        }
        flyingMasks.Clear();

        Debug.Log("🧹 Cleared all flying masks");
    }

    private class FlyingMask
    {
        public GameObject gameObject;
        public RectTransform rectTransform;
        public Vector2 velocity;
        public float spinSpeed;
        public float screenWidth;
        public float screenHeight;
    }
}
