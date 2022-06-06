using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Acid;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Felnum;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Explosives {
	public class Thermite_Launcher : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Thermite Launcher");
			Tooltip.SetDefault("Burn.\nUses Thermite Canisters for ammo");
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.GrenadeLauncher);
            //item.maxStack = 999;
            Item.width = 44;
            Item.height = 18;
            Item.damage = 34;
			Item.value/=2;
			Item.useTime = (int)(Item.useTime*1.35);
			Item.useAnimation = (int)(Item.useAnimation*1.35);
            Item.shoot = ModContent.ProjectileType<Thermite_Canister_P>();
            Item.useAmmo = ModContent.ItemType<Thermite_Canister>();
            Item.knockBack = 2f;
            Item.shootSpeed = 12f;
			Item.rare = ItemRarityID.Green;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(Item);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            type = Item.shoot;
            return true;
        }
    }
    public class Thermite_Canister_P  : ModProjectile {
        public override string Texture => "Origins/Projectiles/Ammo/Thermite_Canister_P";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Thermite Canister");
		}
		public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 900;
            //projectile.aiStyle = 14;
            //projectile.usesLocalNPCImmunity = true;
            //projectile.localNPCHitCooldown = 7;
		}
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(Projectile.velocity.X==0f) {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if(Projectile.velocity.Y==0f) {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            Projectile.timeLeft = 1;
            return true;
        }
        public override void Kill(int timeLeft) {
            Projectile.NewProjectile(Projectile.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 0, 0, Projectile.owner, -1, 1);
            Projectile.damage = (int)(Projectile.damage * 0.75f);
            Projectile.knockBack = 16f;
		    Projectile.position = Projectile.Center;
		    Projectile.width = (Projectile.height = 52);
		    Projectile.Center = Projectile.position;
            Projectile.Damage();
            for(int i = 0; i < 5; i++) {
                Projectile.NewProjectile(Projectile.Center, (Projectile.velocity/2)+Vec2FromPolar((i/Main.rand.NextFloat(5,7))*MathHelper.TwoPi, Main.rand.NextFloat(2,4)), ModContent.ProjectileType<Thermite_P>(), (int)(Projectile.damage*0.75f), 0, Projectile.owner);
            }
        }
        public override void AI() {
            Dust.NewDust(Projectile.Center, 0, 0, DustID.Torch);
        }
    }
    public class Thermite_P : ModProjectile {
        public override string Texture => "Origins/Projectiles/Ammo/Napalm_Pellet_P";
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Thermite Canister");
            Origins.ExplosiveProjectiles[Projectile.type] = true;
		}
        public override void SetDefaults() {
            Projectile.friendly = true;
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = 1;
            Projectile.penetrate = 25;
            Projectile.timeLeft = Main.rand.Next(300, 451);
        }
        public override void AI() {
			float v = 0.75f+(float)(0.125f*(Math.Sin(Projectile.timeLeft/5f)+2*Math.Sin(Projectile.timeLeft/60f)));
            Lighting.AddLight(Projectile.Center, v, v*0.5f, 0);
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
            width = height = 0;
            fallThrough = true;
            return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(Projectile.ai[0]==0f) {
                Projectile.ai[0] = 1f;
                Projectile.aiStyle = 0;
                Projectile.tileCollide = false;
                Projectile.position+=Vector2.Normalize(oldVelocity)*2;
            }
            Projectile.velocity = Vector2.Zero;
            return false;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.OnFire, Main.rand.Next(300, 451));
        }
        public override void OnHitPvp(Player target, int damage, bool crit) {
            target.AddBuff(BuffID.OnFire, Main.rand.Next(300, 451));
        }
        public override Color? GetAlpha(Color lightColor) {
			int v = 200+(int)(25*(Math.Sin(Projectile.timeLeft/5f)+Math.Sin(Projectile.timeLeft/60f)));
            return new Color(v+20,v+25,v-150,0);
        }
    }
}
