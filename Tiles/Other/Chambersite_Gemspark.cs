using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Other {
    public class Chambersite_Gemspark : OriginTile {
        public string[] Categories => [
            "Brick"
        ];
        public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileLighted[Type] = true;
			AddMapEntry(new Color(55, 204, 212));
			DustType = DustID.GemEmerald;//TODO: Chambersite gem dust
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			const float strength = 1f;
			r = 0.216f * strength;
			g = 0.800f * strength;
			b = 0.831f * strength;
		}
		public override void HitWire(int i, int j) {
			Main.tile[i, j].TileType = (ushort)TileType<Chambersite_Gemspark_Off>();
			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendTileSquare(-1, i, j, 1, 1);
			}
		}
	}
    public class Chambersite_Gemspark_Off : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[TileID.DiamondGemsparkOff].ToArray();
			Main.tileMerge[Type][TileID.DiamondGemsparkOff] = true;
			Main.tileMerge[Type][TileType<Chambersite_Gemspark>()] = true;
			AddMapEntry(new Color(17, 74, 77));
			mergeID = TileID.EmeraldGemsparkOff;
			DustType = DustID.GemSapphire;//TODO: Chambersite gem dust
		}
		public override void HitWire(int i, int j) {
			Main.tile[i, j].TileType = (ushort)TileType<Chambersite_Gemspark>();
			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendTileSquare(-1, i, j, 1, 1);
			}
		}
	}
	public class Chambersite_Gemspark_Item : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileType<Chambersite_Gemspark>());
			Item.value = Item.sellPrice(silver: 1);
		}
		public override void AddRecipes() {
			CreateRecipe(20)
			.AddIngredient(ItemID.Glass, 20)
			.AddIngredient<Chambersite_Item>()
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
