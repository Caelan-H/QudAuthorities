using System.Collections;
using UnityEngine;
using XRL.UI;

public class CampfireSounds : MonoBehaviour
{
	public GameObject sources;

	public GameObject harmonicaSource;

	public GameObject extinguishSource;

	public float originalMusicVolume;

	public bool bPlaying;

	private float flourishTimer;

	private float harmonicaStart;

	private string[] flourishes = new string[5] { "Cooking_Chopping_1_N", "Cooking_Chopping_4_N", "Cooking_Chopping_6_N", "Cooking_Pan_1", "Cooking_Pan2" };

	public void Open()
	{
		bPlaying = true;
		if (!(Options.GetOption("OptionSound") != "Yes"))
		{
			extinguishSource.GetComponent<AudioSource>().pitch = Random.value * 0.1f - 0.05f + 1f;
			harmonicaSource.GetComponent<AudioSource>().pitch = Random.value * 0.1f - 0.05f + 1f;
			originalMusicVolume = SoundManager.MusicVolume;
			SoundManager.MusicTargetVolume = 0f;
			flourishTimer = 0f;
			harmonicaStart = -0.75f;
			extinguishSource.SetActive(value: false);
			harmonicaSource.SetActive(value: false);
			StopCoroutine(Closeout());
			sources.SetActive(value: true);
		}
	}

	public void Close()
	{
		bPlaying = false;
		extinguishSource.SetActive(value: true);
		extinguishSource.GetComponent<AudioSource>().Play();
		StartCoroutine(Closeout());
	}

	private IEnumerator Closeout()
	{
		yield return new WaitForSeconds(2f);
		SoundManager.MusicTargetVolume = originalMusicVolume;
		sources.SetActive(value: false);
		bPlaying = false;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!bPlaying)
		{
			return;
		}
		harmonicaStart += Time.deltaTime;
		if ((double)harmonicaStart > 0.25 && !harmonicaSource.activeSelf)
		{
			if (Random.value <= 0.1f)
			{
				harmonicaSource.SetActive(value: true);
			}
			harmonicaStart = 0f;
		}
		flourishTimer += Time.deltaTime;
		if (flourishTimer > 1f)
		{
			flourishTimer = 0f;
			if (Random.value <= 0.2f)
			{
				string text = flourishes[Random.Range(0, flourishes.Length - 1)];
				harmonicaSource.GetComponent<AudioSource>().PlayOneShot(SoundManager.GetClip(text)?.next());
			}
		}
	}
}
