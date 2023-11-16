using System;
using System.Collections.Generic;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class SocialRoles : IPart
{
	public string _Roles;

	[NonSerialized]
	private List<string> _RoleList;

	public string Roles
	{
		get
		{
			return _Roles;
		}
		set
		{
			_Roles = value;
			_RoleList = null;
			if (_Roles.Contains(","))
			{
				RoleList.Sort();
				_Roles = string.Join(",", RoleList.ToArray());
			}
		}
	}

	public List<string> RoleList
	{
		get
		{
			if (_RoleList == null)
			{
				if (string.IsNullOrEmpty(_Roles))
				{
					_RoleList = new List<string>();
				}
				else
				{
					_RoleList = new List<string>(_Roles.Split(','));
				}
			}
			return _RoleList;
		}
	}

	public void AddRole(string Role)
	{
		RoleList.Add(Role);
		RoleList.Sort();
		_Roles = string.Join(",", RoleList.ToArray());
	}

	public void RequireRole(string Role)
	{
		if (!RoleList.Contains(Role))
		{
			AddRole(Role);
		}
	}

	public void RemoveRole(string Role)
	{
		RoleList.Remove(Role);
		if (RoleList.Count == 0)
		{
			ParentObject.RemovePart(this);
		}
		else
		{
			_Roles = string.Join(",", RoleList.ToArray());
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetDisplayNameEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		string text = null;
		switch (RoleList.Count)
		{
		case 1:
			text = ((!ParentObject.HasProperName || ParentObject.pRender.DisplayName.Contains(",")) ? ("and " + RoleList[0]) : (", " + RoleList[0]));
			break;
		default:
			text = ", " + Grammar.MakeAndList(RoleList);
			break;
		case 0:
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (text.Contains("="))
			{
				text = GameText.VariableReplace(text, ParentObject);
			}
			E.AddClause(text, -20);
		}
		return base.HandleEvent(E);
	}
}
