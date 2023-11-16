using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using UnityEngine;

public class QudScreenBufferExtra : IScreenBufferExtra
{
	public Point2D playerPosition = Point2D.invalid;

	public Dictionary<long, ImposterState> imposterUpdates = new Dictionary<long, ImposterState>();

	public static List<ImposterState> offbandUpdates = new List<ImposterState>();

	public QudScreenBufferExtra setPlayerPosition(Point2D pos)
	{
		playerPosition = pos;
		return this;
	}

	public void addUpdate(ImposterState newState)
	{
		prepareImposter(newState.id);
		if (!imposterUpdates[newState.id].destroyed)
		{
			imposterUpdates[newState.id] = newState;
		}
	}

	public void prepareImposter(long ID)
	{
		if (!imposterUpdates.ContainsKey(ID))
		{
			ImposterState qudImposterState = ImposterManager.getQudImposterState(ID);
			if (qudImposterState == null)
			{
				Debug.LogWarning("Imposter ID " + ID + " didn't exist.");
				imposterUpdates.Add(ID, new ImposterState(ID));
			}
			else
			{
				imposterUpdates.Add(ID, qudImposterState.clone());
			}
		}
	}

	public void setImposterPrefab(long ID, string Prefab)
	{
		prepareImposter(ID);
		if (!imposterUpdates[ID].destroyed)
		{
			imposterUpdates[ID].prefab = Prefab;
			ImposterManager.getQudImposterState(ID)?.copyFrom(imposterUpdates[ID]);
		}
	}

	public void setImposterPosition(long ID, Point2D pos, Point2D offset)
	{
		prepareImposter(ID);
		if (!imposterUpdates[ID].destroyed)
		{
			imposterUpdates[ID].position = pos;
			imposterUpdates[ID].offset = offset;
			ImposterManager.getQudImposterState(ID)?.copyFrom(imposterUpdates[ID]);
		}
	}

	public void showImposter(long ID)
	{
		prepareImposter(ID);
		if (!imposterUpdates[ID].destroyed)
		{
			imposterUpdates[ID].visible = true;
			ImposterManager.getQudImposterState(ID)?.copyFrom(imposterUpdates[ID]);
		}
	}

	public void hideImposter(long ID)
	{
		prepareImposter(ID);
		if (!imposterUpdates[ID].destroyed)
		{
			imposterUpdates[ID].visible = false;
			ImposterManager.getQudImposterState(ID)?.copyFrom(imposterUpdates[ID]);
		}
	}

	public void destroyImposter(long ID)
	{
		prepareImposter(ID);
		if (!imposterUpdates[ID].destroyed)
		{
			imposterUpdates[ID].destroyed = true;
			ImposterManager.getQudImposterState(ID)?.copyFrom(imposterUpdates[ID]);
		}
	}

	public void Clear()
	{
		foreach (KeyValuePair<long, ImposterState> imposterUpdate in imposterUpdates)
		{
			imposterUpdate.Value.free();
		}
		imposterUpdates.Clear();
		playerPosition = Point2D.invalid;
	}

	public void Free()
	{
		ImposterManager.freeExtra(this);
	}
}
