using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Riven {
    public class Ameballoon : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ameballoon");
			Tooltip.SetDefault("");
            glowmask = Origins.AddGlowMask(this, "");
            SacrificeTotal = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Grenade);
			Item.damage = 25;
			Item.width = 20;
			Item.height = 22;
			Item.useTime = 18;
			Item.useAnimation = 18;
			//item.knockBack = 5;
            Item.shoot = ModContent.ProjectileType<Ameballoon_P>();
            Item.shootSpeed = 8.75f;
            Item.knockBack = 5f;
			Item.value = 5000;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
            Item.glowMask = glowmask;
        }
    }
    public class Ameballoon_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Riven/Ameballoon";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ameballoon");
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.aiStyle = ProjAIStyleID.GroundProjectile;
            Projectile.penetrate = 1;
			Projectile.width = 22;
			Projectile.height = 22;
            Projectile.scale*=0.6f;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 60;
        }
		public override bool PreKill(int timeLeft) {
			return base.PreKill(timeLeft);
		}
		public override void Kill(int timeLeft) {
            SoundEngine.PlaySound(SoundID.NPCDeath1.WithPitch(0.15f));
            PolarVec2 vel = new PolarVec2(4,0);
			for (int i = Main.rand.Next(12, 16); i-->0;) {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2)vel, ModContent.ProjectileType<Ameballoon_Shrapnel>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                vel.Theta += Main.rand.NextFloat(0.5f) + 1.618033988749894848204586834f;
                vel.R += Main.rand.NextFloat(0.5f);

            }
		}
    }
    public class Ameballoon_Shrapnel : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Riven/Ameballoon_P";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Ameballoon");
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.timeLeft = 3600;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.width = 22;
            Projectile.height = 22;
            //projectile.scale*=1.25f;
            Projectile.ignoreWater = true;
        }
		public override void AI() {
            Projectile.rotation -= MathHelper.PiOver2;
        }
        public override void Kill(int timeLeft) {
            if(timeLeft < 3590)SoundEngine.PlaySound(SoundID.NPCDeath1.WithPitch(0.15f).WithVolumeScale(0.5f));
        }
    }
}
