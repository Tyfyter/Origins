using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Eyndum_Scar : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Sword,
			WikiCategories.ReworkExpected
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Katana);
			Item.damage = 407;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = false;
			Item.noMelee = false;
			Item.width = 70;
			Item.height = 70;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.knockBack = 9.5f;
			Item.shoot = ProjectileID.None;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = CrimsonRarity.ID;
			Item.autoReuse = true;
		}
		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
			if (!(target.boss || NPCID.Sets.ShouldBeCountedAsBoss[target.type])) {
				int quarterHealth = target.lifeMax / 4;
				if (target.life <= quarterHealth) {
					modifiers.SetInstantKill();
				}
			}
		}
	}
}
