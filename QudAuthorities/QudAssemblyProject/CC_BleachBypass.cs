using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Colorful/Bleach Bypass")]
public class CC_BleachBypass : CC_Base
{
	[Range(0f, 1f)]
	public float amount = 1f;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (amount == 0f)
		{
			Graphics.Blit(source, destination);
			return;
		}
		base.material.SetFloat("_Amount", amount);
		Graphics.Blit(source, destination, base.material);
	}
}
