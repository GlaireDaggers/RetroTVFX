namespace JetFistGames.RetroTVFX
{

    using UnityEngine;
    using System.Collections;

    public class GraphicsUtils
    {
        private static Material defaultBlit = new Material(Shader.Find("Hidden/BlitCopy"));

        public static void Blit( Rect rect, RenderTexture src, RenderTexture dest)
        {
            Blit(rect, src, dest, defaultBlit, -1);
        }

        public static void Blit(RenderTexture src, RenderTexture dest, Material material, int pass)
        {
            Blit(new Rect(0f, 0f, 1f, 1f), src, dest, material, pass);
        }

        public static void Blit(Rect rect, RenderTexture src, RenderTexture dest, Material material, int pass)
        {
            GL.PushMatrix();
            GL.LoadOrtho();

            RenderTexture.active = dest;
            GL.Clear(true, true, Color.black);

            material.SetTexture("_MainTex", src);
            material.SetPass(pass);
            GL.Begin(GL.QUADS);
            GL.Color(Color.white);
            GL.TexCoord2(0, 0);
            GL.Vertex3(rect.x, rect.y, 0.1f);

            GL.TexCoord2(1, 0);
            GL.Vertex3(rect.xMax, rect.y, 0.1f);

            GL.TexCoord2(1, 1);
            GL.Vertex3(rect.xMax, rect.yMax, 0.1f);

            GL.TexCoord2(0, 1);
            GL.Vertex3(rect.x, rect.yMax, 0.1f);
            GL.End();

            GL.PopMatrix();
        }
    }

}