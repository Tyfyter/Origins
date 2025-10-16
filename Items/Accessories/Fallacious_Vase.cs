using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Fallacious_Vase : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Movement
		];
		public override void Load() {
			On_WorldGen.SpawnThingsFromPot += On_WorldGen_SpawnThingsFromPot;
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WarriorEmblem);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.dashVase = true;
			originPlayer.dashVaseVisual = !hideVisual;
		}
		public override void UpdateItemDye(Player player, int dye, bool hideVisual) => player.OriginPlayer().dashVaseDye = dye;

		private static void On_WorldGen_SpawnThingsFromPot(On_WorldGen.orig_SpawnThingsFromPot orig, int i, int j, int x2, int y2, int style) {
			if (Player.GetClosestRollLuck(i, j, 1100) == 0f) {
				Item.NewItem(WorldGen.GetItemSource_FromTileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<Fallacious_Vase>());
				return;
			}
			orig(i, j, x2, y2, style);
		}
	}
}
