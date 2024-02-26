using Origins.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    public class Gelled_Knife : ModItem {
        public string[] Categories => new string[] {
            "Torn",
            "TornSource"
        };
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThrowingKnife);
			Item.shoot = ModContent.ProjectileType<Gelled_Knife_P>();
			Item.value = Item.sellPrice(copper: 12);
		}
	}
	public class Gelled_Knife_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ranged/Gelled_Knife";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThrowingKnife);
		}
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            OriginGlobalNPC.InflictTorn(target, 300, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
        }
    }
}
