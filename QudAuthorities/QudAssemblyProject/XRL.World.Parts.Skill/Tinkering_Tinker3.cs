using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering_Tinker3 : BaseSkill
{
	public override bool AddSkill(GameObject GO)
	{
		if (GO.GetIntProperty("ReceivedTinker3Recipe") <= 0)
		{
			Tinkering part = GO.GetPart<Tinkering>();
			if (part != null)
			{
				part.LearnNewRecipe(6, 8);
				GO.SetIntProperty("ReceivedTinker3Recipe", 1);
			}
		}
		if (GO.IsPlayer())
		{
			TinkeringSifrah.AwardInsight();
		}
		return base.AddSkill(GO);
	}
}
