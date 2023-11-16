namespace XRL.World;

public interface IPronounProvider
{
	string Name { get; }

	string CapitalizedName { get; }

	/// Whether the pronoun-providing category is generic to the world, as opposed to being specific to an entity, species,
	/// or culture.
	bool Generic { get; }

	bool Generated { get; }

	/// Whether the entity is plural.
	bool Plural { get; }

	bool PseudoPlural { get; }

	/// Subjective-case personal pronoun: he, she, it, they, etc.
	string Subjective { get; }

	string CapitalizedSubjective { get; }

	string Objective { get; }

	string CapitalizedObjective { get; }

	/// Adjectival possessive pronoun: his, her, its, their, etc.
	string PossessiveAdjective { get; }

	string CapitalizedPossessiveAdjective { get; }

	string SubstantivePossessive { get; }

	string CapitalizedSubstantivePossessive { get; }

	/// Reflexive personal pronoun: himself, herself, itself, themselves, etc.
	string Reflexive { get; }

	string CapitalizedReflexive { get; }

	string PersonTerm { get; }

	string CapitalizedPersonTerm { get; }

	/// The term for an immature person with the pronouns: boy, girl, etc.
	string ImmaturePersonTerm { get; }

	string CapitalizedImmaturePersonTerm { get; }

	string FormalAddressTerm { get; }

	string CapitalizedFormalAddressTerm { get; }

	/// Term for entity as offspring: son, daughter, etc.
	string OffspringTerm { get; }

	string CapitalizedOffspringTerm { get; }

	string SiblingTerm { get; }

	string CapitalizedSiblingTerm { get; }

	/// Term for entity as parent: father, mother, etc.
	string ParentTerm { get; }

	string CapitalizedParentTerm { get; }

	string IndicativeProximal { get; }

	string CapitalizedIndicativeProximal { get; }

	/// Distal indicative pronoun: that, those.
	string IndicativeDistal { get; }

	string CapitalizedIndicativeDistal { get; }

	bool UseBareIndicative { get; }
}
