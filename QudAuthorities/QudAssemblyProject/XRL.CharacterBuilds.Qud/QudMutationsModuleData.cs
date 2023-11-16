using System.Collections.Generic;
using Newtonsoft.Json;

namespace XRL.CharacterBuilds.Qud;

public class QudMutationsModuleData : AbstractEmbarkBuilderModuleData
{
	public int mp = -1;

	public List<QudMutationModuleDataRow> selections = new List<QudMutationModuleDataRow>();

	[JsonIgnore]
	public static List<MutationCategory> categories => MutationFactory.GetCategories();

	public static MutationEntry getMutationEntryByName(string name)
	{
		return MutationFactory.GetMutationEntryByName(name);
	}
}
