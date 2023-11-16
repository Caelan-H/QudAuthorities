using System;
using UnityEngine;

public class SimpleSpriteAnimator : MonoBehaviour, CombatJuice.ICombatJuiceAnimator
{
	public bool loop;

	public float frameSeconds = 1f;

	private SpriteRenderer spr;

	public Sprite[] sprites;

	private int frame;

	private float deltaTime;

	private Action after;

	public void Finish()
	{
		if (after != null)
		{
			after();
			after = null;
		}
	}

	public void play(bool loop = false, Action after = null)
	{
		this.after = after;
		this.loop = loop;
		frame = 0;
		deltaTime = 0f;
	}

	private void Start()
	{
		spr = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		deltaTime += Time.deltaTime;
		while (frameSeconds > 0f && deltaTime >= frameSeconds)
		{
			deltaTime -= frameSeconds;
			frame++;
			if (loop)
			{
				frame %= sprites.Length;
			}
			else if (frame >= sprites.Length)
			{
				frame = sprites.Length - 1;
				base.gameObject.SendMessage("OnSimpleAnimationComplete", SendMessageOptions.DontRequireReceiver);
				if (after != null)
				{
					after();
					after = null;
					return;
				}
			}
		}
		spr.sprite = sprites[frame];
	}
}
