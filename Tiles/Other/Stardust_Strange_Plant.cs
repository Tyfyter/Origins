using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ID.ContentSamples.CreativeHelper;

namespace Origins.Tiles.Other {
	public class Stardust_Strange_Plant : ModTile {
		public override void Load() {
			new TileItem(this, textureOverride: Texture).WithResearchSorting(ItemGroup.DyeMaterial).WithExtraDefaults(item => {
				item.width = 20;
				item.height = 20;
				item.maxStack = Item.CommonMaxStack;
				item.value = 10000;
				item.rare = ItemRarityID.Quest;
				item.useStyle = ItemUseStyleID.Swing;
				item.useTurn = true;
				item.useAnimation = 15;
				item.useTime = 10;
				item.autoReuse = true;
				item.consumable = true;
			}).WithSet(() => ItemID.Sets.ExoticPlantsForDyeTrade).WithExtraStaticDefaults(item => {
				Main.RegisterItemAnimation(item.type, new DrawAnimationDelegated(_ => new(4, 6, 26, 32)));
			}).RegisterItem();
			Origins.DoILEdit(WorldGen.plantDye, BlockNewStrangePlants);
			Origins.DoILEdit(WorldGen.plantDye, AddNewStrangePlants);
		}
		static void BlockNewStrangePlants(ILContext context) {
			ILCursor c = new(context);
			int tile = -1;
			ILLabel label = default;
			while (c.TryGotoNext(
				i => i.MatchLdloca(out tile),
				i => i.MatchCall<Tile>("get_type"),
				i => i.MatchLdindU2(),
				i => i.MatchLdcI4(TileID.DyePlants),
				i => i.MatchBneUn(out label)
			)) {
				c.GotoLabel(label);
				label = c.DefineLabel();
				c.EmitLdarg2();
				c.EmitLdloca(tile);
				c.EmitDelegate((bool exoticPlant, in Tile tile) => {
					if (!tile.HasTile) return true;
					if (!exoticPlant) return true;
					return tile.TileType != ModContent.TileType<Stardust_Strange_Plant>();
				});
				c.EmitBrtrue(label);
				c.EmitRet();
				c.MarkLabel(label);
			}
		}
		static void AddNewStrangePlants(ILContext context) {
			ILCursor find = new(context);
			ILCursor c = new(context);
			while (find.TryGotoNext(MoveType.After,
				i => i.MatchCall<WorldGen>("get_genRand"),
				i => i.MatchLdcI4(8),
				i => i.MatchLdcI4(12)
			)) {
				c.Index = find.Index;
				if(!c.TryGotoPrev(MoveType.AfterLabel, ILPatternMatchingExt.MatchLdarg0)) continue;
				ILLabel label = c.DefineLabel();
				c.EmitLdarg0();
				c.EmitLdarg1();
				c.EmitDelegate((int i, int j) => {
					if (!WorldGen.genRand.NextBool(5)) return true;
					WorldGen.PlaceTile(i, j - 1, ModContent.TileType<Stardust_Strange_Plant>(), mute: true, forced: false, -1);
					return false;
				});
				c.EmitBrtrue(label);
				c.EmitRet();
				c.MarkLabel(label);
			}
		}
		public override void SetStaticDefaults() {
			Main.tileOreFinderPriority[Type] = 750;
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileNoFail[Type] = true;
			Main.tileSpelunker[Type] = true;
			TileID.Sets.ReplaceTileBreakUp[Type] = true;
			TileID.Sets.SwaysInWindBasic[Type] = true;
			TileID.Sets.BreakableWhenPlacing[Type] = true;
			TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
			Main.tileLighted[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.StyleDye);
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(33, 76, 140), this.GetTileItem().DisplayName);
			HitSound = SoundID.Grass;
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			width = 32;
			height = 38;
			//offsetY -= 20;
		}
		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 0) spriteEffects = SpriteEffects.FlipHorizontally;
		}
		public override bool CanPlace(int i, int j) => !Main.tile[i, j].TileIsType(Type);
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			r = 0.016470589f;
			g = 0.18117648f;
			b = 0.27058825f;
		}
	}
}
