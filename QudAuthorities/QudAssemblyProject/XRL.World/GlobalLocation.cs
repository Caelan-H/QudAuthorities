using System;
using System.Text;
using XRL.Core;

namespace XRL.World;

[Serializable]
public class GlobalLocation : IEquatable<Cell>, IEquatable<GlobalLocation>
{
	public int CellX;

	public int CellY;

	private string _World;

	private int _ParasangX;

	private int _ParasangY;

	private int _ZoneX;

	private int _ZoneY;

	private int _ZoneZ;

	[NonSerialized]
	private string _ZoneID;

	public string World
	{
		get
		{
			return _World;
		}
		set
		{
			_World = value;
			_ZoneID = null;
		}
	}

	public int ParasangX
	{
		get
		{
			return _ParasangX;
		}
		set
		{
			_ParasangX = value;
			_ZoneID = null;
		}
	}

	public int ParasangY
	{
		get
		{
			return _ParasangY;
		}
		set
		{
			_ParasangY = value;
			_ZoneID = null;
		}
	}

	public int ZoneX
	{
		get
		{
			return _ZoneX;
		}
		set
		{
			_ZoneX = value;
			_ZoneID = null;
		}
	}

	public int ZoneY
	{
		get
		{
			return _ZoneY;
		}
		set
		{
			_ZoneY = value;
			_ZoneID = null;
		}
	}

	public int ZoneZ
	{
		get
		{
			return _ZoneZ;
		}
		set
		{
			_ZoneZ = value;
			_ZoneID = null;
		}
	}

	public string ZoneID
	{
		get
		{
			if (_ZoneID == null)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append(World ?? "NULL").Append('.').Append(ParasangX)
					.Append('.')
					.Append(ParasangY)
					.Append('.')
					.Append(ZoneX)
					.Append('.')
					.Append(ZoneY)
					.Append('.')
					.Append(ZoneZ);
				_ZoneID = stringBuilder.ToString();
			}
			return _ZoneID;
		}
		set
		{
			_ZoneID = value;
			string[] array = _ZoneID.Split('.');
			_World = array[0];
			_ParasangX = Convert.ToInt32(array[1]);
			_ParasangY = Convert.ToInt32(array[2]);
			_ZoneX = Convert.ToInt32(array[3]);
			_ZoneY = Convert.ToInt32(array[4]);
			_ZoneZ = Convert.ToInt32(array[5]);
		}
	}

	public GlobalLocation()
	{
	}

	/// <summary>
	///             Expected to be in the format 'JoppaWorld.11.22.1.1.10@37,22'
	///             </summary><param name="GlobalLocationSpec" />
	public GlobalLocation(string GlobalLocationSpec)
	{
		string[] array = GlobalLocationSpec.Split('@');
		string[] array2 = array[0].Split('.');
		string[] array3 = array[1].Split(',');
		SetCell(array2[0], Convert.ToInt32(array2[1]), Convert.ToInt32(array2[2]), Convert.ToInt32(array2[3]), Convert.ToInt32(array2[4]), Convert.ToInt32(array2[5]), Convert.ToInt32(array3[0]), Convert.ToInt32(array3[1]));
	}

	public GlobalLocation(string World, int ParasangX, int ParasangY, int ZoneX, int ZoneY, int ZoneZ, int CellX, int CellY)
	{
		SetCell(World, ParasangX, ParasangY, ZoneX, ZoneY, ZoneZ, CellX, CellY);
	}

	public new string ToString()
	{
		return ZoneID + "@" + CellX + "," + CellY;
	}

	public bool IsCell()
	{
		return !IsClear();
	}

	public bool IsClear()
	{
		return World == null;
	}

	public void Clear()
	{
		World = null;
	}

	public void SetCell(Cell C)
	{
		if (C == null)
		{
			World = null;
			return;
		}
		Zone parentZone = C.ParentZone;
		if (parentZone != null)
		{
			SetCell(parentZone.ZoneWorld, parentZone.wX, parentZone.wY, parentZone.X, parentZone.Y, parentZone.Z, C.X, C.Y);
		}
	}

	public override int GetHashCode()
	{
		if (World == null)
		{
			return 0;
		}
		return World.GetHashCode() ^ ParasangX ^ (ParasangY << 2) ^ (ZoneX << 4) ^ (ZoneY << 10) ^ (ZoneZ << 16) ^ (CellX << 22) ^ (CellY << 26);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return World == null;
		}
		if (obj is Cell)
		{
			return Equals(obj as Cell);
		}
		if (obj is GlobalLocation)
		{
			return Equals(obj as GlobalLocation);
		}
		return false;
	}

	public bool Equals(Cell c)
	{
		if (c == null)
		{
			return World == null;
		}
		Zone parentZone = c.ParentZone;
		if (parentZone == null)
		{
			return false;
		}
		if (c.X == CellX && c.Y == CellY && parentZone.ZoneWorld == World && parentZone.wX == ParasangX && parentZone.wY == ParasangY && parentZone.X == ZoneX && parentZone.Y == ZoneY)
		{
			return parentZone.Z == ZoneZ;
		}
		return false;
	}

	public bool Equals(GlobalLocation c)
	{
		if (c.World == null)
		{
			return World == null;
		}
		if (c.CellX == CellX && c.CellY == CellY && c.World == World && c.ParasangX == ParasangX && c.ParasangY == ParasangY && c.ZoneX == ZoneX && c.ZoneY == ZoneY)
		{
			return c.ZoneZ == ZoneZ;
		}
		return false;
	}

	public static GlobalLocation FromZoneId(string ZoneId, int CellX = 40, int CellY = 23)
	{
		return new GlobalLocation
		{
			ZoneID = ZoneId,
			CellX = CellX,
			CellY = CellY
		};
	}

	public GlobalLocation(Cell C)
		: this()
	{
		if (C != null)
		{
			Zone parentZone = C.ParentZone;
			if (parentZone != null)
			{
				World = parentZone.ZoneWorld;
				ParasangX = parentZone.wX;
				ParasangY = parentZone.wY;
				ZoneX = parentZone.X;
				ZoneY = parentZone.Y;
				ZoneZ = parentZone.Z;
				_ZoneID = parentZone.ZoneID;
			}
			CellX = C.X;
			CellY = C.Y;
		}
	}

	public GlobalLocation(GameObject obj)
		: this(obj.CurrentCell)
	{
	}

	public void SetCell(string World, int ParasangX, int ParasangY, int ZoneX, int ZoneY, int ZoneZ, int CellX, int CellY)
	{
		_World = World;
		_ParasangX = ParasangX;
		_ParasangY = ParasangY;
		_ZoneX = ZoneX;
		_ZoneY = ZoneY;
		_ZoneZ = ZoneZ;
		this.CellX = CellX;
		this.CellY = CellY;
		_ZoneID = null;
	}

	public Cell ResolveCell()
	{
		if (ZoneID == null)
		{
			return null;
		}
		return (XRLCore.Core?.Game?.ZoneManager?.GetZone(ZoneID))?.GetCell(CellX, CellY);
	}

	public bool Is(Cell C)
	{
		if (C == null)
		{
			return false;
		}
		if (ZoneID != C.ParentZone?.ZoneID)
		{
			return false;
		}
		if (CellX != C.X)
		{
			return false;
		}
		if (CellY != C.Y)
		{
			return false;
		}
		return true;
	}
}
