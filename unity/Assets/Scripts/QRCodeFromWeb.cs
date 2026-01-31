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
        // Use QR Server API (free, no auth needed)
        string apiUrl = $"https://api.qrserver.com/v1/create-qr-code/?size={qrCodeSize}x{qrCodeSize}&data={UnityWebRequest.EscapeURL(url)}";

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                qrCodeImage.texture = texture;
                Debug.Log($"✅ QR Code generated for: {url}");
            }
            else
            {
                Debug.LogError($"❌ Failed to generate QR code: {request.error}");
            }
        }
    }
}
