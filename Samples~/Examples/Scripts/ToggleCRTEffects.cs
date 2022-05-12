using UnityEngine;
using System.Collections;

using UnityEngine.EventSystems;
using UnityEngine.UI;
using RetroTVFX;

namespace RetroTVFX.Examples
{
    public class ToggleCRTEffects : MonoBehaviour
    {
        public CRTEffect CRTEffect;

        public LoResStandaloneInputModule InputModule;

        public Text ToggleText;
        public Text ModeText;

        private bool toggle = true;
        
        private VideoType _currentMode = VideoType.RF;

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
            if (_currentMode == VideoType.RF)
            {
                _currentMode = VideoType.Composite;
                ModeText.text = "MODE: COMPOSITE";
            }
            else if (_currentMode == VideoType.Composite)
            {
                _currentMode = VideoType.SVideo;
                ModeText.text = "MODE: S-VIDEO";
            }
            else if (_currentMode == VideoType.SVideo)
            {
                _currentMode = VideoType.VGA;
                ModeText.text = "MODE: VGA/SCART";
            }
            else if (_currentMode == VideoType.VGA)
            {
                _currentMode = VideoType.RF;
                ModeText.text = "MODE: RF";
            }

            CRTEffect.VideoMode = _currentMode;
        }
    }
}