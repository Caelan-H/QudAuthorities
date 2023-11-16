using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XRL.UI.Framework;

public class NavigationController
{
	public static NavigationController instance = new NavigationController();

	private Event _currentEvent;

	private NavigationContext _activeContext;

	private NavigationContext _fromContext;

	public static Event currentEvent => instance._currentEvent;

	public NavigationContext activeContext
	{
		get
		{
			return _activeContext;
		}
		set
		{
			if (_activeContext != value)
			{
				if (_fromContext == null)
				{
					_fromContext = _activeContext;
				}
				Dictionary<string, object> data = new Dictionary<string, object>
				{
					{ "from", _fromContext },
					{ "to", value },
					{
						"triggeringEvent",
						triggeringEvent ?? _currentEvent
					}
				};
				FireEvent(Event.Type.Exit, data);
				_activeContext = value;
				FireEvent(Event.Type.Enter, data);
				_fromContext = null;
			}
		}
	}

	public Event triggeringEvent
	{
		get
		{
			object value = null;
			if ((_currentEvent?.data?.TryGetValue("triggeringEvent", out value)).GetValueOrDefault())
			{
				return value as Event;
			}
			return null;
		}
	}

	public async Task<T> SuspendContextWhile<T>(Func<Task<T>> taskCreator)
	{
		NavigationContext oldContext = activeContext;
		NavigationContext globalContext = activeContext?.parents.Last();
		bool? globalContextDisabled = globalContext?.disabled;
		if (globalContext != null)
		{
			globalContext.disabled = true;
		}
		activeContext = null;
		try
		{
			return await taskCreator();
		}
		finally
		{
			activeContext = oldContext;
			if (globalContext != null)
			{
				globalContext.disabled = globalContextDisabled.GetValueOrDefault();
			}
		}
	}

	public Event FireEvent(Event.Type type, Dictionary<string, object> data = null)
	{
		return FireEvent(new Event
		{
			type = type,
			data = data
		});
	}

	public Event FireEvent(Event e)
	{
		NavigationContext parentContext = _activeContext;
		Event @event = _currentEvent;
		Event event2 = (_currentEvent = e);
		while (parentContext != null && !_currentEvent.cancelled && !_currentEvent.handled)
		{
			if (event2.type == Event.Type.Enter)
			{
				parentContext.OnEnter();
			}
			else if (event2.type == Event.Type.Exit)
			{
				parentContext.OnExit();
			}
			else
			{
				parentContext.OnInput();
			}
			parentContext = parentContext.parentContext;
		}
		_currentEvent = @event;
		return event2;
	}

	public Event FireInputCommandEvent(string commandId, Dictionary<string, object> additionalData = null)
	{
		Dictionary<string, object> dictionary = additionalData ?? new Dictionary<string, object>();
		dictionary.Set("commandId", commandId);
		return FireEvent(Event.Type.Input, dictionary);
	}

	public Event FireInputButtonEvent(InputButtonTypes buttonType, Dictionary<string, object> additionalData = null)
	{
		Dictionary<string, object> dictionary = additionalData ?? new Dictionary<string, object>();
		dictionary.Set("button", buttonType);
		return FireEvent(Event.Type.Input, dictionary);
	}

	public Event FireInputAxisEvent(InputAxisTypes axisType, Dictionary<string, object> additionalData = null, int value = 0)
	{
		Dictionary<string, object> dictionary = additionalData ?? new Dictionary<string, object>();
		dictionary.Set("axis", axisType);
		Event e = new Event
		{
			type = Event.Type.Input,
			axisValue = value,
			data = dictionary
		};
		return FireEvent(e);
	}
}
