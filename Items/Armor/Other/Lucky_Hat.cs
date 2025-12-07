using Origins.Tiles.Other;
using PegasusLib;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Items.Armor.Other {
	[AutoloadEquip(EquipType.Head)]
	public class Lucky_Hat : ModItem {
		public string[] Categories => [
			"ArmorSet",
			"ExplosiveBoostGear",
			"RangedBoostGear"
		];
		public override void Load() {
			On_PlayerEyeHelper.UpdateEyeFrameToShow += On_PlayerEyeHelper_UpdateEyeFrameToShow;
		}

		static void On_PlayerEyeHelper_UpdateEyeFrameToShow(On_PlayerEyeHelper.orig_UpdateEyeFrameToShow orig, ref PlayerEyeHelper self, Player player) {
			orig(ref self, player);
			if (self.CurrentEyeFrame == PlayerEyeHelper.EyeFrame.EyeOpen && player.OriginPlayer().LuckyHatSetActive) self.CurrentEyeFrame = PlayerEyeHelper.EyeFrame.EyeHalfClosed;
		}
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
		}
		public override void SetDefaults() {
			Item.width = 22;
			Item.height = 26;
			Item.defense = 3;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Blue;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = Item.useAnimation = 12;
			Item.createTile = ModContent.TileType<Lucky_Hat_Tile>();
			Item.consumable = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.Leather, 8)
			.AddIngredient(ItemID.FallenStar, 3)
			.AddIngredient<Ocotillo_Flower>()
			.Register();
		}
		public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClasses.RangedExplosiveInherit) += 8;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			if (OriginsModIntegrations.CheckAprilFools()) return body.IsAir && legs.IsAir;
			return !body.IsAir && !legs.IsAir;
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Lucky_Hat");
			player.OriginPlayer().luckyHatSet = true;
		}
	}
	public class Lucky_Hat_Tile : ModTile, IItemObtainabilityProvider {
		public override void SetStaticDefaults() {
			// Properties
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileSpelunker[Type] = true;
			Main.tileShine2[Type] = true;
			Main.tileShine[Type] = 100;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			TileID.Sets.GeneralPlacementTiles[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;

			// Names
			AddMapEntry(new Color(100, 100, 100), CreateMapEntryName());

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
			TileObjectData.newTile.CoordinateHeights = [16, 16];
			TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
			TileObjectData.newTile.AnchorBottom = new(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table, 2, 0);
			TileObjectData.newTile.AnchorAlternateTiles = [
				TileID.Containers,
				TileID.TNTBarrel
			];
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.addAlternate(1);

			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.HookCheckIfCanPlace = new PlacementHook(PlacementPreviewHook_CheckIfCanPlace, -1, 0, true);
			TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.AlternateTile, 2, 0);
			TileObjectData.newAlternate.DrawYOffset = 6;
			TileObjectData.addAlternate(0);
			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.newAlternate.HookCheckIfCanPlace = new PlacementHook(PlacementPreviewHook_CheckIfCanPlace, -1, 0, true);
			TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.AlternateTile, 2, 0);
			TileObjectData.newAlternate.DrawYOffset = 6;
			TileObjectData.addAlternate(1);
			TileObjectData.addTile(Type);

			RegisterItemDrop(ModContent.ItemType<Lucky_Hat>(), -1);

			HitSound = SoundID.Item32;
		}
		public static int PlacementPreviewHook_CheckIfCanPlace(int i, int j, int type, int style, int direction, int alternate) {
			Tile anchorTile = Framing.GetTileSafely(i, j + 1);
			if (TileObjectData.GetTileData(anchorTile) is TileObjectData anchorData) {
				TileUtils.GetMultiTileTopLeft(i, j + 1, anchorData, out int left, out _);
				if (left != i) return -1;
			}
			if (anchorTile.TileType == TileID.Containers && TileObjectData.GetTileStyle(anchorTile) != ChestID.Barrel) return -1;
			return 1;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			if (WorldGen.destroyObject && !Framing.GetTileSafely(i, j + 1).HasTile) {
				OriginSystem.QueueTileFrames(i, j);
			}
			return base.TileFrame(i, j, ref resetFrame, ref noBreak);
		}
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			if (TileObjectData.GetTileData(Type, 0).AnchorAlternateTiles.Contains(Framing.GetTileSafely(i, j + 1).TileType)) {
				offsetY += 6;
			}
		}
		public override void NumDust(int i, int j, bool fail, ref int num) => num = 0;

		public override bool RightClick(int i, int j) {
			WorldGen.KillTile(i, j);
			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendTileSquare(-1, i, j);
			}
			return true;
		}

		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;

			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<Lucky_Hat>();

			if (Main.tile[i, j].TileFrameX / 18 < 1) {
				player.cursorItemIconReversed = true;
			}
		}
		public IEnumerable<int> ProvideItemObtainability() {
			yield return ModContent.ItemType<Lucky_Hat>();
		}
	}
}
