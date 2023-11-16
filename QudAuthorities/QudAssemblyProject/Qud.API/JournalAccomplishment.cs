using System;

namespace Qud.API;

[Serializable]
public class JournalAccomplishment : IBaseJournalEntry
{
	public enum MuralWeight
	{
		Nil = 0,
		VeryLow = 1,
		Low = 2,
		Medium = 10,
		High = 50,
		VeryHigh = 100
	}

	public enum MuralCategory
	{
		Generic,
		IsBorn,
		HasInspiringExperience,
		Treats,
		CreatesSomething,
		CommitsFolly,
		WeirdThingHappens,
		EnduresHardship,
		BodyExperienceBad,
		BodyExperienceGood,
		BodyExperienceNeutral,
		Trysts,
		VisitsLocation,
		DoesBureaucracy,
		LearnsSecret,
		FindsObject,
		DoesSomethingRad,
		DoesSomethingHumble,
		DoesSomethingDestructive,
		BecomesLoved,
		Slays,
		Resists,
		AppeasesBaetyl,
		WieldsItemInBattle,
		MeetsWithCounselors,
		CrownedSultan,
		Dies
	}

	public string category;

	public long time;

	public string muralText;

	public MuralCategory muralCategory;

	public MuralWeight muralWeight;
}
