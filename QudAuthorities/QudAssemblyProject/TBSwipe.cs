using UnityEngine;

[AddComponentMenu("FingerGestures/Toolbox/Swipe")]
public class TBSwipe : TBComponent
{
	public bool swipeLeft = true;

	public bool swipeRight = true;

	public bool swipeUp = true;

	public bool swipeDown = true;

	public float minVelocity;

	public Message swipeMessage = new Message("OnSwipe");

	public Message swipeLeftMessage = new Message("OnSwipeLeft", enabled: false);

	public Message swipeRightMessage = new Message("OnSwipeRight", enabled: false);

	public Message swipeUpMessage = new Message("OnSwipeUp", enabled: false);

	public Message swipeDownMessage = new Message("OnSwipeDown", enabled: false);

	private FingerGestures.SwipeDirection direction;

	private float velocity;

	public FingerGestures.SwipeDirection Direction
	{
		get
		{
			return direction;
		}
		protected set
		{
			direction = value;
		}
	}

	public float Velocity
	{
		get
		{
			return velocity;
		}
		protected set
		{
			velocity = value;
		}
	}

	public event EventHandler<TBSwipe> OnSwipe;

	public bool IsValid(FingerGestures.SwipeDirection direction)
	{
		return direction switch
		{
			FingerGestures.SwipeDirection.Left => swipeLeft, 
			FingerGestures.SwipeDirection.Right => swipeRight, 
			FingerGestures.SwipeDirection.Up => swipeUp, 
			FingerGestures.SwipeDirection.Down => swipeDown, 
			_ => false, 
		};
	}

	private Message GetMessageForSwipeDirection(FingerGestures.SwipeDirection direction)
	{
		return direction switch
		{
			FingerGestures.SwipeDirection.Left => swipeLeftMessage, 
			FingerGestures.SwipeDirection.Right => swipeRightMessage, 
			FingerGestures.SwipeDirection.Up => swipeUpMessage, 
			_ => swipeDownMessage, 
		};
	}

	public bool RaiseSwipe(int fingerIndex, Vector2 fingerPos, FingerGestures.SwipeDirection direction, float velocity)
	{
		if (velocity < minVelocity)
		{
			return false;
		}
		if (!IsValid(direction))
		{
			return false;
		}
		base.FingerIndex = fingerIndex;
		base.FingerPos = fingerPos;
		Direction = direction;
		Velocity = velocity;
		if (this.OnSwipe != null)
		{
			this.OnSwipe(this);
		}
		Send(swipeMessage);
		Send(GetMessageForSwipeDirection(direction));
		return true;
	}
}
