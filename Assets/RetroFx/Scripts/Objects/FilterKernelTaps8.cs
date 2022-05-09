namespace RetroFx
{
    public class FilterKernelTaps8 : IFilterKernelTaps
    {
        public float[] LumaFilter => new float[] {
           -0.0020f, -0.0009f, 0.0038f, 0.0178f, 0.0445f,
            0.0817f, 0.1214f, 0.1519f, 0.1634f
        };

        public float[] ChromaFilter => new float[] {
            0.0046f, 0.0082f, 0.0182f, 0.0353f, 0.0501f,
            0.0832f, 0.1062f, 0.1222f, 0.1280f
        };
    }
}