using Terraria;
using Terraria.ModLoader;

namespace Origins.World; 
public class Ashen_Factory_Biome : ModBiome {
	public override bool IsBiomeActive(Player player) => IsActive(player);
	public static bool IsActive(Player player) {
		if (!OriginsSets.Walls.AshenFactoryWalls[Framing.GetTileSafely(player.Center.ToTileCoordinates()).WallType]) return false;
		return OriginSystem.ashenFactoryTiles >= 250;
	}
}
