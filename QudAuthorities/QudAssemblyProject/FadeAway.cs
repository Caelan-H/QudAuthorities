using UnityEngine;

public class FadeAway : MonoBehaviour
{
	public float startTime;

	public float startVolume;

	public float duration = 12f;

	public float lifeTime;

	private AudioSource mysource;

	public void Awake()
	{
		mysource = base.gameObject.GetComponent<AudioSource>();
		startVolume = mysource.volume;
		startTime = Time.time;
	}

	public void Update()
	{
		if (Time.time - startTime > duration)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			mysource.volume = Mathf.Lerp(startVolume, 0f, (Time.time - startTime) / duration);
		}
	}
}
