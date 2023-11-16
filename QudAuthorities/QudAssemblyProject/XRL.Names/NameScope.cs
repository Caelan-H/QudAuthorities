using System.Collections.Generic;

namespace XRL.Names;

public class NameScope : NameElement
{
	public string Genotype;

	public string Subtype;

	public string Species;

	public string Culture;

	public string Faction;

	public string Gender;

	public string Mutation;

	public string Tag;

	public string Special;

	public int Priority;

	public int Chance = 100;

	public bool Combine;

	public bool ApplyTo(string Genotype = null, string Subtype = null, string Species = null, string Culture = null, string Faction = null, string Gender = null, List<string> Mutations = null, string Tag = null, string Special = null)
	{
		if ((!string.IsNullOrEmpty(this.Special) || !string.IsNullOrEmpty(Special)) && Special != this.Special)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(this.Tag) && Tag != this.Tag)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(this.Gender) && Gender != this.Gender)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(Mutation) && (Mutations == null || !Mutations.Contains(Mutation)))
		{
			return false;
		}
		if (!string.IsNullOrEmpty(this.Genotype) && Genotype != this.Genotype)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(this.Subtype) && Subtype != this.Subtype)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(this.Species) && Species != this.Species)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(this.Culture) && Culture != this.Culture)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(this.Faction) && Faction != this.Faction)
		{
			return false;
		}
		return Chance.in100();
	}
}
