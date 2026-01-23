using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Other.Testing;
using Origins.Reflection;
using Origins.Tiles.Defiled;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
namespace Origins.Tiles.Riven {
	public class Riven_Pot : ModTile, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color.DoFancyGlow(new Vector3(0.394f, 0.879f, 0.912f) * GlowValue, tile.TileColor);
		}

		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
			Main.tileSpelunker[Type] = true;
			Main.tileCut[Type] = true;
			TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
			Main.tileFrameImportant[Type] = true;
			//Main.tileNoAttach[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.Pots, 0));
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.CoordinateHeights = [16, 16];
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.AnchorInvalidTiles = [TileID.MagicalIceBlock];
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);
			/*TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Origin = new Point16(0, 1);
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.FindEmptyChest), -1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.AfterPlacement_Hook), -1, 0, false);
			TileObjectData.newTile.AnchorInvalidTiles = new int[] { TileID.MagicalIceBlock };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);*/
			DustType = Riven_Hive.DefaultTileDust;
            HitSound = SoundID.NPCHit20;
			//KillSound = SoundID.NPCDeath12;
		}
		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			TileMethods.WorldGen_SpawnThingsFromPot(i, j, i, j, 0);
			IEntitySource source = WorldGen.GetItemSource_FromTileBreak(i, j);
			Vector2 basePos = new(i * 16, j * 16);
			for (int index = 0; index < 3; index++) {
				Origins.instance.SpawnGoreByName(source, basePos + new Vector2(Main.rand.Next(32), Main.rand.Next(32)), default, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(source, basePos + new Vector2(Main.rand.Next(32), Main.rand.Next(32)), default, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(1, 4));
			}
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.015f * GlowValue;
			g = 0.0375f * GlowValue;
			b = 0.05f * GlowValue;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
	public class Riven_Pot_Item : TestingItem {
		public override string Texture => "Origins/Tiles/Riven/Riven_Pot";
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Riven_Pot>());
		}
		public override bool? UseItem(Player player) {
			WorldGen.Place2x2(Player.tileTargetX, Player.tileTargetY, (ushort)Item.createTile, 0);
			return base.UseItem(player);
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Add(new(Mod, "createTile", Item.createTile + ""));
		}
	}
}
