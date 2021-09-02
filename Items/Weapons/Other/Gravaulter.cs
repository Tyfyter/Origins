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
            item.useTime = 18;
            item.useAnimation = 18;
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
                if (player.controlUseItem) {
                    player.itemTime = 2;
                    player.itemAnimation = 2;
                    player.direction = Math.Sign(unit.X);
                    proj.spriteDirection = player.direction;
                    proj.Center = player.MountedCenter + unit * 16;
                    proj.timeLeft = 600;
                } else {
                    proj.velocity = unit * shootSpeed;
                    proj.tileCollide = true;
                    player.itemAnimation = player.itemAnimationMax;
                    Main.PlaySound(SoundID.Item88, proj.Center);
                }
            }
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            int heldProjectile = player.GetModPlayer<OriginPlayer>().heldProjectile;
            if (heldProjectile < 0 || heldProjectile > Main.maxProjectiles) {
                shootSpeed = (float)Math.Sqrt(speedX * speedX + speedY * speedY);
                Projectile.NewProjectile(player.MountedCenter, Vector2.Zero, Gravaulter_P.ID, damage, knockBack, player.whoAmI, player.itemAnimationMax*3);
                player.itemTime = 2;
                player.itemAnimation = 2;
            }
            return false;
        }
        private void SpawnProjectile(Player player, int damage, float knockBack) {
        }
    }
    public class Gravaulter_P : ModProjectile {
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
            float rotSpeed = projectile.spriteDirection * (0.05f + (0.01f * rocks.Count));
            projectile.rotation += rotSpeed;
            projectile.velocity.Y += 0.06f;
            if (!projectile.tileCollide) {
                Player owner = Main.player[projectile.owner];
                owner.GetModPlayer<OriginPlayer>().heldProjectile = owner.heldProj = projectile.whoAmI;
                projectile.velocity = Vector2.Zero;
                if (rocks.Count<10) {
                    if (++projectile.localAI[0] >= projectile.ai[0]) {
                        projectile.localAI[0] = 0;
                        rocks.Add(new Rock(new PolarVec2(160, Main.rand.NextFloatDirection())));
                    }
                }
                Rock rock;
                for (int i = 0; i < rocks.Count; i++) {
                    rock = rocks[i];
                    if (!rock.attached) {
                        rock.offset.R -= projectile.ai[0]/15;
                        if (rock.offset.R <= rocks.Count) {
                            rock.attached = true;
                        }
                    } else {
                        rock.offset.Theta += rotSpeed;
                        //rock.rotation += rotSpeed;
                    }
                }
            } else if(projectile.timeLeft>=598) {
                Rock rock;
                for (int i = 0; i < rocks.Count; i++) {
                    rock = rocks[i];
                    if (!rock.attached) {
                        rocks.RemoveAt(i--);
                    }
                }
            } else {
                for (int i = 0; i < rocks.Count; i++) {
                    rocks[i].offset.Theta += rotSpeed;
                    //rocks[i].rotation += rotSpeed;
                }
            }
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
}
