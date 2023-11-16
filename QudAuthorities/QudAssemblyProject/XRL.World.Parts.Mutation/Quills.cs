using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Quills : BaseMutation
{
	public const int MINIMUM_QUILLS_TO_FLING = 80;

	public GameObject QuillsObject;

	public int oldLevel = 1;

	public int nMaxQuills = 300;

	public int _nQuills;

	public int nPenalty;

	public int nTurnCounter;

	public Guid QuillFlingActivatedAbilityID = Guid.Empty;

	public float QuillRegenerationCounter;

	public int nQuills
	{
		get
		{
			return _nQuills;
		}
		set
		{
			if (value > nMaxQuills)
			{
				value = nMaxQuills;
			}
			if (_nQuills == value)
			{
				return;
			}
			_nQuills = value;
			if (_nQuills >= nMaxQuills / 2)
			{
				if (nPenalty > 0)
				{
					base.StatShifter.RemoveStatShift(ParentObject, "AV");
					nPenalty = 0;
				}
			}
			else if (nPenalty == 0)
			{
				nPenalty = GetAVPenalty(base.Level);
				base.StatShifter.SetStatShift(ParentObject, "AV", -nPenalty);
			}
			SetMyActivatedAbilityDisplayName(QuillFlingActivatedAbilityID, "Quill Fling [" + nQuills + " quills left]");
		}
	}

	public Quills()
	{
		DisplayName = "Quills";
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeforeApplyDamage");
		Object.RegisterPartEvent(this, "BeginTakeAction");
		Object.RegisterPartEvent(this, "CommandQuillFling");
		Object.RegisterPartEvent(this, "DefenderHit");
		Object.RegisterPartEvent(this, "TookDamage");
		base.Register(Object);
	}

	public void QuillFling(Cell C, int Quills, bool useQuills = true, bool Reactive = false, GameObject Target = null)
	{
		if (C == null || C.OnWorldMap())
		{
			return;
		}
		if (useQuills)
		{
			if (Quills > nQuills)
			{
				return;
			}
			nQuills -= Quills;
			if (nQuills < 0)
			{
				nQuills = 0;
			}
		}
		bool flag = C.IsVisible();
		if (Target == null)
		{
			Target = C.GetCombatTarget(ParentObject, IgnoreFlight: true);
			if (Target == null)
			{
				return;
			}
		}
		int num = 0;
		TextConsole textConsole = Look._TextConsole;
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		The.Core.RenderMapToBuffer(scrapBuffer);
		int num2 = (base.Level - 1) / 2;
		if (num2 > 6)
		{
			num2 = 6;
		}
		for (int i = 0; i < Quills; i++)
		{
			int num3 = Stat.RollDamagePenetrations(Stats.GetCombatAV(Target), num2, num2);
			if (num3 > 0)
			{
				scrapBuffer.Goto(C.X, C.Y);
				switch (Stat.Random(1, 4))
				{
				case 1:
					scrapBuffer.Write("&Y\\");
					break;
				case 2:
					scrapBuffer.Write("&Y-");
					break;
				case 3:
					scrapBuffer.Write("&Y/");
					break;
				case 4:
					scrapBuffer.Write("&Y|");
					break;
				}
				if (flag)
				{
					textConsole.DrawBuffer(scrapBuffer);
					Thread.Sleep(10);
				}
				for (int j = 0; j < num3; j++)
				{
					num += Stat.Random(1, 3);
				}
			}
		}
		GameObject gameObject = Target;
		int amount = num;
		bool accidental = Reactive;
		gameObject.TakeDamage(amount, "from %o quills!", "Stabbing Quills", null, null, null, ParentObject, null, null, accidental);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			float num = base.Level;
			float num2 = ((float)ParentObject.Stat("Willpower") - 16f) * 0.05f;
			float num3 = 1f - num2;
			if ((double)num3 <= 0.2)
			{
				num3 = 0.2f;
			}
			if (num2 < 1f)
			{
				num *= 1f / num3;
			}
			QuillRegenerationCounter += num;
			if (QuillRegenerationCounter >= 4f)
			{
				int num4 = (int)(QuillRegenerationCounter / 4f);
				nQuills += num4;
				QuillRegenerationCounter -= 4 * num4;
			}
			if (nQuills > nMaxQuills)
			{
				nQuills = nMaxQuills;
			}
		}
		else
		{
			if (E.ID == "BeforeApplyDamage")
			{
				if (E.GetParameter("Damage") is Damage damage && damage.HasAttribute("Quills"))
				{
					return false;
				}
				return true;
			}
			if (E.ID == "AIGetOffensiveMutationList")
			{
				if ((double)nQuills > (double)nMaxQuills * 0.65 && (E.GetIntParameter("Distance") <= 1 || ParentObject.HasEffect("Engulfed")) && IsMyActivatedAbilityAIUsable(QuillFlingActivatedAbilityID) && 25.in100())
				{
					E.AddAICommand("CommandQuillFling");
				}
			}
			else if (E.ID == "DefenderHit")
			{
				if (5.in100())
				{
					int num5 = Stat.Random(1, 4);
					if (num5 > nQuills)
					{
						num5 = nQuills;
					}
					if (num5 > 0)
					{
						if (ParentObject.IsPlayer())
						{
							IComponent<GameObject>.AddPlayerMessage("The attack breaks " + Grammar.Cardinal(num5) + " " + ((num5 == 1) ? "quill" : "quills") + "!");
						}
						nQuills -= num5;
					}
				}
			}
			else if (E.ID == "TookDamage")
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
				Damage damage2 = E.GetParameter("Damage") as Damage;
				if (gameObjectParameter != null && gameObjectParameter != ParentObject && !ParentObject.OnWorldMap() && !gameObjectParameter.HasPart("Quills") && damage2.Amount > 0 && !damage2.HasAttribute("reflected") && damage2.HasAttribute("Unarmed"))
				{
					int num6 = (int)((double)nQuills * 0.01) + Stat.Random(1, 2) - 1;
					nQuills -= num6;
					if (num6 > 0)
					{
						int num7 = (int)((float)damage2.Amount * ((float)(num6 * 3) / 100f));
						if (num7 == 0)
						{
							num7 = 1;
						}
						if (num7 > 0)
						{
							if (ParentObject.IsPlayer())
							{
								IComponent<GameObject>.AddPlayerMessage("{{G|" + gameObjectParameter.Does("impale") + " " + gameObjectParameter.itself + " on your quills and " + gameObjectParameter.GetVerb("take") + " " + num7 + " damage!}}");
							}
							else if (gameObjectParameter != null)
							{
								if (gameObjectParameter.IsPlayer())
								{
									IComponent<GameObject>.AddPlayerMessage("You impale " + gameObjectParameter.itself + " on " + ParentObject.poss("quills") + " and take " + num7 + " damage!", 'R');
								}
								else if (IComponent<GameObject>.Visible(gameObjectParameter))
								{
									if (gameObjectParameter.IsPlayerLed())
									{
										IComponent<GameObject>.AddPlayerMessage("{{r|" + gameObjectParameter.Does("impale") + " " + gameObjectParameter.itself + " on " + ParentObject.poss("quills") + " and " + gameObjectParameter.GetVerb("take") + " " + num7 + " damage!}}");
									}
									else
									{
										IComponent<GameObject>.AddPlayerMessage("{{g|" + gameObjectParameter.Does("impale") + " " + gameObjectParameter.itself + " on " + ParentObject.poss("quills") + " and " + gameObjectParameter.GetVerb("take") + " " + num7 + " damage!}}");
									}
								}
							}
							Event @event = new Event("TakeDamage");
							Damage damage3 = new Damage(num7);
							damage3.Attributes = new List<string>(damage2.Attributes);
							if (!damage3.HasAttribute("reflected"))
							{
								damage3.Attributes.Add("reflected");
							}
							@event.SetParameter("Damage", damage3);
							@event.SetParameter("Owner", ParentObject);
							@event.SetParameter("Attacker", ParentObject);
							@event.SetParameter("Message", null);
							gameObjectParameter.FireEvent(@event);
							ParentObject.FireEvent("ReflectedDamage");
						}
					}
				}
			}
			else if (E.ID == "CommandQuillFling")
			{
				if (!ParentObject.CheckFrozen())
				{
					return false;
				}
				if (ParentObject.OnWorldMap())
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You cannot do that on the world map.");
					}
					return false;
				}
				if (nQuills < 80)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You don't have enough quills! You need at least " + 80 + " quills to quill fling.");
					}
					return false;
				}
				GameObject gameObject = null;
				if (ParentObject.GetEffect("Engulfed") is Engulfed engulfed && GameObject.validate(engulfed.EngulfedBy))
				{
					int num8 = (int)((double)nQuills * 0.1);
					if (num8 <= 0)
					{
						return false;
					}
					gameObject = engulfed.EngulfedBy;
					DidX("fling", ParentObject.its + " quills", "!", null, ParentObject);
					QuillFling(gameObject.CurrentCell, num8, useQuills: true, Reactive: false, gameObject);
				}
				else
				{
					List<Cell> adjacentCells = ParentObject.CurrentCell.GetAdjacentCells();
					if (adjacentCells.Count <= 0)
					{
						return false;
					}
					int num9 = (int)((double)nQuills * 0.1) / adjacentCells.Count;
					if (num9 <= 0)
					{
						return false;
					}
					DidX("fling", ParentObject.its + " quills everywhere", "!", null, ParentObject);
					foreach (Cell item in adjacentCells)
					{
						QuillFling(item, num9);
					}
				}
				UseEnergy(1000, "Physical Mutation Quills");
			}
		}
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		return "Hundreds of needle-pointed quills cover your body.";
	}

	public int GetAV(int Level)
	{
		if (Level <= 2)
		{
			return 2;
		}
		return Level / 3 + 2;
	}

	public int GetAVPenalty(int Level)
	{
		return GetAV(Level) / 2;
	}

	public int GetQuillPenetration(int Level)
	{
		return Math.Min(6, (Level - 1) / 2);
	}

	public override string GetLevelText(int Level)
	{
		string text = GetQuillPenetration(Level).ToString();
		int aVPenalty = GetAVPenalty(Level);
		string text2 = "";
		text2 = ((Level != base.Level) ? (text2 + "+{{rules|80-120}} quills\n") : (text2 + "{{rules|" + nMaxQuills + "}} quills\n"));
		text2 = text2 + "May expel 10% of your quills in a burst around yourself ({{c|\u001a}}{{rules|" + text + "}} {{r|\u0003}}1d3)\n";
		text2 = text2 + "Regenerate quills at the approximate rate of {{rules|" + (float)Level / 4f + "}} per round\n";
		text2 = text2 + "+{{rules|" + GetAV(Level) + "}} AV as long as you retain half your quills (+{{rules|" + (GetAV(Level) - aVPenalty) + "}} AV otherwise)\n";
		text2 += "Creatures attacking you in melee may impale themselves on your quills, breaking roughly 1% of them and reflecting 3% damage per quill broken.\n";
		text2 += "Cannot wear body armor\n";
		return text2 + "Immune to other creatures' quills";
	}

	public override bool ChangeLevel(int NewLevel)
	{
		if (NewLevel != oldLevel)
		{
			int num = (NewLevel - oldLevel) * Stat.Random(80, 120);
			nMaxQuills = Math.Max(300, nMaxQuills + num);
			oldLevel = NewLevel;
		}
		nQuills = nMaxQuills;
		if (QuillsObject != null)
		{
			QuillsObject.GetPart<Armor>().AV = GetAV(base.Level);
		}
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		Body body = GO.Body;
		if (body != null)
		{
			BodyPart body2 = body.GetBody();
			if (body2 != null)
			{
				body2.ForceUnequip(Silent: true);
				QuillsObject = GameObject.create("Quills");
				GO.ForceEquipObject(QuillsObject, body2, Silent: true, 0);
				QuillFlingActivatedAbilityID = AddMyActivatedAbility("Quill Fling", "CommandQuillFling", "Physical Mutation", null, "*");
			}
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref QuillFlingActivatedAbilityID);
		CleanUpMutationEquipment(GO, ref QuillsObject);
		base.StatShifter.RemoveStatShift(GO, "AV");
		nPenalty = 0;
		return base.Unmutate(GO);
	}
}
