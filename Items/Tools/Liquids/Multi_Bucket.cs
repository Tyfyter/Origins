using Microsoft.Xna.Framework.Graphics;
using Origins.Reflection;
using Origins.UI;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;

namespace Origins.Items.Tools.Liquids {
	public class Multi_Bucket : ModItem {
		static List<BucketMode> Modes = [];
		public override void SetStaticDefaults() {
			ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = true;
			ItemID.Sets.AlsoABuildingItem[Type] = true; //Unused ID Set, but may be useful for other modders or when the game updates
			ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.BottomlessBucket);
			Item.value = Item.sellPrice(gold: 50);
			Item.tileBoost += 2;
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override void AddRecipes() {
			Recipe r = CreateRecipe()
			.AddIngredient(ItemID.BottomlessBucket)
			.AddIngredient(ItemID.BottomlessLavaBucket)
			.AddIngredient(ItemID.BottomlessHoneyBucket);
			foreach (int bucket in BucketBase.EndlessBucketByLiquid) {
				if (bucket > -1) r.AddIngredient(bucket);
			}
			r.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.SubstituteKeybind(Keybindings.MultiBucket);
		}
		public static BucketMode GetMode(int index) {
			return Modes[index];
		}
		public override void HoldItem(Player player) {
			if (!player.JustDroppedAnItem)//make sure the player hasn't just dropped an item recently
			{
				if (player.whoAmI != Main.myPlayer) //make sure to only run on clients (we sync multiplayer data later down the line)
					return;
				//We make sure the player is in range to be able to place the liquid tile
				if (player.noBuilding || !player.IsTargetTileInItemRange(Item))
					return;
				BucketMode mode = MultiBucketUI.SelectedMode;
				//We set the cursor's item to be of the bucket item. To show the player that they are able to place a liquid tile at that coordinate normally
				if (!Main.GamepadDisableCursorItemIcon) {
					player.cursorItemIconEnabled = true;
					player.cursorItemIconID = mode.Item;
					Main.ItemIconCacheUpdate(mode.Item);
				}
				//Make sure that the item is being used to place liquid
				if (!player.ItemTimeIsZero || player.itemAnimation <= 0 || !player.controlUseItem)
					return;

				ContentExtensions.PourBucket(Item, player, mode.GetLiquid, player.altFunctionUse == 2, mode.ReplaceLiquids, true);
			}
		}
		internal static void Register(BucketMode mode) {
			Modes.Add(mode);
		}
		public static IEnumerable<BucketMode> GetSorted() => Modes;
		internal static void Sort() {
			Modes = new TopoSort<BucketMode>(Modes,
				mode => mode.SortAfter(),
				mode => mode.SortBefore()
			).Sort();
		}
	}
	[Flags]
	public enum BucketPetalData {
		Selected = 1 << 0
	}
	public abstract class BucketMode : ModTexturedType, IFlowerMenuItem<BucketPetalData> {
		public abstract int Item { get; }
		public abstract LocalizedText DisplayName { get; }
		public Asset<Texture2D> Texture2D { get; private set; }
		public virtual bool IsAvailable => true;
		public virtual bool ReplaceLiquids => false;
		public abstract int GetLiquid(int x, int y);
		protected sealed override void Register() {
			Multi_Bucket.Register(this);
			ModTypeLookup<BucketMode>.Register(this);
		}
		public sealed override void SetupContent() {
			if (!Main.dedServ) Texture2D = ModContent.Request<Texture2D>(Texture);
			SetStaticDefaults();
		}
		public static void DrawIcon(Texture2D texture, Vector2 position, Color tint, float scale = 1) {
			Main.spriteBatch.Draw(
				texture,
				position,
				null,
				tint,
				0f,
				texture.Size() * 0.5f,
				scale,
				SpriteEffects.None,
			0f);
		}
		public static void GetTints(bool hovered, bool enabled, out Color backTint, out Color iconTint) {
			if (enabled) {
				backTint = Color.White;
				iconTint = Color.White;
			} else if (hovered) {
				backTint = new Color(200, 200, 200);
				iconTint = new Color(120, 120, 120);
			} else {
				backTint = new Color(100, 100, 100);
				iconTint = new Color(80, 80, 80);
			}
		}
		public void Draw(Vector2 position, bool hovered, BucketPetalData extraData) {
			GetTints(hovered, extraData.HasFlag(BucketPetalData.Selected), out Color backTint, out Color iconTint);
			DrawIcon(TextureAssets.WireUi[hovered.ToInt()].Value, position, backTint);
			DrawIcon(Texture2D.Value, position, iconTint, 0.8f);
			if (hovered) Main.instance.MouseText(PrettyPrintName().Replace("Bottomless ", "").Replace(" Mode", ""));
		}
		public virtual IEnumerable<BucketMode> SortAfter() => [];
		public virtual IEnumerable<BucketMode> SortBefore() => [];
		public bool IsHovered(Vector2 position) => Main.MouseScreen.WithinRange(position, 20);
	}
	[Autoload(false)]
	public class AutoBucketMode(BucketBase bucket) : BucketMode {
		public override string Name => $"{bucket.Name}Mode";
		public override LocalizedText DisplayName => bucket.DisplayName;
		public override string Texture => bucket.Texture;
		public override int Item => bucket.Type;
		public override int GetLiquid(int x, int y) {
			return bucket.GetLiquid(x, y);
		}
		public override IEnumerable<BucketMode> SortAfter() => [ModContent.GetInstance<ShimmerBucketMode>()];
	}
	public class WaterBucketMode : BucketMode {
		public override int Item => ItemID.BottomlessBucket;
		public override LocalizedText DisplayName => Language.GetText($"ItemName.{Item}");
		public override string Texture => $"Terraria/Images/Item_{Item}";
		public override int GetLiquid(int x, int y) {
			return LiquidID.Water;
		}
	}
	public class LavaBucketMode : BucketMode {
		public override int Item => ItemID.BottomlessLavaBucket;
		public override LocalizedText DisplayName => Language.GetText($"ItemName.{Item}");
		public override string Texture => $"Terraria/Images/Item_{Item}";
		public override int GetLiquid(int x, int y) {
			return LiquidID.Lava;
		}
		public override IEnumerable<BucketMode> SortAfter() => [ModContent.GetInstance<WaterBucketMode>()];
	}
	public class HoneyBucketMode : BucketMode {
		public override int Item => ItemID.BottomlessHoneyBucket;
		public override LocalizedText DisplayName => Language.GetText($"ItemName.{Item}");
		public override string Texture => $"Terraria/Images/Item_{Item}";
		public override int GetLiquid(int x, int y) {
			return LiquidID.Honey;
		}
		public override IEnumerable<BucketMode> SortAfter() => [ModContent.GetInstance<LavaBucketMode>()];
	}
	public class ShimmerBucketMode : BucketMode {
		public override int Item => ItemID.BottomlessShimmerBucket;
		public override LocalizedText DisplayName => Language.GetText($"ItemName.{Item}");
		public override string Texture => $"Terraria/Images/Item_{Item}";
		public override bool IsAvailable => Main.Achievements.GetAchievement("CHAMPION_OF_TERRARIA").IsCompleted;
		public override int GetLiquid(int x, int y) {
			return LiquidID.Shimmer;
		}
		public override IEnumerable<BucketMode> SortAfter() => [ModContent.GetInstance<HoneyBucketMode>()];
	}
	[ReinitializeDuringResizeArrays]
	public class MultiBucketUI : ItemModeFlowerMenu<BucketMode, BucketPetalData> {
		public static BucketMode SelectedMode;
		static MultiBucketUI() {
			if (!PrivateClassMethods.ContentLoadingFinished.Value) return;
			Multi_Bucket.Sort();
			SelectedMode = Multi_Bucket.GetSorted().First();
		}
		public override void Click(BucketMode mode) {
			if (RightClicked) return;
			SelectedMode = mode;
		}
		public override float DrawCenter() {
			return 44;
		}
		public override bool GetCursorAreaTexture(BucketMode mode, out Texture2D texture, out Rectangle? frame, out Color color) {
			texture = null;
			frame = null;
			color = default;
			return false;
		}
		public override BucketPetalData GetData(BucketMode mode) {
			BucketPetalData data = 0;
			if (SelectedMode == mode) data |= BucketPetalData.Selected;
			return data;
		}
		public override IEnumerable<BucketMode> GetModes() {
			return Multi_Bucket.GetSorted().Where(mode => mode.IsAvailable);
		}
		public override bool IsActive() => Main.LocalPlayer.HeldItem.ModItem is Multi_Bucket;
		public override bool Toggle => Keybindings.MultiBucket.JustPressed;
	}
}
