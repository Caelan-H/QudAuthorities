using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using Genkit;
using Qud.API;
using Qud.UI;
using UnityEngine;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ThinWorld : IPart
{
	private FastNoise fastNoise = new FastNoise();

	public int seed;

	private List<int> yhitch = new List<int>();

	private List<Location2D> leftovers = new List<Location2D>();

	private bool triggered;

	private long triggertime;

	private float timer;

	private int xoffset;

	private int yoffset;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public static void SetStateKnowAboutSalum()
	{
		The.Game.SetIntGameState("KnowAboutSalum", 1);
	}

	public static bool CrossIntoBrightsheol()
	{
		if (!string.Equals(WorldFactory.Factory.getWorld(IComponent<GameObject>.ThePlayer.GetCurrentCell().ParentZone.ZoneWorld).Protocol, "THIN"))
		{
			Popup.Show("You cannot cross into Brightsheol from this place.");
		}
		if (Popup.AskString("Crossing into Brightsheol will retire your character. Are you sure you want to do it? Type 'CROSS' to confirm.", "", 5).ToUpper() == "CROSS")
		{
			Popup.Show("The gate opens, and you cross into Brightsheol...");
			GameManager.Instance.PushGameView("GameSummary");
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			scrapBuffer.RenderBase();
			scrapBuffer.Draw();
			scrapBuffer.Draw();
			FadeToBlack.FadeOut(8f, new Color(1f, 1f, 1f));
			Thread.Sleep(13000);
			The.Game.SetStringGameState("VictoryCondition", "Brightsheol");
			FadeToBlack.FadeIn(8f, new Color(1f, 1f, 1f));
			JournalAPI.AddAccomplishment("You crossed into Brightsheol.", null, "general", JournalAccomplishment.MuralCategory.Generic, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			AchievementManager.SetAchievement("ACH_CROSS_BRIGHTSHEOL");
			The.Game.DeathReason = "You crossed into Brightsheol.";
			The.Game.Running = false;
			return true;
		}
		return false;
	}

	public static void RunThinWorldIntroSequence(bool express = false)
	{
		Stopwatch stopwatch = new Stopwatch();
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		List<string> list = new List<string>
		{
			"..................", "", "", "         .   .          .                                    ", "   .     |\\  |  o       |                                .   ", " __|__   | \\ |  .  .--. |--. .  . .--..--. .-.  .-..   __|__ ", "   |     |  \\|  |  |  | |  | |  | |   `--.(   )(   |     |   ", "   '     '   '-' `-'  `-'  `-`--`-'   `--' `-'`-`-`|     '   ", "                                                ._.'         ", "",
			"", " ê 00000000 CRK SHELL", " ê 00000010 GET SEED", " ê 00000020 MOV BSHEOL", " ê 00000030 PUT SEED", " ê 00000040 GRW"
		};
		stopwatch.Start();
		for (int i = 0; i < list.Count; i++)
		{
			scrapBuffer.ScrollUp();
			for (int j = 0; j < list[i].Length; j++)
			{
				scrapBuffer.Goto(j, 24);
				scrapBuffer.Write("&c" + list[i][j]);
				if (stopwatch.ElapsedMilliseconds % 500 < 250 && j != list[i].Length - 1)
				{
					scrapBuffer.Write("&C_");
				}
				GameManager.Instance.uiQueue.queueTask(delegate
				{
					SoundManager.PlaySound("sub_bass_mouseover1");
				});
				Thread.Sleep(Math.Min(70, 1400 / list[i].Length));
				scrapBuffer.Draw();
			}
			Thread.Sleep(50);
		}
		GameManager.Instance.uiQueue.queueTask(delegate
		{
			SoundManager.PlaySound("startup");
		});
		stopwatch.Stop();
		ScreenBuffer scrapBuffer2 = ScreenBuffer.GetScrapBuffer2();
		The.Core.RenderBaseToBuffer(scrapBuffer2);
		Stopwatch stopwatch2 = new Stopwatch();
		stopwatch2.Start();
		int num = 0;
		UnityEngine.Debug.Log("starting sequence");
		int num2 = 17000;
		if (express)
		{
			num2 = 0;
		}
		StringBuilder stringBuilder = new StringBuilder();
		while (stopwatch2.ElapsedMilliseconds < num2)
		{
			if (FadeToBlack.stage == FadeToBlack.FadeToBlackStage.FadedIn && stopwatch2.ElapsedMilliseconds > num2 - 2500)
			{
				UnityEngine.Debug.Log("starting fade");
				FadeToBlack.FadeOut(2.4f);
			}
			num++;
			scrapBuffer.ScrollUp();
			int num3 = Stat.Random(0, 79);
			scrapBuffer.Goto(num3, scrapBuffer.Height - 1);
			for (int k = num3; k < 80; k++)
			{
				scrapBuffer.Write("&c" + (char)Stat.RandomCosmetic(1, 254));
			}
			for (int l = Math.Max(0, num); l < scrapBuffer.Height - 1; l++)
			{
				stringBuilder.Length = 0;
				stringBuilder.Append("&c");
				stringBuilder.Append((char)Stat.RandomCosmetic(1, 254));
				scrapBuffer.Write(stringBuilder);
			}
			if (num > 80)
			{
				for (int m = 0; m < scrapBuffer.Width; m++)
				{
					for (int n = 0; n < scrapBuffer.Height; n++)
					{
						if (Stat.RandomCosmetic(0, 10000) <= (stopwatch2.ElapsedMilliseconds - 7000) / 400)
						{
							scrapBuffer[m, n].Copy(scrapBuffer2[m, n]);
							scrapBuffer[m, n].SetBackground('k');
							if (Stat.RandomCosmetic(1, 100) <= 80)
							{
								scrapBuffer[m, n].SetForeground('b');
							}
							else
							{
								scrapBuffer[m, n].SetForeground('B');
							}
						}
					}
				}
			}
			if (num < 100)
			{
				Thread.Sleep(Math.Max(0, 100 - num));
			}
			else
			{
				Thread.Sleep(10);
			}
			scrapBuffer.Draw();
		}
		while (FadeToBlack.stage == FadeToBlack.FadeToBlackStage.FadingOut)
		{
		}
		scrapBuffer.Clear();
		scrapBuffer.Draw();
		Thread.Sleep(5000);
		FadeToBlack.FadeIn(0f);
		Popup.Show("The darkness recedes, and a new light breaks on the shores of your mind.", CopyScrap: true, Capitalize: true, DimBackground: false);
		JournalAPI.AddAccomplishment("You crossed into the Thin World, rarefied, and stood before the gate to Brightsheol.", null, "general", JournalAccomplishment.MuralCategory.Generic, JournalAccomplishment.MuralWeight.Medium, null, -1L);
		SoundManager.PlayMusic("Decompression");
		scrapBuffer2.Draw();
		FadeToBlack.FadeOut(0f);
		FadeToBlack.FadeIn(6f);
		scrapBuffer2.Draw();
		GameManager.Instance.PopGameView();
	}

	public static void TransitToThinWorld(GameObject sender, bool express = false)
	{
		Thread.Sleep(10);
		XRLGame game = The.Game;
		GameObject oldPlayer = IComponent<GameObject>.ThePlayer;
		List<GameObject> objects = oldPlayer.GetCurrentCell().ParentZone.GetObjects((GameObject o) => o.pBrain != null && o.pBrain.PartyLeader == oldPlayer);
		oldPlayer.RemoveEffect("AmbientRealityStabilized");
		game.SetInt64GameState("thinWorldTime", Calendar.TotalTimeTicks);
		game.SetStringGameState("thinWorldPlayerBody", game.ZoneManager.CacheObject(oldPlayer, cacheTwiceOk: false, replaceIfAlreadyCached: true));
		game.SetIntGameState("thinWorldFollowerCount", objects.Count);
		for (int i = 0; i < objects.Count; i++)
		{
			game.SetStringGameState("thinWorldFollower" + i, game.ZoneManager.CacheObject(objects[i], cacheTwiceOk: false, replaceIfAlreadyCached: true));
			objects[i].CurrentCell = null;
			objects[i].MakeInactive();
		}
		GameObject who = oldPlayer.DeepCopy(CopyEffects: false, CopyID: true);
		sender.GetPart<Enclosing>().EnterEnclosure(who);
		GameManager.Instance.PushGameView(Options.StageViewID);
		Cell cell = oldPlayer.CurrentCell;
		oldPlayer.CurrentCell.RemoveObject(oldPlayer);
		if (!express)
		{
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			cell.ParentZone.Render(scrapBuffer);
			CombatJuice.cameraShake(0.25f);
			scrapBuffer.Draw();
			Thread.Sleep(10);
			GameManager.Instance.uiQueue.queueTask(delegate
			{
				SoundManager.PlaySound("WoodDoorClose");
			});
		}
		oldPlayer.MakeInactive();
		if (!express)
		{
			Popup.Show("The colossal lid slams shut. Darkness engulfs you.");
			Thread.Sleep(3000);
			SoundManager.PlaySound("Chop2");
			SoundManager.StopMusic(Crossfade: false);
			CombatJuice._cameraShake(0.5f);
			Thread.Sleep(500);
			Popup.ShowSpace("You died.\n\nEntombed in the burial chamber of Resheph, the Last Sultan.");
			JournalAPI.AddAccomplishment("You died and were entombed in the burial chamber of Resheph, the Last Sultan.", null, "general", JournalAccomplishment.MuralCategory.Generic, JournalAccomplishment.MuralWeight.Medium, null, -1L);
		}
		else
		{
			SoundManager.StopMusic(Crossfade: false);
		}
		The.Core.BuildScore(bReal: false, "You were entombed in the burial chamber of Resheph, the Last Sultan.");
		GameManager.Instance.uiQueue.queueTask(delegate
		{
			SoundManager.PlaySound("Hit_Default");
		});
		GameManager.Instance.PushGameView("GameSummary");
		new Stopwatch();
		ScreenBuffer scrapBuffer2 = ScreenBuffer.GetScrapBuffer1();
		for (int j = 0; j < scrapBuffer2.Width; j++)
		{
			for (int k = 0; k < scrapBuffer2.Height; k++)
			{
				if (Stat.RandomCosmetic(1, 100) <= 10)
				{
					scrapBuffer2[j, k].Char = (char)Stat.RandomCosmetic(0, 254);
				}
				Stat.RandomCosmetic(1, 100);
				_ = 10;
			}
		}
		for (int l = 0; l < 8; l++)
		{
			scrapBuffer2.Draw();
			Keyboard.getch();
			for (int m = 0; m < scrapBuffer2.Width; m++)
			{
				for (int n = 0; n < scrapBuffer2.Height; n++)
				{
					if (Stat.RandomCosmetic(1, 100) <= 10)
					{
						scrapBuffer2[m, n].Char = (char)Stat.RandomCosmetic(0, 254);
					}
					Stat.RandomCosmetic(1, 100);
					_ = 10;
				}
			}
			CombatJuice.cameraShake(0.25f);
			scrapBuffer2.Draw();
			GameManager.Instance.uiQueue.queueTask(delegate
			{
				SoundManager.PlaySound("Hit_Default");
			});
		}
		The.ZoneManager.SetActiveZone("ThinWorld.6.6.10.6.6");
		GameObject gameObject = The.ZoneManager.ActiveZone.GetCell(40, 22).AddObject(oldPlayer.DeepCopy(CopyEffects: true, CopyID: true));
		gameObject.AddPart(new RebornOnDeathInThinWorld());
		game.Player.Body = gameObject;
		gameObject.SetStringProperty("NoPlayerColor", "1");
		gameObject.AddPart(new HologramMaterial());
		gameObject.MakeActive();
		gameObject.Energy.BaseValue = 1000;
		RunThinWorldIntroSequence(express);
	}

	public static void SomethingGoesWrong(GameObject go)
	{
		Anatomy randomAnatomy = Anatomies.GetRandomAnatomy();
		for (int i = 0; i < Stat.Random(1, 3); i++)
		{
			for (int j = 0; j < 10; j++)
			{
				BodyPart randomElement = (from p in go.Body.GetParts()
					where p.Type != "Body" && (!(p.Type == "Head") || !p.Primary) && !p.Abstract
					select p).GetRandomElement();
				AnatomyPart randomElement2 = randomAnatomy.Parts.Where((AnatomyPart p) => p.Type.Type != "Body" && p.Type.Abstract == false).GetRandomElement();
				if (randomElement == null || randomElement2 == null)
				{
					continue;
				}
				BodyPart parentPart = randomElement.GetParentPart();
				if (randomElement.Equipped != null)
				{
					EquipmentAPI.UnequipObject(randomElement.Equipped);
					if (randomElement.Equipped == null)
					{
						randomElement.Dismember(obliterate: true);
					}
				}
				switch (new BallBag<string>
				{
					{ "parentPart", 70 },
					{ "body", 15 },
					{ "random", 15 }
				}.PluckOne())
				{
				case "parentPart":
					randomElement2.ApplyTo(parentPart);
					break;
				case "body":
					randomElement2.ApplyTo(go.Body._Body);
					break;
				case "random":
					randomElement2.ApplyTo((from p in go.Body.GetParts()
						where !p.Abstract
						select p).GetRandomElement());
					break;
				}
				break;
			}
		}
	}

	public static GameObject ReturnBody(GameObject Object)
	{
		Popup.bSuppressPopups = true;
		The.ZoneManager.SetActiveZone(The.Game.GetStringGameState("Recorporealization_ZoneID"));
		Popup.bSuppressPopups = false;
		GameObject gameObject = The.ZoneManager.ActiveZone.FindObject("Full-Scale Recompositer");
		Cell cell = null;
		if (gameObject != null)
		{
			cell = gameObject.CurrentCell;
		}
		if (cell == null)
		{
			cell = The.ZoneManager.ActiveZone.GetEmptyCells().GetRandomElement();
		}
		if (cell == null)
		{
			cell = (from c in The.ZoneManager.ActiveZone.GetCells()
				where c.IsPassable()
				select c).GetRandomElement();
		}
		if (cell == null)
		{
			cell = The.ZoneManager.ActiveZone.GetCells().GetRandomElement();
		}
		GameObject body = The.Game.Player.Body;
		cell.AddObject(Object);
		The.Game.Player.Body = Object;
		Object.SetActive();
		Object.Energy.BaseValue = 1000;
		Object.ApplyEffect(new Dazed(Stat.Random(180, 220)));
		The.Game.TimeTicks = The.Game.GetInt64GameState("thinWorldTime", The.Game.TimeTicks) + 3600;
		if (body != Object)
		{
			body.MakeInactive();
			body.Obliterate();
		}
		MessageQueue.Suppress = true;
		Popup.bSuppressPopups = true;
		try
		{
			IComponent<GameObject>.ThePlayer.Body.GetBody().AddPart("Floating Nearby");
			Stomach part = IComponent<GameObject>.ThePlayer.GetPart<Stomach>();
			if (part != null)
			{
				part.Water = 9999;
			}
			IComponent<GameObject>.ThePlayer.RemovePart("Tattoos");
			return gameObject;
		}
		catch (Exception x)
		{
			MetricsManager.LogException("exception cleaning the player body on recoming", x);
			return gameObject;
		}
	}

	public static void ReturnToQud()
	{
		XRLGame game = The.Game;
		GameObject body = The.Game.Player.Body;
		body = game.ZoneManager.peekCachedObject(game.GetStringGameState("thinWorldPlayerBody"));
		if (body == null)
		{
			body = The.Game.Player.Body;
		}
		if (game.ZoneManager.CachedObjects.ContainsKey(game.GetStringGameState("thinWorldPlayerBody")))
		{
			game.ZoneManager.CachedObjects[game.GetStringGameState("thinWorldPlayerBody")] = null;
			game.ZoneManager.CachedObjects.Remove(game.GetStringGameState("thinWorldPlayerBody"));
		}
		The.Game.FinishQuestStep("Tomb of the Eaters", "Disable the Spindle's Magnetic Field");
		SoundManager.StopMusic(Crossfade: true, 5f);
		FadeToBlack.FadeOut(5f);
		Thread.Sleep(8000);
		GameObject gameObject = ReturnBody(body);
		try
		{
			int intGameState = game.GetIntGameState("thinWorldFollowerCount");
			for (int i = 0; i < intGameState; i++)
			{
				GameObject gameObject2 = game.ZoneManager.peekCachedObject(game.GetStringGameState("thinWorldFollower" + i));
				if (gameObject2 != null)
				{
					Cell connectedSpawnLocation = IComponent<GameObject>.ThePlayer.GetCurrentCell().GetConnectedSpawnLocation();
					if (connectedSpawnLocation != null)
					{
						game.ZoneManager.CachedObjects.Remove(game.GetStringGameState("thinWorldFollower" + i));
						connectedSpawnLocation.AddObject(gameObject2);
						gameObject2.SetPartyLeader(IComponent<GameObject>.ThePlayer, takeOnAttitudesOfLeader: false);
						gameObject2.MakeActive();
						gameObject2.ApplyEffect(new Dazed(Stat.Random(180, 220)));
					}
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("exception returning followers from brightsheol", x);
		}
		try
		{
			GameObject gameObject3 = The.ZoneManager.ActiveZone.FindObject("Recoming Reliquary");
			GameObject[] array;
			if (gameObject3 != null)
			{
				foreach (GameObject item in IComponent<GameObject>.ThePlayer.GetInventoryAndEquipment())
				{
					if (item.Equipped == IComponent<GameObject>.ThePlayer)
					{
						EquipmentAPI.UnequipObject(item);
					}
				}
				Inventory inventory = gameObject3.Inventory;
				array = IComponent<GameObject>.ThePlayer.Inventory.Objects.ToArray();
				foreach (GameObject gameObject4 in array)
				{
					if (!inventory.Objects.Contains(gameObject4))
					{
						inventory.AddObject(gameObject4);
					}
				}
				IComponent<GameObject>.ThePlayer.Inventory.Objects.Clear();
				return;
			}
			List<Cell> connectedSpawnLocations = IComponent<GameObject>.ThePlayer.CurrentCell.GetConnectedSpawnLocations(6);
			if (connectedSpawnLocations == null || connectedSpawnLocations.Count <= 0)
			{
				return;
			}
			foreach (GameObject item2 in IComponent<GameObject>.ThePlayer.GetInventoryAndEquipment())
			{
				if (item2.Equipped == IComponent<GameObject>.ThePlayer)
				{
					EquipmentAPI.UnequipObject(item2);
				}
			}
			array = IComponent<GameObject>.ThePlayer.Inventory.Objects.ToArray();
			foreach (GameObject gO in array)
			{
				connectedSpawnLocations.GetRandomElement().AddObject(gO);
			}
			IComponent<GameObject>.ThePlayer.Inventory.Objects.Clear();
		}
		catch (Exception x2)
		{
			MetricsManager.LogException("ThinWorld return", x2);
		}
		finally
		{
			MessageQueue.Suppress = false;
			Popup.bSuppressPopups = false;
			ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
			scrapBuffer.Clear();
			scrapBuffer.Draw();
			GameManager.Instance.ForceGameView();
			FadeToBlack.FadeIn(0f);
			Popup.Show("The matter of your new body starts to thicken and clot. You're inlaid with ribbons of bone, tissue, nerve, and flesh.", CopyScrap: true, Capitalize: true, DimBackground: false);
			if (gameObject == null)
			{
				Popup.Show("Something went wrong.", CopyScrap: true, Capitalize: true, DimBackground: false);
				try
				{
					SomethingGoesWrong(IComponent<GameObject>.ThePlayer);
				}
				catch
				{
				}
			}
			scrapBuffer.Draw();
			FadeToBlack.FadeOut(0f);
			scrapBuffer.RenderBase();
			scrapBuffer.Draw();
			FadeToBlack.FadeIn(8f);
			Thread.Sleep(8000);
			Popup.Show("The neoteric body is differently charged. Tying inside you is another of the secret knots that bind the world.");
			Popup.Show("You gain an additional {{rules|Floating Nearby}} slot!");
			JournalAPI.AddAccomplishment("You were reconstituted in the material world with a stronger magnetic charge.", null, "general", JournalAccomplishment.MuralCategory.Generic, JournalAccomplishment.MuralWeight.Medium, null, -1L);
			IComponent<GameObject>.TheGame.SetBooleanGameState("Recame", Value: true);
			AchievementManager.SetAchievement("ACH_RECAME");
			CheckpointingSystem.ManualCheckpoint(The.ActiveZone, "Gyl_Recame");
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		int num = 80;
		if (IComponent<GameObject>.ThePlayer != null)
		{
			if (num > 0)
			{
				Cell cell = IComponent<GameObject>.ThePlayer.CurrentCell;
				cell?.ParentZone?.AddLight(cell.X, cell.Y, num, LightLevel.Light);
			}
			if (num < 3)
			{
				Cell cell2 = IComponent<GameObject>.ThePlayer.CurrentCell;
				cell2?.ParentZone?.AddExplored(cell2.X, cell2.Y, 3);
			}
		}
		return base.HandleEvent(E);
	}

	public double sampleSimplexNoise(string type, int x, int y, int z, int amplitude, float frequencyMultiplier = 1f)
	{
		if (seed == 0)
		{
			seed = Stat.Random(0, 2147483646);
		}
		fastNoise.SetSeed(seed);
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.25f * frequencyMultiplier);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(0.7f);
		fastNoise.SetFractalGain(1f);
		return Math.Ceiling((double)fastNoise.GetNoise(x, y, z) * (double)amplitude);
	}

	public double sampleSimplexNoiseRange(string type, int x, int y, int z, float low, float high, float frequencyMultiplier = 1f)
	{
		if (seed == 0)
		{
			seed = Stat.Random(0, 2147483646);
		}
		fastNoise.SetSeed(seed);
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.25f * frequencyMultiplier);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(0.7f);
		fastNoise.SetFractalGain(1f);
		return (fastNoise.GetNoise(x, y, z) + 1f) / 2f * (high - low) + low;
	}

	public override bool FinalRender(RenderEvent E, bool bAlt)
	{
		if (!triggered && (float)XRLCore.FrameTimer.ElapsedMilliseconds - timer >= 200f)
		{
			if (Stat.RandomCosmetic(1, 12) <= 1)
			{
				triggered = true;
				triggertime = XRLCore.FrameTimer.ElapsedMilliseconds;
				yhitch.Clear();
				int num = Stat.RandomCosmetic(-2, 2);
				for (int i = 0; i < 25; i++)
				{
					yhitch.Add(num);
					num += Stat.RandomCosmetic(-2, 2);
				}
			}
			timer = XRLCore.FrameTimer.ElapsedMilliseconds;
			xoffset = Stat.RandomCosmetic(-100000, 100000);
			yoffset = Stat.RandomCosmetic(-100000, 100000);
		}
		E.WantsToPaint = true;
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		for (int i = 0; i < buffer.Width; i++)
		{
			for (int j = 0; j < buffer.Height; j++)
			{
				try
				{
					char foregroundCode = buffer.Buffer[i, j].ForegroundCode;
					char backgroundCode = buffer.Buffer[i, j].BackgroundCode;
					char detailCode = buffer.Buffer[i, j].DetailCode;
					if (foregroundCode != 'b' && foregroundCode != 'B' && foregroundCode != 'k')
					{
						if (foregroundCode >= 'a' && foregroundCode <= 'z')
						{
							buffer.Buffer[i, j].SetForeground('b');
						}
						if (foregroundCode >= 'A' && foregroundCode <= 'Z')
						{
							buffer.Buffer[i, j].SetForeground('B');
						}
					}
					if (backgroundCode != 'b' && backgroundCode != 'B' && backgroundCode != 'k')
					{
						if (backgroundCode >= 'a' && backgroundCode <= 'z')
						{
							buffer.Buffer[i, j].SetBackground('b');
						}
						if (backgroundCode >= 'A' && backgroundCode <= 'Z')
						{
							buffer.Buffer[i, j].SetBackground('B');
						}
					}
					if (detailCode != 'b')
					{
						switch (detailCode)
						{
						case 'a':
						case 'b':
						case 'c':
						case 'd':
						case 'e':
						case 'f':
						case 'g':
						case 'h':
						case 'i':
						case 'j':
						case 'l':
						case 'm':
						case 'n':
						case 'o':
						case 'p':
						case 'q':
						case 'r':
						case 's':
						case 't':
						case 'u':
						case 'v':
						case 'w':
						case 'x':
						case 'y':
						case 'z':
							buffer.Buffer[i, j].SetDetail('b');
							break;
						case 'B':
						case 'k':
							goto end_IL_000f;
						}
						if (detailCode >= 'A' && detailCode <= 'Z')
						{
							buffer.Buffer[i, j].SetDetail('B');
						}
					}
					end_IL_000f:;
				}
				catch (Exception x)
				{
					MetricsManager.LogException("Thin world::OnPaint 1", x);
				}
			}
		}
		if (triggered)
		{
			long num = (The.Game.WallTime.ElapsedMilliseconds - triggertime) / 8;
			for (int k = 0; k < buffer.Width; k++)
			{
				for (int l = 0; l < buffer.Height; l++)
				{
					try
					{
						double num2 = (double)num - sampleSimplexNoiseRange("render", k + xoffset, l + yoffset + yhitch[l], 0, 0f, 160f, 0.25f) + (double)k + (double)(l / 10);
						if (!(num2 > 0.0))
						{
							continue;
						}
						if (num2 <= 3.0)
						{
							buffer.Buffer[k, l].Tile = null;
							buffer.Buffer[k, l].Char = buffer.Buffer[k, l].BackupChar;
							if (Stat.RandomCosmetic(1, 100) <= 50)
							{
								buffer.Buffer[k, l].SetForeground('B');
							}
							if (Stat.RandomCosmetic(1, 100) <= 50)
							{
								buffer.Buffer[k, l].SetForeground('b');
							}
							if (Stat.Random(1, 20) <= 1)
							{
								leftovers.Add(Location2D.get(k, l));
							}
						}
						if (num2 <= 6.0)
						{
							buffer.Buffer[k, l].SetForeground('B');
						}
						if (num2 <= 9.0 && Stat.RandomCosmetic(1, 100) <= 50)
						{
							buffer.Buffer[k, l].SetForeground('b');
						}
					}
					catch (Exception x2)
					{
						MetricsManager.LogException("Thin world::OnPaint 2", x2);
					}
				}
			}
			if (num > 160)
			{
				triggered = false;
			}
		}
		if (leftovers.Count > 0)
		{
			try
			{
				foreach (Location2D leftover in leftovers)
				{
					if (Stat.Random(1, 100) <= 40)
					{
						buffer[leftover.x, leftover.y].Tile = null;
						buffer[leftover.x, leftover.y].Char = buffer.Buffer[leftover.x, leftover.y].BackupChar;
					}
					if (Stat.Random(1, 100) <= 40)
					{
						buffer[leftover.x, leftover.y].SetForeground('B');
						if (Stat.RandomCosmetic(1, 100) <= 10)
						{
							buffer[leftover.x, leftover.y].SetForeground('b');
						}
					}
				}
				leftovers.RemoveAt(0);
			}
			catch (Exception x3)
			{
				MetricsManager.LogException("Thin world::OnPaint 3", x3);
			}
		}
		base.OnPaint(buffer);
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}
}
