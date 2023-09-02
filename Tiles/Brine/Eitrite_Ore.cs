using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Brine
{
    public class Eitrite_Ore : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileOreFinderPriority[Type] = 666;
			Main.tileSpelunker[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			// name.SetDefault("Eitrite Ore");
			AddMapEntry(new Color(79, 86, 207), CreateMapEntryName());
			mergeID = TileID.Mud;
			MinPick = 150;
			HitSound = SoundID.Dig;
		}
		public override bool CanExplode(int i, int j) {
			return false;
		}
	}
	public class Eitrite_Ore_Item : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Eitrite Ore");
			// Tooltip.SetDefault("'So alkaline-concentrated it could be used as a power source'");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TitaniumOre);
			Item.createTile = TileType<Eitrite_Ore>();
		}
	}
}
