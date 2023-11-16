public class SoundRequest
{
	public enum SoundRequestType
	{
		Sound,
		Spatial,
		Music
	}

	public string Clip;

	public SoundRequestType Type;

	public Vector2i Cell = new Vector2i(0, 0);

	public bool Crossfade;

	public float CrossfadeDuration = 12f;

	public float Volume = 1f;

	public float LowPass = 1f;

	public float Pitch = 1f;

	public float PitchVariance;

	public float Delay;
}
