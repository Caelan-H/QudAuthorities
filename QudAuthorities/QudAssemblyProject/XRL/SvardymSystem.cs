using System;
using Genkit;
using XRL.Messages;
using XRL.Rules;
using XRL.World;

namespace XRL;

[Serializable]
public class SvardymSystem : IGameSystem
{
	public string lastZone;

	public bool storming;

	public Location2D epicenter;

	public int radius;

	public int eggs;

	public int nextEgg;

	public int stormTurn;

	public int stormEndingTurn;

	public int lastRadius;

	private static readonly int CHANCE_PER_ROUND_SVARDYM_STORM_DENOM_10000 = 5;

	[NonSerialized]
	private Zone _StormZone;

	public Zone StormZone
	{
		get
		{
			if (lastZone != null)
			{
				if (_StormZone == null)
				{
					_StormZone = The.ZoneManager.GetZone(lastZone);
				}
				else if (_StormZone.bSuspended)
				{
					StormZone = null;
				}
			}
			return _StormZone;
		}
		set
		{
			_StormZone = value;
			lastZone = value?.ZoneID;
		}
	}

	public override int GetDaylightRadius(int radius)
	{
		if (storming && PlayerInStorm())
		{
			if (stormEndingTurn > 0)
			{
				return Math.Min(radius, lastRadius);
			}
			lastRadius = Math.Min(radius, 40) - stormTurn;
			return Math.Max(0, lastRadius);
		}
		return radius;
	}

	public void BeginStorm()
	{
		storming = true;
		stormTurn = 0;
		stormEndingTurn = 0;
		eggs = Stat.Random(10, 15);
		nextEgg = Stat.Random(5, 7);
		if (PlayerInStorm())
		{
			MessageQueue.AddPlayerMessage("Your hear a swelling thpthp sound.");
			SoundManager.PlaySound("EggSackPlague");
			if (Calendar.IsDay())
			{
				MessageQueue.AddPlayerMessage("The sky begins to darken.");
			}
		}
	}

	public void EndStorm()
	{
		storming = false;
		StormZone = null;
	}

	public void SpawnEgg()
	{
		Cell cell = StormZone?.GetEmptyCells().GetRandomElement();
		if (cell == null)
		{
			return;
		}
		GameObject gameObject = cell.AddObject("Svardym Egg Sac");
		if (PlayerInStorm())
		{
			if (gameObject.IsVisible())
			{
				gameObject.Slimesplatter(bSelfsplatter: false);
				gameObject.DustPuff();
			}
			gameObject.PlayWorldSound("svardym_plop", 1f);
		}
	}

	public bool PlayerInStorm()
	{
		return The.Player.CurrentZone == StormZone;
	}

	public void Tick()
	{
		stormTurn++;
		if (eggs > 0 && --nextEgg <= 0)
		{
			SpawnEgg();
			nextEgg = Stat.Random(5, 7);
			if (--eggs <= 3)
			{
				stormEndingTurn = stormTurn;
				if (PlayerInStorm())
				{
					MessageQueue.AddPlayerMessage("The thpthp sound wanes.");
					if (Calendar.IsDay())
					{
						MessageQueue.AddPlayerMessage("The sky begins to brighten.");
					}
				}
			}
		}
		if (stormEndingTurn > 0)
		{
			if (lastRadius < 80)
			{
				lastRadius++;
			}
			else
			{
				EndStorm();
			}
		}
	}

	public override void EndTurn()
	{
		if (storming)
		{
			Tick();
		}
	}

	public override void ZoneActivated(Zone Z)
	{
		if (IsValidZone(Z))
		{
			if (storming)
			{
				StormZone = Z;
			}
			else if (CHANCE_PER_ROUND_SVARDYM_STORM_DENOM_10000.in10000())
			{
				StormZone = Z;
				BeginStorm();
			}
		}
	}

	public bool IsValidZone(Zone Z)
	{
		if (Z.Z == 10 && !Z.HasZoneProperty("NoSvardymStorm"))
		{
			return Z.GetTerrainObject().HasTag("SvardymStorm");
		}
		return false;
	}
}
