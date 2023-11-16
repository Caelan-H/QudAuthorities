using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace XRL.World.Tinkering;

public static class TinkeringHelpers
{
	public static string TinkeredItemDisplayName(string Blueprint)
	{
		GameObject gameObject = GameObject.createSample(Blueprint);
		StripForTinkering(gameObject);
		string displayName = gameObject.GetDisplayName(int.MaxValue, null, "Tinkering", AsIfKnown: true, Single: false, NoConfusion: true);
		gameObject.Obliterate();
		return displayName;
	}

	public static string TinkeredItemShortDisplayName(string Blueprint)
	{
		GameObject gameObject = GameObject.createSample(Blueprint);
		StripForTinkering(gameObject);
		string displayName = gameObject.GetDisplayName(int.MaxValue, null, "Tinkering", AsIfKnown: true, Single: false, NoConfusion: true, NoColor: false, Stripped: false, ColorOnly: false, Visible: true, WithoutEpithet: false, Short: true);
		gameObject.Obliterate();
		return displayName;
	}

	public static void ProcessTinkeredItem(GameObject GO)
	{
		StripForTinkering(GO);
		GO.MakeUnderstood();
		GO.SetIntProperty("TinkeredItem", 1);
	}

	public static void StripForTinkering(GameObject GO)
	{
		EnergyCellSocket energyCellSocket = GO.GetPart("EnergyCellSocket") as EnergyCellSocket;
		if (energyCellSocket?.Cell != null)
		{
			GameObject cell = energyCellSocket.Cell;
			CellChangedEvent.Send(null, GO, cell, null);
			energyCellSocket.Cell = null;
			cell.Obliterate();
		}
		if (GO.GetPart("MagazineAmmoLoader") is MagazineAmmoLoader magazineAmmoLoader)
		{
			magazineAmmoLoader.Ammo = null;
		}
		GO.ForeachPartDescendedFrom(delegate(IEnergyCell P)
		{
			P.TinkerInitialize();
		});
		GO.LiquidVolume?.Empty();
	}

	public static bool ConsiderStandardScrap(GameObject GO)
	{
		return GO.HasTagOrProperty("Scrap");
	}

	public static bool ConsiderScrap(GameObject GO, GameObject who = null)
	{
		if (ConsiderStandardScrap(GO))
		{
			return true;
		}
		if (who?.GetPart("Tinkering_Disassemble") is Tinkering_Disassemble tinkering_Disassemble && tinkering_Disassemble.CheckScrapToggle(GO))
		{
			return true;
		}
		return false;
	}

	public static bool CanBeDisassembled(GameObject GO, GameObject who = null)
	{
		if (!(GO.GetPart("TinkerItem") is TinkerItem tinkerItem))
		{
			return false;
		}
		return tinkerItem.CanBeDisassembled(who);
	}
}
