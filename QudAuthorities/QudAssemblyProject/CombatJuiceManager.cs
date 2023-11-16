using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatJuiceManager : MonoBehaviour
{
	public List<CombatJuiceEntry> active = new List<CombatJuiceEntry>();

	public List<CombatJuiceEntry> waiting = new List<CombatJuiceEntry>();

	public Queue<CombatJuiceEntry> queue = new Queue<CombatJuiceEntry>();

	public Dictionary<Type, Queue<CombatJuiceEntry>> pool = new Dictionary<Type, Queue<CombatJuiceEntry>>();

	public GameManager gameManager;

	public static CombatJuiceManager instance;

	private static List<CombatJuiceEntry> finishedEntries = new List<CombatJuiceEntry>();

	public ex3DSprite2[,] tiles => gameManager.ConsoleCharacter;

	public void Awake()
	{
		instance = this;
		base.gameObject.transform.position = new Vector3(0f, 0f, 0f);
	}

	public static void clearUpToTurn(long n)
	{
		lock (instance.queue)
		{
			foreach (CombatJuiceEntry item in instance.active)
			{
				item.finish();
			}
			instance.active.Clear();
			while (instance.queue.Count > 0 && instance.queue.Peek().turn <= n)
			{
				instance.queue.Dequeue();
			}
			instance.waiting.RemoveAll((CombatJuiceEntry e) => e.turn <= n);
		}
	}

	public static void finishAll()
	{
		if (instance == null)
		{
			return;
		}
		foreach (CombatJuiceEntry item in instance.active)
		{
			item.finish();
		}
		instance.waiting.Clear();
		instance.queue.Clear();
		instance.active.Clear();
		if (CombatJuice.roots == null || !CombatJuice.roots.ContainsKey("_JuiceRoot"))
		{
			return;
		}
		foreach (Transform item2 in CombatJuice.roots["_JuiceRoot"].transform)
		{
			foreach (Transform item3 in item2)
			{
				item3.gameObject.SendMessage("Finish", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public static void pause()
	{
		clearUpToTurn(CombatJuice.juiceTurn - 1);
	}

	public static void enqueueEntry(CombatJuiceEntry newEntry, bool async = false)
	{
		lock (instance.queue)
		{
			newEntry.turn = CombatJuice.juiceTurn;
			newEntry.async = async;
			if (async)
			{
				instance.waiting.Add(newEntry);
			}
			else
			{
				instance.queue.Enqueue(newEntry);
			}
		}
	}

	private static void begin(CombatJuiceEntry effect)
	{
		if (!effect.canStart())
		{
			instance.waiting.Add(effect);
			return;
		}
		effect.start();
		effect.update();
		if (effect.t < effect.duration)
		{
			instance.active.Add(effect);
		}
		else
		{
			effect.finish();
		}
	}

	public static void update()
	{
		finishedEntries.Clear();
		if (instance.active.Count > 0)
		{
			foreach (CombatJuiceEntry item in instance.active)
			{
				item.t += Time.deltaTime;
				if (item.t > item.duration)
				{
					item.update();
					item.finish();
					finishedEntries.Add(item);
				}
				else
				{
					item.update();
				}
			}
		}
		foreach (CombatJuiceEntry finishedEntry in finishedEntries)
		{
			instance.active.Remove(finishedEntry);
		}
		if (instance.queue.Count <= 0 && instance.waiting.Count <= 0)
		{
			return;
		}
		lock (instance.queue)
		{
			for (int i = 0; i < instance.waiting.Count; i++)
			{
				if (instance.waiting[i].canStart())
				{
					begin(instance.waiting[i]);
					instance.waiting.RemoveAt(i);
					i--;
				}
			}
			while (!AnyNonAsync() && instance.queue.Count > 0)
			{
				begin(instance.queue.Dequeue());
			}
		}
	}

	public static bool AnyNonAsync()
	{
		for (int num = instance.active.Count - 1; num >= 0; num--)
		{
			if (!instance.active[num].async)
			{
				return true;
			}
		}
		return false;
	}
}
