using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.DataStructures;
using Origins.Projectiles.Misc;

namespace Origins.Items.Weapons.Other {
    public class Gravaulter : ModItem {
        public override bool CloneNewInstances => true;
        float shootSpeed;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Gravaulter");
            Tooltip.SetDefault("");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.MeteorStaff);
            item.damage = 62;
            item.magic = true;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.width = 58;
            item.height = 58;
            item.useStyle = 5;
            item.useTime = 12;
            item.useAnimation = 12;
            item.knockBack = 9.5f;
            item.value = 500000;
            item.shoot = ProjectileID.None;
            item.rare = ItemRarityID.Purple;
            item.shoot = Gravaulter_P.ID;
            item.shootSpeed = 10f;
            item.autoReuse = true;
            item.scale = 1f;
            item.UseSound = null;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips) {
            tooltips[0].overrideColor = new Color(0, Main.mouseTextColor, 0, Main.mouseTextColor);
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
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            int heldProjectile = player.GetModPlayer<OriginPlayer>().heldProjectile;
            if (heldProjectile < 0 || heldProjectile > Main.maxProjectiles) {
                shootSpeed = (float)Math.Sqrt(speedX * speedX + speedY * speedY);
                Projectile.NewProjectile(player.MountedCenter, Vector2.Zero, Gravaulter_P.ID, damage, knockBack, player.whoAmI, player.itemAnimationMax * 3);
                player.itemTime = 2;
                player.itemAnimation = 2;
            }
            return false;
        }
    }
    public class Gravaulter_P : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public static int ID { get; private set; }
        public static Texture2D[] RockTextures { get; private set; }
        List<Rock> rocks = new List<Rock>();
        internal static void Unload() {
            RockTextures = null;
        }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Gravaulter");
            ID = projectile.type;
            if (Main.netMode == NetmodeID.Server) return;
            RockTextures = new Texture2D[]{
                mod.GetTexture("Items/Weapons/Other/Gravaulter_P"),
                mod.GetTexture("Items/Weapons/Other/Gravaulter_P2"),
                mod.GetTexture("Items/Weapons/Other/Gravaulter_P3")
            };
        }
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Meteor1);
            projectile.width = 14;
            projectile.height = 14;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 10;
            projectile.penetrate = -1;
            projectile.extraUpdates = 1;
            projectile.timeLeft = 600;
            projectile.tileCollide = false;
            projectile.aiStyle = 0;
        }
        public override void AI() {
            float rotSpeed = projectile.spriteDirection * (0.05f + (0.01f * rocks.Where(r => r.attached).Count()));
            projectile.rotation += rotSpeed;
            if (!projectile.tileCollide) {
                Player owner = Main.player[projectile.owner];
                owner.GetModPlayer<OriginPlayer>().heldProjectile = owner.heldProj = projectile.whoAmI;
                projectile.velocity = Vector2.Zero;
                if (rocks.Count < 10) {
                    if (++projectile.localAI[0] >= projectile.ai[0]) {
                        projectile.localAI[0] = 0;
                        float[] laserScanResults = new float[3];
                        int tries = 15;
                        retry:
                        PolarVec2 spawnPosition = new PolarVec2(1, Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi));
                        Collision.LaserScan(projectile.Center, (Vector2)spawnPosition, 1f, 481f, laserScanResults);
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
                    }
                }
                Rock rock;
                for (int i = 0; i < rocks.Count; i++) {
                    rock = rocks[i];
                    if (!rock.attached) {
                        rock.offset.R -= 194.4f / projectile.ai[0];
                        if (rock.offset.R <= rocks.Count) {
                            rock.attached = true;
                        }
                    } else {
                        rock.offset.Theta += rotSpeed;
                    }
                    Lighting.AddLight(projectile.Center + (Vector2)rock.offset, 0, 0.5f, 0);
                }
            } else if (projectile.ai[1] > 0) {
                projectile.ai[1] = 0;
                Rock rock;
                for (int i = 0; i < rocks.Count; i++) {
                    rock = rocks[i];
                    if (!rock.attached) {
                        rocks.RemoveAt(i--);
                        PolarVec2 vel = rock.offset;
                        vel.R = -(194.4f / projectile.ai[0]);
                        Projectile.NewProjectile(projectile.Center + (Vector2)rock.offset, (Vector2)vel, Gravaulter_Rock1.ID+rock.type, projectile.damage / 2, projectile.knockBack, projectile.owner, ai1: rotSpeed * projectile.spriteDirection);
                    }
                    Lighting.AddLight(projectile.Center + (Vector2)rock.offset, 0, 0.5f, 0);
                }
                if (rocks.Count == 0) {
                    projectile.timeLeft = 0;
                } else {
                    projectile.damage = (int)((rocks.Count + 10f) * 0.1f * projectile.damage);
                    Main.PlaySound(SoundID.Item88, projectile.Center);
                }
            } else {
                Rock rock;
                for (int i = 0; i < rocks.Count; i++) {
                    rock = rocks[i];
                    rock.offset.Theta += rotSpeed;
                    Lighting.AddLight(projectile.Center + (Vector2)rock.offset, 0, 0.5f, 0);
                }
                projectile.velocity.Y += 0.06f;
            }
        }
        public void Split() {
            float rotSpeed = (0.15f + (0.03f * rocks.Where(r => r.attached).Count()));
            Rock rock;
            PolarVec2 vel;
            projectile.velocity *= 0.85f;
            for (int i = 0; i < rocks.Count; i++) {
                rock = rocks[i];
                vel = rock.offset;
                vel.R *= rotSpeed;
                vel.Theta += MathHelper.PiOver2 * projectile.spriteDirection;
                Projectile.NewProjectile(projectile.Center + (Vector2)rock.offset, projectile.velocity + (Vector2)vel, Gravaulter_Rock1.ID + rock.type, projectile.damage / 2, projectile.knockBack, projectile.owner, ai1:rotSpeed * projectile.spriteDirection);
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if (target.life > damage) {
                projectile.Kill();
            }
        }
        public override void OnHitPlayer(Player target, int damage, bool crit) {
            if (target.statLife > damage) {
                projectile.Kill();
            }
        }
        public override void Kill(int timeLeft) {
            Split();
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            if (projectile.tileCollide) {
                return null;
            }
            return false;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Rock rock;
            Vector2 center = projectile.Center - Main.screenPosition;
            Texture2D texture;
            for (int i = 0; i < rocks.Count; i++) {
                rock = rocks[i];
                texture = RockTextures[rock.type];
                spriteBatch.Draw(
                    texture,
                    center + (Vector2)rock.offset,
                    null,
                    lightColor,
                    projectile.rotation + rock.rotation,
                    texture.Size() * 0.5f,
                    projectile.scale,
                    projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
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
            projectile.CloneDefaults(ProjectileID.Meteor1);
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.width = 14;
            projectile.height = 14;
            projectile.usesLocalNPCImmunity = false;
            projectile.localNPCHitCooldown = 10;
            projectile.penetrate = 3;
            projectile.extraUpdates = 1;
            projectile.timeLeft = 600;
            projectile.tileCollide = true;
            projectile.aiStyle = 0;
        }
        public override void AI() {
            projectile.velocity.Y += 0.06f;
            projectile.rotation += projectile.ai[1];
            OriginExtensions.LinearSmoothing(ref projectile.ai[1], 0, 0.001f);
            Lighting.AddLight(projectile.Center, 0, 0.5f, 0);
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if (projectile.velocity.X == 0) {
                projectile.velocity.Y *= 0.99f;
            }
            if (projectile.velocity.Y == 0) {
                projectile.velocity.X *= 0.99f;
            }
            projectile.ai[1] = (oldVelocity.X - projectile.velocity.X) * 10;
            return false;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Texture2D texture = Main.projectileTexture[projectile.type];
            spriteBatch.Draw(
                texture,
                projectile.Center - Main.screenPosition,
                null,
                lightColor,
                projectile.rotation,
                texture.Size() * 0.5f,
                projectile.scale,
                projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0);
            return false;
        }
    }
    public class Gravaulter_Rock1 : Gravaulter_Rock {
        public override string Texture => "Origins/Items/Weapons/Other/Gravaulter_P";
        public static int ID { get; private set; }
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Gravaulter");
            ID = projectile.type;
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
