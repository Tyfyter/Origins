using Origins.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Origins.Core.SpecialChest;

namespace Origins.Tiles.Other {
	public class Dirt_Chest : ModSpecialChest {
		public override ChestData CreateChestData() => new Dirt_Chest_Data();
		public override void Load() {
			Mod.AddContent(new TileItem(this).WithExtraStaticDefaults(this.DropTileItem).WithExtraDefaults(item => {
				item.value = 0;
			}).WithOnAddRecipes(item => {
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
			}));
		}
		public override void ModifyTileData() {
			TileObjectData.newTile.Width = 1;
			TileObjectData.newTile.SetHeight(1);
			TileObjectData.newTile.Origin = Point16.Zero;
			AddMapEntry(FromHexRGB(0x976B4B), CreateMapEntryName(), MapChestName);
			AdjTiles = [TileID.Containers];
			DustType = DustID.Dirt;
			Main.tileNoAttach[Type] = true;
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
			protected internal override bool IsValidSpot(Point position) => Main.tile[position].TileIsType(ModContent.TileType<Dirt_Chest>());
		}
	}
}
