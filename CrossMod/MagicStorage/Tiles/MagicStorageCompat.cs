using MagicStorage.Components;
using Terraria.DataStructures;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using MagicStorage;
using Terraria.Audio;
using Terraria.ID;
using Origins.Items.Materials;
using Microsoft.Xna.Framework.Graphics;
using MagicStorageItems = MagicStorage.Items;

namespace Origins.CrossMod.MagicStorage.Tiles {
	[ExtendsFromMod(nameof(MagicStorage))]
	public class Defiled_Storage_Unit() : OriginsStorageUnit<Defiled_Bar, MagicStorageItems.UpgradeHellstone, MagicStorageItems.StorageUnitHellstone>(1) { }
	[ExtendsFromMod(nameof(MagicStorage))]
	public class Encrusted_Storage_Unit() : OriginsStorageUnit<Encrusted_Bar, MagicStorageItems.UpgradeHellstone, MagicStorageItems.StorageUnitHellstone>(1) { }
	[ExtendsFromMod(nameof(MagicStorage))]
	public class Sanguinite_Storage_Unit() : OriginsStorageUnit<Sanguinite_Bar, MagicStorageItems.UpgradeHellstone, MagicStorageItems.StorageUnitHellstone>(1) { }
	[ExtendsFromMod(nameof(MagicStorage))]
	public class OriginsStorageUpgrading : GlobalTile {
		public override void Load() {
			MonoModHooks.Add(typeof(TEStorageUnit).GetMethod(nameof(TEStorageUnit.ValidTile)), ValidTile);
		}
		delegate bool orig_ValidTile(TEStorageUnit self, in Tile tile);
		static bool ValidTile(orig_ValidTile orig, TEStorageUnit self, in Tile tile) {
			if (TileLoader.GetTile(tile.TileType) is StorageUnit && tile.TileFrameX % 36 == 0 && tile.TileFrameY % 36 == 0) return true;
			return orig(self, tile);
		}
		public override void RightClick(int i, int j, int type) {
			TryUpgradeStorage(i, j, type);
		}
		public static void SetTypeAndStyle(int i, int j, ushort type, int style) {
			Main.tile[i, j].TileType = type;
			Main.tile[i + 1, j].TileType = type;
			Main.tile[i, j + 1].TileType = type;
			Main.tile[i + 1, j + 1].TileType = type;

			Main.tile[i, j].TileFrameY = (short)(36 * style);
			Main.tile[i + 1, j].TileFrameY = (short)(36 * style);
			Main.tile[i, j + 1].TileFrameY = (short)(36 * style + 18);
			Main.tile[i + 1, j + 1].TileFrameY = (short)(36 * style + 18);
		}
		internal static int GetCapacityForPrevStyle(int style) {
			style--;
			if (style == 8) return 4;
			if (style > 1) style--;
			int num2 = style + 1;
			if (num2 > 4) num2++;
			if (num2 > 6) num2++;
			if (num2 > 8) num2 += 7;
			return 40 * num2;
		}
		public static void TryUpgradeStorage(int i, int j, int type) {
			if (Main.tile[i, j].TileFrameX % 36 == 18) i--;
			if (Main.tile[i, j].TileFrameY % 36 == 18) j--;
			Player player = Main.LocalPlayer;
			Item item = player.HeldItem;
			if (item?.ModItem is not OriginsStorageUpgrade upgradeItem) return;
			if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is TEStorageUnit storageUnitTE) {
				if (storageUnitTE.Capacity != GetCapacityForPrevStyle(upgradeItem.Unit.Tier)) return;
			} else {
				storageUnitTE = null;
			}
			SetTypeAndStyle(i, j, upgradeItem.Unit.Type, upgradeItem.Unit.Tier);
			player.tileInteractionHappened = true;
			if (storageUnitTE is not null) {
				storageUnitTE.UpdateTileFrame();
				NetMessage.SendTileSquare(Main.myPlayer, i, j, 2, 2);
				TEStorageHeart heart = storageUnitTE.GetHeart();
				if (heart != null) {
					switch (Main.netMode) {
						case NetmodeID.SinglePlayer:
						heart.ResetCompactStage();
						break;

						default:
						NetHelper.SendResetCompactStage(heart.Position);
						break;
					}
				}
				item.stack--;
				if (item.stack <= 0) item.SetDefaults();
				if (player.selectedItem == 58) Main.mouseItem = item.Clone();
				if (player.selectedItem == 58) {
					Main.mouseItem.stack--;
					if (Main.mouseItem.stack <= 0) Main.mouseItem.TurnToAir();
				} else {
					item.stack--;
					if (item.stack <= 0) item.TurnToAir();
				}
				player.ConsumeItem(item.type);
				SoundEngine.PlaySound(in SoundID.MaxMana, storageUnitTE.Position.ToWorldCoordinates());
				Dust.NewDustPerfect(storageUnitTE.Position.ToWorldCoordinates(), 110, Vector2.Zero, 0, Color.Green, 2f);
			}
		}
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public class OriginsStorageUnit<TMaterial, TNextUpgradeItem, TNextUnitItem>(int tier) : OriginsStorageUnit(tier) where TMaterial : ModItem where TNextUpgradeItem : ModItem where TNextUnitItem : ModItem {
		public override int MaterialItem => ModContent.ItemType<TMaterial>();
		public override int NextUpgradeItem => ModContent.ItemType<TNextUpgradeItem>();
		public override int NextUnitItem => ModContent.ItemType<TNextUnitItem>();
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public abstract class OriginsStorageUnit(int tier) : StorageUnit {
		public int Tier { get; } = tier;
		public abstract int MaterialItem { get; }
		public OriginsStorageUpgrade UpgradeItem { get; private set; }
		public virtual int NextUpgradeStyle => Tier + 1;
		public abstract int NextUpgradeItem { get; }
		public abstract int NextUnitItem { get; }
		public override void Load() {
			UpgradeItem = new(this);
			Mod.AddContent(UpgradeItem);
			Mod.AddContent(new OriginsStorageUnitItem(this));
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (Main.tile[i, j].TileFrameX % 36 == 18) i--;
			if (Main.tile[i, j].TileFrameY % 36 == 6) j--;
			if (TileEntity.ByPosition.ContainsKey(new Point16(i, j)) && Main.tile[i, j].TileFrameX / 36 % 3 != 0) fail = true;
		}
		public override bool CanExplode(int i, int j) {
			bool fail = false;
			bool discard = false;
			bool discard2 = false;
			KillTile(i, j, ref fail, ref discard, ref discard2);
			return !fail;
		}
		public override bool RightClick(int i, int j) {
			if (Main.tile[i, j].TileFrameX % 36 == 18) i--;
			if (Main.tile[i, j].TileFrameY % 36 == 18) j--;
			Player player = Main.LocalPlayer;
			Item item = player.HeldItem;
			if (!TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) || te is not TEStorageUnit storageUnitTE) storageUnitTE = null;
			if (item?.type == NextUpgradeItem) {
				OriginsStorageUpgrading.SetTypeAndStyle(i, j, (ushort)ModContent.TileType<StorageUnit>(), Tier);
				return base.RightClick(i, j);
			}
			Main.LocalPlayer.tileInteractionHappened = true;
			string obj = storageUnitTE.Inactive ? Language.GetTextValue("Mods.MagicStorage.Inactive") : Language.GetTextValue("Mods.MagicStorage.Active");
			string fullnessString = Language.GetTextValue("Mods.MagicStorage.Capacity", storageUnitTE.NumItems, storageUnitTE.Capacity);
			Main.NewText(obj + ", " + fullnessString);
			return false;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {
			Tile tile = Main.tile[i, j];
			Vector2 vector = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
			Vector2 position = vector + 16f * new Vector2(i, j) - Main.screenPosition;
			Rectangle value = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
			Color color = Lighting.GetColor(i, j, Color.White);
			Color color2 = Color.Lerp(Color.White, color, 0.5f);
			spriteBatch.Draw(ModContent.Request<Texture2D>(Texture + "_Glow").Value, position, value, color2);
		}
	}
	[ExtendsFromMod(nameof(MagicStorage))]
	public class OriginsStorageUpgrade(OriginsStorageUnit unit) : ModItem {
		public OriginsStorageUnit Unit { get; } = unit;
		public override string Name => Unit.Name[..^"_Unit".Length] + "_Upgrade";
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 10;
		}
		public override void SetDefaults() {
			Item.width = 12;
			Item.height = 12;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 32);
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(Unit.MaterialItem, 10)
			.AddIngredient(ItemID.Amethyst)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}

	[ExtendsFromMod(nameof(MagicStorage))]
	public class OriginsStorageUnitItem(OriginsStorageUnit unit) : ModItem {
		public override string Name => Unit.Name + "_Item";
		public OriginsStorageUnit Unit { get; } = unit;
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 10;
		}
		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 26;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 32);
			Item.createTile = Unit.Type;
			Item.placeStyle = Unit.Tier;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<MagicStorageItems.StorageUnit>()
			.AddIngredient(Unit.UpgradeItem.Type)
			.Register();
			Recipe.Create(Unit.NextUnitItem)
			.AddIngredient(Type)
			.AddIngredient(Unit.NextUpgradeItem)
			.Register();
		}
	}
}
