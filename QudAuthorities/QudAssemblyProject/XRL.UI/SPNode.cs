using XRL.World.Skills;

namespace XRL.UI;

public class SPNode
{
	public SkillEntry Skill;

	public PowerEntry Power;

	public bool Expand;

	public SPNode ParentNode;

	public bool Visible
	{
		get
		{
			if (ParentNode != null)
			{
				return ParentNode.Expand;
			}
			return true;
		}
	}

	public SPNode(SkillEntry Skill, PowerEntry Power, bool Expand, SPNode ParentNode)
	{
		this.Skill = Skill;
		this.Power = Power;
		this.Expand = Expand;
		this.ParentNode = ParentNode;
	}
}
