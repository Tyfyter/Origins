using Origins.Core.Structures;
using Terraria;
using Terraria.ModLoader;

namespace Origins.Dev;
public class StructureCommand : ModCommand {
#if !DEBUG
		public override bool IsLoadingEnabled(Mod mod) => DebugConfig.Instance.ForceEnableDebugItems;
#endif
	public override CommandType Type => CommandType.World;
	public override string Command => "structure";
	public override string Usage => "/structure <structure_path>";
	public override string Description => "";
	public override bool IsCaseSensitive => true;

	public override void Action(CommandCaller caller, string input, string[] args) {
		Point pos = caller.Player.Bottom.ToTileCoordinates();
		DeserializedStructure.AsyncLoad(args[0]).ContinueWith(result => {
			if (result.IsFaulted) {
				caller.Reply($"Failed to generate structure. reason: {result.Exception}", Color.OrangeRed);
			} else {
				result.Result.Generate(pos.X, pos.Y);
			}
		});
	}
}
