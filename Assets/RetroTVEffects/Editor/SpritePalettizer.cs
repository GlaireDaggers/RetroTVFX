using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class SpriteColorComparer : IEqualityComparer<Color32>
{
    public bool Equals(Color32 a, Color32 b)
    {
        if (a.a == 0 && b.a == 0) return true;

        return a.Equals(b);
    }

    public int GetHashCode(Color32 c)
    {
        return c.GetHashCode();
    }
}

public class SpritePalettizer : EditorWindow
{
    public Sprite Sprite;

    private Color32[] colorPalette = null;
    private Sprite lastSprite = null;

    [MenuItem("Window/RetroTV/Sprite Palettizer")]
    static void Init()
    {
        GetWindow<SpritePalettizer>();
    }

    void OnGUI()
    {
        Sprite = (Sprite)EditorGUILayout.ObjectField(Sprite, typeof(Sprite), false);

        if (Sprite != lastSprite)
        {
            lastSprite = Sprite;
            colorPalette = null;
        }

        if (Sprite == null)
        {
            EditorGUILayout.HelpBox("Please select a sprite", MessageType.Info);
            return;
        }

        ShowSprite();
        ShowPalette();
    }

    void ShowSprite()
    {
        Rect r = GUILayoutUtility.GetRect(this.position.width, 256f);
        GUI.Box(r, "");
        EditorGUI.DrawTextureTransparent(r, Sprite.texture, ScaleMode.ScaleToFit);
    }

    void ShowPalette()
    {
        if (GUILayout.Button("Calculate Palette"))
        {
            calcPalette();
        }

        if (colorPalette == null) return;

        GUILayout.Label("Found " + ( colorPalette.Length - 1 ) + " unique colors (plus transparency)");
    }

    void calcPalette()
    {
        string path = AssetDatabase.GetAssetPath(Sprite.texture);
        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
        if (!importer.isReadable)
        {
            Debug.Log("Sprite texture was not readable, fixing...");
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
        
        Color32[] colors = Sprite.texture.GetPixels32();

        List<Color32> palette = new List<Color32>();
        palette.Add(new Color32(0, 0, 0, 0));

        palette.AddRange(colors.Distinct(new SpriteColorComparer()));

        colorPalette = palette.ToArray();
    }
}