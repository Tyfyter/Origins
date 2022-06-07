using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles.Weapons;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Acid {
	public class Acid_Grenade : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acid Grenade");
			Tooltip.SetDefault("");
            glowmask = Origins.AddGlowMask(this);
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Grenade);
            //item.maxStack = 999;
            Item.damage = 30;
			Item.value*=14;
            Item.shoot = ModContent.ProjectileType<Acid_Grenade_P>();
			Item.shootSpeed*=1.5f;
            Item.knockBack = 5f;
            Item.ammo = ItemID.Grenade;
			Item.rare = ItemRarityID.Lime;
            Item.glowMask = glowmask;
        }
    }
    public class Acid_Grenade_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Acid/Acid_Grenade";
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.timeLeft = 135;
            Projectile.penetrate = 1;
        }
        public override bool PreKill(int timeLeft) {
            Projectile.type = ProjectileID.Grenade;
            return true;
        }
        public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
			//Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 122, 2f, 1f);
            int t = ModContent.ProjectileType<Acid_Shot>();
            for(int i = Main.rand.Next(3); i < 6; i++)Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Main.rand.NextVector2Unit()*4)+(Projectile.velocity/8), t, Projectile.damage/8, 6, Projectile.owner, ai1:-0.5f).scale = 0.85f;
        }
    }
}
