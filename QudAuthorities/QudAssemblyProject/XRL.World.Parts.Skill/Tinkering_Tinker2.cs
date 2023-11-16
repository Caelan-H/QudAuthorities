using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering_Tinker2 : BaseSkill
{
	public override bool AddSkill(GameObject GO)
	{
		if (GO.GetIntProperty("ReceivedTinker2Recipe") <= 0)
		{
			Tinkering part = GO.GetPart<Tinkering>();
			if (part != null)
			{
				part.LearnNewRecipe(3, 5);
				GO.SetIntProperty("ReceivedTinker2Recipe", 1);
			}
		}
		if (GO.IsPlayer())
		{
			TinkeringSifrah.AwardInsight();
		}
		return base.AddSkill(GO);
	}
}
