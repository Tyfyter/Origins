using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Brine {
	public class Eitrite_Ore : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			ItemDrop = ItemType<Eitrite_Ore_Item>();
			ModTranslation name = CreateMapEntryName();
			name.SetDefault("Eitrite Ore");
			AddMapEntry(new Color(79, 86, 207));
			mergeID = TileID.Mud;
			//HitSound = SoundID.digstone;
		}
		public override bool CanExplode(int i, int j) {
			return false;
		}
		public void MinePower(int i, int j, int minePower, ref int damage) {
			if (minePower >= 150 || j <= Main.worldSurface) {
				damage += (int)(minePower / MineResist);
			}
		}
	}
	public class Eitrite_Ore_Item : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Eitrite Ore");
			Tooltip.SetDefault("'So acidic it could be used as a power source'");
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TitaniumOre);
			Item.createTile = TileType<Eitrite_Ore>();
		}
	}
}
