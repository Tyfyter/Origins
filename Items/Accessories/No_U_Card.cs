using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class No_U_Card : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(16, 18);
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightPurple;
		}
		public override void UpdateEquip(Player player) {
			player.thorns += 0.5f;
			player.GetModPlayer<OriginPlayer>().noU = true;
			for (int i = 0; i < Player.MaxBuffs; i++) {
				int buffType = player.buffType[i];
				if (!Main.debuff[buffType] || BuffID.Sets.NurseCannotRemoveDebuff[buffType]) continue;
				switch (buffType) {
					case BuffID.Weak:
					case BuffID.BrokenArmor:
					case BuffID.Bleeding:
					case BuffID.Poisoned:
					case BuffID.Slow:
					case BuffID.Confused:
					case BuffID.Silenced:
					case BuffID.Cursed:
					case BuffID.Darkness:
                    //case Rasterized_Debuff.ID:
                    player.DelBuff(i--);
					break;
				}
			}
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.AnkhCharm);
			recipe.AddIngredient(ModContent.ItemType<Return_To_Sender>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
