using System;

namespace XRL.World.Encounters;

[Serializable]
public class PsychicFaction
{
	public string factionName;

	public string preferredMutation;

	public string mainColor;

	public string detailColor;

	public string a;

	public string e;

	public string i;

	public string o;

	public string u;

	public string A;

	public string E;

	public string I;

	public string O;

	public string U;

	public string c;

	public string f;

	public string n;

	public string t;

	public string y;

	public string B;

	public string C;

	public string Y;

	public string L;

	public string R;

	public string N;

	public int dimensionSymbol;

	public int cultSymbol;

	public string cultForm;

	public string dimensionName;

	public string dimensionSecretID;

	public int dimensionalWeaponIndex;

	public int dimensionalMissileWeaponIndex;

	public int dimensionalArmorIndex;

	public int dimensionalShieldIndex;

	public int dimensionalMiscIndex;

	public string dimensionalTraining;

	public string Weirdify(string Text)
	{
		return Text.Replace("a", a).Replace("A", A).Replace("e", e)
			.Replace("E", E)
			.Replace("i", i)
			.Replace("I", I)
			.Replace("o", o)
			.Replace("O", O)
			.Replace("u", u)
			.Replace("U", U)
			.Replace("c", c)
			.Replace("f", f)
			.Replace("n", n)
			.Replace("t", t)
			.Replace("y", y)
			.Replace("B", B)
			.Replace("C", C)
			.Replace("Y", Y)
			.Replace("L", L)
			.Replace("R", R)
			.Replace("N", N);
	}
}
