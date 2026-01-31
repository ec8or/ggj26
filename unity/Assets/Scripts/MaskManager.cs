using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance { get; private set; }

    [SerializeField] private Transform maskDisplayContainer;
    [SerializeField] private GameObject maskPrefab; // Image with RectTransform
    [SerializeField] private bool usePlaceholderSprites = true;

    private Dictionary<int, Sprite> maskSprites = new Dictionary<int, Sprite>();
    private List<GameObject> activeMaskDisplays = new List<GameObject>();
    private Dictionary<int, GameObject> maskIdToDisplay = new Dictionary<int, GameObject>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadMaskSprites();
    }

    void LoadMaskSprites()
    {
        if (usePlaceholderSprites)
        {
            // Create placeholder colored sprites for testing
            Debug.Log("🎭 Creating placeholder mask sprites...");

            for (int i = 1; i <= 60; i++)
            {
                Texture2D tex = new Texture2D(256, 256);
                Color color = Color.HSVToRGB((i * 6) / 360f, 0.7f, 0.9f);

                // Create a simple circle pattern
                for (int x = 0; x < 256; x++)
                {
                    for (int y = 0; y < 256; y++)
                    {
                        float distFromCenter = Vector2.Distance(new Vector2(x, y), new Vector2(128, 128));
                        if (distFromCenter < 100)
                        {
                            tex.SetPixel(x, y, color);
                        }
                        else
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                }
                tex.Apply();

                maskSprites[i] = Sprite.Create(tex, new Rect(0, 0, 256, 256), Vector2.one * 0.5f);
            }

            Debug.Log($"✅ Created {maskSprites.Count} placeholder mask sprites");
        }
        else
        {
            // Load from Resources/Masks/ folder by name (avoid alphabetical sort issue)
            Debug.Log("🎭 Loading mask sprites from Resources/Masks/...");

            for (int i = 1; i <= 60; i++)
            {
                // Load mask_X.png where X matches the maskId
                // Files: mask_0.png to mask_59.png, so mask_1.png is for maskId 1, etc.
                string fileName = $"Masks/mask_{i}";
                Sprite sprite = Resources.Load<Sprite>(fileName);

                if (sprite != null)
                {
                    maskSprites[i] = sprite;
                    Debug.Log($"✅ Loaded {fileName} for maskId {i}");
                }
                else
                {
                    Debug.LogWarning($"⚠️ Could not load sprite: {fileName}");
                }
            }

            Debug.Log($"✅ Loaded {maskSprites.Count} mask sprites from Resources");
        }
    }

    public void DisplayMasks(List<int> maskIds)
    {
        ClearMasks();

        if (maskDisplayContainer == null)
        {
            Debug.LogError("Mask display container not assigned!");
            return;
        }

        foreach (int maskId in maskIds)
        {
            if (maskPrefab == null)
            {
                Debug.LogError("Mask prefab not assigned!");
                continue;
            }

            GameObject maskObj = Instantiate(maskPrefab, maskDisplayContainer);
            Image img = maskObj.GetComponent<Image>();

            if (img != null && maskSprites.ContainsKey(maskId))
            {
                img.sprite = maskSprites[maskId];
            }

            activeMaskDisplays.Add(maskObj);
            maskIdToDisplay[maskId] = maskObj;
        }

        // Arrange in grid or horizontal line
        ArrangeMasks();
    }

    void ArrangeMasks()
    {
        if (activeMaskDisplays.Count == 0) return;

        // Simple horizontal layout
        float spacing = 450f;
        float startX = -(activeMaskDisplays.Count - 1) * spacing / 2f;

        for (int i = 0; i < activeMaskDisplays.Count; i++)
        {
            RectTransform rt = activeMaskDisplays[i].GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(startX + i * spacing, 0);
                rt.sizeDelta = new Vector2(400, 400);
            }
        }
    }

    public void ClearMasks()
    {
        foreach (var obj in activeMaskDisplays)
        {
            Destroy(obj);
        }
        activeMaskDisplays.Clear();
        maskIdToDisplay.Clear();
    }

    public void AnimateSafeTap(int maskId)
    {
        if (!maskIdToDisplay.ContainsKey(maskId)) return;

        GameObject maskObj = maskIdToDisplay[maskId];
        RectTransform rt = maskObj.GetComponent<RectTransform>();
        if (rt == null) return;

        StartCoroutine(SafeTapAnimation(rt));
    }

    private System.Collections.IEnumerator SafeTapAnimation(RectTransform rt)
    {
        Vector2 originalPos = rt.anchoredPosition;
        Vector3 originalScale = rt.localScale;

        float duration = 0.3f;
        float elapsed = 0f;

        // Animate up and scale
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeOut = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease-out

            rt.anchoredPosition = Vector2.Lerp(originalPos, originalPos + Vector2.up * 20f, easeOut);
            rt.localScale = Vector3.Lerp(originalScale, originalScale * 1.15f, easeOut);

            yield return null;
        }

        // Hold for a moment
        yield return new WaitForSeconds(0.1f);

        // Animate back
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeIn = Mathf.Pow(t, 3f); // Cubic ease-in

            rt.anchoredPosition = Vector2.Lerp(originalPos + Vector2.up * 20f, originalPos, easeIn);
            rt.localScale = Vector3.Lerp(originalScale * 1.15f, originalScale, easeIn);

            yield return null;
        }

        rt.anchoredPosition = originalPos;
        rt.localScale = originalScale;
    }

    public Sprite GetMaskSprite(int maskId)
    {
        return maskSprites.ContainsKey(maskId) ? maskSprites[maskId] : null;
    }

    public List<GameObject> GetActiveMaskDisplays()
    {
        return activeMaskDisplays;
    }
}
