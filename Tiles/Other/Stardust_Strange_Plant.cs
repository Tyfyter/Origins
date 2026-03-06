using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ID.ContentSamples.CreativeHelper;

namespace Origins.Tiles.Other {
	public class Stardust_Strange_Plant : ModTile {
		public override void Load() => new TileItem(this, textureOverride: Texture).WithResearchSorting(ItemGroup.DyeMaterial).WithExtraDefaults(item => {
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
			TileObjectData.newTile.CopyFrom(TileObjectData.StyleDye);
			TileObjectData.addTile(Type);
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			width = 32;
			height = 38;
			//offsetY -= 20;
		}
		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 0) spriteEffects = SpriteEffects.FlipHorizontally;
		}
	}
}
