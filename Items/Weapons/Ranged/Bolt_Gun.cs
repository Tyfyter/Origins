using Origins.Dev;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Ranged {
	public class Bolt_Gun : ModItem {
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Handgun);
			Item.damage = 38;
			Item.knockBack = 5;
			Item.crit = 4;
			Item.useTime = Item.useAnimation = 38;
			Item.shoot = ProjectileID.Bullet;
			Item.shootSpeed = 8;
			Item.width = 38;
			Item.height = 18;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = Origins.Sounds.HeavyCannon;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile p = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
			if (p.penetrate > 0) {
				p.penetrate++;
				p.localNPCHitCooldown = 10;
				p.usesLocalNPCImmunity = true;
			}
			return false;
		}
	}
}
