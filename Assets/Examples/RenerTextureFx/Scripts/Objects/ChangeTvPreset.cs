using UnityEngine;
using RetroFx.Presets;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RetroFx.RenerTextureFx
{
    [RequireComponent(typeof(Button))]
    public class ChangeTvPreset : MonoBehaviour
    {
        [SerializeField] private RetroTvEffectPreset _preset;
        [SerializeField] private Tv _tv;
        [SerializeField] private bool _clickOnAwake;

        public void Change()
        {
            _tv.ChangePreset(_preset);
        }

        private void Awake()
        {
            if (_clickOnAwake)
            {
                var button = GetComponent<Button>();
                button.onClick.Invoke();
                button.Select();
            }
        }
    }
}