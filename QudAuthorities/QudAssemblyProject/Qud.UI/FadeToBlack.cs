using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.UI;

namespace Qud.UI;

public class FadeToBlack : SingletonWindowBase<FadeToBlack>
{
	public enum FadeToBlackStage
	{
		FadingOut,
		FadedOut,
		FadingIn,
		FadedIn
	}

	public bool WantsToBeSeen;

	public Image image;

	public static FadeToBlackStage stage;

	private bool Active;

	private long Start;

	private float Duration = 3f;

	private CanvasGroup group;

	public static void FadeOut(float duration)
	{
		FadeOut(duration, ConsoleLib.Console.ColorUtility.ColorMap['k']);
	}

	public static void FadeOut(float duration, Color color)
	{
		stage = FadeToBlackStage.FadingOut;
		WindowBase.queueUIAction(delegate
		{
			stage = FadeToBlackStage.FadingOut;
			SingletonWindowBase<FadeToBlack>.instance.image.color = color;
			SingletonWindowBase<FadeToBlack>.instance.WantsToBeSeen = true;
			SingletonWindowBase<FadeToBlack>.instance.Start = WindowBase.gameTimeMS;
			SingletonWindowBase<FadeToBlack>.instance.Duration = duration;
			SingletonWindowBase<FadeToBlack>.instance.Active = true;
			SingletonWindowBase<FadeToBlack>.instance.GetComponent<CanvasGroup>().alpha = 0f;
			SingletonWindowBase<FadeToBlack>.instance.Show();
		});
	}

	public static void FadeIn(float duration)
	{
		FadeIn(duration, ConsoleLib.Console.ColorUtility.ColorMap['k']);
	}

	public static void FadeIn(float duration, Color color)
	{
		stage = FadeToBlackStage.FadingIn;
		WindowBase.queueUIAction(delegate
		{
			stage = FadeToBlackStage.FadingIn;
			SingletonWindowBase<FadeToBlack>.instance.image.color = color;
			SingletonWindowBase<FadeToBlack>.instance.WantsToBeSeen = false;
			SingletonWindowBase<FadeToBlack>.instance.Start = WindowBase.gameTimeMS;
			SingletonWindowBase<FadeToBlack>.instance.Duration = duration;
			SingletonWindowBase<FadeToBlack>.instance.Active = true;
			SingletonWindowBase<FadeToBlack>.instance.GetComponent<CanvasGroup>().alpha = 1f;
			SingletonWindowBase<FadeToBlack>.instance.Show();
		});
	}

	public override void Init()
	{
		base.Init();
		stage = FadeToBlackStage.FadedIn;
	}

	public void Update()
	{
		if (!Active)
		{
			return;
		}
		if (group == null)
		{
			group = GetComponent<CanvasGroup>();
		}
		if (WantsToBeSeen)
		{
			Show();
			if (group.alpha < 1f)
			{
				group.alpha = Mathf.Lerp(0f, 1f, (float)(WindowBase.gameTimeMS - Start) / 1000f / Duration);
				return;
			}
			stage = FadeToBlackStage.FadedOut;
			Active = false;
		}
		else if (group.alpha > 0f)
		{
			group.alpha = Mathf.Lerp(1f, 0f, (float)(WindowBase.gameTimeMS - Start) / 1000f / Duration);
		}
		else
		{
			stage = FadeToBlackStage.FadedIn;
			Active = false;
			Hide();
		}
	}
}
