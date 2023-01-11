using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Felnum {
    public class Maelstrom_Incantation : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Maelstrom Incantation");
            Tooltip.SetDefault("Receives 50% higher damage bonuses");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.CrystalVileShard);
            Item.damage = 19;
            Item.DamageType = DamageClass.Summon;
            Item.shoot = ModContent.ProjectileType<Maelstrom_Incantation_P>();
            Item.noUseGraphic = true;
            Item.shootSpeed = 16f;
            Item.UseSound = null;
            Item.mana = 10;
            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(silver: 60);
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Book);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 7);
            recipe.AddIngredient(ItemID.FallenStar);
            recipe.AddTile(TileID.Bookcases);
            recipe.Register();
        }
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
            damage = damage.MultiplyBonuses(1.5f);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            //SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), position);
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, player.itemTime);
            //Projectile.NewProjectile(position, speed, 777, damage, knockBack, item.owner, position.X, position.Y);
            return false;
        }
    }
    public class Maelstrom_Incantation_P : ModProjectile {
        const int lifespan = 1800;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Maelstrom Incantation");
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.AmberBolt);
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.aiStyle = 0;
            Projectile.extraUpdates = 1;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.timeLeft = lifespan;
            Projectile.penetrate = -1;
            Projectile.scale = 0.85f;
            Projectile.alpha = 255;
        }
		public override void AI() {
            Player owner = Main.player[Projectile.owner];
			if (Projectile.ai[0] > 0) {
                int age = lifespan - Projectile.timeLeft;
                if (age <= Projectile.ai[0]) {
                    Projectile.Center = owner.MountedCenter;
                    float ageFactor = age / Projectile.ai[0];

                    Projectile.alpha = (int)(255 * (1 - ageFactor));
                    Projectile.scale = 0.4f + 0.45f * ageFactor;

                    owner.heldProj = Projectile.whoAmI;
                    float rotation = (Main.MouseWorld - Projectile.Center).ToRotation();
                    Projectile.velocity = OriginExtensions.Vec2FromPolar(rotation, Projectile.velocity.Length());
                    if (Projectile.owner == Main.myPlayer) {
                        Projectile.netUpdate = true;
                    }
                    owner.direction = Math.Sign(Projectile.velocity.X);
                    owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation - MathHelper.PiOver2);
                    if (age == (int)Projectile.ai[0]) {
                        SoundEngine.PlaySound(SoundID.Item122.WithPitchRange(0.9f, 1.1f).WithVolume(2), Projectile.Center);
                        SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithPitchRange(0.0f, 0.2f).WithVolume(0.5f), Projectile.Center);
                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.position,
                            Projectile.velocity,
                            ModContent.ProjectileType<Maelstrom_Incantation_P_Trail>(),
                            (Projectile.damage * 3) / 4,
                            Projectile.knockBack,
                            Projectile.owner,
                            ai1: Projectile.whoAmI
                        );
                    }
                } else {
                    Projectile.alpha = 0;
                    Projectile.scale = 0.85f;
                    Projectile.friendly = true;
                }
                Projectile.frameCounter++;
                if (Projectile.frameCounter >= 6) {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame >= 4) {
                        Projectile.frame = 0;
                    }
                }
			} else {
				if (--Projectile.ai[0] <= -16) {
                    if (Projectile.ai[0] > -24) {
                        if (Projectile.ai[0] == -12) {
                            SoundEngine.PlaySound(SoundID.Item122.WithPitchRange(0.9f, 1.1f).WithVolume(2), Projectile.Center);
                            SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithPitchRange(0.0f, 0.2f).WithVolume(0.75f), Projectile.Center);
                        }
                        Projectile.scale += 0.5f;
                    } else {
                        if (Projectile.ai[0] < -32) {
                            Projectile.Kill();
                        }
                    }
					if ((int)Projectile.ai[0] % 2 == 0) {
                        float angle = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
                        Vector2 targetEnd = OriginExtensions.Vec2FromPolar(angle, Main.rand.NextFloat(18, 24) * Projectile.scale) + Projectile.position;
                        Vector2 targetStart = OriginExtensions.Vec2FromPolar(angle, Main.rand.NextFloat(4) * Projectile.scale) + Projectile.position;
                        Projectile.NewProjectileDirect(
                            Projectile.GetSource_FromThis(),
                            targetEnd,
                            default,
                            Projectiles.Misc.Felnum_Shock_Arc.ID,
                            0,
                            0,
                            Projectile.owner,
                            targetStart.X,
                            targetStart.Y
                        );
                    }
                    Projectile.frameCounter++;
                    if (Projectile.frameCounter >= 5) {
                        Projectile.frameCounter = 0;
                        Projectile.frame++;
                        if (Projectile.frame >= 4) {
                            Projectile.frame = 0;
                        }
                    }
                } else {
                    Projectile.scale = 0f;
                }
			}
        }
		public override bool? CanHitNPC(NPC target) {
            if (Projectile.ai[0] > 0) {
                if (lifespan - Projectile.timeLeft > Projectile.ai[0]) return null;
			} else {
                if(Projectile.ai[0] < -16) return null;
            }
            return false;
		}
        private void startExplosion() {
            if (Projectile.ai[0] > 0) {
                Projectile.velocity = Vector2.Zero;
                Projectile.ai[0] = 0;
            }
        }
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
            startExplosion();
            target.AddBuff(Maelstrom_Buff_Damage.ID, 240);
            target.AddBuff(Maelstrom_Buff_Zap.ID, 240);
        }
		public override Color? GetAlpha(Color lightColor) {
            int val = 255 - Projectile.alpha;
			return new Color(val, val, val, val);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
            startExplosion();
            return false;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
            width = 6;
            height = 6;
            return true;
        }
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
            int size = (int)(20 * 0.5f * Projectile.scale * 1.1764705882352941176470588235294f);
            hitbox.Inflate(size, size);
		}
		public override bool PreDraw(ref Color lightColor) {
            Texture2D texture = null;
            int frame = Projectile.frame;
            if (Projectile.frame < 4) {
                texture = TextureAssets.Projectile[Type].Value;
			} else {
                texture = TextureAssets.Projectile[Type].Value;
            }
            int width = texture.Width;
            int frameHeight = texture.Height / Main.projFrames[Type];
            int frameY = frameHeight * frame;
            Main.EntitySpriteDraw(
                texture,
                Projectile.position - Main.screenPosition,
                new Rectangle(0, frameY, width, frameHeight),
                Projectile.GetAlpha(lightColor),
                Projectile.rotation,
                new Vector2(width * 0.5f, frameHeight * 0.5f),
                Projectile.scale,
                0,
                0
            );
            return false;
        }
    }
    public class Maelstrom_Incantation_P_Trail : ModProjectile {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PiercingStarlight;
        Triangle Hitbox {
            get {
                Vector2 direction = Vector2.Normalize(Projectile.velocity) * -64;
                Vector2 side = new Vector2(direction.Y, -direction.X) * (float)Math.Pow(Projectile.ai[0], 0.35f) * 0.65f;

                Vector2 basePos = Projectile.position + direction * Projectile.ai[0];
                return new Triangle(
                    Projectile.position,
                    basePos + side,
                    basePos - side
                );
			}
		}
		public override void SetStaticDefaults() {
            DisplayName.SetDefault("Maelstrom Incantation");
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.CultistBossLightningOrbArc);
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.width = 0;
            Projectile.height = 0;
            Projectile.aiStyle = 0;
            Projectile.extraUpdates = 1;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.timeLeft *= 3;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
        }
        public override void AI() {
			if (Projectile.ai[1] < 0 || Projectile.ai[1] > Main.maxProjectiles) {
                Projectile.Kill();
                return;
			}
            Projectile owner = Main.projectile[(int)Projectile.ai[1]];
            if (!owner.active) {
                Projectile.Kill();
                return;
			} else {
                Projectile.timeLeft = 2;
			}
            if (owner.velocity != default) Projectile.velocity = owner.velocity;
            Projectile.position = owner.position - Projectile.velocity;
            if (owner.ai[0] > 0) {
                if (Projectile.ai[0] < 1f) {
                    Projectile.ai[0] += 0.05f;
                }
			} else {
                Projectile.friendly = false;
                if (Projectile.ai[0] > 0.1f) {
                    Projectile.ai[0] -= 0.1f;
                    if (Projectile.ai[0] < 0.005f) {
                        Projectile.Kill();
                    }
                }
            }
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
            target.AddBuff(Maelstrom_Buff_Damage.ID, 240);
        }
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            Triangle hitbox = Hitbox;
            if (!hitbox.HasNaNs() && hitbox.Intersects(targetHitbox)) {
                return true;
			}
			return false;
        }
        public override bool PreDraw(ref Color lightColor) {
            Triangle hitbox = Hitbox;
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 scale = (new Vector2(1.85f, 0.25f) / new Vector2(36, 72)) * (hitbox.b - hitbox.a).Length();
            Rectangle frame = new Rectangle(0, 0, 36, 72);
            //Main.CurrentDrawnEntityShader = Terraria.Graphics.Shaders.GameShaders.Armor.GetShaderIdFromItemId(ItemID.MidnightRainbowDye);
            //Main.spriteBatch.Restart(SpriteSortMode.Immediate);
            Color color = Color.Lerp(new Color(255, 255, 255, 0), lightColor, 0.75f);
            Main.EntitySpriteDraw(
                texture,
                hitbox.a - Main.screenPosition,
                frame,
                color,
                (hitbox.b - hitbox.a).ToRotation() + MathHelper.Pi,
                new Vector2(36, 36),
                scale,
                SpriteEffects.None,
            0);
            Main.EntitySpriteDraw(
                texture,
                hitbox.a - Main.screenPosition,
                frame,
                color,
                (hitbox.c - hitbox.a).ToRotation() + MathHelper.Pi,
                new Vector2(36, 36),
                scale,
                SpriteEffects.None,
            0);
            //Main.spriteBatch.Restart();
            return false;
        }
    }
}
