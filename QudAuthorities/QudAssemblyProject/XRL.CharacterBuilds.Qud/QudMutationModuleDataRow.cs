using System;
using Newtonsoft.Json;
using XRL.World.Parts.Mutation;

namespace XRL.CharacterBuilds.Qud;

[Serializable]
public class QudMutationModuleDataRow
{
	public string Mutation;

	public int Count;

	public int Variant;

	private BaseMutation _instance;

	public string variantName
	{
		get
		{
			if (_instance == null)
			{
				_instance = entry.CreateInstance();
			}
			return _instance?.GetVariants()?[Variant] ?? "";
		}
	}

	[JsonIgnore]
	public MutationEntry entry => QudMutationsModuleData.getMutationEntryByName(Mutation);
}
