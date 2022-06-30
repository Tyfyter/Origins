using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Origins.Items.Materials;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;

namespace Origins.NPCs.Defiled {
    public class Defiled_Amalgamation : ModNPC {
		public override string Texture => "Origins/NPCs/Defiled/Defiled_Amalgamation_Body";
		public static bool spawnDA = false;
        float rightArmRot = 0;
        float leftArmRot = 0;
        float time = 0;
        //public float SpeedMult => npc.frame.Y==510?1.6f:0.8f;
        //bool attacking = false;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Amalgamation");
            Main.npcFrameCount[NPC.type] = 1;
            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused
	            }
            };
            NPCID.Sets.DebuffImmunitySets[Type] = debuffData;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.Zombie);
            NPC.boss = true;
            NPC.BossBar = ModContent.GetInstance<Boss_Bar_DA>();
            NPC.aiStyle = NPCAIStyleID.None;
            NPC.lifeMax = 2400;
            NPC.defense = 14;
            NPC.damage = 36;
            NPC.width = 122;
            NPC.height = 114;
            NPC.friendly = false;
            NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0f, 0.25f);
            NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(0f, 0.25f);
            NPC.noGravity = true;
            NPC.npcSlots = 200;
            Music = Origins.Music.DefiledBoss;
        }
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return spawnDA ? 10 : 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			//bestiaryEntry.AddTags();
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Defiled_Spirit>(), 10));
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bombardment>(), 8));
        }
		public override void AI() {
            NPC.TargetClosest();
            if (NPC.HasPlayerTarget) {

                Player target = Main.player[NPC.target];
                float leftArmTarget = 0.5f;
                float rightArmTarget = 0.25f;
                float armSpeed = 0.01f;

                int difficultyMult = Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);

                int tickCount = (10 - difficultyMult * 2);
                int tickSize = NPC.lifeMax / tickCount;
                float currentTick = (NPC.life / tickSize);

                switch ((int)NPC.ai[0]) {
                    default: {
                        float targetHeight = 128 + (float)(Math.Sin(++time * (0.04f + (0.01f * difficultyMult))) + 0.5f) * 32 * difficultyMult;
                        float targetX = 288 + (float)Math.Sin(++time * (0.02f + (0.01f * difficultyMult))) * 48 * difficultyMult;
                        float speed = 4;

                        float diffY = NPC.Bottom.Y - (target.MountedCenter.Y - targetHeight);
                        float diffX = NPC.Center.X - target.MountedCenter.X;
                        diffX -= Math.Sign(diffX) * targetX;
                        OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), 0.4f);
                        OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed, speed), 0.4f);

                        if (NPC.ai[0] <= 0) {
                            NPC.ai[1] += 0.75f + (0.25f * difficultyMult);
                            NPC.ai[1] += 0.5f * (difficultyMult - 1) * (1f - (currentTick / tickCount));
                            if (NPC.ai[1] > 300) {
                                Terraria.Utilities.WeightedRandom<int> rand = new(
                                    Main.rand,
                                    new Tuple<int, double>[] {
                                    new(1, 1f),
                                    new(2, 0.9f),
                                    new(3, 0.35f),
                                    new(4, 0.5f)
                                    }
                                );
                                int lastUsedAttack = (-1) - (int)NPC.ai[0];

                                if (lastUsedAttack >= 0) {
                                    rand.elements[lastUsedAttack] = new(rand.elements[lastUsedAttack].Item1, rand.elements[lastUsedAttack].Item2 / 3f);
                                }

                                NPC.ai[0] = rand.Get();
                                NPC.ai[2] = target.MountedCenter.X;
                                NPC.ai[3] = target.MountedCenter.Y;
                                NPC.ai[1] = 0;
                                if (NPC.ai[0] == 1) {
                                    SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-0.9f), NPC.Center);
                                }
                                if (NPC.ai[0] == 4) {
                                    SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-0.5f), NPC.Center);
                                }
                            }
                        }
                    }
                    break;

                    case 1: {
                        NPC.ai[1]++;
                        NPC.velocity = NPC.oldVelocity;
                        if (NPC.ai[1] < 20) {
                            float speed = 6;
                            OriginExtensions.LinearSmoothing(ref NPC.velocity, (NPC.Center - new Vector2(NPC.ai[2], NPC.ai[3])).WithMaxLength(speed), 1.8f);
                            //NPC.oldVelocity = NPC.velocity;
                        } else if(NPC.ai[1] < 30) {
                            float speed = 11 + difficultyMult;
                            OriginExtensions.LinearSmoothing(ref NPC.velocity, (new Vector2(NPC.ai[2], NPC.ai[3]) - NPC.Center).WithMaxLength(speed), 3F);
                            //NPC.oldVelocity = NPC.velocity;
                        } else if (NPC.ai[1] > 80 || NPC.collideX || NPC.collideY) {
                            NPC.ai[0] = -1;
                            NPC.ai[1] = 0;
                        }
                    }
                    break;

                    case 2: {
                        NPC.ai[1] += Main.rand.NextFloat(0.9f, 1f);
                        float targetHeight = 96 + (float)(Math.Sin(++time * 0.02f) + 0.5f) * 32;
                        float targetX = 320 + (float)Math.Sin(++time * 0.01f) * 32;
                        float speed = 3;

                        float diffY = NPC.Bottom.Y - (target.MountedCenter.Y - targetHeight);
                        float diffX = NPC.Center.X - target.MountedCenter.X;
                        diffX -= Math.Sign(diffX) * targetX;
                        OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), 0.4f);
                        OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed, speed), 0.4f);
                        leftArmTarget = -0.75f;
                        rightArmTarget = -0.75f;
                        armSpeed = 0.1f;

                        switch ((int)NPC.ai[1]) {
                            case 10:
                            case 15:
                            case 20:
                            case 60:
                            case 70:
                            SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(-0.6f, -0.4f), NPC.Center);
                            Projectile.NewProjectileDirect(
                                NPC.GetSource_FromAI(),
                                NPC.Center,
                                Vector2.Normalize(target.MountedCenter - NPC.Center).RotatedByRandom(0.15f) * (10 + difficultyMult * 2),
                                ModContent.ProjectileType<Items.Weapons.Defiled.Low_Signal_P>(),
                                19, // for some reason NPC projectile damage is just arbitrarily doubled
                                0f,
                                Main.myPlayer
                            );
                            break;
                            case 12:
                            case 17:
                            case 65:
							if (difficultyMult > 1) {
                                goto case 10;
                            }
                            break;
                            default:
							if (NPC.ai[1] > 100) {
                                NPC.ai[0] = -2;
                                NPC.ai[1] = 0;
                            }
                            break;
						}
                    }
                    break;

                    case 3: {
                        NPC.ai[1]++;
                        int cycleLength = 100 - (difficultyMult * 4);
                        int dashLength = 60 - (difficultyMult * 2);
                        int activeLength = cycleLength * 2 + dashLength;
                        if (NPC.ai[1] < activeLength) {
							if (NPC.ai[1] % cycleLength is < 2 and >= 1) {
                                SoundEngine.PlaySound(Origins.Sounds.DefiledHurt.WithPitch(-1), NPC.Center);
                            }
                            NPC.velocity = NPC.oldVelocity;
                            if (NPC.ai[1] % cycleLength < 18 - (difficultyMult * 3)) {
                                float speed = 6 - (2 * difficultyMult);
                                OriginExtensions.LinearSmoothing(ref NPC.velocity, (NPC.Center - new Vector2(NPC.ai[2], NPC.ai[3])).WithMaxLength(speed), 1.8f);
                                //NPC.oldVelocity = NPC.velocity;
                            } else if (NPC.ai[1] % cycleLength < 26 - (difficultyMult * 2)) {
                                float speed = 14 + (2 * difficultyMult);
                                OriginExtensions.LinearSmoothing(ref NPC.velocity, (new Vector2(NPC.ai[2], NPC.ai[3]) - NPC.Center).WithMaxLength(speed), 3F);
                                //NPC.oldVelocity = NPC.velocity;
                            } else if (NPC.ai[1] % cycleLength > dashLength || NPC.collideX || NPC.collideY) {
                                NPC.ai[2] = target.MountedCenter.X;
                                NPC.ai[3] = target.MountedCenter.Y;
                                goto default;
                            }
						} else {
                            int inactiveTime = 360 / difficultyMult;
							if (NPC.ai[1] - activeLength < inactiveTime) {
                                NPC.velocity.Y += 0.12f;
                                leftArmTarget = 0;
                                rightArmTarget = 0;
                                armSpeed *= 3;
							} else {
                                NPC.ai[0] = -3;
                                NPC.ai[1] = 0;
                            }
						}
                    }
                    break;

                    case 4: {
                        NPC.ai[1]++;
                        float targetHeight = 128 + (float)(Math.Sin(++time * 0.05f) + 0.5f) * 32;
                        float targetX = 288 + (float)Math.Sin(++time * 0.03f) * 48;
                        float speed = 4 * difficultyMult;

                        float diffY = NPC.Bottom.Y - (target.MountedCenter.Y - targetHeight);
                        float diffX = NPC.Center.X - target.MountedCenter.X;
                        diffX += Math.Sign(diffX) * targetX;
                        OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), 0.4f);
                        OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed * 4, speed * 4), 2.4f);
						if (Math.Abs(diffX) < 64 || NPC.ai[1] > 25) {
                            NPC.ai[0] = -4;
                            NPC.ai[1] = 160 + (difficultyMult * 40);
						}
                    }
                    break;
                }
                OriginExtensions.AngularSmoothing(ref rightArmRot, rightArmTarget, armSpeed);
                OriginExtensions.AngularSmoothing(ref leftArmRot, leftArmTarget, armSpeed * 1.5f);
            }
        }
		public override void UpdateLifeRegen(ref int damage) {
			if ((int)NPC.ai[0] != 3) {
                int difficultyMult = Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);
                int tickSize = NPC.lifeMax / (10 - difficultyMult * 2);
                int threshold = ((NPC.life / tickSize) + 1) * tickSize;
				if (NPC.life < threshold - 1) {
                    NPC.lifeRegen += 6 + (difficultyMult * 2);
                }
            }
		}
		public override void OnKill() {
            NPC.downedBoss2 = true;
		}
		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
            Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
            for(int i = Main.rand.Next(3); i-->0;)Gore.NewGore(NPC.GetSource_OnHurt(projectile), Main.rand.NextVectorIn(spawnbox), projectile.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
        }
        public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) {
            int halfWidth = NPC.width / 2;
            int baseX = player.direction > 0 ? 0 : halfWidth;
            for(int i = Main.rand.Next(3); i-->0;)Gore.NewGore(NPC.GetSource_OnHurt(player), NPC.position+new Vector2(baseX + Main.rand.Next(halfWidth),Main.rand.Next(NPC.height)), new Vector2(knockback*player.direction, -0.1f*knockback), Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(NPC.life<0) {
                for(int i = 0; i < 6; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
                for(int i = 0; i < 10; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            }
        }
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
            bool dir = NPC.spriteDirection == 1;
            spriteBatch.Draw(Mod.Assets.Request<Texture2D>("NPCs/Defiled/Defiled_Amalgamation_RA").Value,
                NPC.Center - new Vector2(-46 * NPC.spriteDirection, 12) * NPC.scale - screenPos,
                null,
                drawColor,
                rightArmRot * NPC.spriteDirection,
                new Vector2(dir ? 7 : 23, 19),
                NPC.scale,
                dir ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
            0);

            spriteBatch.Draw(Mod.Assets.Request<Texture2D>("NPCs/Defiled/Defiled_Amalgamation_LA").Value,
                NPC.Center - new Vector2(36 * NPC.spriteDirection, 0) * NPC.scale - screenPos,
                null,
                drawColor,
                -leftArmRot * NPC.spriteDirection,
                new Vector2(dir ? 23 : 7, 19),
                NPC.scale,
                dir ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
            0);
        }
	}
    public class Boss_Bar_DA : ModBossBar {
        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
            return TextureAssets.NpcHead[36]; // Corgi head as placeholder
        }
    }
}
