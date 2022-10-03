using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Armor.Fiberglass;
using Origins.Items.Weapons.Fiberglass;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Origins.OriginExtensions;
using static Tyfyter.Utils.KinematicUtils;

namespace Origins.NPCs.Fiberglass {
    public class Fiberglass_Weaver : ModNPC, IMeleeCollisionDataNPC {
		public override string Texture => "Origins/NPCs/Fiberglass/Fiberglass_Weaver_Body";
        public static AutoCastingAsset<Texture2D> UpperLegTexture { get; private set; }
        public static AutoCastingAsset<Texture2D> LowerLegTexture { get; private set; }
        Arm[] legs;
        Vector2[] legTargets;
        static IItemDropRule normalDropRule;
        const float upperLegLength = 70.1f;
        const float lowerLegLength = 76f;
        const float totalLegLength = upperLegLength + lowerLegLength;
        public static int DifficultyMult => Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Fiberglass Weaver");
            if (!Main.dedServ) {
                UpperLegTexture = Mod.Assets.Request<Texture2D>("NPCs/Fiberglass/Fiberglass_Weaver_Leg_Upper");
                LowerLegTexture = Mod.Assets.Request<Texture2D>("NPCs/Fiberglass/Fiberglass_Weaver_Leg_Lower");
            }
        }
		public override void Unload() {
            UpperLegTexture = null;
            LowerLegTexture = null;
        }
		public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.PossessedArmor);
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.damage = 14;
            NPC.lifeMax = 1400;
            NPC.defense = 26;
            NPC.aiStyle = 0;
            NPC.width = NPC.height = 68;
            NPC.knockBackResist = 0.1f;
        }
		public override void OnSpawn(IEntitySource source) {
            if (legs is null) {
                legs = new Arm[8];
                legTargets = new Vector2[8];
                NPCAimedTarget target = NPC.GetTargetData();
                NPC.rotation = NPC.AngleTo(target.Center) + MathHelper.PiOver2;
                for (int i = 0; i < 8; i++) {
                    legs[i] = new Arm() {
                        bone0 = new PolarVec2(upperLegLength, 0),
                        bone1 = new PolarVec2(lowerLegLength, 0)
                    };
                    switch (i / 2) {
                        case 0:
                        legs[i].start = new Vector2(i % 2 == 0 ? -15 : 15, -38);
                        break;
                        case 1:
                        legs[i].start = new Vector2(i % 2 == 0 ? -15 : 15, -30);
                        break;
                        case 2:
                        legs[i].start = new Vector2(i % 2 == 0 ? -17 : 17, -16);
                        break;
                        case 3:
                        legs[i].start = new Vector2(i % 2 == 0 ? -15 : 15, -7);
                        break;
                    }
                    legTargets[i] = ((legs[i].start + new Vector2(0, 20)) * new Vector2(1, 1.5f)).RotatedBy(NPC.rotation) * 4 + NPC.Center + new Vector2(0, (((i / 2) % 2) == i % 2 ? 12f : -12f));
                }
            }
        }
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            //bestiaryEntry.

        }
        public override void AI() {
            NPCAimedTarget target = NPC.GetTargetData();
            float jumpSpeed = 10 + DifficultyMult * 2;
            switch ((int)NPC.ai[0]) {
                case 0: {
                    AngularSmoothing(ref NPC.rotation, NPC.AngleTo(target.Center) + MathHelper.PiOver2, 0.05f);
                    for (int i = 0; i < 8; i++) {
                        Vector2 legStart = legs[i].start.RotatedBy(NPC.rotation) + NPC.Center;
						if (NPC.ai[0] != 0 && i == NPC.ai[2]) {
                            continue;
						}
						if (i <= 1 && legStart.DistanceSQ(target.Center) < (totalLegLength * totalLegLength * 0.75f)) {
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 3;
                            NPC.ai[2] = i;
                        } else if (legStart.DistanceSQ(legTargets[i]) > (totalLegLength * totalLegLength)) {
                            legTargets[i] = GetStandPosition(
                                ((legs[i].start + new Vector2(0, 15.75f + (i / 2) * 0.0625f)) * new Vector2(0.85f, 2.04f)).RotatedBy(NPC.rotation) * 4 + NPC.Center,
                                legStart
                            );
                        }
                    }
                    NPC.velocity = Vector2.Lerp(NPC.velocity, (Vector2)new PolarVec2(3 + DifficultyMult, NPC.rotation - MathHelper.PiOver2), 0.1f);
					if (++NPC.ai[1] > 240) {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = Main.rand.Next(1, 3);
					}
                    break;
                }
                case 1: {
					if (NPC.ai[1] == 0) {
                        AngularSmoothing(ref NPC.rotation, NPC.AngleTo(target.Center) + MathHelper.PiOver2, 0.05f);
                        for (int i = 0; i < 8; i++) {
                            Vector2 legStart = legs[i].start.RotatedBy(NPC.rotation) + NPC.Center;
                            if (legStart.Distance(legTargets[i]) > totalLegLength - 8) {
                                NPC.ai[1] = -1;
                                break;
                            }
                        }
                        NPC.velocity = Vector2.Lerp(NPC.velocity, (Vector2)new PolarVec2(1.5f, NPC.rotation + MathHelper.PiOver2), 0.5f);
					}else if (NPC.ai[1] < 0) {
                        NPC.velocity = Vector2.Lerp(NPC.velocity, (Vector2)new PolarVec2(0.5f, NPC.rotation + MathHelper.PiOver2), 0.5f);
						if (--NPC.ai[1] < -30) {
                            NPC.ai[1] = 1;
                        }
                    } else if (NPC.ai[1] == 1) {
                        float targetDist = NPC.Distance(target.Center);
                        NPC.ai[1] = 300 - (targetDist / jumpSpeed);
                        NPC.velocity = Vec2FromPolar(jumpSpeed, NPC.rotation - MathHelper.PiOver2);
                        legTargets[0] = legTargets[1] = Vec2FromPolar(NPC.rotation - MathHelper.PiOver2, targetDist);

					} else if (++NPC.ai[1] >= 300) {
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;
					} else {
                        NPC.velocity = (Vector2)new PolarVec2(jumpSpeed, NPC.rotation - MathHelper.PiOver2);
                    }
                    break;
                }
                case 2: {
                    NPC.velocity = Vector2.Lerp(NPC.velocity, default, 0.1f);
                    NPC.ai[1]+=1f;
                    float leg0Factor = (float)Math.Sin(NPC.ai[1] / 3);
                    float leg0Factor2 = (float)Math.Cos(NPC.ai[1] / 3);
                    float leg1Factor = (float)Math.Sin(NPC.ai[1] / 3 + Math.PI);
                    float leg1Factor2 = (float)Math.Cos(NPC.ai[1] / 3 + 0.1);
                    legTargets[0] = NPC.Center + (Vector2)new PolarVec2(84 + (8 * leg0Factor), NPC.rotation - MathHelper.PiOver2 + 0.09f + leg0Factor2 * 0.10f);
                    legTargets[1] = NPC.Center + (Vector2)new PolarVec2(86 + (8 * leg1Factor), NPC.rotation - MathHelper.PiOver2 - 0.09f - leg1Factor2 * 0.10f);
                    if (NPC.ai[1] > 180 - DifficultyMult * 30) {
                        Vector2 position;
                        int projectileType = ModContent.ProjectileType<Fiberglass_Thread>();
                        for (int i = 5 + DifficultyMult; i-->0;) {
                            PolarVec2 vec = new PolarVec2(Main.rand.Next(8 - DifficultyMult, 12 - DifficultyMult) * 64, Main.rand.NextFloat(0, MathHelper.TwoPi));
                            Collision.AimingLaserScan(target.Center, target.Center + (Vector2)vec, 2, 3, out position, out float[] samples);
                            position = position.SafeNormalize(Vector2.One) * samples.Average() + target.Center;
                            Vector2 velocity = (Vector2)vec.RotatedBy(MathHelper.PiOver2 + Main.rand.NextFloat(0.1f, 0.1f)).WithLength(12);
                            int firstEnd = Projectile.NewProjectile(NPC.GetSource_FromAI(), position, velocity, projectileType, 10, 1, Main.myPlayer, ai1:i + 1);
                            int secondEnd = Projectile.NewProjectile(NPC.GetSource_FromAI(), position, -velocity, projectileType, 10, 1, Main.myPlayer, firstEnd, ai1: i + 1);

                            int durBoost = Main.rand.Next(60, 120) * DifficultyMult;
                            Main.projectile[firstEnd].timeLeft += durBoost;
                            Main.projectile[firstEnd].netUpdate = true;
                            Main.projectile[secondEnd].timeLeft += durBoost;
                            Main.projectile[secondEnd].netUpdate = true;
                        }
                        NPC.ai[1] = 0;
                        NPC.ai[0] = 0;
                    }
                    break;
                }
                case 3: {
					if (++NPC.ai[1] > 16 - DifficultyMult) {
                        legTargets[(int)NPC.ai[2]] = target.Center;
                        if (NPC.ai[1] > 48 - DifficultyMult * 6) {
                            NPC.ai[1] = 0;
                            NPC.ai[0] = 0;
                        }
                    }
                    goto case 0;
                }
            }
        }
        public static Vector2 GetStandPosition(Vector2 target, Vector2 legStart) {//candidate
            HashSet<Point> checkedPoints = new HashSet<Point>();
            Queue<Point> candidates = new Queue<Point>();
            candidates.Enqueue(target.ToWorldCoordinates().ToPoint());
            Tile tile;
            Point current;
			while (candidates.Count > 0) {
                current = candidates.Dequeue();
                checkedPoints.Add(current);
				if (legStart.DistanceSQ(current.ToWorldCoordinates()) > (totalLegLength * totalLegLength)) {
                    continue;
				}
                tile = Main.tile[current];
                if ((tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])) || tile.WallType != WallID.None) {
                    return current.ToWorldCoordinates();
                }
                if (!checkedPoints.Contains(current + new Point(1, 0))) candidates.Enqueue(current + new Point(1, 0));
                if (!checkedPoints.Contains(current + new Point(-1, 0))) candidates.Enqueue(current + new Point(-1, 0));
                if (!checkedPoints.Contains(current + new Point(0, 1))) candidates.Enqueue(current + new Point(0, 1));
                if (!checkedPoints.Contains(current + new Point(0, -1))) candidates.Enqueue(current + new Point(0, -1));
            }
            return target;
        }
        public void GetMeleeCollisionData(Rectangle victimHitbox, int enemyIndex, ref int specialHitSetter, ref float damageMultiplier, ref Rectangle npcRect, ref float knockbackMult) {
            Rectangle legHitbox = new Rectangle(-4, -4, 8, 8);
            for (int i = 0; i < 8; i++) {
                Rectangle hitbox = legHitbox;
                Vector2 legStart = legs[i].start.RotatedBy(NPC.rotation) + NPC.Center;
                Vector2 offset = legStart + (Vector2)legs[i].bone0 + (Vector2)legs[i].bone1;
                hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(victimHitbox)) {
                    npcRect = hitbox;
                    damageMultiplier = NPC.ai[0] == 3 ? 3 : 1.5f;
                    return;
				}
            }
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            normalDropRule = ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ModContent.ItemType<Fiberglass_Helmet>(), ModContent.ItemType<Fiberglass_Body>(), ModContent.ItemType<Fiberglass_Legs>());
            normalDropRule.OnSuccess(ItemDropRule.OneFromOptionsNotScalingWithLuck(1, ModContent.ItemType<Fiberglass_Bow>(), ModContent.ItemType<Fiberglass_Sword>(), ModContent.ItemType<Fiberglass_Pistol>()));
            npcLoot.Add(new DropBasedOnExpertMode(
                normalDropRule,
                new DropLocalPerClientAndResetsNPCMoneyTo0(ItemID.BossBagDarkMage, 1, 1, 1, null)
            ));
            //npcLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(ModContent.ItemType<Fiberglass_Helmet>(), ModContent.ItemType<Fiberglass_Body>(), ModContent.ItemType<Fiberglass_Legs>(), 1));
            //npcLoot.Add(ItemDropRule.OneFromOptionsNotScalingWithLuck(ModContent.ItemType<Fiberglass_Bow>(), ModContent.ItemType<Fiberglass_Sword>(), ModContent.ItemType<Fiberglass_Pistol>(), 1));
        }
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            Main.CurrentDrawnEntityShader = Terraria.Graphics.Shaders.GameShaders.Armor.GetShaderIdFromItemId(ItemID.ReflectiveDye);
            if (legs is null) return false;
            for (int i = 0; i < 8; i++) {
                bool flip = (i % 2 != 0) == i < 4;
                Vector2 baseStart = legs[i].start;
                legs[i].start = legs[i].start.RotatedBy(NPC.rotation) + NPC.Center;
                float[] targets = legs[i].GetTargetAngles(legTargets[i], flip);
                AngularSmoothing(ref legs[i].bone0.Theta, targets[0], 0.3f);
                AngularSmoothing(ref legs[i].bone1.Theta, targets[1], NPC.ai[0] == 2 && NPC.ai[2] == i? 1f : 0.3f);

                Vector2 screenStart = legs[i].start - Main.screenPosition;
                Main.EntitySpriteDraw(UpperLegTexture, screenStart, null, drawColor, legs[i].bone0.Theta, new Vector2(5, flip ? 3 : 9), 1f, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
                Main.EntitySpriteDraw(LowerLegTexture, screenStart + (Vector2)legs[i].bone0, null, drawColor, legs[i].bone0.Theta + legs[i].bone1.Theta, new Vector2(6, flip ? 2 : 6), 1f, flip ? SpriteEffects.FlipVertically : SpriteEffects.None, 0);
                legs[i].start = baseStart;
            }
            Main.EntitySpriteDraw(TextureAssets.Npc[Type].Value, NPC.Center - screenPos, null, drawColor, NPC.rotation, new Vector2(34, 70), 1f, SpriteEffects.None, 0);
            return false;
		}
		public override void HitEffect(int hitDirection, double damage) {
            NPC.velocity.X += hitDirection * 3;
            if(NPC.life<0) {
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/FG1_Gore"));
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/FG2_Gore"));
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/FG3_Gore"));
            } else if(damage>NPC.lifeMax*0.1f) {
                Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.GetGoreSlot($"Gores/NPCs/FG{Main.rand.Next(3)+1}_Gore"));
            }
        }
	}
    public class Fiberglass_Thread : ModProjectile {
		public override string Texture => "Origins/Projectiles/Pixel";
		public override void SetStaticDefaults() {
            DisplayName.SetDefault("Fiberglass Thread");
		}
		public override void SetDefaults() {
            Projectile.hostile = true;
            Projectile.timeLeft = 600;
            Projectile.width = Projectile.height = 8;
		}
		public override void OnSpawn(IEntitySource source) {
			//if (source.Context is string s) {
                //Projectile.ai[0] = s[0];
            Projectile other;
			for (int i = 0; i < Main.maxProjectiles; i++) {
                if (i == Projectile.whoAmI) continue;
                other = Main.projectile[i];
				if (i == Projectile.ai[0]) {
                    other.ai[0] = Projectile.whoAmI;
                    continue;
				}
				if (other.type == Type && other.ai[1] == Projectile.ai[1]) {
                    other.Kill();
				}
			}
			//}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            if (Projectile.ai[0] > -1) {
                Projectile other = Main.projectile[(int)Projectile.ai[0]];
                return Collision.CheckAABBvLineCollision2(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, other.Center);
            }
            return false;
		}
		public override void OnHitPlayer(Player target, int damage, bool crit) {
			if (Main.masterMode && Main.rand.NextBool(4)) {
                target.AddBuff(BuffID.Webbed, 30);
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
            Projectile.velocity = default;
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.ai[0] > -1) {
                Projectile other = Main.projectile[(int)Projectile.ai[0]];
                Vector2 pos = new Vector2(Projectile.Center.X - Main.screenPosition.X, Projectile.Center.Y - Main.screenPosition.Y);
                Vector2 scale = new Vector2((other.Center - Projectile.Center).Length(), 2);

                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, pos, null, lightColor, (other.Center - Projectile.Center).ToRotation(), new Vector2(0.5f, 0), scale, SpriteEffects.None, 0);
            }
			return false;
		}
	}
}
