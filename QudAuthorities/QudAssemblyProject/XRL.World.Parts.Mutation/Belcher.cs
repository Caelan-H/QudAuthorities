using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using XRL.Language;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Belcher : BaseMutation
{
	public string EventKey;

	public string Description;

	public string BelchTable;

	public string CommandName;

	public string CommandDescription;

	public string SpawnEvent = "PuffPlease";

	public string BelchVerb = "belch";

	public string BelchPreposition = "forth";

	public string BelchAdverb;

	public int puffs;

	public int MaxPuffs = 6;

	public new Guid ActivatedAbilityID;

	public Belcher()
	{
		DisplayName = "Belcher";
		Type = "Physical";
	}

	public int GetMaxDistance()
	{
		return base.Level * 5;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterPartEvent(this, EventKey);
		Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");
		Object.RegisterPartEvent(this, "BeforeDie");
		base.Register(Object);
	}

	public virtual int GetRange(int Level)
	{
		return 3 + Level;
	}

	public virtual int GetRadius()
	{
		return 4;
	}

	public virtual int GetNumber(int Level)
	{
		return Stat.Roll(1, 2) + Level / 4;
	}

	public virtual int GetCooldown(int Level)
	{
		return 210 - 10 * Level;
	}

	public virtual string GetBelchObject()
	{
		return PopulationManager.RollOneFrom(BelchTable).Blueprint;
	}

	public override string GetDescription()
	{
		return Description;
	}

	public override string GetLevelText(int Level)
	{
		return Description;
	}

	public static bool Cast(Belcher mutation = null, string level = "5-6", bool doEffect = true, bool self = false)
	{
		if (mutation == null)
		{
			mutation = new Belcher();
			mutation.Level = level.RollCached();
			mutation.ParentObject = IComponent<GameObject>.ThePlayer;
		}
		GameObject who = mutation.ParentObject;
		if (!GameObject.validate(who))
		{
			return false;
		}
		Cell cell = who.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		List<Cell> list;
		if (self)
		{
			list = cell.GetLocalAdjacentCells(mutation.GetRadius());
		}
		else
		{
			list = mutation.PickBurst(mutation.GetRadius(), mutation.GetRange(mutation.Level), bLocked: false, AllowVis.OnlyVisible);
			if (list == null)
			{
				return false;
			}
			foreach (Cell item in list)
			{
				if (item.DistanceTo(who) > mutation.GetRadius() + mutation.GetRange(mutation.Level))
				{
					if (who.IsPlayer())
					{
						Popup.Show("That is out of range! (" + mutation.GetRange(mutation.Level) + " squares)");
					}
					return false;
				}
			}
		}
		list = list.FindAll((Cell c) => c.IsEmpty() && who.HasLOSTo(c));
		if (list == null)
		{
			return false;
		}
		if (list.Count <= 0)
		{
			return false;
		}
		list.ShuffleInPlace();
		mutation.UseEnergy(1000, "Physical Mutation Belch Urchin");
		mutation.CooldownMyActivatedAbility(mutation.ActivatedAbilityID, mutation.GetCooldown(mutation.Level));
		int num = Math.Min(mutation.GetNumber(mutation.Level), list.Count);
		List<GameObject> list2 = Event.NewGameObjectList();
		for (int i = 0; i < num; i++)
		{
			GameObject gameObject = GameObject.create(mutation.GetBelchObject());
			list[i].AddObject(gameObject);
			gameObject.MakeActive();
			if (!string.IsNullOrEmpty(mutation.SpawnEvent))
			{
				gameObject.FireEvent(mutation.SpawnEvent);
			}
			list2.Add(gameObject);
		}
		if (IComponent<GameObject>.Visible(who))
		{
			if (list2.Count > 0 && !string.IsNullOrEmpty(mutation.BelchVerb))
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>(list2.Count);
				Dictionary<string, GameObject> dictionary2 = new Dictionary<string, GameObject>(list2.Count);
				foreach (GameObject item2 in list2)
				{
					string shortDisplayName = item2.ShortDisplayName;
					if (dictionary.ContainsKey(shortDisplayName))
					{
						dictionary[shortDisplayName]++;
					}
					else
					{
						dictionary.Add(shortDisplayName, 1);
					}
					if (!dictionary2.ContainsKey(shortDisplayName))
					{
						dictionary2.Add(shortDisplayName, item2);
					}
				}
				List<string> list3 = new List<string>(dictionary.Count);
				StringBuilder stringBuilder = Event.NewStringBuilder();
				foreach (string key in dictionary.Keys)
				{
					stringBuilder.Clear();
					int num2 = dictionary[key];
					if (num2 == 1)
					{
						stringBuilder.Append(dictionary2[key].a).Append(key);
						list3.Add(stringBuilder.ToString());
					}
					else
					{
						stringBuilder.Append(Grammar.Cardinal(num2)).Append(' ').Append(Grammar.Pluralize(key));
						list3.Add(stringBuilder.ToString());
					}
				}
				stringBuilder.Clear().Append(who.The).Append(who.ShortDisplayName)
					.Compound(mutation.BelchAdverb)
					.Append(who.GetVerb(mutation.BelchVerb))
					.Compound(mutation.BelchPreposition)
					.Compound(Grammar.MakeAndList(list3))
					.Append(who.IsHostileTowards(IComponent<GameObject>.ThePlayer) ? '!' : '.');
				IComponent<GameObject>.AddPlayerMessage(stringBuilder.ToString());
			}
			if (doEffect)
			{
				ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
				int num3 = 0;
				List<List<Tuple<Cell, char>>> list4 = new List<List<Tuple<Cell, char>>>();
				foreach (GameObject item3 in list2)
				{
					List<Tuple<Cell, char>> lineTo = who.GetLineTo(item3);
					if (lineTo.Count > num3)
					{
						num3 = lineTo.Count;
					}
					list4.Add(lineTo);
				}
				for (int j = 0; j < num3; j++)
				{
					scrapBuffer.RenderBase();
					foreach (List<Tuple<Cell, char>> item4 in list4)
					{
						int num4 = num3 - item4.Count;
						int num5 = j - num4;
						if (num5 >= 0 && num5 < item4.Count)
						{
							scrapBuffer.Goto(item4[num5].Item1.X, item4[num5].Item1.Y);
							scrapBuffer.Write("&G*");
							for (int k = num5 - 1; k >= num5 - 2 && k >= 0 && k < item4.Count; k++)
							{
								scrapBuffer.Goto(item4[k].Item1.X, item4[k].Item1.Y);
								scrapBuffer.Write("&g.");
							}
						}
					}
					scrapBuffer.Draw();
					Thread.Sleep(10);
				}
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AIGetOffensiveMutationList")
		{
			if (puffs < MaxPuffs && E.GetIntParameter("Distance") <= GetRange(base.Level) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
			{
				E.AddAICommand(EventKey);
			}
		}
		else if (E.ID == EventKey)
		{
			puffs++;
			if (!Cast(this, base.Level.ToString()))
			{
				return false;
			}
		}
		else if (E.ID == "BeforeDie")
		{
			Cast(this, base.Level.ToString(), doEffect: true, self: true);
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility(CommandName, EventKey, "Physical Mutation", null, "*");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}
