using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using QupKit;
using UnityEngine;
using XRL.Core;
using XRL.UI;
using XRL.World;

public static class ImposterManager
{
	private static Queue<QudScreenBufferExtra> extraPool = new Queue<QudScreenBufferExtra>();

	public static Dictionary<long, bool> destroyedImposter = new Dictionary<long, bool>();

	private static Dictionary<long, ImposterState> qudImposterState = new Dictionary<long, ImposterState>();

	private static Dictionary<long, ImposterState> unityImposterState = new Dictionary<long, ImposterState>();

	private static UnityEngine.GameObject imposterRoot;

	public static bool forceOff = false;

	public static long nImposterCount = 0L;

	public static long clearMax = 0L;

	public static bool clearQueued = false;

	public static long currentImposterFrame = 0L;

	public static ImposterState getQudImposterState(long ID)
	{
		if (qudImposterState.TryGetValue(ID, out var value))
		{
			return value;
		}
		return null;
	}

	public static void enableImposters(bool bEnable)
	{
		if (forceOff)
		{
			bEnable = false;
		}
		if (imposterRoot == null)
		{
			imposterRoot = new UnityEngine.GameObject("imposter root");
		}
		if (imposterRoot != null && imposterRoot.activeSelf != bEnable)
		{
			imposterRoot.SetActive(bEnable);
		}
	}

	public static long RegisterNewImposter()
	{
		nImposterCount++;
		qudImposterState.Add(nImposterCount, ImposterState.getNew(nImposterCount, visible: false, destroyed: false, Point2D.invalid, Point2D.zero, null));
		return nImposterCount;
	}

	public static void Update()
	{
		if (clearQueued)
		{
			clearQueued = false;
			ImposterState.clearImpostersUpTo(clearMax);
		}
		enableImposters(Globals.RenderMode == RenderModeType.Tiles && !Input.GetKey(UnityEngine.KeyCode.LeftAlt) && !Input.GetKey(UnityEngine.KeyCode.RightAlt));
	}

	public static void hideTextCoveredImposters()
	{
		foreach (ImposterState value in unityImposterState.Values)
		{
			if (!(value.unityImposter != null) || !value.visible || value.destroyed)
			{
				continue;
			}
			Point2D position = value.position;
			if (position.x < 0 || position.x >= 80 || position.y < 0 || position.y >= 25)
			{
				continue;
			}
			ConsoleChar consoleChar = TextConsole.CurrentBuffer.Buffer[position.x, position.y];
			if (consoleChar.Char != 0 || string.IsNullOrEmpty(consoleChar.Tile) || ScreenBuffer.ImposterSuppression[position.x, position.y])
			{
				if (value.unityImposter.activeInHierarchy)
				{
					value.unityImposter.SetActive(value: false);
				}
			}
			else if (!value.unityImposter.activeInHierarchy)
			{
				value.unityImposter.SetActive(value: true);
			}
		}
	}

	public static void qudClearImposters()
	{
		clearQueued = true;
		clearMax = nImposterCount;
		qudImposterState.Clear();
		GameManager.Instance.uiQueue.queueSingletonTask("ClearQudImposters", delegate
		{
			foreach (KeyValuePair<long, ImposterState> item in unityImposterState)
			{
				if (item.Value.unityImposter != null)
				{
					UnityEngine.Object.Destroy(item.Value.unityImposter);
					item.Value.unityImposter = null;
				}
			}
			unityImposterState.Clear();
		});
		ImposterState.clearAll();
	}

	public static void freeImposter(UnityEngine.GameObject imposter)
	{
		if (imposter != null)
		{
			UnityEngine.Object.Destroy(imposter);
		}
	}

	public static void unityProcessImposterUpdate(ImposterState newState)
	{
		if (destroyedImposter.ContainsKey(newState.id))
		{
			if (!newState.destroyed)
			{
				Debug.LogError("Trying to update destroyed imposter: " + newState.id);
			}
			return;
		}
		ImposterState value = null;
		if (!unityImposterState.TryGetValue(newState.id, out value))
		{
			value = ImposterState.getNew(newState.id, visible: false, destroyed: false, Point2D.invalid, Point2D.zero, null);
			unityImposterState.Add(newState.id, value);
		}
		if (newState.destroyed)
		{
			if (value != null && value.unityImposter != null)
			{
				freeImposter(value.unityImposter);
			}
			if (newState != null && newState.unityImposter != null)
			{
				freeImposter(newState.unityImposter);
			}
			if (qudImposterState.ContainsKey(newState.id))
			{
				qudImposterState[newState.id].free();
				qudImposterState.Remove(newState.id);
			}
			if (unityImposterState.ContainsKey(newState.id))
			{
				unityImposterState[newState.id].free();
				unityImposterState.Remove(newState.id);
			}
			destroyedImposter.Add(newState.id, value: true);
			return;
		}
		if (value.prefab != newState.prefab)
		{
			if (value.unityImposter != null)
			{
				freeImposter(value.unityImposter);
			}
			value.prefab = newState.prefab;
		}
		if (value.unityImposter == null && newState.visible && !string.IsNullOrEmpty(value.prefab))
		{
			if (value.prefab != null)
			{
				if (imposterRoot == null)
				{
					imposterRoot = new UnityEngine.GameObject("imposter root");
				}
				if (value.unityImposter != null)
				{
					freeImposter(value.unityImposter);
				}
				value.unityImposter = PrefabManager.CreateRoot(value.prefab);
				value.unityImposter.gameObject.transform.parent = imposterRoot.transform;
				value.unityImposter.SetActive(newState.visible);
				value.visible = newState.visible;
				value.position = Point2D.invalid;
			}
			else
			{
				value.unityImposter = null;
			}
		}
		if (value.visible != newState.visible)
		{
			if (value.unityImposter != null)
			{
				value.unityImposter.SetActive(newState.visible);
			}
			value.visible = newState.visible;
		}
		if (value.position != newState.position)
		{
			value.position = newState.position;
			value.offset = newState.offset;
			int x = value.position.x;
			int y = value.position.y;
			int x2 = value.offset.x;
			int y2 = value.offset.y;
			int num = 50;
			if (value.unityImposter != null)
			{
				value.unityImposter.transform.position = new Vector3(-640 + x * 16 + x2 + 8, 300f - (float)(y * 24) - 12f - (float)y2, 100 - num);
			}
		}
	}

	public static QudScreenBufferExtra getNewExtra()
	{
		while (extraPool.Count > 0)
		{
			QudScreenBufferExtra qudScreenBufferExtra = extraPool.Dequeue();
			if (qudScreenBufferExtra != null)
			{
				return qudScreenBufferExtra;
			}
		}
		for (int i = 0; i < 100; i++)
		{
			extraPool.Enqueue(new QudScreenBufferExtra());
		}
		return new QudScreenBufferExtra();
	}

	public static void freeExtra(QudScreenBufferExtra extra)
	{
		try
		{
			extra.Clear();
			extraPool.Enqueue(extra);
		}
		catch (Exception x)
		{
			extraPool = new Queue<QudScreenBufferExtra>();
			MetricsManager.LogException("ImposterManager::freeExtra", x);
		}
	}

	public static QudScreenBufferExtra getImposterUpdateFrame()
	{
		if (Globals.RenderMode == RenderModeType.Text || Options.GetOption("OptionDisableImposters", "No") == "Yes")
		{
			QudScreenBufferExtra newExtra = getNewExtra();
			newExtra.playerPosition = XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.Pos2D;
			QudScreenBufferExtra.offbandUpdates.Clear();
			return newExtra;
		}
		QudScreenBufferExtra newExtra2 = getNewExtra();
		currentImposterFrame++;
		if (XRLCore.Core.Game != null)
		{
			try
			{
				if (XRLCore.Core.Game.Player != null && XRLCore.Core.Game.Player.Body != null && XRLCore.Core.Game.Player.Body.pPhysics != null && XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell != null)
				{
					newExtra2.playerPosition = XRLCore.Core.Game.Player.Body.pPhysics.CurrentCell.Pos2D;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
			}
			if (XRLCore.Core.Game.ZoneManager != null && XRLCore.Core.Game.ZoneManager.ActiveZone != null)
			{
				Zone activeZone = XRLCore.Core.Game.ZoneManager.ActiveZone;
				int num = 0;
				for (int i = 0; i < activeZone.Width; i++)
				{
					for (int j = 0; j < activeZone.Height; j++)
					{
						Cell cell = activeZone.GetCell(i, j);
						for (int k = 0; k < cell.Objects.Count; k++)
						{
							XRL.World.GameObject gameObject = cell.Objects[k];
							if (gameObject.HasIntProperty("HasImposter"))
							{
								num++;
								for (int l = 0; l < gameObject.PartsList.Count; l++)
								{
									gameObject.PartsList[l].UpdateImposter(newExtra2);
								}
							}
						}
					}
				}
			}
		}
		for (int m = 0; m < QudScreenBufferExtra.offbandUpdates.Count; m++)
		{
			newExtra2.addUpdate(QudScreenBufferExtra.offbandUpdates[m]);
		}
		QudScreenBufferExtra.offbandUpdates.Clear();
		return newExtra2;
	}
}
