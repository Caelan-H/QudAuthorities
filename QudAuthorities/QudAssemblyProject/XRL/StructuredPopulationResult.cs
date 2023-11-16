using System.Collections.Generic;

namespace XRL;

public class StructuredPopulationResult
{
	public string Hint;

	public List<PopulationResult> Objects = new List<PopulationResult>();

	public List<StructuredPopulationResult> ChildGroups = new List<StructuredPopulationResult>();
}
