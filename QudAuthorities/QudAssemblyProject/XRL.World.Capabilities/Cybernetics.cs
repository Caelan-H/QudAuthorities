using System.Text;

namespace XRL.World.Capabilities;

public static class Cybernetics
{
	public static StringBuilder GetCreationDetails(GameObjectBlueprint cyber)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (cyber != null)
		{
			string tag = cyber.GetTag("CyberneticsCreationDetails");
			if (!string.IsNullOrEmpty(tag))
			{
				stringBuilder.Append(tag);
			}
			else
			{
				stringBuilder.Append(cyber.GetPartParameter("Description", "Short").Replace("\r", ""));
				tag = cyber.GetPartParameter("CyberneticsBaseItem", "BehaviorDescription");
				if (!string.IsNullOrEmpty(tag))
				{
					stringBuilder.Append("\n\n{{rules|").Append(tag).Append("}}");
				}
			}
			stringBuilder.Append("\n\n");
			if (cyber.HasTag("CyberneticsDestroyOnRemoval"))
			{
				stringBuilder.Append("{{rules|Destroyed when uninstalled}}\n");
			}
			stringBuilder.Append("{{rules|License points: ").Append(cyber.GetPartParameter("CyberneticsBaseItem", "Cost")).Append("}}");
		}
		else
		{
			stringBuilder.Append("{{rules|+1 Toughness}}\n\n").Append("{{R|-2 License Tier (down to 0)}}");
		}
		return stringBuilder;
	}
}
