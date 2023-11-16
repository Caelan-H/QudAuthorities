using ConsoleLib.Console;
using Kobold;
using UnityEngine;
using UnityEngine.UI;
using XRL.World;

public class UIThreeColorProperties : MonoBehaviour
{
	[SerializeField]
	private Color _Foreground = Color.white;

	[SerializeField]
	private Color _Detail = Color.black;

	[SerializeField]
	private Color _Background = new Color(0f, 0f, 0f, 0f);

	public Image image;

	public bool Updated;

	private Material uiMateral;

	public Color Foreground
	{
		get
		{
			return _Foreground;
		}
		set
		{
			if (_Foreground != value)
			{
				_Foreground = value;
				UpdateColors();
			}
		}
	}

	public Color Detail
	{
		get
		{
			return _Detail;
		}
		set
		{
			if (_Detail != value)
			{
				_Detail = value;
				UpdateColors();
			}
		}
	}

	public Color Background
	{
		get
		{
			return _Background;
		}
		set
		{
			if (_Background != value)
			{
				_Background = value;
				UpdateColors();
			}
		}
	}

	public void SetColors(Color F, Color D, Color B)
	{
		_Foreground = F;
		_Detail = D;
		_Background = B;
		UpdateColors();
	}

	public void FromConsoleChar(ConsoleChar c)
	{
		if (c.Char == '\0')
		{
			image.sprite = SpriteManager.GetUnitySprite(c.Tile);
			SetColors(c.Foreground, c.Detail, c.Background);
		}
		else
		{
			image.sprite = SpriteManager.GetUnitySprite("Text/" + (int)c.Char + ".bmp");
			SetColors(c.Background, c.Foreground, c.Foreground);
		}
	}

	public void FromRenderEvent(RenderEvent e)
	{
		FromRenderable(e);
	}

	public void FromRenderable(IRenderable e, bool transparentBlackBackgrounds = true)
	{
		if (e == null)
		{
			SetColors(Color.clear, Color.clear, Color.clear);
			return;
		}
		ColorChars value = (e?.getColorChars()).Value;
		if (e != null && e.getTile() != null)
		{
			image.sprite = SpriteManager.GetUnitySprite(e.getTile());
			SetColors(ConsoleLib.Console.ColorUtility.ColorMap[value.foreground], (value.detail == '\0') ? Color.clear : ConsoleLib.Console.ColorUtility.ColorMap[value.detail], (transparentBlackBackgrounds && value.background == 'k') ? Color.clear : ConsoleLib.Console.ColorUtility.ColorMap[value.background]);
		}
		else if (e != null && e.getRenderString() != null)
		{
			image.sprite = SpriteManager.GetUnitySprite("Text/" + (int)e.getRenderString()[0] + ".bmp");
			SetColors(ConsoleLib.Console.ColorUtility.ColorMap[value.background], ConsoleLib.Console.ColorUtility.ColorMap[value.foreground], (transparentBlackBackgrounds && value.background == 'k') ? Color.clear : ConsoleLib.Console.ColorUtility.ColorMap[value.background]);
		}
		else
		{
			Debug.LogWarning("What is this render" + e);
		}
	}

	private void UpdateColors()
	{
		if (!(image == null))
		{
			Material material = Object.Instantiate(image.material);
			if (!(material == null) && material.name.StartsWith("UI Tile Material"))
			{
				material.name = "UI Tile Material (new)";
				material.SetColor("_Foreground", _Foreground);
				material.SetColor("_Detail", _Detail);
				material.SetColor("_Background", _Background);
				image.material = material;
			}
		}
	}

	private void Awake()
	{
		if (image.material == null || !image.material.name.StartsWith("UI Tile Material"))
		{
			image.material = Resources.Load<Material>("Materials/UI Tile Material");
		}
		UpdateColors();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
