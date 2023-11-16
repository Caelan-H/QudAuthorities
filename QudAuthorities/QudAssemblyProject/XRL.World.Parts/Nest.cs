using System;

namespace XRL.World.Parts;

[Serializable]
public class Nest : IPart
{
	public int NumberToSpawn = 30;

	public int ChancePerSpawn = 100;

	[FieldSaveVersion(253)]
	public float XPFactor = 0.25f;

	public string TurnsPerSpawn = "15-20";

	public string NumberSpawned = "1";

	public string SpawnMessage = "A giant centipede crawls out of the nest.";

	public string CollapseMessage = "The nest collapses.";

	public string SpawnParticle = "&w.";

	public string BlueprintSpawned = "Giant Centipede";

	public int SpawnCooldown = int.MinValue;

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TurnNumber)
	{
		if (NumberToSpawn <= 0)
		{
			if (Visible())
			{
				IComponent<GameObject>.AddPlayerMessage(CollapseMessage);
			}
			ParentObject.Destroy();
			return;
		}
		if (SpawnCooldown == int.MinValue)
		{
			SpawnCooldown = TurnsPerSpawn.RollCached();
		}
		if (ParentObject.CurrentCell == null)
		{
			return;
		}
		SpawnCooldown--;
		if (SpawnCooldown > 0)
		{
			return;
		}
		int i = 0;
		for (int num = NumberSpawned.RollCached(); i < num; i++)
		{
			Cell randomLocalAdjacentCell = ParentObject.CurrentCell.GetRandomLocalAdjacentCell();
			if (randomLocalAdjacentCell != null && randomLocalAdjacentCell.IsEmpty() && ChancePerSpawn.in100())
			{
				if (NumberToSpawn > 0)
				{
					NumberToSpawn--;
				}
				GameObject gameObject = GameObject.create(BlueprintSpawned);
				if (gameObject.HasStat("XPValue"))
				{
					gameObject.GetStat("XPValue").BaseValue = (int)Math.Round((float)gameObject.GetStat("XPValue").BaseValue * XPFactor / 5f) * 5;
				}
				gameObject.TakeOnAttitudesOf(ParentObject);
				gameObject.SetFeeling(ParentObject, 200);
				gameObject.MakeActive();
				randomLocalAdjacentCell.AddObject(gameObject);
				if (Visible())
				{
					IComponent<GameObject>.AddPlayerMessage(SpawnMessage);
				}
				ParentObject.Slimesplatter(bSelfsplatter: false, SpawnParticle);
				SpawnCooldown = TurnsPerSpawn.RollCached();
			}
		}
	}
}
