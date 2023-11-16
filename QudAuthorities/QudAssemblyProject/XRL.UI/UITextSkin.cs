using Qud.UI;
using TMPro;
using UnityEngine;

namespace XRL.UI;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class UITextSkin : MonoBehaviour
{
	public enum Size
	{
		normal,
		header,
		subscript,
		topstatusbar,
		bottomstatusbar
	}

	private RectTransform _rt;

	private TextMeshProUGUI _tmp;

	public Size style;

	/// <summary>
	///             Call Apply() after setting this.
	///             </summary>
	[Multiline]
	public string text = "";

	public bool useBlockWrap = true;

	public int blockWrap = 72;

	protected string formattedText;

	public bool bold;

	public Color color = new Color(0.69f, 0.78f, 0.76f);

	protected bool _StripFormatting;

	private string lasttext;

	public RectTransform rectTransform => _rt ?? (_rt = GetComponent<RectTransform>());

	private TextMeshProUGUI tmp
	{
		get
		{
			if (_tmp == null)
			{
				_tmp = GetComponent<TextMeshProUGUI>();
			}
			return _tmp;
		}
	}

	public float preferredHeight => Updated().preferredHeight;

	public float preferredWidth => Updated().preferredWidth;

	public bool StripFormatting
	{
		get
		{
			return _StripFormatting;
		}
		set
		{
			_StripFormatting = value;
			formattedText = null;
		}
	}

	private TextMeshProUGUI Updated()
	{
		Apply();
		return tmp;
	}

	public void SetText(string text)
	{
		this.text = text;
		formattedText = null;
		Apply();
	}

	public void Apply()
	{
		int num = 16;
		if (style == Size.normal)
		{
			num = 16;
		}
		if (style == Size.header)
		{
			num = 24;
		}
		if (style == Size.subscript)
		{
			num = 8;
		}
		if (style == Size.topstatusbar)
		{
			num = 16;
		}
		if (style == Size.bottomstatusbar)
		{
			num = 14;
		}
		if (formattedText == null || lasttext != text)
		{
			lasttext = text;
			formattedText = RTF.FormatToRTF(text, "FF", useBlockWrap ? blockWrap : (-1), StripFormatting);
		}
		if (tmp.text != formattedText)
		{
			tmp.text = formattedText;
		}
		if (tmp.fontSize != (float)num)
		{
			tmp.fontSize = num;
		}
		if (tmp.text != formattedText)
		{
			tmp.text = formattedText;
		}
		if (tmp.color != color)
		{
			tmp.color = color;
		}
		if (bold)
		{
			if ((tmp.fontStyle & FontStyles.Bold) == 0)
			{
				tmp.fontStyle += 1;
			}
		}
		else if ((tmp.fontStyle & FontStyles.Bold) != 0)
		{
			tmp.fontStyle -= 1;
		}
	}

	public void Awake()
	{
		Apply();
	}
}
