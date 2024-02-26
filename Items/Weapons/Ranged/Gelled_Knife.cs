using Origins.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    public class Gelled_Knife : ModItem {
        static short glowmask;
        public string[] Categories => new string[] {
            "Torn",
            "TornSource",
            "OtherRanged"
        };
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
        }
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThrowingKnife);
			Item.shoot = ModContent.ProjectileType<Gelled_Knife_P>();
			Item.value = Item.sellPrice(copper: 12);
            Item.glowMask = glowmask;
        }
	}
	public class Gelled_Knife_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ranged/Gelled_Knife";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThrowingKnife);
		}
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
            if (Main.rand.NextBool(2)) {
                OriginGlobalNPC.InflictTorn(target, 120, 300, 0.45f, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
            }
        }
    }
}