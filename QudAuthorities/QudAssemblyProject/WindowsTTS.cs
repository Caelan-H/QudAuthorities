using System.Runtime.InteropServices;
using UnityEngine;

public class WindowsTTS : MonoBehaviour
{
	public static WindowsTTS instance = null;

	private static bool m_UseNVDA = false;

	private static float m_NVDAIsSpeakingTimer = -1f;

	[DllImport("WindowsTTS")]
	public static extern void Initialize();

	[DllImport("WindowsTTS")]
	public static extern void DestroySpeech();

	[DllImport("WindowsTTS")]
	public static extern void StopSpeech();

	[DllImport("WindowsTTS")]
	public static extern void AddToSpeechQueue(string s);

	[DllImport("WindowsTTS")]
	public static extern bool IsVoiceSpeaking();

	[DllImport("nvdaControllerClient")]
	internal static extern int nvdaController_testIfRunning();

	[DllImport("nvdaControllerClient", CharSet = CharSet.Auto)]
	internal static extern int nvdaController_speakText(string text);

	[DllImport("nvdaControllerClient")]
	internal static extern int nvdaController_cancelSpeech();

	private void Awake()
	{
		m_UseNVDA = nvdaController_testIfRunning() == 0;
	}

	public static bool IsScreenReaderDetected()
	{
		m_UseNVDA = nvdaController_testIfRunning() == 0;
		return m_UseNVDA;
	}

	private void Start()
	{
		if (instance == null)
		{
			instance = this;
			Initialize();
		}
		else
		{
			Debug.LogError("[Accessibility] Trying to create another Windows TTS instance, when there already is one.");
			Object.DestroyImmediate(base.gameObject);
		}
	}

	public static void Speak(string msg)
	{
		if (m_UseNVDA)
		{
			nvdaController_speakText(msg);
			m_NVDAIsSpeakingTimer += (float)msg.Length / 16f;
		}
		else
		{
			AddToSpeechQueue(msg);
		}
	}

	public static void Stop()
	{
		if (m_UseNVDA)
		{
			nvdaController_cancelSpeech();
			m_NVDAIsSpeakingTimer = 0f;
		}
		else
		{
			StopSpeech();
		}
	}

	public static bool IsSpeaking()
	{
		if (!Application.isPlaying)
		{
			return false;
		}
		if (m_UseNVDA)
		{
			return m_NVDAIsSpeakingTimer > 0f;
		}
		return IsVoiceSpeaking();
	}

	private void Update()
	{
		if (m_NVDAIsSpeakingTimer > 0f)
		{
			m_NVDAIsSpeakingTimer -= Time.unscaledDeltaTime;
		}
	}

	private void OnDestroy()
	{
		if (instance == this)
		{
			DestroySpeech();
			instance = null;
		}
	}
}
