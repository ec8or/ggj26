using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskDisplayGrid : MonoBehaviour
{

    public GameObject borderActiveGrp;
    public GameObject borderInactiveGrp;
    public Image mainImg;
    public GameObject outGrp;

    
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
        borderActiveGrp.SetActive(player != null);
        borderInactiveGrp.SetActive(player == null);
        
        mainImg.gameObject.SetActive(player != null);

        outGrp.SetActive(player != null && !player.IsAlive);
        
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
            
            mainImg.color = player.IsAlive ? Color.white : Color.white * 0.3f;
        }
    }
    
    
}
