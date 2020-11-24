using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Acid;
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
            item.damage = 17;
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
                    return true;
                }
                if(type == ModContent.ProjectileType<Impact_Grenade_P>()) {
                    type = ModContent.ProjectileType<Impact_Grenade_Blast>();
                    //Vector2 speed = new Vector2(speedX, speedY);
                    position+=new Vector2(speedX, speedY).SafeNormalize(Vector2.Zero)*40;
                    /*float mult = 0.75f;
                    for(int i = 0; ++i < 5;) {
                        switch(i) {
                            case 2:
                            mult = 1.5f;
                            break;
                            case 1:
                            case 3:
                            mult = 1f;
                            break;
                            case 0:
                            case 4:
                            mult = 0.75f;
                            break;
                        }
                        Projectile.NewProjectile(position, speed.RotatedBy(((i-5/2f)/5))*mult, type, damage/6, knockBack, player.whoAmI, speed.X*mult, speed.Y*mult);
                    }*/
			        Main.PlaySound(2, (int)position.X, (int)position.Y, 14, 1f);
                    //Projectile.NewProjectile(position, speed, type, damage*10, knockBack*3, player.whoAmI);
                    damage*=10;
                    knockBack*=3;
                    return true;
                }
                if(type == ModContent.ProjectileType<Acid_Grenade_P>()) {
                    Vector2 speed = new Vector2(speedX, speedY);
                    position+=speed.SafeNormalize(Vector2.Zero)*40;
                    type = ModContent.ProjectileType<Acid_Splash_P>();
                    damage-=20;
                    for(int i = Main.rand.Next(2); ++i < 5;) {
                        Projectile.NewProjectileDirect(position, speed.RotatedByRandom(0.1*i)*0.6f, type, damage/2, knockBack, player.whoAmI).scale = 0.85f;
                    }
                    return false;
                }
                if(type == ModContent.ProjectileType<Crystal_Grenade_P>()) {
                    Vector2 speed = new Vector2(speedX, speedY);
                    position+=speed.SafeNormalize(Vector2.Zero)*40;
                    type = ModContent.ProjectileType<Crystal_Grenade_Shard>();
                    damage-=10;
                    for(int i = Main.rand.Next(3); ++i < 10;) {
                        int p = Projectile.NewProjectile(position, speed.RotatedByRandom(0.025*i)*0.6f, type, damage/2, knockBack, player.whoAmI);
                        Main.projectile[p].timeLeft+=90;
                        Main.projectile[p].extraUpdates++;
                    }
                    return false;
                }
            }
            if(type == ModContent.ProjectileType<Acid_Grenade_P>()) {
                damage-=15;
            }
            return true;
        }
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
    }
    public class Awe_Grenade_P : ModProjectile {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Awe Grenade");
		}
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
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Awe Grenade");
		}
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
            return (projectile.Center-closest).Length()<=160*((duration-projectile.timeLeft) / (float)duration)*projectile.scale;
        }
        public override bool? CanHitNPC(NPC target) {
            return target.friendly?false:base.CanHitNPC(target);
        }
        public override bool CanHitPlayer(Player target) {
            Vector2 closest = projectile.Center.Clamp(target.TopLeft, target.BottomRight);
            return (projectile.Center-closest).Length()<=160*((duration-projectile.timeLeft) / (float)duration)*projectile.scale;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            damage-=(int)(damage*((duration-projectile.timeLeft) / (float)duration)*0.6f);
        }
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
            damage-=(int)(damage*((duration-projectile.timeLeft) / (float)duration)*0.95f);
        }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit) {
            damage-=(int)(damage*((duration-projectile.timeLeft) / (float)duration)*0.6f);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			float percent = (duration-projectile.timeLeft) / (float)duration;
			DrawData data = new DrawData(TextureManager.Load("Images/Misc/Perlin"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 600, 600), new Color(new Vector4(0.35f,0.35f,0.35f,0.6f)*(1f - percent)), 0, new Vector2(300f, 300f), new Vector2(percent, percent/1.61803399f)*projectile.scale, SpriteEffects.None, 0);
			GameShaders.Misc["ForceField"].UseColor(new Vector3(2f));
			GameShaders.Misc["ForceField"].Apply(data);
			data.Draw(spriteBatch);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
            return false;
        }
    }
    public class Impact_Grenade_Blast  : ModProjectile {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Blast Grenade");
		}
        public override string Texture => "Terraria/Projectile_694";
        public override bool CloneNewInstances => true;
        float dist;

        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Grenade);
            projectile.aiStyle = 0;
            projectile.timeLeft = 8;
            projectile.width = projectile.height = 5;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            if(!Main.projectileLoaded[694]) {
                Main.projectileTexture[694] = Main.instance.OurLoad<Texture2D>(string.Concat(new object[] { "Images", Path.DirectorySeparatorChar, "Projectile_694" }));
                Main.projectileLoaded[694] = true;
            }
        }
        public override void AI() {
			Player player = Main.player[projectile.owner];
            Vector2 unit = projectile.velocity.SafeNormalize(Vector2.Zero);
            projectile.Center = player.MountedCenter + unit*36 + unit.RotatedBy(MathHelper.PiOver2*player.direction)*-2;
            projectile.rotation = projectile.velocity.ToRotation();
            /*projectile.velocity = Vector2.Lerp(projectile.velocity, new Vector2(projectile.ai[0], projectile.ai[1]), 0.05f);
            Dust dust = Dust.NewDustPerfect(projectile.Center, 6, Vector2.Zero, 100, Scale: 1.25f);
            //dust.shader = GameShaders.Armor.GetSecondaryShader(6, Main.LocalPlayer);
            dust.noGravity = true;*/
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            Vector2 closest = (projectile.Center+projectile.velocity*2).Clamp(targetHitbox.TopLeft(), targetHitbox.BottomRight());
            double rot = AngleDif((closest-projectile.Center).ToRotation(), projectile.rotation)+0.5f;//Math.Abs(((projectile.Center-closest).ToRotation()+Math.PI)-(projectile.rotation+Math.PI))+0.5;
            /*if((projectile.Center-closest).Length()<=48) {
                //Main.NewText($"{(projectile.Center-closest).ToRotation()} - {projectile.rotation} + 0.5 = {rot}");
                //Main.NewText($"{AngleDif((projectile.Center-closest).ToRotation(), projectile.rotation)}");
                Main.NewText($"{(projectile.Center-closest).Length()}<={48/rot} (48/{rot})");
            }*/
            dist = (float)((projectile.Center-closest).Length()*rot/5.5f)+1;
            return (projectile.Center-closest).Length()<=48/rot;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            damage = (int)(damage/dist);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
            int frame = (8 - projectile.timeLeft)/2;
            spriteBatch.Draw(Main.projectileTexture[694], projectile.Center - Main.screenPosition, new Rectangle(0,80*frame,80,80), lightColor, projectile.rotation+MathHelper.PiOver2, new Vector2(40, 80), 1f, SpriteEffects.None, 0f);
			return false;
        }
    }
}
