namespace RetroFx
{
	public interface IFilterKernelTaps
	{
		public float[] LumaFilter { get; }
		public float[] ChromaFilter { get; }
	}
}