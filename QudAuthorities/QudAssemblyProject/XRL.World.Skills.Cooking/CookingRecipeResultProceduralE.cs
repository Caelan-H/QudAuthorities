using System;
using Newtonsoft.Json;
using XRL.Core;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Skills.Cooking;

[Serializable]
public class CookingRecipeResultProceduralEffect : ICookingRecipeResult
{
	public string campfire;

	public string effectJson;

	public CookingRecipeResultProceduralEffect(ProceduralCookingEffect effect)
	{
		effectJson = JsonConvert.SerializeObject(effect, new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.All
		});
		if (XRLCore.Core.Game != null)
		{
			try
			{
				campfire = Campfire.ProcessEffectDescription(effect.GetTemplatedProceduralEffectDescription(), XRLCore.Core.Game.Player.Body);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}

	public string GetCampfireDescription()
	{
		return campfire;
	}

	public string apply(GameObject eater)
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.TypeNameHandling = TypeNameHandling.All;
		ProceduralCookingEffect proceduralCookingEffect = JsonConvert.DeserializeObject(effectJson, jsonSerializerSettings) as ProceduralCookingEffect;
		proceduralCookingEffect.Init(eater);
		eater.ApplyEffect(proceduralCookingEffect);
		return proceduralCookingEffect.GetProceduralEffectDescription();
	}
}
