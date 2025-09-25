using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Other.Testing;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
namespace Origins.Tiles.Defiled {
	public class Defiled_Fissure : ModTile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileHammer[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.CoordinateHeights = [18, 18];
			//TileObjectData.newTile.AnchorBottom = new AnchorData();
			TileObjectData.addTile(Type);
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("{$Defiled} Fissure");
			AddMapEntry(new Color(40, 40, 40), name);
			//disableSmartCursor = true;
			AdjTiles = [TileID.ShadowOrbs];
			ID = Type;
			AnimationFrameHeight = 144 / 4;
			HitSound = Origins.Sounds.DefiledIdle;
			DustType = Defiled_Wastelands.DefaultTileDust;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 9) {
				frameCounter = 0;
				frame = ++frame % 4;
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			// Tweak the frame drawn by x position so tiles next to each other are off-sync and look much more interesting
			int uniqueAnimationFrame = Main.tileFrame[Type] + i;
			if (i % 2 == 0)
				uniqueAnimationFrame += 3;
			if (i % 3 == 0)
				uniqueAnimationFrame += 3;
			if (i % 4 == 0)
				uniqueAnimationFrame += 3;
			uniqueAnimationFrame %= 4;

			frameYOffset = uniqueAnimationFrame * AnimationFrameHeight;
		}
		public override bool CanKillTile(int i, int j, ref bool blockDamaged) {
			Player player = Main.LocalPlayer;
			if (player.HeldItem.hammer >= 45) {
				return true;
			}
			Projectile.NewProjectile(WorldGen.GetItemSource_FromTileBreak(i, j), new Vector2((i + 1) * 16, (j + 1) * 16), Vector2.Zero, ModContent.ProjectileType<Projectiles.Misc.Defiled_Wastelands_Signal>(), 0, 0, ai0: 0, ai1: Main.myPlayer);
			return false;
		}
		public override void NearbyEffects(int i, int j, bool closer) {
			//Projectile.NewProjectile(spawnSource: this, new Vector2((i + 1) * 16, (j + 1) * 16), Vector2.Zero, ModContent.ProjectileType<Projectiles.Misc.Defiled_Wastelands_Signal>(), 0, 0, ai0: 0, ai1: Main.myPlayer);
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			World.BiomeData.Defiled_Wastelands.CheckFissure(i, j, Type);
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = g = b = 0.3f;
		}
	}
	public class Defiled_Fissure_Item : TestingItem {
		public override string Texture => "Origins/Tiles/Defiled/Defiled_Fissure";
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 500;
			Item.createTile = ModContent.TileType<Defiled_Fissure>();
		}
	}
}
