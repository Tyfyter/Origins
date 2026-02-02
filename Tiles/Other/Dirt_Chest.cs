using Origins.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Origins.Core.SpecialChest;

namespace Origins.Tiles.Other {
	public class Dirt_Chest : ModSpecialChest {
		public override string Texture => $"Terraria/Images/Tiles_{TileID.Dirt}";
		public override string HighlightTexture => base.Texture + "_Highlight";
		public override bool IsMultitile => false;
		public override ChestData CreateChestData() => new Dirt_Chest_Data();
		public override void Load() {
			new TileItem(this, textureOverride: base.Texture + "_Item")
			.WithExtraStaticDefaults(this.DropTileItem)
			.WithExtraDefaults(item => item.value = 0)
			.WithOnAddRecipes(item => {
				Recipe.Create(item.type)
				.AddIngredient(ItemID.DirtBlock, 10)
				.AddTile(TileID.WorkBenches)
				.AddCondition(OriginsModIntegrations.AprilFools)
				.Register();
				Recipe.Create(item.type)
				.AddIngredient(ItemID.DirtiestBlock)
				.AddTile(TileID.WorkBenches)
				.AddCondition(OriginsModIntegrations.AprilFools)
				.Register();
			}).RegisterItem();
		}
		public override void ModifyTileData() {
			AddMapEntry(FromHexRGB(0x976B4B), CreateMapEntryName(), MapChestName);
			AdjTiles = [TileID.Containers, TileID.Dirt];
			DustType = DustID.Dirt;
			TileID.Sets.Dirt[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[TileID.Dirt];
			Main.tileFrameImportant[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileNoAttach[Type] = false;
		}
		public override LocalizedText DefaultContainerName(int frameX, int frameY) => CreateMapEntryName();
		public override void MouseOver(int i, int j) => SpecialChest.MouseOver(i, j);
		public override void MouseOverFar(int i, int j) {
			MouseOver(i, j);
			Player player = Main.LocalPlayer;
			if (player.cursorItemIconText == "") {
				player.cursorItemIconEnabled = false;
				player.cursorItemIconID = ItemID.None;
			}
		}
		public override void ModifyFrameMerge(int i, int j, ref int up, ref int down, ref int left, ref int right, ref int upLeft, ref int upRight, ref int downLeft, ref int downRight) {
			WorldGen.TileMergeAttempt(Type, TileID.Dirt, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
			WorldGen.TileMergeAttempt(Type, Main.tileMoss, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
			WorldGen.TileMergeAttempt(Type, TileID.Sets.tileMossBrick, ref up, ref down, ref left, ref right, ref upLeft, ref upRight, ref downLeft, ref downRight);
		}
		public record class Dirt_Chest_Data() : Storage_Container_Data() {
			public override int Capacity => 1;
			public override int Width => 1;
			public override int Height => 1;
			public override IEnumerable<SpecialChestButton> Buttons => [
				ModContent.GetInstance<LootAllButton>(),
				ModContent.GetInstance<DepositAllButton>(),
				ModContent.GetInstance<QuickStackButton>(),
				ModContent.GetInstance<RestockButton>(),
				..RenameButton.Buttons,
				..SpecialChestButton.GlobalButtons
			];
			protected internal override bool IsValidSpot(Point position) => Main.tile[position].TileIsType(ModContent.TileType<Dirt_Chest>()) || Main.tile[position].TileIsType(ModContent.TileType<Dirt_Chest_Natural>());
		}
	}
	public class Dirt_Chest_Natural : Dirt_Chest {
		public override void Load() => Mod.AddContent(new TileItem(this, true));
		public override void ModifyTileData() {
			Main.tileSpelunker[Type] = false;
			Main.tileShine2[Type] = false;
			Main.tileShine[Type] = 0;
			Main.tileOreFinderPriority[Type] = 1;
			AddMapEntry(FromHexRGB(0x976B4B));
			AdjTiles = [TileID.Containers, TileID.Dirt];
			DustType = DustID.Dirt;
			TileID.Sets.Dirt[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.HasOutlines[Type] = false;
			Main.tileMerge[Type] = Main.tileMerge[TileID.Dirt];
			Main.tileFrameImportant[Type] = false;
			Main.tileSolid[Type] = true;
			Main.tileNoAttach[Type] = false;
			RegisterItemDrop(TileItem.Get<Dirt_Chest>().Type);
		}
		public override void MouseOver(int i, int j) { }
	}
}
