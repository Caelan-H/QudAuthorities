using System;

namespace XRL.World;

public static class BodyPartCategory
{
	public const int ANIMAL = 1;

	public const int ARTHROPOD = 2;

	public const int PLANT = 3;

	public const int FUNGAL = 4;

	public const int PROTOPLASMIC = 5;

	public const int CYBERNETIC = 6;

	public const int MECHANICAL = 7;

	public const int METAL = 8;

	public const int WOODEN = 9;

	public const int STONE = 10;

	public const int GLASS = 11;

	public const int LEATHER = 12;

	public const int BONE = 13;

	public const int CHITIN = 14;

	public const int PLASTIC = 15;

	public const int CLOTH = 16;

	public const int PSIONIC = 17;

	public const int EXTRADIMENSIONAL = 18;

	public const int MOLLUSK = 19;

	public const int JELLY = 20;

	public const int CRYSTAL = 21;

	public const int LIGHT = 22;

	public static string GetColor(int Category)
	{
		if (Category == 6)
		{
			return "C";
		}
		return null;
	}

	public static string GetName(int Category)
	{
		return Category switch
		{
			1 => "Animal", 
			2 => "Arthropod", 
			3 => "Plant", 
			4 => "Fungal", 
			5 => "Protoplasmic", 
			6 => "Cybernetic", 
			7 => "Mechanical", 
			8 => "Metal", 
			19 => "Mollusk", 
			9 => "Wooden", 
			10 => "Stone", 
			11 => "Glass", 
			12 => "Leather", 
			13 => "Bone", 
			14 => "Chitin", 
			15 => "Plastic", 
			16 => "Cloth", 
			17 => "Psionic", 
			18 => "Extradimensional", 
			20 => "Jelly", 
			21 => "Crystal", 
			22 => "Light", 
			_ => throw new Exception("invalid category " + Category), 
		};
	}

	public static int GetCode(string Name)
	{
		return Name switch
		{
			"Animal" => 1, 
			"Arthropod" => 2, 
			"Plant" => 3, 
			"Fungal" => 4, 
			"Protoplasmic" => 5, 
			"Cybernetic" => 6, 
			"Mechanical" => 7, 
			"Metal" => 8, 
			"Mollusk" => 19, 
			"Wooden" => 9, 
			"Stone" => 10, 
			"Glass" => 11, 
			"Leather" => 12, 
			"Bone" => 13, 
			"Chitin" => 14, 
			"Plastic" => 15, 
			"Cloth" => 16, 
			"Psionic" => 17, 
			"Extradimensional" => 18, 
			"Jelly" => 20, 
			"Crystal" => 21, 
			"Light" => 22, 
			_ => throw new Exception("invalid category name '" + Name + "'"), 
		};
	}

	public static int BestGuessForCategoryDerivedFromGameObject(GameObject obj)
	{
		string propertyOrTag = obj.GetPropertyOrTag("DerivedBodyPartCategory");
		if (propertyOrTag != null)
		{
			return GetCode(propertyOrTag);
		}
		if (obj.HasPart("ModExtradimensional") || obj.HasPart("Extradimensional"))
		{
			return 18;
		}
		if (obj.HasTagOrProperty("LivePlant"))
		{
			return 3;
		}
		if (obj.HasTagOrProperty("LiveFungus"))
		{
			return 4;
		}
		if (obj.HasTagOrProperty("LiveAnimal"))
		{
			return 1;
		}
		if (obj.pRender.DisplayName.Contains("skull"))
		{
			return 13;
		}
		if (obj.pRender.DisplayName.Contains("shell"))
		{
			return 14;
		}
		if (obj.HasPart("EnergyCellSocket"))
		{
			return 7;
		}
		if (obj.HasPart("LiquidFueledPowerPlant"))
		{
			return 7;
		}
		if (obj.HasPart("Metal"))
		{
			return 8;
		}
		return 12;
	}

	public static bool IsLiveCategory(int Category)
	{
		if ((uint)(Category - 1) <= 4u || Category == 18)
		{
			return true;
		}
		return false;
	}
}
