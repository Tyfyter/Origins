using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.World.BiomeData;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Riven {
	public class Riven_Cactus : ModCactus, IGlowingModPlant, ICustomWikiStat, INoSeperateWikiPage {
		public static AutoLoadingAsset<Texture2D> GlowTexture = typeof(Riven_Cactus).GetDefaultTMLName() + "_Glow";
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = new Vector3(0.394f, 0.879f, 0.912f) * Riven_Hive.NormalGlowValue.GetValue();
		}
		public override void SetStaticDefaults() {
			GrowsOnTileId = new int[] { ModContent.TileType<Silica>() };
		}
		public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>(typeof(Riven_Cactus).GetDefaultTMLName());
		public override Asset<Texture2D> GetFruitTexture() => ModContent.Request<Texture2D>(typeof(Riven_Cactus).GetDefaultTMLName() + "_Fruit");// + "_Fruit"
		public bool ShouldHavePage => false;
	}
	public class Riven_Cactus_Item : ModItem, ICustomWikiStat, INoSeperateWikiPage {
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(TileID.Cactus);
		}
		public bool ShouldHavePage => false;
	}
}
