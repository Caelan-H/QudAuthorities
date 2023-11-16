using System.Collections.Generic;

namespace XRL;

public abstract class PopulationItem
{
	public string Name;

	public string Hint;

	public uint Weight = 1u;

	public abstract List<PopulationResult> Generate(Dictionary<string, string> Vars, string DefaultHint);

	public abstract PopulationResult GenerateOne(Dictionary<string, string> Vars, string DefaultHint);

	public abstract void GenerateStructured(StructuredPopulationResult result, Dictionary<string, string> Vars);

	public abstract void GetEachUniqueObject(List<string> List);
}
