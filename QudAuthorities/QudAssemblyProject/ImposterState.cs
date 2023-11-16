using System.Collections.Generic;
using Genkit;
using UnityEngine;

public class ImposterState
{
	public long id = -1L;

	public bool visible;

	public bool destroyed;

	public Point2D position = Point2D.invalid;

	public Point2D offset = Point2D.zero;

	public string prefab;

	public GameObject unityImposter;

	private static List<ImposterState> issuedStates = new List<ImposterState>();

	private static Queue<ImposterState> imposterStatePool = new Queue<ImposterState>();

	public ImposterState(long nid)
	{
		id = nid;
	}

	public void copyFrom(ImposterState source)
	{
		id = source.id;
		visible = source.visible;
		destroyed = source.destroyed;
		position = source.position;
		offset = source.offset;
		prefab = source.prefab;
	}

	public ImposterState clone()
	{
		return getNew(id, visible, destroyed, position, offset, prefab);
	}

	public static void clearImpostersUpTo(long i)
	{
		for (int j = 0; j < issuedStates.Count; j++)
		{
			if (issuedStates[j].id <= i && issuedStates[j].unityImposter != null)
			{
				ImposterManager.freeImposter(issuedStates[j].unityImposter);
				issuedStates[j].unityImposter = null;
			}
		}
	}

	public static void clearAll()
	{
		lock (imposterStatePool)
		{
			imposterStatePool.Clear();
			for (int i = 0; i < issuedStates.Count; i++)
			{
				imposterStatePool.Enqueue(issuedStates[i]);
			}
		}
	}

	public static ImposterState getNew(long id, bool visible, bool destroyed, Point2D pos, Point2D offset, string prefab)
	{
		lock (imposterStatePool)
		{
			if (imposterStatePool.Count == 0)
			{
				Debug.Log("pooling more imposters");
				for (int i = 0; i < 200; i++)
				{
					ImposterState item = new ImposterState(-1L);
					imposterStatePool.Enqueue(item);
					issuedStates.Add(item);
				}
			}
			ImposterState imposterState = imposterStatePool.Dequeue();
			if (imposterState == null)
			{
				imposterState = new ImposterState(-1L);
			}
			imposterState.id = id;
			imposterState.visible = visible;
			imposterState.destroyed = destroyed;
			imposterState.position = pos;
			imposterState.offset = offset;
			imposterState.prefab = prefab;
			return imposterState;
		}
	}

	public void free()
	{
		unityImposter = null;
		lock (imposterStatePool)
		{
			issuedStates.Remove(this);
			imposterStatePool.Enqueue(this);
		}
	}
}
