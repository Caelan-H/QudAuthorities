using System;
using ConsoleLib.Console;
using Kobold;
using UnityEngine;
using UnityEngine.UI;
using XRL.World;
using XRL.World.Parts;

namespace XRL.EditorFormats.Map;

public class MapFileCellRender
{
	public string Tile;

	public char Char;

	public Color Foreground = Color.grey;

	public Color Background = Color.black;

	public Color Detail = Color.black;

	public void RenderBlueprint(GameObjectBlueprint bp, MapFileCellReference cref)
	{
		if (!bp.HasPart("Render"))
		{
			return;
		}
		Background = ConsoleLib.Console.ColorUtility.ColorMap['k'];
		Foreground = ConsoleLib.Console.ColorUtility.ColorMap['y'];
		string partParameter = bp.GetPartParameter("Render", "RenderString");
		if (partParameter != null && partParameter.Length > 1)
		{
			bp.GetPart("Render").Parameters["RenderString"] = Render.ProcessRenderString(partParameter);
		}
		if (bp.GetPart("Render").Parameters.ContainsKey("DetailColor"))
		{
			Detail = ConsoleLib.Console.ColorUtility.ColorMap[bp.GetPart("Render").Parameters["DetailColor"][0]];
		}
		else
		{
			Detail = ConsoleLib.Console.ColorUtility.ColorMap['k'];
		}
		if (!bp.Tags.TryGetValue("PaintWith", out var value))
		{
			value = "";
		}
		if (bp.GetPart("Render").Parameters["RenderString"].Length > 0)
		{
			Char = bp.GetPart("Render").Parameters["RenderString"][0];
			if (bp.GetPart("Render").Parameters.ContainsKey("ColorString"))
			{
				string text = bp.GetPart("Render").Parameters["ColorString"];
				int num = text.IndexOf('&');
				if (num != -1)
				{
					Foreground = ConsoleLib.Console.ColorUtility.ColorMap[text[num + 1]];
				}
				num = text.IndexOf('^');
				if (num != -1)
				{
					Background = ConsoleLib.Console.ColorUtility.ColorMap[text[num + 1]];
				}
			}
		}
		if (bp.GetPart("Render").Parameters.ContainsKey("Tile") && !string.IsNullOrEmpty(bp.GetPart("Render").Parameters["Tile"]))
		{
			Tile = bp.GetPart("Render").Parameters["Tile"];
			Char = '\0';
			string partParameter2 = bp.GetPartParameter("Render", "TileColor", bp.GetPartParameter("Render", "ColorString", "&y"));
			int num2 = partParameter2.IndexOf('&');
			if (num2 != -1)
			{
				Foreground = ConsoleLib.Console.ColorUtility.ColorMap[partParameter2[num2 + 1]];
			}
			num2 = partParameter2.IndexOf('^');
			if (num2 != -1)
			{
				Background = ConsoleLib.Console.ColorUtility.ColorMap[partParameter2[num2 + 1]];
			}
		}
		string value3;
		if (bp.Tags.TryGetValue("PaintedFence", out var value2))
		{
			if (bp.Tags.TryGetValue("PaintedFenceAtlas", out value3))
			{
				Tile = value3;
			}
			else
			{
				Tile = "Tiles/";
			}
			Tile = Tile + value2.GetRandomSubstring(',', Trim: false, new System.Random(cref.x ^ cref.y)) + "_";
			if (cref.region.width == 1 && cref.region.height == 1)
			{
				Tile += "ew";
			}
			else
			{
				Tile += (cref.region.HasBlueprintInCell(cref.x, cref.y - 1, bp.Name, value) ? "n" : "");
				Tile += (cref.region.HasBlueprintInCell(cref.x, cref.y + 1, bp.Name, value) ? "s" : "");
				Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y, bp.Name, value) ? "e" : "");
				Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y, bp.Name, value) ? "w" : "");
			}
			if (bp.Tags.TryGetValue("PaintedFenceExtension", out value2))
			{
				Tile += value2;
			}
			else
			{
				Tile += ".bmp";
			}
		}
		else if (bp.Tags.TryGetValue("PaintedWall", out value2))
		{
			if (bp.Tags.TryGetValue("PaintedWallAtlas", out value3))
			{
				Tile = value3;
			}
			else
			{
				Tile = "Tiles/";
			}
			Tile = Tile + value2.GetRandomSubstring(',', Trim: false, new System.Random(cref.x ^ cref.y)) + "-";
			Tile += (cref.region.HasBlueprintInCell(cref.x, cref.y - 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y - 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y + 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x, cref.y + 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y + 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y - 1, bp.Name, value) ? "1" : "0");
			if (bp.Tags.TryGetValue("PaintedWallExtension", out value2))
			{
				Tile += value2;
			}
			else
			{
				Tile += ".bmp";
			}
		}
		else if (bp.Tags.TryGetValue("PaintedLiquid", out value2))
		{
			if (bp.Tags.TryGetValue("PaintedLiquidAtlas", out value3))
			{
				Tile = value3;
			}
			else
			{
				Tile = "Assets_Content_Textures_Water_";
			}
			Tile = Tile + value2 + "-";
			Tile += (cref.region.HasBlueprintInCell(cref.x, cref.y - 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y - 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x + 1, cref.y + 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x, cref.y + 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y + 1, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y, bp.Name, value) ? "1" : "0");
			Tile += (cref.region.HasBlueprintInCell(cref.x - 1, cref.y - 1, bp.Name, value) ? "1" : "0");
			if (bp.Tags.TryGetValue("PaintedWallExtension", out value2))
			{
				Tile += value2;
			}
			else
			{
				Tile += ".bmp";
			}
		}
	}

	public void To3C(UIThreeColorProperties img)
	{
		if (Char == '\0')
		{
			img.image.sprite = SpriteManager.GetUnitySprite(Tile);
			img.SetColors(Foreground, Detail, Background);
			return;
		}
		Image image = img.image;
		int @char = Char;
		image.sprite = SpriteManager.GetUnitySprite("Text/" + @char + ".bmp");
		img.SetColors(Background, Foreground, Foreground);
	}
}
