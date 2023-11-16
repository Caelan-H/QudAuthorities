using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace XRL;

public class ModSettings
{
	/// <summary>A copy of the mod's display title, for easier reading of ModSettings.json by humans.</summary>
	public string Title;

	public bool Enabled = true;

	/// <summary>The last approved hash of all file records within the mod.</summary>
	public string FilesHash;

	public string SourceHash;

	/// <value><c>true</c> if the mod has failed to compile; otherwise, <c>false</c>.</value>
	[JsonIgnore]
	public bool Failed;

	[JsonIgnore]
	public List<string> Errors = new List<string>();

	/// <summary>A list of warnings attributed to this mod.</summary>
	[JsonIgnore]
	public List<string> Warnings = new List<string>();

	public string CalcFilesHash(IEnumerable<FileInfo> Files, string Root)
	{
		using SHA1 sHA = SHA1.Create();
		foreach (FileInfo File in Files)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(File.FullName.Replace(Root, ""));
			sHA.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
			bytes = BitConverter.GetBytes(File.Length);
			sHA.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
		}
		sHA.TransformFinalBlock(new byte[0], 0, 0);
		return string.Concat(sHA.Hash.Select((byte x) => x.ToString("X2")));
	}

	/// <summary>
	///             Compute the hash value for the specified files' contents.
	///             </summary><returns>A hex string of the computed hash code.</returns>
	public string CalcSourceHash(IEnumerable<FileInfo> Files)
	{
		using SHA1 sHA = SHA1.Create();
		byte[] array = new byte[8192];
		int num = 0;
		foreach (FileInfo File in Files)
		{
			using FileStream fileStream = File.OpenRead();
			do
			{
				num = fileStream.Read(array, 0, 8192);
				sHA.TransformBlock(array, 0, num, array, 0);
			}
			while (num > 0);
		}
		sHA.TransformFinalBlock(array, 0, 0);
		return string.Concat(sHA.Hash.Select((byte x) => x.ToString("X2")));
	}
}
