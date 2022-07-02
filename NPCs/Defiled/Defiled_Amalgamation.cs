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
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.Utilities;
using Origins.Items.Weapons.Defiled;
using Tyfyter.Utils;
using Origins.Projectiles.Enemies;
using Origins.Tiles.Defiled;

namespace Origins.NPCs.Defiled {
    [AutoloadBossHead]
    public class Defiled_Amalgamation : ModNPC {
		public override string Texture => "Origins/NPCs/Defiled/Defiled_Amalgamation_Body";
		public override string BossHeadTexture => "Origins/UI/BossMap/Map_Icon_DA";
        public static bool spawnDA = false;
        float rightArmRot = 0;
        float leftArmRot = 0;
        float time = 0;
        int trappedTime = 0;
        int roars = 0;
        public static int DifficultyMult => Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);
        public static int TripleDashCD {
            get {
                int inactiveTime = 420 / DifficultyMult;
                if (DifficultyMult == 3) {
                    inactiveTime += 30;
                }
                return inactiveTime;
            }
        }
		//public float SpeedMult => npc.frame.Y==510?1.6f:0.8f;
		//bool attacking = false;
		public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Amalgamation");
            Main.npcFrameCount[NPC.type] = 8;
            NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused
	            }
            };
            NPCID.Sets.DebuffImmunitySets[Type] = debuffData;
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
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
            NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(-1f, -0.75f);
            NPC.noGravity = true;
            NPC.npcSlots = 200;
            NPC.value = 10000;
            Music = Origins.Music.DefiledBoss;
            NPC.knockBackResist = 0;// actually a multiplier
        }
		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			switch (DifficultyMult) {
                case 1:
                NPC.lifeMax = 2400;
                NPC.defense = 14;
                NPC.damage = 36;
                break;

                case 2:
                NPC.lifeMax = 3840 / 2;
                NPC.defense = 15;
                NPC.damage = 36;
                break;

                case 3:
                NPC.lifeMax = 6144 / 3;
                NPC.defense = 16;
                NPC.damage = 36;
                break;
            }
		}

        public override void OnSpawn(IEntitySource source) {
            spawnDA = false;
        }
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			//bestiaryEntry.AddTags();
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
            IItemDropRuleCondition notExpert = new Conditions.NotExpert();
            npcLoot.Add(ItemDropRule.ByCondition(notExpert, ModContent.ItemType<Defiled_Ore_Item>(), 1, 140, 330));
            npcLoot.Add(ItemDropRule.ByCondition(notExpert, ModContent.ItemType<Undead_Chunk>(), 1, 40, 100));
            npcLoot.Add(new LeadingConditionRule(notExpert).OnSuccess(ItemDropRule.OneFromOptions(1, ModContent.ItemType<Low_Signal>()))); //I am in agreement with this change.
            npcLoot.Add(ItemDropRule.ByCondition(notExpert, ModContent.ItemType<Undead_Chunk>(), 1));
            //add Fiend Staff & Return To Sender to the below rule once they're added, there were 2 items listed with 33% chance, perfect for fitting in a third
        }
        public override void AI() {
            NPC.TargetClosest();
            if (NPC.HasPlayerTarget) {

                Player target = Main.player[NPC.target];
                float leftArmTarget = 0.5f;
                float rightArmTarget = 0.25f;
                float armSpeed = 0.03f;

                int difficultyMult = DifficultyMult;// just saving the value as a slight optimization 

                int tickCount = 10 - difficultyMult * 2;
                int tickSize = NPC.lifeMax / tickCount;
                float currentTick = (NPC.life / tickSize);

                switch ((int)NPC.ai[0]) {
                    //default state, uses default case so that negative values can be used for which action was taken last
                    default: {
                        CheckTrappedCollision();
                        float targetHeight = 96 + (float)(Math.Sin(++time * (0.04f + (0.01f * difficultyMult))) + 0.5f) * 32 * difficultyMult;
                        float targetX = 256 + (float)Math.Sin(++time * (0.02f + (0.01f * difficultyMult))) * 48 * difficultyMult;
                        float speed = 5;

                        float diffY = NPC.Bottom.Y - (target.MountedCenter.Y - targetHeight);
                        float diffX = NPC.Center.X - target.MountedCenter.X;
                        diffX -= Math.Sign(diffX) * targetX;
						if (NPC.DistanceSQ(target.MountedCenter) > 640 * 640) {
                            speed *= 5;
						}
                        OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), (Math.Abs(NPC.velocity.Y) > 16 ? 4 : 0.4f));
                        OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed, speed), (Math.Abs(NPC.velocity.X) > 16 ? 4 : 0.4f));

                        if (NPC.ai[0] <= 0) {
                            NPC.ai[1] += 0.75f + (0.25f * difficultyMult);
                            NPC.ai[1] += 0.5f * (difficultyMult - 1) * (1f - (currentTick / tickCount));
                            if (NPC.ai[1] > 300) {
                                WeightedRandom<int> rand = new(
                                    Main.rand,
                                    new Tuple<int, double>[] {
                                    new(1, 1f),
                                    new(2, 0.9f),
                                    new(3, 0.35f),
                                    new(4, 0.45f + (0.05f * difficultyMult))
                                    }
                                );
                                int lastUsedAttack = (-1) - (int)NPC.ai[0];

                                if (lastUsedAttack >= 0) {
                                    rand.elements[lastUsedAttack] = new(rand.elements[lastUsedAttack].Item1, rand.elements[lastUsedAttack].Item2 / 3f);
                                }

                                if (!Collision.CanHitLine(target.position, target.width, target.height, NPC.Center, 16, 16)) {
                                    rand.elements[0] = new(rand.elements[0].Item1, rand.elements[0].Item2 / 3f);
                                    rand.elements[1] = new(rand.elements[1].Item1, rand.elements[1].Item2 * 6f);
                                    rand.elements[2] = new(rand.elements[2].Item1, rand.elements[2].Item2 / 3f);
                                }

                                NPC.ai[0] = rand.Get();
                                NPC.ai[2] = target.MountedCenter.X;
                                NPC.ai[3] = target.MountedCenter.Y;
                                NPC.ai[1] = 0;

                                int roarCount = difficultyMult;
                                int roarHP = NPC.lifeMax / (roarCount + 1);

                                if (roarCount - roars > NPC.life / roarHP) {
                                    NPC.ai[0] = 5;
                                    roars++;
                                }

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

                    //single dash
                    case 1: {
                        NPC.ai[1]++;
                        NPC.velocity = NPC.oldVelocity;
                        if (NPC.ai[1] < 20) {
                            float speed = 6;
                            OriginExtensions.LinearSmoothing(ref NPC.velocity, (NPC.Center - new Vector2(NPC.ai[2], NPC.ai[3])).WithMaxLength(speed), 1.8f);
                            NPC.oldVelocity = NPC.velocity;
                        } else if (NPC.ai[1] < 30) {
                            float speed = 11 + difficultyMult;
                            OriginExtensions.LinearSmoothing(ref NPC.velocity, (new Vector2(NPC.ai[2], NPC.ai[3]) - NPC.Center).WithMaxLength(speed), 3F);
                            NPC.oldVelocity = NPC.velocity;
                        } else if ((NPC.collideX || NPC.collideY) && NPC.ai[1] <= 80) {
                            NPC.ai[1] += 2;
                            NPC.velocity *= 0.99f;
                            NPC.oldVelocity = NPC.velocity;
                        } else if (NPC.ai[1] > 80) {
                            NPC.ai[0] = -1;
                            NPC.ai[1] = 0;
                        }
                    }
                    break;

                    //projectile spray
                    case 2: {
                        CheckTrappedCollision();
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
                                Vector2.Normalize(target.MountedCenter - NPC.Center).RotatedByRandom(0.15f) * (10 + difficultyMult * 2) * Main.rand.NextFloat(0.9f, 1.1f),
                                ModContent.ProjectileType<Low_Signal_P>(),
                                22 - (difficultyMult * 3), // for some reason NPC projectile damage is just arbitrarily doubled
                                0f,
                                Main.myPlayer
                            ).tileCollide = Collision.CanHitLine(target.position, target.width, target.height, NPC.Center, 8, 8);
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

                    //triple dash and downtime after
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
                                NPC.oldVelocity = NPC.velocity;
                            } else if (NPC.ai[1] % cycleLength < 26 - (difficultyMult * 2)) {
                                float speed = 14 + (2 * difficultyMult);
                                OriginExtensions.LinearSmoothing(ref NPC.velocity, (new Vector2(NPC.ai[2], NPC.ai[3]) - NPC.Center).WithMaxLength(speed), 3F);
                                NPC.oldVelocity = NPC.velocity;
                            } else if (NPC.ai[1] % cycleLength > dashLength || NPC.collideX || NPC.collideY) {
                                NPC.ai[2] = target.MountedCenter.X;
                                NPC.ai[3] = target.MountedCenter.Y;
                                goto default;
                            }
						} else {
                            NPC.collideX = NPC.collideY = false;
                            CheckTrappedCollision();
							if (NPC.ai[1] - activeLength < TripleDashCD) {
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

                    //"sidestep" dash
                    case 4: {
						if ((int)NPC.ai[1] == 0) {
                            NPC.ai[2] = target.MountedCenter.X + Math.Sign(NPC.Center.X - target.MountedCenter.X) * 288;
                            NPC.ai[3] = target.MountedCenter.Y - 128;
                        }
                        NPC.ai[1]++;
                        float targetHeight = (float)(Math.Sin(++time * 0.05f) + 0.5f) * 32;
                        float targetX = (float)Math.Sin(++time * 0.03f) * 48;
                        float speed = 5 * difficultyMult;

                        float diffY = NPC.Bottom.Y - (NPC.ai[2] - targetHeight);
                        float diffX = NPC.Center.X - NPC.ai[3];
                        OriginExtensions.LinearSmoothing(ref NPC.velocity.Y, Math.Clamp(-diffY, -speed, speed), 0.4f);
                        OriginExtensions.LinearSmoothing(ref NPC.velocity.X, Math.Clamp(-diffX, -speed * 4, speed * 4), 2.4f);
						if (Math.Abs(diffX) < 64 || NPC.ai[1] > 25) {
                            NPC.ai[0] = -4;
                            NPC.ai[1] = 160 + (difficultyMult * 40);
                        }
                        NPC.noTileCollide = true;
                    }
                    break;

                    //"beckoning roar"
                    case 5: {
                        NPC.ai[1]++;
                        NPC.velocity *= 0.9f;
                        if (NPC.ai[1] < 40) {
                            leftArmTarget = 0;
                            rightArmTarget = 0;
                            armSpeed *= 0.5f;
                        } else if (NPC.ai[1] > 60) {
                            NPC.ai[0] = 0;
                            NPC.ai[1] = -40 + (difficultyMult * 20);
                        } else if (NPC.ai[1] >= 45) {
                            NPC.velocity = new Vector2(0, -4);
                            if ((int)NPC.ai[1] == 45) {
                                SoundEngine.PlaySound(SoundID.ForceRoar.WithPitchRange(0.1f, 0.2f));
                                for (int i = 3 + difficultyMult; i-- > 0;) {
                                    Projectile.NewProjectileDirect(
                                        NPC.GetSource_FromAI(),
                                        target.Center - new Vector2(Main.rand.Next(80, 640) * (Main.rand.Next(2) * 2 - 1), 640),
                                        new Vector2(0, 8),
                                        ModContent.ProjectileType<Defiled_Enemy_Summon>(),
                                        0,
                                        0f,
                                        Main.myPlayer
                                    );
                                }
                            }
                            leftArmTarget = -1.25f;
                            rightArmTarget = -1.25f;
                            armSpeed *= 5f;
                        }
                    }
                    break;
                }
                OriginExtensions.AngularSmoothing(ref rightArmRot, rightArmTarget, armSpeed);
                OriginExtensions.AngularSmoothing(ref leftArmRot, leftArmTarget, armSpeed * 1.5f);
            }
            NPC.alpha = NPC.noTileCollide ? 75 : 0;
        }
        public void CheckTrappedCollision() {
            if (Collision.IsClearSpotTest(NPC.position + new Vector2(16), 16f, NPC.width - 32, NPC.height - 32, checkSlopes: true)) {
                NPC.noTileCollide = false;
            }
            if (NPC.collideX || NPC.collideY) {
                if (++trappedTime > 30) {
                    NPC.noTileCollide = true;
                    NPC.collideX = NPC.collideY = false;
                }
            } else if (trappedTime > 0) {
                trappedTime--;
            }
        }
		public override void FindFrame(int frameHeight) {
            int cycleLength = 100 - (DifficultyMult * 4);
            int dashLength = 60 - (DifficultyMult * 2);
            int activeLength = cycleLength * 2 + dashLength;
            if ((int)NPC.ai[0] == 3 && NPC.ai[1] > activeLength) {
                NPC.frame = new Rectangle(0, (frameHeight * (int)(Math.Pow((NPC.ai[1] - activeLength) / TripleDashCD, 3) * 5) + frameHeight * 7) % (frameHeight * 8), 122, frameHeight);
            } else if (++NPC.frameCounter > 7) {
                NPC.frame = new Rectangle(0, (NPC.frame.Y + frameHeight) % (frameHeight * 3) + frameHeight * 4, 122, frameHeight);
                NPC.frameCounter = 0;
            }
        }
		public override void UpdateLifeRegen(ref int damage) {
			if ((int)NPC.ai[0] != 3) {
                int tickSize = NPC.lifeMax / (10 - DifficultyMult * 2);
                int threshold = (((NPC.life - 1) / tickSize) + 1) * tickSize;
				if (NPC.life < threshold) {
                    NPC.lifeRegen += 6 + (DifficultyMult * 2);
                }
            }
		}
		public override void OnKill() {
            NPC.downedBoss2 = true;
		}
		public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
            switch ((int)NPC.ai[0]) {
                case 2:
                case 3:
                break;
                default: {
                    Rectangle highHitbox = NPC.Hitbox;
                    highHitbox.Height /= 4;

                    Rectangle lowHitbox = NPC.Hitbox;
                    lowHitbox.Y += highHitbox.Height;
                    lowHitbox.Height -= highHitbox.Height;
                    lowHitbox.Width /= 2;
                    lowHitbox.X += lowHitbox.Width / 2;

                    if (!highHitbox.Intersects(projectile.Hitbox) && !lowHitbox.Intersects(projectile.Hitbox)) {
                        damage -= (Math.Max(NPC.defense - projectile.ArmorPenetration, 0) * (1 + DifficultyMult)) / 4;
                    }
                }
                break;
            }
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
            drawColor *= (255 - NPC.alpha) / 255f;
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
        float lastTickPercent = 1f;
        public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
            return Asset<Texture2D>.Empty;
        }
        public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float lifePercent, ref float shieldPercent) {
            NPC owner = Main.npc[info.npcIndexToAimAt];

            int tickCount = 10 - Defiled_Amalgamation.DifficultyMult * 2;
            int tickSize = owner.lifeMax / tickCount;
            float lifeTarget = ((owner.life + tickSize - 1) / tickSize) / (float)tickCount;
            OriginExtensions.LinearSmoothing(ref lastTickPercent, lifeTarget, 0.015f);
            lifePercent = lastTickPercent;
            shieldPercent = owner.life / (float)owner.lifeMax;
            return lifePercent > 0;
		}
	}
}
