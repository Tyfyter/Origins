using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Other.Testing;
using Origins.Projectiles;
using Origins.Tiles.Other;
using PegasusLib;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Brine {
	public class Mildew : OriginTile {
		public string[] Categories => [
			WikiCategories.Grass
		];
		public override void SetStaticDefaults() {
			//Main.tileMergeDirt[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			RegisterItemDrop(ItemType<Mildew_Item>());
			AddMapEntry(new Color(18, 160, 56));
			HitSound = SoundID.Dig;
			DustType = DustID.Bone;
		}
	}
	public class Mildew_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Mildew>());
		}
	}
}
