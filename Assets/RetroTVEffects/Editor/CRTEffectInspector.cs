using UnityEngine;
using UnityEditor;
using System.Collections;

using UnityEditor.AnimatedValues;

using JetFistGames.RetroTVFX;

[CustomEditor(typeof(CRTEffect))]
public class CRTEffectInspector : Editor
{
    SerializedProperty videoMode;

    SerializedProperty displaySizeX;
    SerializedProperty displaySizeY;
    SerializedProperty enableCurvature;
    SerializedProperty curvature;
    SerializedProperty tvOverlay;
    SerializedProperty stretchToFillScreen;
    SerializedProperty aspectRatio;

    SerializedProperty enablePixelMask;
    SerializedProperty pixelMaskTexture;
    SerializedProperty pixelMaskRepeatX;
    SerializedProperty pixelMaskRepeatY;
    SerializedProperty pixelMaskBrightness;

    SerializedProperty enableBurstCountAnimation;
    SerializedProperty antiFlicker;

    SerializedProperty enableRollingFlicker;
    SerializedProperty rollingFlickerStrength;
    SerializedProperty rollingFlickerSyncTime;

    SerializedProperty iqOffset;
    SerializedProperty iqOffsetX;
    SerializedProperty iqOffsetY;
    SerializedProperty iqScale;
    SerializedProperty iqScaleX;
    SerializedProperty iqScaleY;

    SerializedProperty quantizeRGB;
    SerializedProperty rBits;
    SerializedProperty gBits;
    SerializedProperty bBits;

    SerializedProperty rfNoise;
	SerializedProperty lumaSharpen;

	AnimBool showTVCurvatureProperties;
    AnimBool showPixelMaskProperties;
    AnimBool showRollingFlickerProperties;
    AnimBool showNTSCFlickerProperties;
    AnimBool showAntiFlicker;
    AnimBool showRGBBits;
    AnimBool showQuantizeRGB;
    AnimBool showRFNoise;

    private string[] videoModes = new string[]
    {
        "RF",
        "Composite",
        "S-Video",
        "Component",
        "VGA \u2215 SCART",
        "VGA \u2215 SCART (Fast)"
    };

    private AnimBool newFoldout(bool val)
    {
        var animBool = new AnimBool(val);
        animBool.valueChanged.AddListener(Repaint);
        return animBool;
    }

    void OnEnable()
    {
        videoMode = serializedObject.FindProperty("VideoMode");

        displaySizeX = serializedObject.FindProperty("DisplaySizeX");
        displaySizeY = serializedObject.FindProperty("DisplaySizeY");
        enableCurvature = serializedObject.FindProperty("EnableTVCurvature");
        curvature = serializedObject.FindProperty("Curvature");
        tvOverlay = serializedObject.FindProperty("TVOverlay");

        stretchToFillScreen = serializedObject.FindProperty("StretchToDisplay");
        aspectRatio = serializedObject.FindProperty("AspectRatio");

        enablePixelMask = serializedObject.FindProperty("EnablePixelMask");
        pixelMaskTexture = serializedObject.FindProperty("PixelMaskTexture");
        pixelMaskRepeatX = serializedObject.FindProperty("MaskRepeatX");
        pixelMaskRepeatY = serializedObject.FindProperty("MaskRepeatY");
        pixelMaskBrightness = serializedObject.FindProperty("PixelMaskBrightness");

        enableBurstCountAnimation = serializedObject.FindProperty("EnableBurstCountAnimation");
        antiFlicker = serializedObject.FindProperty("AntiFlicker");

        enableRollingFlicker = serializedObject.FindProperty("EnableRollingFlicker");
        rollingFlickerStrength = serializedObject.FindProperty("RollingFlickerFactor");
        rollingFlickerSyncTime = serializedObject.FindProperty("RollingVSyncTime");

        quantizeRGB = serializedObject.FindProperty("QuantizeRGB");
        rBits = serializedObject.FindProperty("RBits");
        gBits = serializedObject.FindProperty("GBits");
        bBits = serializedObject.FindProperty("BBits");

        iqOffset = serializedObject.FindProperty("IQOffset");
        iqOffsetX = iqOffset.FindPropertyRelative("x");
        iqOffsetY = iqOffset.FindPropertyRelative("y");
        iqScale = serializedObject.FindProperty("IQScale");
        iqScaleX = iqScale.FindPropertyRelative("x");
        iqScaleY = iqScale.FindPropertyRelative("y");

        rfNoise = serializedObject.FindProperty("RFNoise");
		lumaSharpen = serializedObject.FindProperty("LumaSharpen");

        showTVCurvatureProperties = newFoldout(enableCurvature.boolValue);
        showPixelMaskProperties = newFoldout(enablePixelMask.boolValue);
        showRollingFlickerProperties = newFoldout(enableRollingFlicker.boolValue);

        showNTSCFlickerProperties = newFoldout(videoMode.enumValueIndex == (int)VideoType.Composite || videoMode.enumValueIndex == (int)VideoType.SVideo);
        showAntiFlicker = newFoldout(enableBurstCountAnimation.boolValue);

        showRGBBits = newFoldout(quantizeRGB.boolValue);
        showQuantizeRGB = newFoldout(videoMode.enumValueIndex != (int)VideoType.VGAFast);
        showRFNoise = newFoldout(videoMode.enumValueIndex == (int)VideoType.RF);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        videoMode.enumValueIndex = EditorGUILayout.Popup("Video Mode", videoMode.enumValueIndex, videoModes);

        showQuantizeRGB.target = videoMode.enumValueIndex != (int)VideoType.VGAFast;
        using (var group = new EditorGUILayout.FadeGroupScope(showQuantizeRGB.faded))
        {
            if (group.visible)
            {
                GUILayout.Space(10f);

                EditorGUILayout.LabelField("Quantize RGB", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(quantizeRGB);
                showRGBBits.target = quantizeRGB.boolValue;

                using (var group2 = new EditorGUILayout.FadeGroupScope(showRGBBits.faded))
                {
                    if (group2.visible)
                    {
                        EditorGUI.indentLevel++;

                        EditorGUILayout.PropertyField(rBits);
                        EditorGUILayout.PropertyField(gBits);
                        EditorGUILayout.PropertyField(bBits);

                        EditorGUI.indentLevel--;
                    }
                }
            }
        }

        GUILayout.Space(10f);

        EditorGUILayout.LabelField("Display Properties", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(displaySizeX);
        EditorGUILayout.PropertyField(displaySizeY);

        EditorGUILayout.PropertyField(stretchToFillScreen);
        EditorGUILayout.PropertyField(aspectRatio);

        GUILayout.Space(10f);

        EditorGUILayout.PropertyField(enableCurvature);
        showTVCurvatureProperties.target = enableCurvature.boolValue;
        using (var group = new EditorGUILayout.FadeGroupScope(showTVCurvatureProperties.faded))
        {
            if (group.visible)
            {
                EditorGUILayout.PropertyField(curvature);
                EditorGUILayout.PropertyField(tvOverlay);
            }
        }

        GUILayout.Space(10f);

        EditorGUILayout.PropertyField(enablePixelMask);
        showPixelMaskProperties.target = enablePixelMask.boolValue;
        using (var group = new EditorGUILayout.FadeGroupScope(showPixelMaskProperties.faded))
        {
            if (group.visible)
            {
                EditorGUILayout.PropertyField(pixelMaskTexture);
                EditorGUILayout.PropertyField(pixelMaskRepeatX);
                EditorGUILayout.PropertyField(pixelMaskRepeatY);
                EditorGUILayout.PropertyField(pixelMaskBrightness);
            }
        }

        EditorGUI.indentLevel--;

        GUILayout.Space(10f);

        EditorGUILayout.LabelField("Rolling Sync Flicker", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(enableRollingFlicker);
        showRollingFlickerProperties.target = enableRollingFlicker.boolValue;
        using (var group = new EditorGUILayout.FadeGroupScope(showRollingFlickerProperties.faded))
        {
            if (group.visible)
            {
                EditorGUILayout.PropertyField(rollingFlickerStrength);
                EditorGUILayout.PropertyField(rollingFlickerSyncTime);
            }
        }
        EditorGUI.indentLevel--;

        bool showNTSCFlickerProps = videoMode.enumValueIndex != (int)VideoType.VGA && videoMode.enumValueIndex != (int)VideoType.VGAFast;
        showNTSCFlickerProperties.target = showNTSCFlickerProps;

        GUILayout.Space(10f);

        showRFNoise.target = videoMode.enumValueIndex == (int)VideoType.RF;

        using (var group = new EditorGUILayout.FadeGroupScope(showRFNoise.faded))
        {
            if (group.visible)
            {
                EditorGUILayout.LabelField("RF Noise", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(rfNoise);

                EditorGUI.indentLevel--;

                GUILayout.Space(10f);
            }
        }
		
        using (var group = new EditorGUILayout.FadeGroupScope(showNTSCFlickerProperties.faded))
        {
            if (group.visible)
            {
                EditorGUILayout.LabelField("YIQ Filter", EditorStyles.boldLabel);

                EditorGUI.indentLevel++;

                iqScaleX.floatValue = EditorGUILayout.Slider("Chroma Scale X", iqScaleX.floatValue, 0f, 1f);
                iqScaleY.floatValue = EditorGUILayout.Slider("Chroma Scale Y", iqScaleY.floatValue, 0f, 1f);

                iqOffsetX.floatValue = EditorGUILayout.Slider("Chroma Offset X", iqOffsetX.floatValue, -0.5f, 0.5f);
                iqOffsetY.floatValue = EditorGUILayout.Slider("Chroma Offset Y", iqOffsetY.floatValue, -0.5f, 0.5f);

				if (videoMode.enumValueIndex == (int)VideoType.RF || videoMode.enumValueIndex == (int)VideoType.Composite)
				{
					lumaSharpen.floatValue = EditorGUILayout.Slider("Luma Sharpness", lumaSharpen.floatValue, 0f, 4f);
				}

                EditorGUI.indentLevel--;

                if (videoMode.enumValueIndex != (int)VideoType.Component)
                {
                    GUILayout.Space(10f);

                    EditorGUILayout.LabelField("NTSC Scanline Flicker", EditorStyles.boldLabel);

                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(enableBurstCountAnimation);
                    showAntiFlicker.target = enableBurstCountAnimation.boolValue;
                    using (var group2 = new EditorGUILayout.FadeGroupScope(showAntiFlicker.faded))
                    {
                        if (group2.visible)
                        {
                            EditorGUILayout.PropertyField(antiFlicker);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}