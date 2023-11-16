using ConsoleLib.Console;
using Kobold;
using QupKit;
using UnityEngine;
using XRL.UI;
using XRL.World;

public class QudItemListElement : ObjectPool<QudItemListElement>
{
	public string displayName = "<unknown>";

	public string rightText = "";

	public XRL.World.GameObject go;

	public Color foregroundColor;

	public Color backgroundColor;

	public Color detailColor;

	public string tile;

	public string category;

	public exTextureInfo spriteInfo;

	public int weight;

	public void PoolReset()
	{
		weight = 0;
		go = null;
		rightText = null;
		spriteInfo = null;
		tile = null;
		displayName = null;
		category = null;
	}

	public void InitFrom(XRL.World.GameObject go)
	{
		if (go == null)
		{
			tile = "Items/bit1.bmp";
			displayName = Sidebar.FormatToRTF("&knothing");
			rightText = "";
			foregroundColor = new Color(0f, 0f, 0f, 0f);
			detailColor = new Color(0f, 0f, 0f, 0f);
			backgroundColor = new Color(0f, 0f, 0f, 0f);
			weight = 0;
			return;
		}
		this.go = go;
		displayName = go.DisplayName;
		if (displayName.Contains("&") || displayName.Contains("^") || displayName.Contains("{{"))
		{
			displayName = Sidebar.FormatToRTF(displayName);
		}
		if (go.pPhysics != null)
		{
			weight = go.pPhysics.Weight;
			rightText = weight + " lbs";
		}
		if (go.pRender == null || go.pRender.Tile == null)
		{
			return;
		}
		tile = go.pRender.Tile;
		foregroundColor = Color.white;
		detailColor = Color.black;
		backgroundColor = new Color(0f, 0f, 0f, 0f);
		if (!string.IsNullOrEmpty(go.pRender.DetailColor))
		{
			detailColor = ConsoleLib.Console.ColorUtility.ColorMap[go.pRender.DetailColor[0]];
		}
		if (!string.IsNullOrEmpty(go.pRender.TileColor))
		{
			for (int i = 0; i < go.pRender.TileColor.Length; i++)
			{
				if (go.pRender.TileColor[i] == '&' && i < go.pRender.TileColor.Length - 1)
				{
					if (go.pRender.TileColor[i + 1] == '&')
					{
						i++;
					}
					else
					{
						foregroundColor = ConsoleLib.Console.ColorUtility.ColorMap[go.pRender.TileColor[i + 1]];
					}
				}
				if (go.pRender.TileColor[i] == '^' && i < go.pRender.TileColor.Length - 1)
				{
					if (go.pRender.TileColor[i + 1] == '^')
					{
						i++;
					}
					else
					{
						backgroundColor = ConsoleLib.Console.ColorUtility.ColorMap[go.pRender.TileColor[i + 1]];
					}
				}
			}
		}
		else
		{
			if (string.IsNullOrEmpty(go.pRender.ColorString))
			{
				return;
			}
			for (int j = 0; j < go.pRender.ColorString.Length; j++)
			{
				if (go.pRender.ColorString[j] == '&' && j < go.pRender.ColorString.Length - 1)
				{
					if (go.pRender.ColorString[j + 1] == '&')
					{
						j++;
					}
					else
					{
						foregroundColor = ConsoleLib.Console.ColorUtility.ColorMap[go.pRender.ColorString[j + 1]];
					}
				}
				if (go.pRender.ColorString[j] == '^' && j < go.pRender.ColorString.Length - 1)
				{
					if (go.pRender.ColorString[j + 1] == '^')
					{
						j++;
					}
					else
					{
						backgroundColor = ConsoleLib.Console.ColorUtility.ColorMap[go.pRender.ColorString[j + 1]];
					}
				}
			}
		}
	}

	public Sprite GenerateSprite()
	{
		if (tile == null)
		{
			return null;
		}
		if (spriteInfo == null)
		{
			spriteInfo = SpriteManager.GetTextureInfo(tile);
		}
		return SpriteManager.GetUnitySprite(spriteInfo);
	}
}
