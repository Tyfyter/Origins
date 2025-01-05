using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Gun_Magazine : ModItem, ICustomWikiStat { //condition for guns only
		public string[] Categories => [
			"Combat",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Ranged) += 0.05f;
		}
	}
	public class Demonite_Gun_Magazine : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Ranged) += 0.1f;
		}
	}
	public class Crimtane_Gun_Magazine : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Ranged) += 0.12f;
		}
	}
	public class Defiled_Gun_Magazine : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Ranged) += 0.1f;
		}
	}
	public class Encrusted_Gun_Magazine : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Ranged) += 0.1f;
		}
	}
	public class Sanguinite_Gun_Magazine : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Ranged) += 0.1f;
		}
	}
	public class Molten_Gun_Magazine : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Ranged) += 0.15f;
		}
	}
	public class Eitrite_Gun_Magazine : ModItem, ICustomWikiStat { //extra bulet function
		public string[] Categories => [
			"Combat",
			"RangedBoostAcc"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Ranged) += 0.2f;
		}
	}
}
