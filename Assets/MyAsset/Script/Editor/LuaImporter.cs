using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;

[ScriptedImporter(1, "lua")]
public class LuaImporter : ScriptedImporter
{
	public override void OnImportAsset(AssetImportContext ctx)
	{
		var text = File.ReadAllText(ctx.assetPath);
		TextAsset textAsset = new TextAsset(text);
		ctx.AddObjectToAsset("Main", textAsset);
		ctx.SetMainObject(textAsset);
	}
}
