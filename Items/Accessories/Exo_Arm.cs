using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Exo_Arm : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.ExplosiveBoostAcc
		];
		public static int HandsOnID { get; private set; }
		public override void SetStaticDefaults() {
			HandsOnID = Item.handOnSlot;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 32);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().thrownProjectileSpeed += 0.2f;
			player.pickSpeed -= 0.2f;
			player.GetAttackSpeed(DamageClass.Melee) += 0.1f;
			player.GetAttackSpeed(DamageClasses.Explosive) += 0.1f;
			player.GetKnockback(DamageClass.Melee) += 0.1f;
		}
	}
}
