using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Back)]
	public class Helium_Tank : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Misc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.buffImmune[BuffID.Suffocation] = true;
			player.AddMaxBreath(257);
			if (!hideVisual) UpdateSqueakiness(player, false);
		}
		public override void UpdateVanity(Player player) {
			UpdateSqueakiness(player, true);
		}
		public void UpdateSqueakiness(Player player, bool vanitySlot) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.heliumTank = true;
			int count = player.armor.Length / 2;
			int offset = vanitySlot ? count : 0;
			for (int i = 0; i < count; i++) {
				if (player.armor[i + offset] == Item) {
					originPlayer.heliumTankStrength = (i + 1) / (float)count;
					break;
				}
			}
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.WhoopieCushion)
			.AddIngredient(ModContent.ItemType<Air_Tank>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
