using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class PaxKlanqMadness : GoalHandler
{
	private bool bCanFight;

	private int Style = -1;

	private int Stage;

	public override bool Finished()
	{
		return false;
	}

	public override bool CanFight()
	{
		return bCanFight;
	}

	public override void TakeAction()
	{
		if (base.ParentObject.IsPlayer())
		{
			Pop();
			return;
		}
		if (Style == -1)
		{
			Style = Stat.Random(0, 1);
		}
		if (Style == 0)
		{
			if (Stage == 0)
			{
				if (base.ParentObject.HasPart("Body"))
				{
					List<BodyPart> equippedParts = base.ParentObject.Body.GetEquippedParts();
					for (int i = 0; i < equippedParts.Count; i++)
					{
						GameObject equipped = equippedParts[i].Equipped;
						if (base.ParentObject.FireEvent(Event.New("CommandUnequipObject", "BodyPart", equippedParts[i])) && equipped != null && equipped.IsValid() && base.ParentObject.CurrentCell != null && equipped.Equipped == null)
						{
							Cell randomLocalAdjacentCell = base.ParentObject.CurrentCell.GetRandomLocalAdjacentCell();
							if (randomLocalAdjacentCell != null)
							{
								equipped.pPhysics.CurrentCell = randomLocalAdjacentCell;
								randomLocalAdjacentCell.AddObject(equipped);
								GoalHandler.AddPlayerMessage(base.ParentObject.The + base.ParentObject.DisplayNameOnly + "&y" + base.ParentObject.GetVerb("toss") + " " + equipped.a + equipped.DisplayNameOnly + " aside.");
							}
						}
					}
				}
				Stage++;
				base.ParentObject.UseEnergy(1000);
				return;
			}
			if (Stage == 1)
			{
				PushGoal(new WanderRandomly(40));
				Stage++;
				base.ParentObject.UseEnergy(1000);
				return;
			}
			if (Stage == 2)
			{
				base.ParentObject.ApplyEffect(new Asleep(100));
				Stage++;
				base.ParentObject.UseEnergy(1000);
				Pop();
				return;
			}
		}
		if (Style == 1)
		{
			if (Stage == 0)
			{
				if (base.ParentObject.CurrentCell != null)
				{
					GameObject gameObject = base.ParentObject.CurrentCell.ParentZone.FindClosestObjectWithTag(base.ParentObject, "Wall");
					if (gameObject != null)
					{
						bCanFight = true;
						PushGoal(new Kill(gameObject));
					}
					Stage++;
					base.ParentObject.UseEnergy(1000);
				}
				return;
			}
			if (Stage == 1)
			{
				base.ParentObject.ApplyEffect(new Asleep(100));
				Stage++;
				base.ParentObject.UseEnergy(1000);
				Pop();
				return;
			}
		}
		if (Style != 2)
		{
			return;
		}
		if (Stage == 0)
		{
			if (base.ParentObject.IsVisible())
			{
				base.ParentObject.ParticleText("KLANQ!", 'O');
				MessageQueue.AddPlayerMessage(base.ParentObject.The + base.ParentObject.DisplayNameOnlyStripped + " shouts {{O|KLANQ}}!");
			}
			base.ParentObject.UseEnergy(1000);
			if (5.in100())
			{
				Stage++;
			}
		}
		else if (Stage == 1)
		{
			base.ParentObject.ApplyEffect(new Asleep(100));
			Stage++;
			base.ParentObject.UseEnergy(1000);
			Pop();
		}
	}
}
