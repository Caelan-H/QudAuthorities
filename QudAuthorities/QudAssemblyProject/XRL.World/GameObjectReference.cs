using System;
using XRL.Core;

namespace XRL.World;

[Serializable]
public class GameObjectReference
{
	public string id;

	[NonSerialized]
	private GameObject _go;

	[NonSerialized]
	private bool failed;

	public GameObjectReference()
	{
	}

	public GameObjectReference(GameObject go)
	{
		id = go.id;
		_go = go;
	}

	public void free()
	{
		_go = null;
	}

	public GameObject go()
	{
		if (failed)
		{
			return null;
		}
		if (GameObject.validate(_go))
		{
			return _go;
		}
		_go = XRLCore.Core?.Game?.ZoneManager.findObjectByID(id);
		if (_go == null)
		{
			failed = true;
		}
		return _go;
	}
}
