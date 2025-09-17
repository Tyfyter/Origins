using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.NPCs.Critters;
using Origins.World.BiomeData;
using PegasusLib;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Riven {
	public class Riven_Large_Foliage : ModTile, IGlowingModTile {
		public AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public Color GlowColor => new Color(GlowValue, GlowValue, GlowValue, GlowValue);
		public float GlowValue => Riven_Hive.NormalGlowValue.GetValue();
		public void FancyLightingGlowColor(Tile tile, ref Vector3 color) {
			color = Vector3.Max(color, new Vector3(0.394f, 0.879f, 0.912f) * GlowValue);
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = false;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new Color(175, 175, 175));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			int[] validTiles = [
				ModContent.TileType<Riven_Grass>(),
				ModContent.TileType<Spug_Flesh>()
			];

			TileObjectData.newTile.AnchorValidTiles = [..validTiles,
				TileID.Stone,
				TileID.Grass
			];

			TileObjectData.newTile.RandomStyleRange = 4;

			TileObjectData.addTile(Type);

			PileConversionGlobal.AddConversion(TileID.LargePiles, [7, 8, 9, 10, 11, 12], Type, [..validTiles]);
			//soundType = SoundID.Grass;
		}
		public override bool CanDrop(int i, int j) => true;
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			int critterType = ModContent.NPCType<Amoeba_Buggy>();
			int chance = 6, _ = 0;
			TileLoader.DropCritterChance(i, j, Type, ref _, ref chance, ref _);
			if (chance > 0 && NPC.CountNPCS(critterType) < 5 && WorldGen.genRand.NextBool(chance)) {
				NPC npc = NPC.NewNPCDirect(new EntitySource_TileBreak(i, j), i * 16 + 10, j * 16, critterType);
				npc.TargetClosest();
				npc.velocity.Y = WorldGen.genRand.Next(-50, -21) * 0.1f;
				npc.velocity.X = WorldGen.genRand.Next(0, 26) * -0.1f * npc.direction;
				npc.direction *= -1;
				npc.netUpdate = true;
			}
			yield break;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			this.DrawTileGlow(i, j, spriteBatch);
		}
		public override void Load() => this.SetupGlowKeys();
		public Graphics.CustomTilePaintLoader.CustomTileVariationKey GlowPaintKey { get; set; }
	}
}
