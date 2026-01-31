# Generate QR Code in Unity

**Problem**: Server URL changes (ngrok), QR code needs to match
**Solution**: Generate QR code in Unity, always shows correct URL

---

## Option 1: ZXing.Net (Recommended) ⭐

### Install

**Via Package Manager:**
1. Window → Package Manager
2. Click "+" → Add package from git URL
3. Enter: `https://github.com/micjahn/ZXing.Net.git`

**OR Manual Install:**
1. Download: https://github.com/micjahn/ZXing.Net/releases
2. Extract `ZXing.Net.dll` to `Assets/Plugins/`

### Create QR Code Generator Script

**File**: `Assets/Scripts/QRCodeGenerator.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class QRCodeGenerator : MonoBehaviour
{
    [SerializeField] private RawImage qrCodeImage;
    [SerializeField] private int qrCodeSize = 256;

    public void GenerateQRCode(string url)
    {
        var encoded = new Texture2D(qrCodeSize, qrCodeSize);
        var color32 = EncodeToQR(url, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();

        qrCodeImage.texture = encoded;
        Debug.Log($"QR Code generated for: {url}");
    }

    private Color32[] EncodeToQR(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width,
                Margin = 1
            }
        };

        return writer.Write(textForEncoding);
    }
}
```

### Add to UIManager

Update `UIManager.cs`:

```csharp
[Header("QR Code")]
[SerializeField] private QRCodeGenerator qrCodeGenerator;
[SerializeField] private TextMeshProUGUI urlText; // Show URL as text too

void Start()
{
    GenerateQRCodeForServer();
}

void GenerateQRCodeForServer()
{
    // Get server URL from NetworkManager
    string serverUrl = NetworkManager.Instance.GetServerUrl();

    // Generate QR code
    if (qrCodeGenerator != null)
    {
        qrCodeGenerator.GenerateQRCode(serverUrl);
    }

    // Show URL as text for manual entry
    if (urlText != null)
    {
        urlText.text = serverUrl;
    }
}
```

### Update NetworkManager

Add getter for server URL:

```csharp
public string GetServerUrl()
{
    return serverUrl;
}
```

### UI Setup in Unity

1. In your QRCodePanel:
   - Add **RawImage** component (name it "QRCodeImage")
   - Set size to 256x256 or larger
   - Set color to white

2. Add **TextMeshProUGUI** below QR code:
   - For displaying URL as text
   - Small font, center aligned

3. Add **QRCodeGenerator** component to QRCodePanel

4. Link references in UIManager Inspector:
   - QR Code Generator → QRCodeGenerator component
   - URL Text → TextMeshProUGUI

---

## Option 2: QR Code Unity Package (Asset Store)

### Install

1. Open Asset Store in Unity
2. Search for "QR Code Generator"
3. Free options:
   - "QR Code Generator and Reader" by Vuplex
   - "Simple QR Code Generator" by Indie Pixel

### Usage (varies by package)

Most follow this pattern:

```csharp
using QRCodeGenerator; // Package-specific namespace

public void GenerateQR(string url)
{
    QRCode qr = new QRCode(url);
    Texture2D texture = qr.Generate();
    qrCodeImage.texture = texture;
}
```

---

## Option 3: Use Web API (No Package Needed) ⭐⭐

**Easiest option** - use a web service to generate the QR code image!

### Create Script

**File**: `Assets/Scripts/QRCodeFromWeb.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class QRCodeFromWeb : MonoBehaviour
{
    [SerializeField] private RawImage qrCodeImage;
    [SerializeField] private int qrCodeSize = 256;

    public void GenerateQRCode(string url)
    {
        StartCoroutine(LoadQRCode(url));
    }

    IEnumerator LoadQRCode(string url)
    {
        // Use Google Charts API (free, no auth needed)
        string apiUrl = $"https://chart.googleapis.com/chart?cht=qr&chs={qrCodeSize}x{qrCodeSize}&chl={UnityWebRequest.EscapeURL(url)}";

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                qrCodeImage.texture = texture;
                Debug.Log($"QR Code generated for: {url}");
            }
            else
            {
                Debug.LogError($"Failed to generate QR code: {request.error}");
            }
        }
    }
}
```

**Pros:**
- ✅ No packages needed
- ✅ Works immediately
- ✅ Free
- ✅ Simple code

**Cons:**
- ⚠️ Requires internet connection
- ⚠️ Slight delay (1-2 seconds)

---

## Option 4: Pre-generate QR Code (Manual)

For a game jam quick fix:

### Generate QR Code Online

1. Go to: https://www.qr-code-generator.com/
2. Enter your ngrok URL: `https://abc123.ngrok.io`
3. Download as PNG (512x512)
4. Save to `Assets/Resources/QRCode.png`

### Display in Unity

```csharp
public class QRCodeDisplay : MonoBehaviour
{
    [SerializeField] private RawImage qrCodeImage;

    void Start()
    {
        Texture2D qrTexture = Resources.Load<Texture2D>("QRCode");
        qrCodeImage.texture = qrTexture;
    }
}
```

**Update when URL changes:**
1. Generate new QR code image
2. Replace `QRCode.png` in Resources folder
3. Unity auto-reimports

---

## Recommended Approach for GGJ26

**Use Option 3 (Web API)** - fastest setup:

### Step 1: Create Script

Copy `QRCodeFromWeb.cs` above into `Assets/Scripts/`

### Step 2: Add to Scene

1. Select QRCodePanel in hierarchy
2. Add Component → QRCodeFromWeb
3. Link RawImage reference

### Step 3: Update UIManager

```csharp
[Header("QR Code")]
[SerializeField] private QRCodeFromWeb qrCodeGenerator;
[SerializeField] private TextMeshProUGUI urlText;

void Start()
{
    ShowQRCode();
}

public void ShowQRCode()
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

    if (qrCodePanel != null)
    {
        qrCodePanel.SetActive(true);
    }
}
```

### Step 4: Update NetworkManager

Add this method:

```csharp
public string GetServerUrl()
{
    return serverUrl;
}
```

### Step 5: Test

1. Press Play
2. QR code should appear in lobby
3. Scan with phone
4. Should open your server URL!

---

## Complete UI Layout

```
┌─────────────────────────────────┐
│  CROWD CONTROL                  │
│                                 │
│  ┌───────────────────┐          │
│  │                   │          │
│  │    [QR CODE]      │          │
│  │                   │          │
│  └───────────────────┘          │
│                                 │
│  Or visit:                      │
│  https://abc123.ngrok.io        │
│                                 │
│  Players Connected: 0           │
│                                 │
│  Press SPACE to start           │
└─────────────────────────────────┘
```

---

## Troubleshooting

### QR Code doesn't appear

**Check:**
- RawImage component exists
- QRCodeGenerator script attached
- References linked in Inspector
- Console for errors

### QR Code is blank/white

**Check:**
- Internet connection (for Option 3)
- Server URL is valid (not empty)
- RawImage color is white (not transparent)

### Phone can't scan QR code

**Solutions:**
- Increase QR code size (512x512 or larger)
- Make sure it's on white background
- Increase contrast
- Try different QR code library

### "DLL not found" error (Option 1)

**Solution:**
- Download ZXing.Net correctly
- Place in Assets/Plugins/
- Restart Unity

---

## Bonus: Dynamic URL Updates

If you want to regenerate QR when URL changes:

```csharp
public class DynamicQRCode : MonoBehaviour
{
    [SerializeField] private QRCodeFromWeb qrGenerator;
    private string lastUrl;

    void Update()
    {
        string currentUrl = NetworkManager.Instance.GetServerUrl();

        if (currentUrl != lastUrl)
        {
            lastUrl = currentUrl;
            qrGenerator.GenerateQRCode(currentUrl);
            Debug.Log("QR Code updated for new URL");
        }
    }
}
```

This auto-updates if you change the server URL in Unity while running!

---

## Alternative: Show URL Only

If QR codes are troublesome, just show the URL as big text:

```csharp
[SerializeField] private TextMeshProUGUI bigUrlText;

void Start()
{
    string url = NetworkManager.Instance.GetServerUrl();
    bigUrlText.text = url;
    bigUrlText.fontSize = 48;
}
```

Players can manually type it in their phone browsers. Not as elegant, but works!

---

**Recommended**: Use **Option 3 (Web API)** for fastest implementation!

Takes 10 minutes, works reliably, and automatically shows the correct URL every time! 🎯

---

**Last Updated**: 2026-01-31
**Estimated Time**: 10-15 minutes for Option 3
