using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Nectar_RelicTonic_Applicator : IPart
{
	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "ApplyTonic");
		base.Register(Object);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetUtilityScoreEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetUtilityScoreEvent E)
	{
		if (E.Damage == null && (E.ForPermission || E.Actor.Health() < 0.1))
		{
			E.ApplyScore(1);
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyTonic")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Target");
			string text = "";
			if (ParentObject != null && ParentObject.IsTemporary && !gameObjectParameter.IsTemporary)
			{
				text += "The experience is fleeting.";
			}
			else
			{
				gameObjectParameter.PermuteRandomMutationBuys();
				if (gameObjectParameter.IsTrueKin())
				{
					if (gameObjectParameter.HasStat("AP"))
					{
						gameObjectParameter.GetStat("AP").BaseValue += 2;
						text += "{{C|You gain 2 attribute points!}}";
					}
				}
				else if (50.in100())
				{
					int num = Stat.Random(1, 6);
					string text2 = "Strength";
					if (num == 1)
					{
						text2 = "Strength";
					}
					if (num == 2)
					{
						text2 = "Intelligence";
					}
					if (num == 3)
					{
						text2 = "Ego";
					}
					if (num == 4)
					{
						text2 = "Agility";
					}
					if (num == 5)
					{
						text2 = "Willpower";
					}
					if (num == 6)
					{
						text2 = "Toughness";
					}
					if (gameObjectParameter.HasStat(text2))
					{
						gameObjectParameter.GetStat(text2).BaseValue += 2;
						text = text + "{{C|You gain 2 points of " + text2 + "!}}";
					}
				}
				else if (gameObjectParameter.HasStat("MP"))
				{
					gameObjectParameter.GainMP(2);
					text += "{{C|You gain 2 mutation points!}}";
				}
				if (gameObjectParameter.IsPlayer())
				{
					string text3 = "You taste life as it was distilled by the Eaters, Qud's primordial masons.";
					if (!string.IsNullOrEmpty(text))
					{
						text3 = text3 + "\n\n" + text;
					}
					Popup.Show(text3);
				}
			}
		}
		return base.FireEvent(E);
	}
}
