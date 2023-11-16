using System;
using XRL.UI;
using XRL.World.Skills;

namespace XRL.World.Parts;

[Serializable]
public class TrainingBook : IPart
{
	public string Attribute;

	public string Skill;

	public override bool SameAs(IPart p)
	{
		TrainingBook trainingBook = p as TrainingBook;
		if (trainingBook.Attribute != Attribute)
		{
			return false;
		}
		if (trainingBook.Skill != Skill)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Initialize()
	{
		base.Initialize();
		ParentObject.SetStringProperty("BookID", ParentObject.id);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIBoredEvent.ID && ID != GetInventoryActionsEvent.ID && ID != GetShortDescriptionEvent.ID && ID != HasBeenReadEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(HasBeenReadEvent E)
	{
		if (HasRead(E.Actor))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		if (!HasRead(E.Actor))
		{
			InventoryActionEvent.Check(ParentObject, E.Actor, ParentObject, "Read");
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!string.IsNullOrEmpty(Attribute))
		{
			E.Postfix.AppendRules("Increases the " + Attribute + " of anyone who reads " + ParentObject.them + ".");
		}
		if (!string.IsNullOrEmpty(Skill))
		{
			PowerEntry value2;
			if (SkillFactory.Factory.SkillByClass.TryGetValue(Skill, out var value))
			{
				if (value.Initiatory == true)
				{
					E.Postfix.AppendRules("Allows anyone who reads " + ParentObject.them + " to initiate themselves in " + value.Name + ".");
				}
				else
				{
					E.Postfix.AppendRules("Teaches " + value.Name + " to anyone who reads " + ParentObject.them + ".");
				}
			}
			else if (SkillFactory.Factory.PowersByClass.TryGetValue(Skill, out value2))
			{
				E.Postfix.AppendRules("Teaches " + value2.Name + " to anyone who reads " + ParentObject.them + ".");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Read", "read", "Read", null, 'r', FireOnActor: false, (string.IsNullOrEmpty(Skill) || E.Actor.HasSkill(Skill)) ? 1 : 100, 0, Override: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Read")
		{
			string readKey = GetReadKey();
			if (!E.Actor.IsPlayer())
			{
				IComponent<GameObject>.XDidYToZ(E.Actor, "read", ParentObject);
			}
			if (!string.IsNullOrEmpty(Attribute) && !HasRead(E.Actor, readKey) && E.Actor.HasStat(Attribute))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("Your " + Attribute + " is increased by {{G|1}}!");
				}
				E.Actor.GetStat(Attribute).BaseValue++;
			}
			if (!string.IsNullOrEmpty(Skill))
			{
				PowerEntry value2;
				if (SkillFactory.Factory.SkillByClass.TryGetValue(Skill, out var value))
				{
					if (value.Initiatory == true)
					{
						if (!HasRead(E.Actor, readKey))
						{
							if (!E.Actor.HasSkill(value.Class))
							{
								E.Actor.AddSkill(value.Class);
								string text = null;
								foreach (PowerEntry value3 in value.Powers.Values)
								{
									if (E.Actor.HasSkill(value3.Class))
									{
										text = value3.Name;
										break;
									}
								}
								if (E.Actor.IsPlayer())
								{
									Popup.Show(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("guide") + " you through a rite of ancient mystery, one not for profane eyes or ears. You have begun your journey upon " + value.Name + ((text == null) ? "" : (" with initiation into " + text)) + ".");
								}
							}
							else
							{
								PowerEntry powerEntry = null;
								foreach (PowerEntry value4 in value.Powers.Values)
								{
									if (!E.Actor.HasSkill(value4.Class) && value4.MeetsRequirements(E.Actor))
									{
										powerEntry = value4;
										break;
									}
								}
								if (powerEntry == null)
								{
									Popup.ShowFail("You have completed " + value.Name + ".");
								}
								else
								{
									E.Actor.AddSkill(powerEntry.Class);
									if (E.Actor.IsPlayer())
									{
										PowerEntry powerEntry2 = null;
										foreach (PowerEntry value5 in value.Powers.Values)
										{
											if (!E.Actor.HasSkill(value5.Class) && value5.MeetsRequirements(E.Actor))
											{
												powerEntry2 = value5;
												break;
											}
										}
										Popup.Show(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("guide") + " you through a mysterious rite. Your journey upon " + value.Name + ((powerEntry2 == null) ? " has reached completion" : " continues") + (string.IsNullOrEmpty(powerEntry.Name) ? "" : (" with initiation into " + powerEntry.Name)) + ".");
									}
								}
							}
						}
						else if (E.Actor.IsPlayer())
						{
							Popup.Show("You have already gleaned as many insights into " + value.Name + " from " + ParentObject.the + ParentObject.ShortDisplayName + " as you are going to.");
						}
					}
					else if (!E.Actor.HasSkill(value.Class) && !HasRead(E.Actor, readKey))
					{
						if (E.Actor.IsPlayer())
						{
							Popup.Show("You learn " + value.Name + "!");
						}
						E.Actor.AddSkill(value.Class);
					}
				}
				else if (SkillFactory.Factory.PowersByClass.TryGetValue(Skill, out value2) && !E.Actor.HasSkill(value2.Class) && !HasRead(E.Actor, readKey))
				{
					if (E.Actor.IsPlayer())
					{
						Popup.Show("You learn " + value2.Name + "!");
					}
					E.Actor.AddSkill(value2.Class);
				}
			}
			E.Actor.SetIntProperty(readKey, 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (string.IsNullOrEmpty(Skill) && string.IsNullOrEmpty(Attribute))
		{
			AssignRandomTraining();
		}
		return base.HandleEvent(E);
	}

	public string GetReadKey()
	{
		return "HasReadBook_" + ParentObject.GetStringProperty("BookID");
	}

	public bool HasRead(GameObject who, string ReadKey)
	{
		return who.GetIntProperty(ReadKey) > 0;
	}

	public bool HasRead(GameObject who)
	{
		return HasRead(who, GetReadKey());
	}

	public void AssignRandomTraining()
	{
		if (70.in100())
		{
			PowerEntry randomElement = SkillFactory.GetPowers().GetRandomElement();
			if (randomElement.ParentSkill != null && (randomElement.Cost <= 0 || randomElement.ParentSkill.Initiatory == true))
			{
				Skill = randomElement.ParentSkill.Class;
			}
			else
			{
				Skill = randomElement.Class;
			}
		}
		else
		{
			Attribute = Statistic.Attributes.GetRandomElement();
		}
	}
}
