using UnityEngine;
using System.Collections;

using UnityStandardAssets.ImageEffects;

using UnityEngine.EventSystems;
using UnityEngine.UI;

using JetFistGames.RetroTVFX;

public class ToggleCRTEffects : MonoBehaviour
{
    public CRTEffect CRTEffect;

    public LoResStandaloneInputModule InputModule;

    public Text ToggleText;
    public Text ModeText;

    private bool toggle = true;
    
    private VideoType currentMode = VideoType.RF;

    public void Toggle()
    {
        toggle = !toggle;

        ToggleText.text = toggle ? "DISABLE CRT MODE" : "ENABLE CRT MODE";
        
        CRTEffect.enabled = toggle;
        InputModule.FisheyeX = toggle ? CRTEffect.Curvature : 0f;
        InputModule.FisheyeY = toggle ? CRTEffect.Curvature : 0f;
    }

    public void NextMode()
    {
        if (currentMode == VideoType.RF)
        {
            currentMode = VideoType.Composite;
            ModeText.text = "MODE: COMPOSITE";
        }
        else if (currentMode == VideoType.Composite)
        {
            currentMode = VideoType.SVideo;
            ModeText.text = "MODE: S-VIDEO";
        }
        else if (currentMode == VideoType.SVideo)
        {
            currentMode = VideoType.VGA;
            ModeText.text = "MODE: VGA/SCART";
        }
        else if (currentMode == VideoType.VGA)
        {
            currentMode = VideoType.RF;
            ModeText.text = "MODE: RF";
        }

        CRTEffect.VideoMode = currentMode;
    }
}