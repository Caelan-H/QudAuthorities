using Qud.UI;
using QupKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.QupKit.Components;

public class HoverTextButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public string CommandID;

	public AudioClip HighlightClip;

	public KeyCode Hotkey;

	public bool HotkeyCapitalized;

	private AudioSource HighlightSource;

	public bool AllowHotkeyWithRewired = true;

	public Color NormalColor = new Color(1f, 1f, 1f, 0.8f);

	public Color HighlightColor = new Color(1f, 1f, 1f, 1f);

	private Text _theText;

	private Button parentButton;

	private Text theText
	{
		get
		{
			if (_theText == null)
			{
				_theText = base.gameObject.GetComponent<Text>();
			}
			return _theText;
		}
	}

	public void Update()
	{
		if ((AllowHotkeyWithRewired || !GameManager.Instance.PrereleaseInput) && parentButton != null && parentButton.interactable && Hotkey != 0 && Input.GetKeyDown(Hotkey) && HotkeyCapitalized == (GameManager.capslock || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && UIManager.instance.AllowPassthroughInput())
		{
			Input.ResetInputAxes();
			LegacyViewManager.Instance.OnCommand(CommandID);
		}
	}

	public void Awake()
	{
		if (theText != null)
		{
			theText.color = NormalColor;
		}
		if (HighlightClip != null)
		{
			HighlightSource = base.gameObject.AddComponent<AudioSource>();
			HighlightSource.volume = 1f;
			HighlightSource.clip = HighlightClip;
			HighlightSource.playOnAwake = false;
		}
		parentButton = GetComponent<Button>();
		GetComponent<Button>().onClick.AddListener(delegate
		{
			LegacyViewManager.Instance.OnCommand(CommandID);
		});
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (theText != null)
		{
			theText.color = HighlightColor;
		}
		if (HighlightSource != null)
		{
			HighlightSource.PlayOneShot(HighlightClip);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (theText != null)
		{
			theText.color = NormalColor;
		}
	}
}
