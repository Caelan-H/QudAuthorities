using System;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class Disguised : Effect
{
	public string BlueprintName;

	public string Tile;

	public string RenderString;

	public string ColorString;

	public string TileColor;

	public string DetailColor;

	public string Appearance;

	public Disguised()
	{
		base.Duration = 1;
		base.DisplayName = "{{K|disguised}}";
	}

	public Disguised(string BlueprintName)
		: this()
	{
		this.BlueprintName = BlueprintName;
		GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(BlueprintName);
		if (blueprintIfExists == null)
		{
			throw new Exception("no such blueprint " + BlueprintName);
		}
		string partParameter = blueprintIfExists.GetPartParameter("RandomTile", "Tiles");
		if (!string.IsNullOrEmpty(partParameter))
		{
			Tile = partParameter.CachedCommaExpansion().GetRandomElement();
		}
		else
		{
			Tile = blueprintIfExists.GetPartParameter("Render", "Tile");
		}
		RenderString = XRL.World.Parts.Render.ProcessRenderString(blueprintIfExists.GetPartParameter("Render", "RenderString"));
		ColorString = blueprintIfExists.GetPartParameter("Render", "ColorString");
		TileColor = blueprintIfExists.GetPartParameter("Render", "TileColor", ColorString);
		DetailColor = blueprintIfExists.GetPartParameter("Render", "DetailColor");
	}

	public Disguised(string BlueprintName, string Appearance)
		: this(BlueprintName)
	{
		this.Appearance = Appearance;
	}

	public override int GetEffectType()
	{
		return 1;
	}

	public override string GetDetails()
	{
		return "Has the appearance of " + (Appearance ?? "another creature") + ".";
	}

	public override bool Apply(GameObject Object)
	{
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		bool useTiles = Options.UseTiles;
		if (useTiles && !string.IsNullOrEmpty(Tile))
		{
			E.Tile = Tile;
		}
		if (!string.IsNullOrEmpty(RenderString))
		{
			E.RenderString = RenderString;
		}
		string text = ((!string.IsNullOrEmpty(TileColor) && !string.IsNullOrEmpty(Tile) && useTiles) ? TileColor : ColorString);
		if (!string.IsNullOrEmpty(text))
		{
			E.ColorString = text;
		}
		if (!string.IsNullOrEmpty(DetailColor))
		{
			E.DetailColor = DetailColor;
		}
		return true;
	}

	public override bool OverlayRender(RenderEvent E)
	{
		if (Options.UseTiles && !string.IsNullOrEmpty(Tile))
		{
			E.Tile = Tile;
		}
		if (!string.IsNullOrEmpty(RenderString))
		{
			E.RenderString = RenderString;
		}
		return true;
	}
}
