using System.Collections.Generic;
using XRL.World.Parts;

namespace XRL.World;

public class InventoryObject
{
	public string Blueprint = "";

	public string Number = "1";

	public int Chance = 100;

	public bool BoostModChance;

	public bool NoEquip;

	public bool NoSell;

	public bool NotReal;

	public bool Full;

	public int? CellChance;

	public int? CellFullChance;

	public string CellType;

	public Dictionary<string, string> StringProperties;

	public Dictionary<string, int> IntProperties;

	private InventoryObject()
	{
	}

	public InventoryObject(string Blueprint, string Number, int Chance, bool NoEquip, bool NoSell, bool NotReal, bool Full, int? CellChance, int? CellFullChance, string CellType, Dictionary<string, string> StringProperties, Dictionary<string, int> IntProperties)
		: this()
	{
		this.Blueprint = Blueprint;
		this.Number = Number;
		this.Chance = Chance;
		this.NoEquip = NoEquip;
		this.NoSell = NoSell;
		this.NotReal = NotReal;
		this.Full = Full;
		this.CellChance = CellChance;
		this.CellFullChance = CellFullChance;
		this.CellType = CellType;
		this.StringProperties = StringProperties;
		this.IntProperties = IntProperties;
	}

	public InventoryObject(InventoryObject Source)
	{
		CopyFrom(Source);
	}

	public void CopyFrom(InventoryObject IO)
	{
		Blueprint = IO.Blueprint;
		Number = IO.Number;
		Chance = IO.Chance;
		BoostModChance = IO.BoostModChance;
		NoEquip = IO.NoEquip;
		NoSell = IO.NoSell;
		NotReal = IO.NotReal;
		Full = IO.Full;
		CellChance = IO.CellChance;
		CellFullChance = IO.CellFullChance;
		CellType = IO.CellType;
		StringProperties = IO.StringProperties;
		IntProperties = IO.IntProperties;
	}

	public bool NeedsToPreconfigureObject()
	{
		if (!CellChance.HasValue && !CellFullChance.HasValue && CellType == null && StringProperties == null)
		{
			return IntProperties != null;
		}
		return true;
	}

	public void PreconfigureObject(GameObject obj)
	{
		if ((CellChance.HasValue || CellFullChance.HasValue || CellType != null) && obj.GetPart("EnergyCellSocket") is EnergyCellSocket energyCellSocket)
		{
			if (CellChance.HasValue)
			{
				energyCellSocket.ChanceSlotted = CellChance.Value;
			}
			if (CellFullChance.HasValue)
			{
				energyCellSocket.ChanceFullCell = CellFullChance.Value;
			}
			if (CellType != null)
			{
				energyCellSocket.SlottedType = CellType;
			}
		}
		if (StringProperties != null)
		{
			foreach (KeyValuePair<string, string> stringProperty in StringProperties)
			{
				obj.SetStringProperty(stringProperty.Key, stringProperty.Value);
			}
		}
		if (IntProperties == null)
		{
			return;
		}
		foreach (KeyValuePair<string, int> intProperty in IntProperties)
		{
			obj.SetIntProperty(intProperty.Key, intProperty.Value);
		}
	}

	public override string ToString()
	{
		return Blueprint + " x" + Number;
	}
}
