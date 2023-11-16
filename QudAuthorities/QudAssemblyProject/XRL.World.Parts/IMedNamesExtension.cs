using System.Collections.Generic;

namespace XRL.World.Parts;

public interface IMedNamesExtension
{
	int Priority();

	void OnInitializeMedNames(List<string> medNames);
}
