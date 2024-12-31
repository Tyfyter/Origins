using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Buffs;

namespace Origins.Items.Weapons.Ranged {
    public class Chipper_Knife : ModItem, ICustomWikiStat {
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThrowingKnife);
			Item.shoot = ModContent.ProjectileType<Chipper_Knife_P>();
			Item.value = Item.sellPrice(copper: 12);
        }
	}
	public class Chipper_Knife_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ranged/Chipper_Knife";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThrowingKnife);
			AIType = ProjectileID.ThrowingKnife;
		}
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Rasterized_Debuff.ID, 16);
        }
    }
}
