using System;
using System.Collections.Generic;
using Genkit;
using UnityEngine;
using XRL.UI;
using XRL.World;

public static class CombatJuice
{
	public interface ICombatJuiceAnimator
	{
		void play(bool loop = false, Action after = null);
	}

	public static Dictionary<string, UnityEngine.GameObject> prefabAnimations = new Dictionary<string, UnityEngine.GameObject>();

	public static Dictionary<string, Queue<UnityEngine.GameObject>> prefabPool = new Dictionary<string, Queue<UnityEngine.GameObject>>();

	public static CC_Blend lowHPBlend = null;

	public static Dictionary<string, UnityEngine.GameObject> roots = new Dictionary<string, UnityEngine.GameObject>();

	public static long juiceTurn = 0L;

	public static GameManager gameManager => GameManager.Instance;

	public static bool soundsEnabled => Options.GetOption("OptionUseCombatSounds") == "Yes";

	public static bool enabled => Options.GetOption("OptionUseOverlayCombatEffects") == "Yes";

	public static CombatJuiceManager juiceManager => CombatJuiceManager.instance;

	public static void _setLowHPIndicator(float percent)
	{
		if (lowHPBlend == null)
		{
			lowHPBlend = GameManager.MainCamera.GetComponent<CC_Blend>();
		}
		if (!(lowHPBlend != null))
		{
			return;
		}
		if (percent <= 0f && lowHPBlend.enabled)
		{
			lowHPBlend.enabled = false;
		}
		else if (percent > 0f)
		{
			if (!lowHPBlend.enabled)
			{
				lowHPBlend.enabled = true;
			}
			lowHPBlend.amount = percent * 0.5f;
		}
	}

	public static UnityEngine.GameObject getInstance(string name, string root)
	{
		UnityEngine.GameObject gameObject = null;
		if (!roots.ContainsKey(root))
		{
			if (!roots.ContainsKey("_JuiceRoot"))
			{
				UnityEngine.GameObject gameObject2 = new UnityEngine.GameObject();
				gameObject2.name = "_JuiceRoot";
				gameObject2.transform.position = new Vector3(0f, 0f, 0f);
				roots.Add("_JuiceRoot", gameObject2);
			}
			UnityEngine.GameObject gameObject3 = new UnityEngine.GameObject();
			gameObject3.transform.parent = roots["_JuiceRoot"].transform;
			gameObject3.name = root;
			gameObject3.transform.position = new Vector3(0f, 0f, 0f);
			roots.Add(root, gameObject3);
		}
		if (!prefabPool.ContainsKey(name))
		{
			prefabPool.Add(name, new Queue<UnityEngine.GameObject>());
		}
		if (prefabPool.ContainsKey(name) && prefabPool[name].Count > 0)
		{
			gameObject = prefabPool[name].Dequeue();
		}
		if (!prefabAnimations.ContainsKey(name))
		{
			prefabAnimations.Add(name, Resources.Load("Prefabs/" + name) as UnityEngine.GameObject);
		}
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(prefabAnimations[name]);
			gameObject.transform.SetParent(roots[root].transform, worldPositionStays: true);
		}
		gameObject.SetActive(value: true);
		return gameObject;
	}

	public static void pool(string name, UnityEngine.GameObject prefab)
	{
		prefab.SetActive(value: false);
		prefabPool[name].Enqueue(prefab);
	}

	public static void _cameraShake(float duration)
	{
		CameraShake.shakeDuration += duration;
	}

	public static void cameraShake(float duration)
	{
		CombatJuiceManager.enqueueEntry(new CombatJuiceEntryCameraShake(duration));
	}

	public static void _text(Vector3 start, Vector3 end, string text, Color color, float floatTime, float scale = 1f)
	{
		UnityEngine.GameObject instance = getInstance("CombatJuice/CombatJuiceText", "CombatJuiceText");
		instance.transform.position = start;
		instance.transform.localScale = new Vector3(scale, scale, 1f);
		instance.GetComponent<SimpleTextMeshTweener>().init(start, end, color, new Color(color.r, color.g, color.b, 0f), floatTime, "CombatJuice/CombatJuiceText");
		TextMesh component = instance.GetComponent<TextMesh>();
		component.color = color;
		component.text = text;
	}

	public static void _playPrefabAnimation(Vector3 location, string name)
	{
		UnityEngine.GameObject animation = getInstance(name, "PrefabAnimation_" + name);
		animation.transform.position = location;
		animation.GetComponent<ICombatJuiceAnimator>().play(loop: false, delegate
		{
			pool(name, animation);
		});
	}

	public static void _playPrefabAnimation(int x, int y, string name)
	{
		playPrefabAnimation(GameManager.Instance.getTileCenter(x, y, 100), name);
	}

	public static void playWorldSound(XRL.World.GameObject obj, string clip, float volume = 0.5f, float pitchVariance = 0f, float t = 0f, float delay = 0f)
	{
		if (soundsEnabled && obj != null && obj.IsValid() && obj.IsVisible())
		{
			CombatJuiceEntryWorldSound juiceWorldSound = obj.pPhysics.GetJuiceWorldSound(clip, volume, pitchVariance, delay);
			if (juiceWorldSound != null)
			{
				juiceWorldSound.t = t;
				CombatJuiceManager.enqueueEntry(juiceWorldSound);
			}
		}
	}

	public static void _playPrefabAnimation(XRL.World.GameObject gameObject, string animation)
	{
		try
		{
			if (gameObject != null && gameObject.IsVisible() && gameObject.IsValid())
			{
				playPrefabAnimation(GameManager.Instance.getTileCenter(gameObject.pPhysics.CurrentCell.location.x, gameObject.pPhysics.CurrentCell.location.y, 100), animation);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("CombatJuice::playPrefabAnimation", x);
		}
	}

	public static void floatingText(Cell C, string text, Color color, float duration = 1.5f, float floatLength = 24f, float scale = 1f, bool ignoreVisibility = true, XRL.World.GameObject gameObject = null)
	{
		if (C != null && (ignoreVisibility || C.IsVisible()))
		{
			Vector3 vector = gameManager.getTileCenter(C.X, C.Y, 100) + new Vector3(0f, 12f, 0f);
			Vector3 end = vector + new Vector3(0f, floatLength, 0f);
			CombatJuiceManager.enqueueEntry(new CombatJuiceEntryText(vector, end, duration, text, color, gameObject, scale));
		}
	}

	public static void floatingText(XRL.World.GameObject gameObject, string text, Color color, float duration = 1.5f, float floatLength = 24f, float scale = 1f, bool ignoreVisibility = false)
	{
		if (XRL.World.GameObject.validate(ref gameObject) && (ignoreVisibility || gameObject.IsVisible()))
		{
			floatingText(gameObject.CurrentCell, text, color, duration, floatLength, scale, ignoreVisibility: true, gameObject);
		}
	}

	public static void punch(XRL.World.GameObject attacker, XRL.World.GameObject defender, float t = 0.2f, Easing.Functions ease = Easing.Functions.SineEaseInOut)
	{
		try
		{
			if (attacker != null && attacker.IsVisible() && defender != null && defender.IsVisible() && attacker.IsValid() && defender.IsValid())
			{
				punch(attacker.CurrentCell.location, defender.CurrentCell.location, t, ease);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("CombatJuice::punch", x);
		}
	}

	public static void punch(Location2D attackerCellLocation, Location2D defenderCellLocation, float t = 0.2f, Easing.Functions ease = Easing.Functions.SineEaseInOut)
	{
		ex3DSprite2 tile = gameManager.getTile(attackerCellLocation.x, attackerCellLocation.y);
		Vector3 tileCenter = gameManager.getTileCenter(attackerCellLocation.x, attackerCellLocation.y);
		Vector3 tileCenter2 = gameManager.getTileCenter(defenderCellLocation.x, defenderCellLocation.y);
		CombatJuiceManager.enqueueEntry(new CombatJuiceEntryPunch(tile, tileCenter, tileCenter2, t, ease));
	}

	public static void Hover(Location2D Location, float Duration = 10f, float Rise = 1f)
	{
		ex3DSprite2 tile = gameManager.getTile(Location.x, Location.y);
		Vector3 tileCenter = gameManager.getTileCenter(Location.x, Location.y);
		CombatJuiceManager.enqueueEntry(new CombatJuiceEntryHover(tile, tileCenter, 12.5f, Duration, Rise), async: true);
	}

	public static void playPrefabAnimation(Vector3 location, string animation)
	{
		CombatJuiceManager.enqueueEntry(new CombatJuiceEntryPrefabAnimation(location, animation));
	}

	public static void clearUpToTurn(long n)
	{
		CombatJuiceManager.clearUpToTurn(n);
	}

	public static void finishAll()
	{
		GameManager.Instance?.uiQueue?.queueTask(CombatJuiceManager.finishAll);
	}

	public static void startTurn()
	{
		juiceTurn++;
		GameManager.Instance.uiQueue.queueSingletonTask("combatJuiceClearTurn", delegate
		{
			clearUpToTurn(juiceTurn - 2);
		});
	}

	public static void playPrefabAnimation(XRL.World.GameObject gameObject, string animation)
	{
		if (gameObject != null && gameObject.IsValid() && gameObject.IsVisible())
		{
			playPrefabAnimation(gameObject.pPhysics.CurrentCell.location, animation);
		}
	}

	public static void playPrefabAnimation(Location2D cellLocation, string animation)
	{
		playPrefabAnimation(gameManager.getTileCenter(cellLocation.x, cellLocation.y, 100), animation);
	}
}
