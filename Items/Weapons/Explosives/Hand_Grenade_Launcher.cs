using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Felnum;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Explosives {
	public class Hand_Grenade_Launcher : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Hand Grenade Launcher");
			Tooltip.SetDefault("Doesn't this defeat the purpose of a hand grenade?");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.GrenadeLauncher);
            //item.maxStack = 999;
            item.width = 44;
            item.height = 18;
            item.damage = 15;
			item.value/=2;
			item.useTime = (int)(item.useTime*1.15);
			item.useAnimation = (int)(item.useAnimation*1.15);
            item.shoot = ProjectileID.Grenade;
            item.useAmmo = ItemID.Grenade;
            item.knockBack = 2f;
            item.shootSpeed = 5f;
			item.rare = ItemRarityID.Green;
		}
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            if(player.altFunctionUse == 2) {
                if(type == ModContent.ProjectileType<Shock_Grenade_P>()) {
                    damage-=15;
                    damage+=(damage-16)*2;
                    type = ModContent.ProjectileType<Awe_Grenade_P>();
                    speedX*=1.25f;
                    speedY*=1.25f;
                }
            }
            return true;
        }
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
    }
    public class Awe_Grenade_P : ModProjectile {
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Grenade);
            projectile.timeLeft = 45;
            projectile.penetrate = 1;
        }
        /*public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.Grenade;
            return true;
        }*/
        public override void Kill(int timeLeft) {
			/*
            projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 128;
			projectile.height = 128;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
            */
			Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 38, 0.75f, 1f);
            PlaySound("DeepBoom", projectile.Center, 5);
            Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<Awe_Grenade_Blast>(), projectile.damage, 24, projectile.owner);
        }
    }
    public class Awe_Grenade_Blast  : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        const int duration = 15;
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Grenade);
            projectile.hostile = true;
            projectile.aiStyle = 0;
            projectile.timeLeft = duration;
            projectile.width = projectile.height = 160;
            projectile.penetrate = -1;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = duration;
            projectile.tileCollide = false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            Vector2 closest = projectile.Center.Clamp(targetHitbox.TopLeft(), targetHitbox.BottomRight());
            return (projectile.Center-closest).Length()<=160*((duration-projectile.timeLeft) / (float)duration);
        }
        public override bool? CanHitNPC(NPC target) {
            return target.friendly?false:base.CanHitNPC(target);
        }
        public override bool CanHitPlayer(Player target) {
            Vector2 closest = projectile.Center.Clamp(target.TopLeft, target.BottomRight);
            return (projectile.Center-closest).Length()<=160*((duration-projectile.timeLeft) / (float)duration);
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            damage-=(int)(damage*((duration-projectile.timeLeft) / (float)duration)*0.6f);
        }
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
            damage-=(int)(damage*((duration-projectile.timeLeft) / (float)duration)*0.8f);
        }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit) {
            damage-=(int)(damage*((duration-projectile.timeLeft) / (float)duration)*0.6f);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			float percent = (duration-projectile.timeLeft) / (float)duration;
			DrawData data = new DrawData(TextureManager.Load("Images/Misc/Perlin"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 600, 600), new Color(new Vector4(0.35f,0.35f,0.35f,0.6f)*(1f - percent)), 0, new Vector2(300f, 300f), new Vector2(percent, percent/1.61803399f), SpriteEffects.None, 0);
			GameShaders.Misc["ForceField"].UseColor(new Vector3(2f));
			GameShaders.Misc["ForceField"].Apply(data);
			data.Draw(spriteBatch);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
            return false;
        }
    }
}
