using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooPita.RetroTvFx
{
    public class ChangeTvPreset : MonoBehaviour
    {
        [SerializeField] private RetroTvEffectPreset _preset;
        [SerializeField] private Tv _tv;
        public void Change()
        {
            _tv.ChangePreset(_preset);
        }
    }
}