using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.DataStructures;
using Origins.Projectiles.Misc;
using Terraria.Graphics.Shaders;
using Tyfyter.Utils;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Other {
    public class Gravaulter : ModItem {
        protected override bool CloneNewInstances => true;
        float shootSpeed;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Gravaulter");
            Tooltip.SetDefault("");
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.MeteorStaff);
            Item.damage = 99;
            Item.DamageType = DamageClass.Magic;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.width = 58;
            Item.height = 58;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.knockBack = 9.5f;
            Item.value = 500000;
            Item.rare = ItemRarityID.Purple;
            Item.shoot = Gravaulter_P.ID;
            Item.shootSpeed = 10f;
            Item.autoReuse = false;
            Item.scale = 1f;
            Item.mana = 6;
            Item.UseSound = null;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips[0].OverrideColor = new Color(0, Main.mouseTextColor, 0, Main.mouseTextColor);
        }
        public override void HoldItem(Player player) {
            int heldProjectile = player.GetModPlayer<OriginPlayer>().heldProjectile;

            if (heldProjectile < 0 || heldProjectile > Main.maxProjectiles) {
                return;
            }
            Projectile proj = Main.projectile[heldProjectile];
            if (proj.type == Gravaulter_P.ID) {
                Vector2 unit = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);
                if (player.controlUseItem && !player.controlUseTile) {
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                    player.direction = Math.Sign(unit.X);
                    proj.spriteDirection = player.direction;
                    proj.Center = player.MountedCenter + unit * 16;
                    proj.timeLeft = 600;
                } else {
                    proj.velocity = unit * shootSpeed;
                    proj.ai[1] = 1;
                    proj.tileCollide = true;
                    if (player.controlUseTile) {
                        proj.timeLeft = 2;
                        proj.velocity *= 1.1f;
                        proj.damage += proj.damage / 2;
                    }
                    proj.netUpdate = true;
                    player.itemAnimation = player.itemAnimationMax;
                }
            }
        }
        public override bool CanUseItem(Player player) {
            return !player.controlUseTile;
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		    if (player.controlUseTile) {
                return false;
            }
            int heldProjectile = player.GetModPlayer<OriginPlayer>().heldProjectile;
            if (heldProjectile < 0 || heldProjectile > Main.maxProjectiles) {
                shootSpeed = velocity.Length();
                Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, Gravaulter_P.ID, damage, knockback, player.whoAmI, player.itemAnimationMax * 2.5f);
                player.itemTime = 1;
                player.itemAnimation = 1;
            }
            return false;
        }
    }
    public class Gravaulter_P : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public static int ID { get; private set; }
        public static AutoCastingAsset<Texture2D>[] RockTextures { get; private set; }
        List<Rock> rocks = new List<Rock>();
        public override void Unload() {
            RockTextures = null;
        }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Gravaulter");
            ID = Projectile.type;
            if (!Main.dedServ) {
                RockTextures = new AutoCastingAsset<Texture2D>[]{
                    Mod.Assets.Request<Texture2D>("Items/Weapons/Other/Gravaulter_P"),
                    Mod.Assets.Request<Texture2D>("Items/Weapons/Other/Gravaulter_P2"),
                    Mod.Assets.Request<Texture2D>("Items/Weapons/Other/Gravaulter_P3")
                };
            }
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Meteor1);
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.aiStyle = 0;
            Projectile.localAI[0] = 1000f;
        }
        public override void AI() {
            float rotSpeed = Projectile.spriteDirection * (0.05f + (0.01f * rocks.Where(r => r.attached).Count()));
            Projectile.rotation += rotSpeed;
            if (!Projectile.tileCollide) {
                Player owner = Main.player[Projectile.owner];
                owner.GetModPlayer<OriginPlayer>().heldProjectile = owner.heldProj = Projectile.whoAmI;
                Projectile.velocity = Vector2.Zero;
                if (rocks.Count < 10) {
                    if (++Projectile.localAI[0] >= Projectile.ai[0] && owner.CheckMana(owner.HeldItem, pay:true)) {
                        Projectile.localAI[0] = 0;
                        float[] laserScanResults = new float[3];
                        int tries = 15;
                        retry:
                        PolarVec2 spawnPosition = new PolarVec2(1, Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi));
                        Collision.LaserScan(Projectile.Center, (Vector2)spawnPosition, 1f, 481f, laserScanResults);
                        float dist = laserScanResults.Average();
                        if (dist >= 480f) {
                            if (tries-- > 0) {
                                goto retry;
                            }
                            dist = 640;
                            spawnPosition = new PolarVec2(1, Main.rand.NextFloat(MathHelper.Pi-MathHelper.PiOver4, MathHelper.PiOver4));
                        }
                        spawnPosition.R = dist;
                        rocks.Add(new Rock(spawnPosition));
                        Vector2 soundPosition = Projectile.Center + (Vector2)spawnPosition;
                        SoundEngine.PlaySound(SoundID.Item28.WithPitch(-0.3f), soundPosition);
                    }
                }
                Rock rock;
                for (int i = 0; i < rocks.Count; i++) {
                    rock = rocks[i];
                    if (!rock.attached) {
                        rock.offset.R -= 162f / Projectile.ai[0];
                        if (rock.offset.R <= rocks.Count) {
                            rock.attached = true;
                        }
                    } else {
                        rock.offset.Theta += rotSpeed;
                    }
                    LightAndDust(Projectile.Center + (Vector2)rock.offset);
                }
            } else if (Projectile.ai[1] > 0) {
                Projectile.ai[1] = 0;
                Rock rock;
                for (int i = 0; i < rocks.Count; i++) {
                    rock = rocks[i];
                    if (!rock.attached) {
                        rocks.RemoveAt(i--);
                        PolarVec2 vel = rock.offset;
                        vel.R = -(162f / Projectile.ai[0]);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + (Vector2)rock.offset, (Vector2)vel, Gravaulter_Rock1.ID+rock.type, Projectile.damage / 2, Projectile.knockBack, Projectile.owner, ai1: rotSpeed * Projectile.spriteDirection);
                    }
                    LightAndDust(Projectile.Center + (Vector2)rock.offset);
                }
                if (rocks.Count == 0) {
                    Projectile.timeLeft = 0;
                } else {
                    Projectile.damage = (int)((rocks.Count + 5f) * 0.2f * Projectile.damage);
                    SoundEngine.PlaySound(SoundID.Item88, Projectile.Center);
                }
            } else {
                Rock rock;
                for (int i = 0; i < rocks.Count; i++) {
                    rock = rocks[i];
                    rock.offset.Theta += rotSpeed;
                    LightAndDust(Projectile.Center + (Vector2)rock.offset);
                }
                Projectile.velocity.Y += 0.06f;
            }
        }
        public void Split() {
            float rotSpeed = (0.15f + (0.03f * rocks.Where(r => r.attached).Count()));
            Rock rock;
            PolarVec2 vel;
            Projectile.velocity *= 0.85f;
            for (int i = 0; i < rocks.Count; i++) {
                rock = rocks[i];
                vel = rock.offset;
                vel.R *= rotSpeed;
                vel.Theta += MathHelper.PiOver2 * Projectile.spriteDirection;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + (Vector2)rock.offset, Projectile.velocity + (Vector2)vel, Gravaulter_Rock1.ID + rock.type, Projectile.damage / 2, Projectile.knockBack, Projectile.owner, ai1:rotSpeed * Projectile.spriteDirection);
            }
            SoundEngine.PlaySound(SoundID.Item89, Projectile.Center);
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if (target.life > damage) {
                Projectile.Kill();
            }
        }
        public override void OnHitPlayer(Player target, int damage, bool crit) {
            if (target.statLife > damage) {
                Projectile.Kill();
            }
        }
        public override void Kill(int timeLeft) {
            Split();
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            if (Projectile.tileCollide) {
                return null;
            }
            return false;
        }
        public void LightAndDust(Vector2 position) {
            Lighting.AddLight(position, 0, 0.5f, 0);
            if (Main.rand.NextBool(90)) {
                Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.Electric, 0, 0, 100, new Color(0, 255, 0), 0.5f);
                dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
                dust.fadeIn = Main.rand.NextFloat(0.1f);
                dust.noGravity = false;
                dust.noLight = true;
            }
        }
        public override bool PreDraw(ref Color lightColor) {
            Rock rock;
            Vector2 center = Projectile.Center - Main.screenPosition;
            Texture2D texture;
            for (int i = 0; i < rocks.Count; i++) {
                rock = rocks[i];
                texture = RockTextures[rock.type];
                Main.EntitySpriteDraw(
                    texture,
                    center + (Vector2)rock.offset,
                    null,
                    lightColor,
                    Projectile.rotation + rock.rotation,
                    texture.Size() * 0.5f,
                    Projectile.scale,
                    Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    0);
            }
            return false;
        }
        class Rock {
            internal bool attached = false;
            internal PolarVec2 offset = PolarVec2.Zero;
            internal float rotation = 0f;
            internal int type = 0;
            Rock() {
                rotation = Main.rand.NextFloatDirection();
                type = Main.rand.Next(3);
            }
            internal Rock(PolarVec2 offset) : this() {
                this.offset = offset;
            }
        }
    }
    public abstract class Gravaulter_Rock : ModProjectile {
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Meteor1);
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.usesLocalNPCImmunity = false;
            Projectile.localNPCHitCooldown = 10;
            Projectile.penetrate = 3;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.aiStyle = 0;
        }
        public override void AI() {
            Projectile.velocity.Y += 0.06f;
            Projectile.rotation += Projectile.ai[1];
            OriginExtensions.LinearSmoothing(ref Projectile.ai[1], 0, 0.001f);
            Lighting.AddLight(Projectile.Center, 0, 0.5f, 0);
            if (Main.rand.Next(100) <= Projectile.velocity.Length()) {
                SpawnDust(0f);
            }
        }
        public override void Kill(int timeLeft) {
            for (int i = 0; i < 6; i++) {
                SpawnDust();
            }
        }
        public void SpawnDust(float velocityMultiplier = 0.75f) {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Electric, Projectile.velocity.X * velocityMultiplier, Projectile.velocity.Y * velocityMultiplier, 100, new Color(0, 255, 0), 0.5f);
            dust.shader = GameShaders.Armor.GetSecondaryShader(18, Main.LocalPlayer);
            dust.fadeIn = Main.rand.NextFloat(0.1f);
            dust.noGravity = false;
            dust.noLight = true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if (Projectile.velocity.Length() < oldVelocity.Length()*0.2f) {
                Projectile.velocity = oldVelocity;
                SoundEngine.PlaySound(SoundID.Item89.WithVolume(0.5f), Projectile.Center);
                return true;
            }
            if (Projectile.velocity.X == 0) {
                Projectile.velocity.Y *= 0.99f;
            }
            if (Projectile.velocity.Y == 0) {
                Projectile.velocity.X *= 0.99f;
            }
            Projectile.ai[1] = (oldVelocity.X - Projectile.velocity.X) * 10;
            return false;
        }
        public override bool PreDraw(ref Color lightColor) {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                texture.Size() * 0.5f,
                Projectile.scale,
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0);
            return false;
        }
    }
    public class Gravaulter_Rock1 : Gravaulter_Rock {
        public override string Texture => "Origins/Items/Weapons/Other/Gravaulter_P";
        public static int ID { get; private set; }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Gravaulter");
            ID = Projectile.type;
        }
    }
    public class Gravaulter_Rock2 : Gravaulter_Rock {
        public override string Texture => "Origins/Items/Weapons/Other/Gravaulter_P2";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Gravaulter");
        }
    }
    public class Gravaulter_Rock3 : Gravaulter_Rock {
        public override string Texture => "Origins/Items/Weapons/Other/Gravaulter_P3";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Gravaulter");
        }
    }
}
