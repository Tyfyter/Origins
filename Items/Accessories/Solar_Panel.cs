using Origins.Dev;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Solar_Panel : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Vitality
		];
		public static int BackSlot { get; private set; }
		public override void SetStaticDefaults() {
			BackSlot = Item.backSlot;
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.Lime;
			Item.value = Item.sellPrice(gold: 6);
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().solarPanel = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.BandofStarpower)
			.AddIngredient(ItemID.SunStone)
			.AddIngredient(ModContent.ItemType<Silicon_Bar>(), 8)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public static float GetSunlightFactor(int waterStyle, params Span<(int i, int j)> positions) {
			static Vector3 DoTileCalcs(int x, int top, Vector3 sunLight, ref Vector3? waterFactor, int waterStyle) {
				for (int y = top; y > 0; y--) {
					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.HasUnactuatedTile && Main.tileBlockLight[tile.TileType]) {
						sunLight = Vector3.Zero;
					} else if (tile.LiquidAmount > 0) {
						switch (tile.LiquidType) {
							case LiquidID.Water:
							waterFactor ??= GetWaterFactor(waterStyle);
							sunLight *= waterFactor.Value;
							sunLight -= waterFactor.Value * 0.01f;
							break;
							case LiquidID.Lava:
							sunLight = Vector3.Zero;
							break;
							case LiquidID.Honey:
							sunLight *= new Vector3(0.25f, 0.25f, 0f);
							sunLight -= new Vector3(0.0025f, 0.0025f, 0f);
							break;
						}
					} else if (y > Main.worldSurface) {
						sunLight *= new Vector3(0.999f, 0.999f, 0.999f);
						sunLight -= new Vector3(0.001f, 0.001f, 0.001f);
					}
					if (sunLight.X < 0) {
						sunLight.X = 0;
					}
					if (sunLight.Y < 0) {
						sunLight.Y = 0;
					}
					if (sunLight.Z < 0) {
						sunLight.Z = 0;
					}
					if (sunLight == Vector3.Zero) {
						break;
					}
				}
				return sunLight;
			}
			Vector3 sunLight = new(1f, 1f, 0.67f);
			float sunFactor = 0;
			Vector3? waterFactor = null;
			waterFactor /= 1.015f;
			for (int i = 0; i < positions.Length; i++) sunFactor += DoTileCalcs(positions[i].i, positions[i].j, sunLight, ref waterFactor, waterStyle).Length() / 1.56f;
			return sunFactor / positions.Length;
			static Vector3 GetWaterFactor(int waterStyle) {
				switch (waterStyle) {
					case WaterStyleID.Purity:
					case WaterStyleID.Lava:
					case WaterStyleID.Underground:
					case WaterStyleID.Cavern:
					return new Vector3(0.88f, 0.96f, 1.015f);

					case WaterStyleID.Corrupt:
					return new Vector3(0.94f, 0.85f, 1.01f) * 0.91f;
					case WaterStyleID.Jungle:
					return new Vector3(0.84f, 0.95f, 1.015f) * 0.91f;
					case WaterStyleID.Hallow:
					return new Vector3(0.90f, 0.86f, 1.01f) * 0.91f;
					case WaterStyleID.Snow:
					return new Vector3(0.64f, 0.99f, 1.01f) * 0.91f;
					case WaterStyleID.Desert:
					return new Vector3(0.93f, 0.83f, 0.98f) * 0.91f;
					case WaterStyleID.Bloodmoon:
					return new Vector3(1f, 0.88f, 0.84f) * 0.91f;
					case WaterStyleID.Crimson:
					return new Vector3(0.83f, 1f, 1f) * 0.91f;
					case WaterStyleID.UndergroundDesert:
					return new Vector3(0.95f, 0.98f, 0.85f) * 0.91f;
					case 13:
					return new Vector3(0.9f, 1f, 1.02f) * 0.91f;

					case WaterStyleID.Honey:
					return new Vector3(0f);

					default:
					Vector3 waterFactor = Vector3.One;
					if (LoaderManager.Get<WaterStylesLoader>().Get(Main.waterStyle) is ModWaterStyle _waterStyle) _waterStyle.LightColorMultiplier(ref waterFactor.X, ref waterFactor.Y, ref waterFactor.Z);
					return waterFactor;
				}
			}
		}
	}
}
