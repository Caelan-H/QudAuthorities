using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Rules;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class MutationInfection : Effect
{
	public MutationInfection()
	{
		base.DisplayName = "&minhabited";
	}

	public MutationInfection(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 67108868;
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyGirshInfection", "Duration", base.Duration)))
		{
			return false;
		}
		return true;
	}

	public override void Register(GameObject Object)
	{
		Object.RegisterEffectEvent(this, "BeginTakeAction");
		base.Register(Object);
	}

	public override void Unregister(GameObject Object)
	{
		Object.UnregisterEffectEvent(this, "BeginTakeAction");
		base.Unregister(Object);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			base.Duration--;
			if (base.Duration == 0)
			{
				List<BaseMutation> list = new List<BaseMutation>();
				List<BaseMutation> list2 = new List<BaseMutation>();
				GameObject @object = base.Object;
				Mutations mutations = @object.GetPart("Mutations") as Mutations;
				foreach (BaseMutation mutation in mutations.MutationList)
				{
					list.Add(mutation);
				}
				int num = 0;
				for (int i = 0; i < 1 && num < 50; list2.Add(list[list.Count - 1]), list.RemoveAt(list.Count - 1), i++)
				{
					while (num <= 50)
					{
						num++;
						if (@object.IsEsper())
						{
							MutationsAPI.AddNewMentalMutation(list, 2);
						}
						else if (@object.IsChimera())
						{
							MutationsAPI.AddNewPhysicalMutation(list, 2);
						}
						else if (Stat.RandomLevelUpChoice(0, 1) == 0)
						{
							MutationsAPI.AddNewMentalMutation(list, 2);
						}
						else
						{
							MutationsAPI.AddNewPhysicalMutation(list, 2);
						}
						for (int j = 0; j < list.Count - 1; j++)
						{
							if (list[j].DisplayName == list[list.Count - 1].DisplayName)
							{
								list.RemoveAt(list.Count - 1);
								goto IL_009a;
							}
							foreach (BaseMutation item in list2)
							{
								if (item.DisplayName == list[list.Count - 1].DisplayName)
								{
									list.RemoveAt(list.Count - 1);
									goto IL_009a;
								}
							}
						}
						goto IL_01af;
						IL_009a:;
					}
					break;
					IL_01af:;
				}
				if (list2.Count > 0)
				{
					Popup.Show("You gain " + list2[0].DisplayName + "!");
					JournalAPI.AddAccomplishment("Your larva gestated and you gained the " + list2[0].DisplayName + " mutation.", null, "general", JournalAccomplishment.MuralCategory.Generic, JournalAccomplishment.MuralWeight.Medium, null, -1L);
					mutations.AddMutation(list2[0], 1);
				}
			}
		}
		return true;
	}
}
