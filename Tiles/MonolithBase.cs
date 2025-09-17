using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Tiles.Other;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles {
	[Autoload(false)]
	public class MonolithItem(MonolithBase tile) : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Monolith"
		];
		[CloneByReference]
		public MonolithBase Tile { get; } = tile;
		protected override bool CloneNewInstances => true;
		public override string Texture => Tile.Texture + "_Item";
		public override string Name => Tile.Name + "_Item";
		public event Action<Item> ExtraDefaults;
		public event Action<Item> OnAddRecipes;
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
			Tile.RegisterItemDrop(Type);
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(Tile.Type);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(gold: 4);
			Item.accessory = true;
			Item.vanity = true;
			if (ExtraDefaults is not null) {
				ExtraDefaults(Item);
				ExtraDefaults = null;
			}
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			if (!hideVisual) UpdateVanity(player);
		}
		public override void UpdateVanity(Player player) {
			if (player.whoAmI == Main.myPlayer) Tile.ApplyEffectEquipped(player);
		}
		public override void AddRecipes() {
			if (OnAddRecipes is not null) {
				OnAddRecipes(Item);
				OnAddRecipes = null;
			}
		}
		public MonolithItem WithExtraDefaults(Action<Item> extra) {
			ExtraDefaults += extra;
			return this;
		}
		public MonolithItem WithOnAddRecipes(Action<Item> recipes) {
			OnAddRecipes += recipes;
			return this;
		}
	}
	public abstract class MonolithBase : ModTile {
		public virtual int Height => 3;
		public abstract int Frames { get; }
		public abstract Color MapColor { get; }
		public MonolithItem Item { get; private set; }
		public sealed override void Load() {
			Mod.AddContent(Item = new MonolithItem(this));
			OnLoad();
		}
		public virtual void OnLoad() { }
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.VoidMonolith, 0));
			TileObjectData.newTile.Height = Height;
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, Height).ToArray();
			TileObjectData.newTile.Origin = new(1, Height - 1);
			TileObjectData.addTile(Type);

			AddMapEntry(MapColor, Mod.GetLocalization($"Items.{Name}_Item.DisplayName"));
			DustType = DustID.Stone;
		}
		public bool IsEnabled(int i, int j) => Main.tile[i, j].TileFrameY >= 18 * Height;
		public bool IsEnabled(Tile tile) => tile.TileFrameY >= 18 * Height;
		public override void NearbyEffects(int i, int j, bool closer) {
			if (closer && IsEnabled(i, j)) ApplyEffect();
		}
		public virtual void ApplyEffectEquipped(Player player) => ApplyEffect();
		public abstract void ApplyEffect();
		public override void MouseOver(int i, int j) {
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			
			player.cursorItemIconID = TileLoader.GetItemDropFromTypeAndStyle(Type, TileObjectData.GetTileStyle(Main.tile[i, j]));
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override bool RightClick(int i, int j) {
			SoundEngine.PlaySound(SoundID.Mech, new Vector2(i * 16, j * 16));
			ToggleTile(i, j);
			return true;
		}

		public override void HitWire(int i, int j) {
			ToggleTile(i, j);
		}

		// ToggleTile is a method that contains code shared by HitWire and RightClick, since they both toggle the state of the tile.
		// Note that TileFrameY doesn't necessarily match up with the image that is drawn, AnimateTile and AnimateIndividualTile contribute to the drawing decisions.
		public void ToggleTile(int i, int j) {
			Tile tile = Main.tile[i, j];
			int height = Height * 18;
			int topX = i - tile.TileFrameX % 36 / 18;
			int topY = j - tile.TileFrameY % height / 18;

			short frameAdjustment = (short)(tile.TileFrameY >= height ? -height : height);

			for (int x = topX; x < topX + 2; x++) {
				for (int y = topY; y < topY + Height; y++) {
					Main.tile[x, y].TileFrameY += frameAdjustment;

					if (Wiring.running) {
						Wiring.SkipWire(x, y);
					}
				}
			}

			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendTileSquare(-1, topX, topY, 3, 2);
			}
		}

		public override void AnimateTile(ref int frame, ref int frameCounter) {
			if (++frameCounter >= 4) {
				frameCounter = 0;
				frame = (frame + 1) % Frames;
			}
		}
		public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset) {
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameY >= Height * 18) {
				frameYOffset = Main.tileFrame[type] * Height * 18;
			} else {
				frameYOffset = 0;
			}
		}
	}
}
