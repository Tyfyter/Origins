using Origins.CrossMod;
using Origins.Dev;
using System.Collections.Generic;
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
	public sealed class WaterFountainItem(WaterFountain fountainTile) : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"WaterFountain"
		];
		public WaterFountain FountainTile { get; } = fountainTile;
		public override string Name => FountainTile.Name + "_Item";
		public override string Texture => FountainTile.Texture + "_Item";
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
			FountainTile.RegisterItemDrop(Type);
			if (!Main.dedServ && ModLoader.HasMod(nameof(Fargowiltas))) {
				FargosFountainsForceFbiomes.AddFountainAssociation(FountainTile.Biome, FountainTile.WaterStyle.Slot, FountainTile.SetBiomeActive);
			}
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(FountainTile.Type);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(gold: 4);
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (!ModLoader.HasMod(nameof(Fargowiltas))) return;
			static string ExpandedTooltipLoc(ModBiome biome) =>
				Language.GetOrRegister("Mods.Origins.CrossMod.ExpandedTooltips.Fountain").Format(biome.DisplayName);
			TooltipLine FountainTooltip(ModBiome biome) {
				return new TooltipLine(Mod, "Tooltip0", $"[i:909] [c/AAAAAA:{ExpandedTooltipLoc(biome)}]");
			}
			tooltips.Add(FountainTooltip(FountainTile.Biome));
		}
	}
	[ExtendsFromMod(nameof(Fargowiltas))]
	class FargosFountainFtooltipForderer : GlobalItem {
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.ModItem is WaterFountainItem;
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			int tooltipLine = tooltips.FindIndex((line) => line.Mod == nameof(Fargowiltas) && line.Name == "TooltipNPCSold");
			if (tooltipLine != -1) {
				(tooltips[tooltipLine - 1], tooltips[tooltipLine]) = (tooltips[tooltipLine], tooltips[tooltipLine - 1]);
			}
		}
	}
	public abstract class WaterFountain : ModTile {
		public virtual int Height => 4;
		public virtual int Frames => 6;
		WaterFountainItem item;
		public static int ItemType<TTile>() where TTile : WaterFountain {
			TTile tile = ModContent.GetInstance<TTile>();
			return tile.item.Type;
		}
		public sealed override void Load() {
			Mod.AddContent(item = new WaterFountainItem(this));
			OnLoad();
		}
		public virtual void OnLoad() {

		}
		public abstract ModBiome Biome { get; }
		public virtual ModWaterStyle WaterStyle => Biome.WaterStyle;
		public abstract void SetBiomeActive();
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileLighted[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.GetTileData(TileID.WaterFountain, 0));
			TileObjectData.newTile.Height = Height;
			TileObjectData.newTile.CoordinateHeights = Enumerable.Repeat(16, Height).ToArray();
			TileObjectData.newTile.Origin = new(1, Height - 1);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(100, 100, 100), Language.GetText("MapObject.WaterFountain"));
			DustType = DustID.Stone;
		}
		public bool IsEnabled(int i, int j) => Main.tile[i, j].TileFrameY >= 18 * Height;
		public bool IsEnabled(Tile tile) => tile.TileFrameY >= 18 * Height;
		public override void NearbyEffects(int i, int j, bool closer) {
			if (closer && IsEnabled(i, j)) Main.SceneMetrics.ActiveFountainColor = WaterStyle.Slot;
		}
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
	public abstract class WaterFountainBase<TBiome> : WaterFountain where TBiome : ModBiome {
		public override ModBiome Biome => ModContent.GetInstance<TBiome>();
	}
}
