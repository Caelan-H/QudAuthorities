using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Farport : BaseMutation
{
	public Guid FarportActivatedAbilityID = Guid.Empty;

	public Farport()
	{
		DisplayName = "Farport";
		Type = "Mental";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetItemElementsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		E.Add("travel", 1);
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "CommandFarport");
		base.Register(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandFarport")
		{
			Cell cell = ParentObject.pPhysics.CurrentCell;
			if (!ParentObject.FireEvent(Event.New("InitiateRealityDistortionLocal", "Object", ParentObject, "Mutation", this), E))
			{
				return true;
			}
			string text = Popup.AskString("Where would you like to travel?", "Joppa", 999);
			if (text != "")
			{
				string[] array = WishSearcher.SearchForZone(text).Result.Split('.');
				string text2 = array[0];
				int num = Convert.ToInt32(array[1]);
				int num2 = Convert.ToInt32(array[2]);
				int num3 = Convert.ToInt32(array[3]);
				int num4 = Convert.ToInt32(array[4]);
				int num5 = Convert.ToInt32(array[5]);
				int num6 = Stat.Random(1, 100) + XRLCore.Core.Game.Player.Body.Statistics["Intelligence"].Modifier * 10;
				if (num6 < 90)
				{
					if (num6 <= 10)
					{
						num += Stat.Roll("4d2") - 2;
						num2 += Stat.Roll("4d2") - 2;
						Popup.Show("You feel dizzy!");
					}
					else if (num6 <= 30)
					{
						num += Stat.Roll("2d2") - 2;
						num2 += Stat.Roll("2d2") - 2;
						Popup.Show("You feel very dizzy!");
					}
					else if (num6 <= 70)
					{
						num += Stat.Roll("1d2") - 1;
						num2 += Stat.Roll("1d2") - 1;
						Popup.Show("You feel disoriented!");
					}
					else if (num6 <= 90)
					{
						num3 = Stat.Roll("1d3") - 1;
						num4 = Stat.Roll("1d3") - 1;
						Popup.Show("You feel very disoriented!");
					}
					if (num < 0)
					{
						num = 0;
					}
					if (num2 < 0)
					{
						num2 = 0;
					}
					if (num > 79)
					{
						num = 79;
					}
					if (num2 > 23)
					{
						num2 = 23;
					}
				}
				if (num5 > 10)
				{
					int num7 = Stat.Random(1, 100) + XRLCore.Core.Game.Player.Body.StatMod("Intelligence") * 10;
					if (num7 < 70)
					{
						if (num7 <= 10)
						{
							num5 += Stat.Roll("2d4+10") - 9;
						}
						else if (num7 <= 40)
						{
							num5 += Stat.Roll("2d3+4") - 7;
						}
						else if (num7 <= 70)
						{
							num5 += Stat.Roll("2d2") - 4;
						}
						if (num5 < 10)
						{
							num5 = 10;
						}
					}
				}
				string zoneID = text2 + "." + num + "." + num2 + "." + num3 + "." + num4 + "." + num5;
				The.ZoneManager.SetActiveZone(zoneID);
				Cell cell2 = null;
				for (int num8 = 23; num8 >= 0; num8--)
				{
					for (int num9 = 40; num9 >= 0; num9--)
					{
						Cell cell3 = XRLCore.Core.Game.ZoneManager.ActiveZone.GetCell(num9, num8);
						if (cell3.IsReachable() && cell3.IsEmpty())
						{
							cell2 = cell3;
							break;
						}
						Cell cell4 = XRLCore.Core.Game.ZoneManager.ActiveZone.GetCell(40 - num9, num8);
						if (cell4.IsReachable() && cell4.IsEmpty())
						{
							cell2 = cell4;
							break;
						}
					}
					if (cell2 != null)
					{
						break;
					}
				}
				if (cell2 != null)
				{
					cell2.FireEvent(Event.New("InitiateRealityDistortionRemote", "Object", ParentObject, "Mutation", this, "Forced", 1));
					ParentObject.DirectMoveTo(cell2);
					The.ZoneManager.ProcessGoToPartyLeader();
					int turns = Math.Max(300 - 20 * base.Level, 5);
					CooldownMyActivatedAbility(FarportActivatedAbilityID, turns);
				}
				else
				{
					Popup.Show("You teleport into solid stone, which is very unhealthy!");
					cell.AddObject(ParentObject);
				}
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		FarportActivatedAbilityID = AddMyActivatedAbility("Farport", "CommandFarport", "Mental Mutation", null, "\u0017", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref FarportActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
