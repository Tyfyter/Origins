using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Acid;
using Origins.Items.Weapons.Felnum;
using Origins.Projectiles.Weapons;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Origins.OriginExtensions;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Explosives {
	public class Hand_Grenade_Launcher : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Hand Grenade Launcher");
			Tooltip.SetDefault("'Doesn't this defeat the purpose of a hand grenade?'");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.GrenadeLauncher);
            //item.maxStack = 999;
            Item.width = 44;
            Item.height = 18;
            Item.damage = 17;
			Item.value/=2;
			Item.useTime = (int)(Item.useTime*1.15);
			Item.useAnimation = (int)(Item.useAnimation*1.15);
            Item.shoot = ProjectileID.Grenade;
            Item.useAmmo = ItemID.Grenade;
            Item.knockBack = 2f;
            Item.shootSpeed = 5f;
			Item.rare = ItemRarityID.Green;
		}
        public override bool AltFunctionUse(Player player) {
            return true;
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		    if(player.altFunctionUse == 2) {
                if(type == ModContent.ProjectileType<Shock_Grenade_P>()) {
                    damage-=15;
                    damage+=(damage-16)*2;
                    type = ModContent.ProjectileType<Awe_Grenade_P>();
                    velocity *= 1.25f;
                    knockback *= 3; Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                    return false;
                }
                if(type == ModContent.ProjectileType<Impact_Grenade_P>()) {
                    type = ModContent.ProjectileType<Impact_Grenade_Blast>();
                    //Vector2 speed = new Vector2(speedX, speedY);
                    position+=velocity.SafeNormalize(Vector2.Zero)*40;
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
			        SoundEngine.PlaySound(SoundID.Item14.WithPitchRange(1, 1), position);
                    //Projectile.NewProjectile(position, speed, type, damage*10, knockBack*3, player.whoAmI);
                    damage*=10;
                    knockback*=3; Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                    return false;
                }
                if(type == ModContent.ProjectileType<Acid_Grenade_P>()) {
                    position+= velocity.SafeNormalize(Vector2.Zero)*40;
                    type = ModContent.ProjectileType<Acid_Shot>();
                    damage-=20;
                    for(int i = Main.rand.Next(2); ++i < 5;) {
                        Projectile.NewProjectileDirect(source, position, velocity.RotatedByRandom(0.1*i)*0.6f, type, damage/2, knockback, player.whoAmI).scale = 0.85f;
                    }
                    return false;
                }
                if(type == ModContent.ProjectileType<Crystal_Grenade_P>()) {
                    position+= velocity.SafeNormalize(Vector2.Zero)*40;
                    type = ModContent.ProjectileType<Crystal_Grenade_Shard>();
                    damage-=10;
                    for(int i = Main.rand.Next(3); ++i < 10;) {
                        int p = Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.025*i)*0.6f, type, damage/2, knockback, player.whoAmI);
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
    }
    public class Awe_Grenade_P : ModProjectile {
        Vector2 oldVelocity;
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Awe Grenade");
            //Origins.ExplosiveProjectiles[Projectile.type] = true;
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
            Projectile.timeLeft = 45;
            Projectile.penetrate = 1;
        }
		public override void OnSpawn(IEntitySource source) {
			oldVelocity = Projectile.velocity;
		}
		/*public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.Grenade;
            return true;
        }*/
		public override void AI() {
            float diff = (Projectile.velocity - oldVelocity).LengthSquared();
            if (diff > 256) {
                Projectile.timeLeft = 0;
                //Projectile.Kill();
            }
            oldVelocity = Projectile.velocity;
        }
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return true;
		}
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
			SoundEngine.PlaySound(SoundID.Item38.WithVolume(0.75f), Projectile.Center);
            SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithVolume(5), Projectile.Center);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Awe_Grenade_Blast>(), Projectile.damage, 24, Projectile.owner);
        }
    }
    public class Awe_Grenade_Blast  : ModProjectile {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Awe Grenade");
            //Origins.ExplosiveProjectiles[Projectile.type] = true;
		}
        public override string Texture => "Origins/Projectiles/Pixel";
        const int duration = 15;
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = duration;
            Projectile.width = Projectile.height = 160;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = duration;
            Projectile.tileCollide = false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            Vector2 closest = Projectile.Center.Clamp(targetHitbox.TopLeft(), targetHitbox.BottomRight());
            return (Projectile.Center-closest).Length()<=160*((duration-Projectile.timeLeft) / (float)duration)*Projectile.scale;
        }
        public override bool? CanHitNPC(NPC target) {
            return target.friendly?false:base.CanHitNPC(target);
        }
        public override bool CanHitPlayer(Player target) {
            Vector2 closest = Projectile.Center.Clamp(target.TopLeft, target.BottomRight);
            return (Projectile.Center-closest).Length()<=160*((duration-Projectile.timeLeft) / (float)duration)*Projectile.scale;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            damage-=(int)(damage*((duration-Projectile.timeLeft) / (float)duration)*0.6f);
        }
        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
            damage-=(int)(damage*((duration-Projectile.timeLeft) / (float)duration)*0.95f);
        }
        public override void ModifyHitPvp(Player target, ref int damage, ref bool crit) {
            damage-=(int)(damage*((duration-Projectile.timeLeft) / (float)duration)*0.6f);
        }
        public override bool PreDraw(ref Color lightColor){
            Main.spriteBatch.Restart(
                sortMode: SpriteSortMode.Immediate,
                samplerState: SamplerState.PointClamp,
                transformMatrix: Main.LocalPlayer.gravDir == 1f ? Main.GameViewMatrix.ZoomMatrix : Main.GameViewMatrix.TransformationMatrix
            );
            float percent = (duration-Projectile.timeLeft) / (float)duration;
			DrawData data = new DrawData(Main.Assets.Request<Texture2D>("Images/Misc/Perlin").Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 600, 600), new Color(new Vector4(0.35f,0.35f,0.35f,0.6f)*(1f - percent)), 0, new Vector2(300f, 300f), new Vector2(percent, percent/1.61803399f)*Projectile.scale, SpriteEffects.None, 0);
			GameShaders.Misc["ForceField"].UseColor(new Vector3(2f));
			GameShaders.Misc["ForceField"].Apply(data);
			data.Draw(Main.spriteBatch);
			Main.spriteBatch.Restart();
			return false;
        }
    }
    public class Impact_Grenade_Blast  : ModProjectile {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Blast Grenade");
		}
        public override string Texture => "Terraria/Images/Projectile_"+ProjectileID.DD2ExplosiveTrapT1Explosion;
        protected override bool CloneNewInstances => true;
        float dist;

        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 8;
            Projectile.width = Projectile.height = 5;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            if(Main.netMode!=NetmodeID.Server && !TextureAssets.Projectile[694].IsLoaded) {
                Main.instance.LoadProjectile(694);
            }
        }
        public override void AI() {
			Player player = Main.player[Projectile.owner];
            Vector2 unit = Projectile.velocity.SafeNormalize(Vector2.Zero);
            Projectile.Center = player.MountedCenter + unit*36 + unit.RotatedBy(MathHelper.PiOver2*player.direction)*-2;
            Projectile.rotation = Projectile.velocity.ToRotation();
            /*projectile.velocity = Vector2.Lerp(projectile.velocity, new Vector2(projectile.ai[0], projectile.ai[1]), 0.05f);
            Dust dust = Dust.NewDustPerfect(projectile.Center, 6, Vector2.Zero, 100, Scale: 1.25f);
            //dust.shader = GameShaders.Armor.GetSecondaryShader(6, Main.LocalPlayer);
            dust.noGravity = true;*/
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            Vector2 closest = (Projectile.Center+Projectile.velocity*2).Clamp(targetHitbox.TopLeft(), targetHitbox.BottomRight());
            double rot = GeometryUtils.AngleDif((closest-Projectile.Center).ToRotation(), Projectile.rotation, out _)+0.5f;//Math.Abs(((projectile.Center-closest).ToRotation()+Math.PI)-(projectile.rotation+Math.PI))+0.5;
            /*if((projectile.Center-closest).Length()<=48) {
                //Main.NewText($"{(projectile.Center-closest).ToRotation()} - {projectile.rotation} + 0.5 = {rot}");
                //Main.NewText($"{AngleDif((projectile.Center-closest).ToRotation(), projectile.rotation)}");
                Main.NewText($"{(projectile.Center-closest).Length()}<={48/rot} (48/{rot})");
            }*/
            dist = (float)((Projectile.Center-closest).Length()*rot/5.5f)+1;
            return (Projectile.Center-closest).Length()<=48/rot;
        }
        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            damage = (int)(damage/dist);
        }
        public override bool PreDraw(ref Color lightColor){
            int frame = (8 - Projectile.timeLeft)/2;
            Main.EntitySpriteDraw(TextureAssets.Projectile[694].Value, Projectile.Center - Main.screenPosition, new Rectangle(0,80*frame,80,80), lightColor, Projectile.rotation+MathHelper.PiOver2, new Vector2(40, 80), 1f, SpriteEffects.None, 0);
			return false;
        }
    }
}
