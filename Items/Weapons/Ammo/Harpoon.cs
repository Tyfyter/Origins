using Microsoft.Xna.Framework;
using Origins.Projectiles.Weapons;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
    public class Harpoon : ModItem {
        public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Harpoon");
            SacrificeTotal = 99;
            ID = Type;
        }
        public override void SetDefaults() {
            Item.damage = 10;
            Item.DamageType = DamageClass.Ranged;
            Item.consumable = true;
            Item.maxStack = 99;
            Item.shoot = Harpoon_P.ID;
            Item.ammo = Type;
        }
    }
    public class Harpoon_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Harpoon;
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Harpoon");
            ID = Type;
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Harpoon);
        }
		public override void OnSpawn(IEntitySource source) {
            if (Projectile.ai[1] == 1) {
                Projectile.penetrate = 2;
            }
		}
		public override void AI() {
            if (Projectile.ai[0] == 1 && Projectile.penetrate >= 0) {
                Projectile.aiStyle = 1;
                Projectile.velocity = Projectile.oldVelocity;
                Projectile.tileCollide = true;
                Vector2 diff = Main.player[Projectile.owner].itemLocation - Projectile.Center;
                SoundEngine.PlaySound(SoundID.Item10, Projectile.Center + diff / 2);
                float len = diff.Length() * 0.25f;
                diff /= len;
                Vector2 pos = Projectile.Center;
                for (int i = 0; i < len; i++) {
                    Dust.NewDust(pos - new Vector2(2), 4, 4, DustID.Stone, Scale:0.75f);
                    pos += diff;
				}
            }
		}
	}
}
