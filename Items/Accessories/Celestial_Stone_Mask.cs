using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Celestial_Stone_Mask : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override void SetStaticDefaults() {
			ArmorIDs.Face.Sets.DrawInFaceUnderHairLayer[Item.faceSlot] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 24);
			Item.value = Item.sellPrice(gold: 18);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Melee) += 0.15f;
			player.GetDamage(DamageClass.Generic) += 0.1f;
			player.GetCritChance(DamageClass.Generic) += 4f;
			player.lifeRegen += 4;
			player.statDefense += 8;
			player.pickSpeed -= 0.25f;
			player.GetKnockback(DamageClass.Summon).Base += 1;

			player.OriginPlayer().moveSpeedMult *= 0.9f;
			player.jumpSpeedBoost -= 1.8f;
		}

        public override void AddRecipes() {
            CreateRecipe()
            .AddIngredient(ItemID.CelestialStone)
            .AddIngredient(ModContent.ItemType<Stone_Mask>())
            .AddTile(TileID.TinkerersWorkbench)
            .Register();
        }
    }
}
