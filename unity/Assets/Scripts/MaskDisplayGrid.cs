using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskDisplayGrid : MonoBehaviour
{

    public GameObject borderGrp;
    public Image mainImg;

    
    public Player player { get; private set; }
    
    
    public void SetPlayer(Player player)
    {
        this.player = player;
        Refresh();
    }

    public void Reset()
    {
        player = null;
        Refresh();
    }


    public void Refresh()
    {
        mainImg.gameObject.SetActive(player != null);

        if (player != null)
        {
            Sprite maskSprite = MaskManager.Instance.GetMaskSprite(player.MaskId);
            if (maskSprite != null)
            {
                mainImg.sprite = maskSprite;
            }
            else
            {
                // Fallback: colored circle
                Color color = Color.HSVToRGB((player.MaskId * 6f) / 360f, 0.7f, 0.9f);
                mainImg.color = color;
            }
        }
    }
    
    
}
