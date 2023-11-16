using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Encounters;

public class EncounterTable
{
	public string Name;

	public string Density;

	public string Load;

	public int MaxChance;

	public int MaxWeight;

	public List<EncounterObjectBase> Objects = new List<EncounterObjectBase>();

	public List<EncounterMergeTable> MergeTables = new List<EncounterMergeTable>();

	private static EncounterTable NewTable = new EncounterTable();

	public void MergeWith(EncounterTable Source)
	{
		if (Source.Density != null)
		{
			Density = Source.Density;
		}
		MaxChance += Source.MaxChance;
		MaxWeight += Source.MaxWeight;
		Objects.AddRange(Source.Objects);
		MergeTables.AddRange(Source.MergeTables);
	}

	public void AddMergeTable(EncounterMergeTable NewTable)
	{
		MergeTables.Add(NewTable.MakeCopy());
		MaxWeight += NewTable.Weight;
	}

	public EncounterMergeTable RollMergeTable()
	{
		if (MaxWeight > 0)
		{
			int num = Stat.Random(0, MaxWeight - 1);
			int num2 = 0;
			int i = 0;
			for (int count = MergeTables.Count; i < count; i++)
			{
				EncounterMergeTable encounterMergeTable = MergeTables[i];
				num2 += encounterMergeTable.Weight;
				if (num2 >= num)
				{
					return encounterMergeTable;
				}
			}
		}
		else if (MergeTables.Count > 0)
		{
			MetricsManager.LogError("encounter table " + Name + " had zero weight rolling merge tables");
		}
		return null;
	}

	public EncounterTable MakeTempCopy()
	{
		NewTable.Name = Name;
		NewTable.Density = Density;
		NewTable.MaxWeight = 0;
		NewTable.MaxChance = MaxChance;
		NewTable.Objects.Clear();
		int i = 0;
		for (int count = Objects.Count; i < count; i++)
		{
			NewTable.Objects.Add(Objects[i]);
		}
		NewTable.MergeTables.Clear();
		int j = 0;
		for (int count2 = MergeTables.Count; j < count2; j++)
		{
			NewTable.AddMergeTable(MergeTables[j]);
		}
		return NewTable;
	}
}
