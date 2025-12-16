using Microsoft.Xna.Framework.Graphics;
using Origins.Graphics;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Ranged;
using Origins.Music;
using Origins.World.BiomeData;
using PegasusLib;
using ReLogic.Content;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.Brine.Boss {
	public class SpawnNPCFlicker(NPC npc, int flickerRange, int flickerTimeMin, int flickerTimeMax, int timesToFlicker = int.MaxValue) : IBlackout {
		readonly int initialNPCType = npc.type;
		public bool Finished { get; set; }
		int flickersDone;
		public bool lightsOff;
		int time;
		int currentFlickerTime;
		public void Update() {
			if (!npc.active || npc.type != initialNPCType) {
				Finished = true;
				return;
			}
			if (currentFlickerTime <= 0) currentFlickerTime = Main.rand.Next(flickerTimeMin, flickerTimeMax + 1);
			if (++time >= currentFlickerTime) {
				time = 0;
				lightsOff = !lightsOff;
				flickersDone++;
				currentFlickerTime = 0;
			}
			if (flickersDone >= timesToFlicker) Finished = true;
			else if (lightsOff) {
				BlackoutSystem.Blackout((int)(npc.Center.X / 16), (int)(npc.Center.Y / 16), flickerRange, flickerRange, flickerRange);
			}
		}
	}
	public class Lost_Diver_Spawn : Lost_Diver {
		public override string Texture => "Origins/NPCs/Brine/Boss/Lost_Diver";
		private SpawnNPCFlicker flicker;
		public override void SetStaticDefaults() {
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.dontTakeDamage = true;
			NPC.damage = 0;
			NPC.noGravity = true;
			NPC.BossBar = null;
			NPC.hide = true;
		}
		public override void AI() {
			if (flicker is null) {
				NPC.ai[1] = Main.rand.NextFloat(11, 17) * 12f;
				NPC.ai[0] = Main.rand.NextFloat(0.3f, 1) * NPC.ai[1] - 24;
				BlackoutSystem.Add(flicker = new SpawnNPCFlicker(NPC, 30, 2, 5));
			}

			if (--NPC.ai[1] <= 0) {
				NPC.Transform(ModContent.NPCType<Lost_Diver>());
			}
			if (--NPC.ai[0] <= 0 && flicker.lightsOff && NPC.hide) {
				NPC.hide = false;
			}
			DoTargeting();
			if (TargetPos.X > NPC.Center.X) NPC.direction = 1;
			else NPC.direction = -1;
		}
	}
	[AutoloadBossHead]
	public class Lost_Diver : Brine_Pool_NPC {
		public static int HeadID { get; private set; } = -1;
		public override bool AggressivePathfinding => true;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			HeadID = NPC.GetBossHeadTextureIndex();
			NPCID.Sets.CantTakeLunchMoney[Type] = !OriginsModIntegrations.CheckAprilFools();
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() {
				//CustomTexturePath = "Origins/NPCs/Brine/Boss/Rock_Bottom", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(0f, 0f),
				PortraitPositionXOverride = 0f,
				PortraitPositionYOverride = 0f
			};
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
			Origins.RasterizeAdjustment[Type] = (10, 0.07f, 1f);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 28;
			NPC.lifeMax = 7800;
			NPC.defense = 26;
			NPC.aiStyle = Terraria.ID.NPCAIStyleID.FaceClosestPlayer;
			NPC.width = 20;
			NPC.height = 42;
			NPC.knockBackResist = 0f;
			NPC.HitSound = SoundID.NPCHit4.WithPitchRange(-0.8f, -0.4f);
			NPC.DeathSound = NPC.HitSound;
			NPC.value = 0;//Item.buyPrice(gold: 5);
			NPC.BossBar = ModContent.GetInstance<Boss_Bar_LD>();
			NPC.npcSlots = 200;
			SpawnModBiomes = [
				ModContent.GetInstance<Brine_Pool>().Type
			];
		}
		public AIModes AIMode {
			get => (AIModes)NPC.aiAction;
			set {
				NPC.ai[0] = 0;
				NPC.aiAction = (int)value;
			}
		}
		public int HeldProjectile {
			get => (int)NPC.ai[3];
			set => NPC.ai[3] = value;
		}
		public AIModes LastMode {
			get => (AIModes)(int)NPC.localAI[3];
			set => NPC.localAI[3] = (int)value;
		}
		public override bool CanTargetPlayer(Player player) => NPC.WithinRange(player.MountedCenter, 16 * 400);
		public override bool CanTargetNPC(NPC other) => !OriginsSets.NPCs.TargetDummies[other.type] && NPC.WithinRange(other.Center, 16 * 400) && CanHitNPC(other);
		public override bool CanHitNPC(NPC target) => !Mildew_Creeper.FriendlyNPCTypes.Contains(target.type);
		public override bool CheckTargetLOS(Vector2 target) {
			if (!NPC.wet) return true;
			if (!base.CheckTargetLOS(target)) return false;
			for (int i = 0; i < 2; i++) {
				for (int j = 0; j < 2; j++) {
					if (!CollisionExt.CanHitRay(NPC.position + new Vector2((NPC.width - 2) * i + 1, (NPC.height - 2) * j + 1), target)) return false;
				}
			}
			return true;
		}
		public override float RippleTargetWeight(float magnitude, float distance) => 0;
		public override bool? CanFallThroughPlatforms() => NPC.wet || NPC.targetRect.Bottom > NPC.BottomLeft.Y;
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public int enragedAttackCount = 0;
		public override void AI() {
			float difficultyMult = ContentExtensions.DifficultyDamageMultiplier;
			DoTargeting();
			Vector2 targetVelocity = Vector2.Zero;
			bool targetWet = false;
			if (NPC.HasPlayerTarget) {
				Player player = Main.player[NPC.target];
				targetWet = player.wet;
				targetVelocity = player.velocity;
			} else if (NPC.HasNPCTarget) {
				NPC npcTarget = Main.npc[NPC.TranslatedTargetIndex];
				targetWet = npcTarget.wet;
				targetVelocity = npcTarget.velocity;
			}
			bool enraged = NPC.wet && !targetWet;
			Vector2 differenceFromTarget = TargetPos - NPC.Center;
			float distanceFromTarget = differenceFromTarget.Length();
			Vector2 direction = differenceFromTarget / distanceFromTarget;
			
			if (swimTime > 0) {
				NPC.frameCounter += 2.0;
				while (NPC.frameCounter > 8.0) {
					NPC.frameCounter -= 8.0;
					legFrame.Y += legFrame.Height;
				}
				if (legFrame.Y < legFrame.Height * 7) {
					legFrame.Y = legFrame.Height * 19;
				} else if (legFrame.Y > legFrame.Height * 19) {
					legFrame.Y = legFrame.Height * 7;
				}
				if (swimTime > 20) {
					bodyFrame.Y = 0;
				} else if (swimTime > 10) {
					bodyFrame.Y = bodyFrame.Height * 5;
				} else {
					bodyFrame.Y = 0;
				}
				swimTime--;
			} else if (NPC.velocity.Y != 0f) {
				bodyFrame.Y = bodyFrame.Height * 5;
			} else {
				if (NPC.wet) NPC.frameCounter = 0;
				bodyFrame.Y = 0;
			}
			void TrySwim() {
				int oldTime = swimTime;
				swimTime = Math.Min(swimCharge, 30);
				swimCharge -= swimTime - oldTime;
			}
			if (enraged) swimCharge = 60 * 10;
			if (NPC.wet) {
				NPC.velocity *= 0.96f;
				if (TargetPos != default) {
					if (Math.Abs(differenceFromTarget.Y) > 16 * 100) {
						swimCharge = 60 * 10;
					}
					const float fast_speed = 0.6f;
					float slow_speed = CanSeeTarget ? 0.2f : 0.4f;
					float fast_distance = 16 * 20;
					float slow_distance = CanSeeTarget ? 16 * 15 : 0;
					float away_distance = CanSeeTarget ? 16 * (6 + difficultyMult) : 0;
					if (differenceFromTarget.Y > 64) {
						NPC.velocity.Y += 0.6f;
					} else if (differenceFromTarget.Y > (CanSeeTarget ? 32 : 0) || swimCharge <= 0) {
						NPC.velocity.Y += 0.2f;
						if (swimTime <= 0) swimTime = 30;
					} else {
						if (differenceFromTarget.Y < (CanSeeTarget ? -64 : -4)) {
							if (Collision.WetCollision(NPC.position - Vector2.UnitY * 6, NPC.width, NPC.height)) {
								NPC.velocity.Y = -6;
							} else {
								NPC.velocity.Y *= 0.5f;
								slow_distance /= 2;
								fast_distance /= 2;
							}
							if (swimTime <= 10) TrySwim();
						} else {
							if (swimTime <= 0) TrySwim();
						}
					}
					if (differenceFromTarget.X > fast_distance) {
						NPC.velocity.X += fast_speed;
					} else if (differenceFromTarget.X > slow_distance) {
						NPC.velocity.X += slow_speed;
					} else if (differenceFromTarget.X < -fast_distance) {
						NPC.velocity.X -= fast_speed;
					} else if (differenceFromTarget.X < -slow_distance) {
						NPC.velocity.X -= slow_speed;
					} else if (differenceFromTarget.X < away_distance) {
						NPC.velocity.X -= fast_speed;
					} else if (differenceFromTarget.X > -away_distance) {
						NPC.velocity.X += fast_speed;
					}
					if (NPC.collideY) {
						swimCharge = 60 * 10;
					}
				}
			} else {
				swimTime = 0;
				if (NPC.collideY) {
					if (Math.Abs(NPC.velocity.X) > 2) NPC.velocity.X *= 0.98f;
					swimCharge = 60 * 10;
				}
				if (Math.Abs(direction.X) >= 0.1f) {
					if (Math.Abs(NPC.velocity.Y) < 0.01f && Math.Sign(direction.X) != -Math.Sign(NPC.velocity.X)) {
						int xDirection = Math.Sign(direction.X);
						NPC.frameCounter += 0.9f + NPC.localAI[1];
						while (NPC.frameCounter > 8.0) {
							NPC.frameCounter -= 8.0;
							legFrame.Y += legFrame.Height;
						}
						if (legFrame.Y < legFrame.Height * 7) {
							legFrame.Y = legFrame.Height * 19;
						} else if (legFrame.Y > legFrame.Height * 19) {
							legFrame.Y = legFrame.Height * 7;
						}
						if (legFrame.Y / legFrame.Height is 9 or 17) NPC.velocity.X = xDirection * (0.01f + NPC.localAI[1] * 0.2f);
						else NPC.velocity.X = xDirection * 0.7f * (1 + NPC.localAI[1] * 0.5f);
						if (NPC.localAI[1] < 1) NPC.localAI[1] += 0.01f / 60;
					}
				}
				NPC.velocity.Y += 0.4f;
				Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
				Collision.StepDown(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			}
			bool useItemRotFrame = false;
			heldItemType = -1;
			startMode:
			float attackSpeed = 0.65f + (0.35f * difficultyMult);
			switch (AIMode) {
				default:
				case AIModes.Idle:
				NPC.ai[0] += attackSpeed;
				if (NPC.ai[0] > 120 && NPC.HasValidTarget) {
					HeldProjectile = -1;
					WeightedRandom<AIModes> rand = new(Main.rand);
					void AddMode(AIModes mode, double weight) {
						if (enraged && mode == AIModes.Boat_Rocker) weight *= 2f;
						else if (mode == LastMode) weight *= 0.3f;
						rand.Add(mode, weight);
					}
					float rangeFactor = Math.Max(0, distanceFromTarget / (15 * 16) - 1);
					AddMode(AIModes.Idle, enraged ? 0 : 0 + rangeFactor);
					AddMode(AIModes.Boat_Rocker, 1 + (Main.expertMode ? rangeFactor * 0.5f : 0));
					AddMode(AIModes.Depth_Charge, 1 - rangeFactor);
					AddMode(AIModes.Torpedo_Tube, 1);
					AddMode(AIModes.Mildew_Whip, NPC.life > NPC.lifeMax / 2 ? 0 : (1 - rangeFactor));
					AIMode = rand.Get();
					LastMode = AIMode;
					NPC.netUpdate = true;
					if (enraged) {
						if (enragedAttackCount < 5) enragedAttackCount++;
					} else if (enragedAttackCount > 0) {
						enragedAttackCount--;
					}
					goto startMode;
				}
				break;
				case AIModes.Boat_Rocker:
				if (HeldProjectile < 0) {
					float speed = enraged ? 24 : 12;
					Vector2 dir = direction * speed;
					float lastAngle = dir.ToRotation();
					int tries = 0;
					improve:
					if (GeometryUtils.AngleToTarget((differenceFromTarget + targetVelocity * 2) - dir * Math.Min(10, (distanceFromTarget / 4) / speed), speed, 0.3f) is float angle) {
						dir = GeometryUtils.Vec2FromPolar(speed, angle);
						if (++tries < 2 && GeometryUtils.AngleDif(lastAngle, angle, out _) > 0.1f) {
							lastAngle = angle;
							goto improve;
						}
					} else {
						dir = new Vector2(Math.Sign(direction.X), 0).RotatedBy(Math.Sign(direction.X) * -0.75f) * speed;
					}
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						HeldProjectile = Projectile.NewProjectileDirect(
							NPC.GetSource_FromAI(),
							NPC.Center,
							dir,
							ModContent.ProjectileType<Lost_Diver_Harpoon>(),
							28 + (int)(6 * difficultyMult),
							4,
							ai2: NPC.whoAmI
						).identity;
					}
					itemRotation = dir.ToRotation();
					if (dir.X < 0)
						itemRotation += MathHelper.Pi;

					itemRotation = MathHelper.WrapAngle(itemRotation);
				} else {
					Projectile heldProjectile = Main.projectile.FirstOrDefault(x => x.active && x.identity == HeldProjectile);
					if (heldProjectile is null) {
						HeldProjectile = -1;
						AIMode = AIModes.Idle;
						NPC.ai[0] = difficultyMult * 25;
						if (enraged) NPC.ai[0] = 115 - attackSpeed * 20;
					}
				}
				heldItemType = ModContent.ItemType<Boat_Rocker>();
				useItemRotFrame = true;
				break;
				case AIModes.Depth_Charge:
				if (HeldProjectile < 0) {
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						float speed = enraged ? 10 : 8;
						HeldProjectile = Projectile.NewProjectileDirect(
							NPC.GetSource_FromAI(),
							NPC.Center,
							direction.RotatedBy(NPC.direction * -0.5f) * speed,
							ModContent.ProjectileType<Lost_Diver_Depth_Charge>(),
							30 + (int)(15 * difficultyMult),
							4,
							ai2: NPC.whoAmI
						).identity;
					}
				} else {
					Projectile heldProjectile = Main.projectile.FirstOrDefault(x => x.active && x.identity == HeldProjectile);
					if (heldProjectile is null) {
						HeldProjectile = -1;
						AIMode = AIModes.Idle;
					}
				}
				useItemRotFrame = true;
				break;
				case AIModes.Torpedo_Tube:
				if (NPC.ai[0] <= 0) {
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						float speed = targetWet ? 8 : 12;
						Projectile.NewProjectileDirect(
							NPC.GetSource_FromAI(),
							NPC.Center,
							direction * speed,
							ModContent.ProjectileType<Lost_Diver_Torpedo_Tube>(),
							26 + (int)(9 * difficultyMult),
							4,
							ai2: NPC.whoAmI
						);
					}
					itemRotation = direction.ToRotation();
					if (direction.X < 0)
						itemRotation += MathHelper.Pi;

					itemRotation = MathHelper.WrapAngle(itemRotation);
				} else if (NPC.ai[0] > 30) {
					AIMode = AIModes.Idle;
					break;
				}
				heldItemType = ModContent.ItemType<Torpedo_Tube>();
				useItemRotFrame = true;
				NPC.ai[0]++;
				break;
				case AIModes.Mildew_Whip:
				if (HeldProjectile < 0) {
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						float speed = enraged ? 6 : 4;
						HeldProjectile = Projectile.NewProjectileDirect(
							NPC.GetSource_FromAI(),
							NPC.Center,
							direction * speed,
							ModContent.ProjectileType<Lost_Diver_Mildew_Whip>(),
							24 + (int)(11 * difficultyMult),
							2,
							ai2: NPC.whoAmI
						).identity;
					}
				} else {
					Projectile heldProjectile = Main.projectile.FirstOrDefault(x => x.active && x.identity == HeldProjectile);
					if (heldProjectile is null) {
						HeldProjectile = -1;
						AIMode = AIModes.Idle;
					}
				}
				if (NPC.ai[0] < Lost_Diver_Mildew_Whip.UseTime * 0.333f) {
					bodyFrame.Y = bodyFrame.Height;
				} else if (NPC.ai[0] < Lost_Diver_Mildew_Whip.UseTime * 0.666f) {
					bodyFrame.Y = bodyFrame.Height * 2;
				} else {
					bodyFrame.Y = bodyFrame.Height * 3;
				}
				NPC.ai[0]++;
				//useItemRotFrame = true;
				break;
			}
			if (useItemRotFrame) {
				float rot = itemRotation * NPC.direction;
				bodyFrame.Y = bodyFrame.Height * 3;
				if (rot < -0.75f) {
					bodyFrame.Y = bodyFrame.Height * 2;
				}
				if (rot > 0.6f) {
					bodyFrame.Y = bodyFrame.Height * 4;
				}
			}
		}
		public Vector2 GetHandPosition() {
			Vector2 vector = Main.OffsetsPlayerOnhand[bodyFrame.Y / 56] * 2f;

			vector -= new Vector2(bodyFrame.Width - NPC.width, bodyFrame.Height - 42) / 2f;
			return NPC.Center - new Vector2(20f, 42f) / 2f + vector + Vector2.UnitY * NPC.gfxOffY;
		}
		public int heldItemType;
		public int swimTime;
		public int swimCharge;
		public float itemRotation;
		public Rectangle bodyFrame = new(0, 0, 40, 56);
		public Rectangle legFrame = new(0, 0, 40, 56);
		AutoLoadingAsset<Texture2D> headTexture = typeof(Lost_Diver).GetDefaultTMLName() + "_Head";
		AutoLoadingAsset<Texture2D> bodyTexture = typeof(Lost_Diver).GetDefaultTMLName() + "_Body";
		AutoLoadingAsset<Texture2D> legTexture = typeof(Lost_Diver).GetDefaultTMLName() + "_Legs";
		AutoLoadingAsset<Texture2D> backTexture = typeof(Lost_Diver).GetDefaultTMLName() + "_Back";
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (NPC.IsABestiaryIconDummy && !OriginsModIntegrations.CheckAprilFools()) AutoLoadingAsset<Texture2D>.Wait(headTexture, bodyTexture, legTexture, backTexture);
			//conveniently, the drop it has that'd use the proper composite arms is the keytar, so nothing it's definitely going to do needs me to support them
			SpriteEffects effect = NPC.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Vector2 headVect = legFrame.Size() * new Vector2(0.5f, 0.4f);
			Vector2 bodyVect = legFrame.Size() * new Vector2(0.5f, 0.5f);
			Vector2 legVect = legFrame.Size() * new Vector2(0.5f, 0.75f);
			Vector2 position = NPC.position + Main.OffsetsPlayerHeadgear[legFrame.Y / legFrame.Height];
			Color headColor = Lighting.GetColorClamped((int)NPC.Center.X / 16, (int)(position.Y + NPC.height * 0.25) / 16, Color.White);
			Color bodyColor = Lighting.GetColorClamped((int)NPC.Center.X / 16, (int)(position.Y + NPC.height * 0.75) / 16, Color.White);
			Color legColor = Lighting.GetColorClamped((int)NPC.Center.X / 16, (int)(position.Y + NPC.height * 0.75) / 16, Color.White);
			if (NPC.IsABestiaryIconDummy) {
				headColor = Color.White;
				bodyColor = Color.White;
				legColor = Color.White;
			} else {
				NPCLoader.DrawEffects(NPC, ref headColor);
				headColor = NPC.GetNPCColorTintedByBuffs(headColor);

				NPCLoader.DrawEffects(NPC, ref bodyColor);
				bodyColor = NPC.GetNPCColorTintedByBuffs(bodyColor);

				NPCLoader.DrawEffects(NPC, ref legColor);
				legColor = NPC.GetNPCColorTintedByBuffs(legColor);
			}

			#region composite data
			Vector2 vector = new Vector2((int)(position.X - (bodyFrame.Width / 2) + (NPC.width / 2)), (int)(position.Y + NPC.height - bodyFrame.Height + 4f)) + new Vector2(bodyFrame.Width / 2, bodyFrame.Height / 2) - screenPos;
			Vector2 vector2 = Main.OffsetsPlayerHeadgear[bodyFrame.Y / bodyFrame.Height];
			vector2.Y -= 2f;
			vector -= vector2;
			Vector2 compositeOffset_BackArm = new(6 * NPC.direction, 2);
			Vector2 backArmPosition = vector + compositeOffset_BackArm;
			Vector2 backArmOrigin = bodyVect + compositeOffset_BackArm;

			Vector2 compositeOffset_FrontArm = new(-5 * NPC.direction, 0);
			Vector2 frontArmPosition = vector + compositeOffset_FrontArm;
			Vector2 frontArmOrigin = bodyVect + compositeOffset_FrontArm;
			Vector2 frontShoulderPosition = frontArmPosition + new Vector2(3 * NPC.direction, 0);
			Point backShoulderFrameIndex = new(1, 1);
			Point frontShoulderFrameIndex = new(0, 1);
			Point backArmframeIndex = default;
			Point frontArmframeIndex = default;
			Point torsoFrameIndex = default;
			static Rectangle CreateCompositeFrameRect(Point pt) {
				return new Rectangle(pt.X * 40, pt.Y * 56, 40, 56);
			}
			int shoulderDrawPosition = 1; // -1 if hideCompositeShoulders would be true, 0 if compShoulderOverFrontArm would be false, 1 if compShoulderOverFrontArm would be true
			switch (bodyFrame.Y / bodyFrame.Height) {
				case 0:
				frontArmframeIndex.X = 2;
				break;
				case 1:
				frontArmframeIndex.X = 3;
				shoulderDrawPosition = 0;
				break;
				case 2:
				frontArmframeIndex.X = 4;
				shoulderDrawPosition = 0;
				break;
				case 3:
				frontArmframeIndex.X = 5;
				shoulderDrawPosition = 1;
				break;
				case 4:
				frontArmframeIndex.X = 6;
				shoulderDrawPosition = 1;
				break;
				case 5:
				frontArmframeIndex.X = 2;
				frontArmframeIndex.Y = 1;
				torsoFrameIndex.X = 1;
				shoulderDrawPosition = -1;
				break;
				case 6:
				frontArmframeIndex.X = 3;
				frontArmframeIndex.Y = 1;
				break;
				case 7:
				case 8:
				case 9:
				case 10:
				frontArmframeIndex.X = 4;
				frontArmframeIndex.Y = 1;
				break;
				case 11:
				case 12:
				case 13:
				frontArmframeIndex.X = 3;
				frontArmframeIndex.Y = 1;
				break;
				case 14:
				frontArmframeIndex.X = 5;
				frontArmframeIndex.Y = 1;
				break;
				case 15:
				case 16:
				frontArmframeIndex.X = 6;
				frontArmframeIndex.Y = 1;
				break;
				case 17:
				frontArmframeIndex.X = 5;
				frontArmframeIndex.Y = 1;
				break;
				case 18:
				case 19:
				frontArmframeIndex.X = 3;
				frontArmframeIndex.Y = 1;
				break;
			}
			backArmframeIndex.X = frontArmframeIndex.X;
			backArmframeIndex.Y = frontArmframeIndex.Y + 2;
			Rectangle compBackShoulderFrame = CreateCompositeFrameRect(backShoulderFrameIndex);
			Rectangle compBackArmFrame = CreateCompositeFrameRect(backArmframeIndex);
			Rectangle compFrontShoulderFrame = CreateCompositeFrameRect(frontShoulderFrameIndex);
			Rectangle compFrontArmFrame = CreateCompositeFrameRect(frontArmframeIndex);
			Rectangle compTorsoFrame = CreateCompositeFrameRect(torsoFrameIndex);
			if (compFrontArmFrame.X / compFrontArmFrame.Width >= 7) {
				frontArmPosition += new Vector2(NPC.direction, 1);
			}
			#endregion

			//index 10, back acc
			Vector2 vec = position + new Vector2(NPC.width / 2, NPC.height - bodyFrame.Height / 2) + new Vector2(0f, 4f) - screenPos;
			vec = vec.Floor();
			spriteBatch.Draw(
				backTexture,
				vec,
				bodyFrame,
				bodyColor,
				0,
				bodyVect,
				1f,
				effect,
			0);

			//index 12, back arm
			spriteBatch.Draw(
				bodyTexture,
				vector,
				compBackShoulderFrame,
				bodyColor,
				0,
				bodyVect,
				1f,
				effect,
			0);
			spriteBatch.Draw(
				bodyTexture,
				backArmPosition,
				compBackArmFrame,
				bodyColor,
				0,
				backArmOrigin,
				1f,
				effect,
			0);

			//index 13, legs
			spriteBatch.Draw(
				legTexture,
				new Vector2((int)(NPC.position.X - (legFrame.Width / 2) + (NPC.width / 2)), (int)(NPC.position.Y + NPC.height - legFrame.Height + 4f)) + legVect - screenPos,
				legFrame,
				legColor,
				0,
				legVect,
				1f,
				effect,
			0);

			//index 17, torso
			spriteBatch.Draw(
				bodyTexture,
				vector,
				compTorsoFrame,
				bodyColor,
				0,
				bodyVect,
				1f,
				effect,
			0);

			//index 21, head
			Rectangle headFrame = this.bodyFrame;
			headFrame.Height -= 4;
			spriteBatch.Draw(
				headTexture,
				new Vector2((int)(position.X - (bodyFrame.Width / 2) + (NPC.width / 2)), (int)(position.Y + NPC.height - bodyFrame.Height + 4f)) + headVect - screenPos,
				headFrame,
				headColor,
				0,
				headVect,
				1f,
				effect,
			0);

			if (heldItemType >= 0) {
				Main.instance.LoadItem(heldItemType);
				Texture2D value = TextureAssets.Item[heldItemType].Value;
				Rectangle itemDrawFrame = value.Bounds;
				if (Main.itemAnimations[heldItemType] != null) {
					itemDrawFrame = Main.itemAnimations[heldItemType].GetFrame(value);
				}
				Item heldItem = ContentSamples.ItemsByType[heldItemType];
				Vector2 vector9 = new(0f, itemDrawFrame.Height / 2);
				Vector2 vector10 = new(10, itemDrawFrame.Height * 0.5f);
				ItemLoader.HoldoutOffset(1, heldItemType, ref vector10);
				int num12 = (int)vector10.X;
				vector9.Y = vector10.Y;
				Vector2 origin7 = new(-num12, itemDrawFrame.Height / 2);
				if (NPC.direction == -1) {
					origin7 = new Vector2(itemDrawFrame.Width + num12, itemDrawFrame.Height / 2);
				}
				spriteBatch.Draw(
					value,
					new Vector2((int)(position.X + NPC.width * 0.5f - (NPC.direction * 2) + vector9.X), (int)(NPC.Center.Y - itemDrawFrame.Height * 0.5f + vector9.Y)) - screenPos,
					itemDrawFrame,
					heldItem.GetAlpha(bodyColor),
					itemRotation,
					origin7,
					heldItem.scale,
					effect,
				0);
			}

			//index 28, front arm
			if (shoulderDrawPosition == 0) {
				spriteBatch.Draw(
				bodyTexture,
				frontShoulderPosition,
				compFrontShoulderFrame,
				bodyColor,
				0,
				bodyVect,
				1f,
				effect,
				0);
			}
			spriteBatch.Draw(
			bodyTexture,
			frontArmPosition,
			compFrontArmFrame,
			bodyColor,
			0,
			frontArmOrigin,
			1f,
			effect,
			0);
			if (shoulderDrawPosition == 1) {
				spriteBatch.Draw(
				bodyTexture,
				frontShoulderPosition,
				compFrontShoulderFrame,
				bodyColor,
				0,
				bodyVect,
				1f,
					effect,
				0);
			}
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
			}
		}
		public override void OnKill() {
			NPC transformation = NPC.NewNPCDirect(NPC.GetSource_Death(), NPC.Center, ModContent.NPCType<Lost_Diver_Transformation>(), ai1: NPC.direction);
			transformation.Center = NPC.Center;
			transformation.velocity = NPC.velocity;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)NPC.aiAction);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.aiAction = reader.ReadByte();
		}
		public enum AIModes {
			Idle,
			Boat_Rocker,
			Depth_Charge,
			Torpedo_Tube,
			Mildew_Whip,
		}
	}
	public class LD_Music_Scene_Effect : BossMusicSceneEffect {
		public override int Music => Origins.Music.LostDiver;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			npcIDs[ModContent.NPCType<Lost_Diver_Spawn>()] = true;
			npcIDs[ModContent.NPCType<Lost_Diver>()] = true;
			npcIDs[ModContent.NPCType<Rock_Bottom>()] = true;
		}
		public override bool CanNPCActivateSceneEffect(NPC npc) => npc.ModNPC is not Rock_Bottom || npc.ai[1] != 0;
	}
	public class Boss_Bar_LD : ModBossBar {
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			if (Lost_Diver.HeadID == -1) return null;
			return TextureAssets.NpcHeadBoss[Lost_Diver.HeadID];
		}
		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
			Boss_Bar_LD_Transformation.lastLostDiverMaxHealth = drawParams.LifeMax;
			BossBarLoader.DrawFancyBar_TML(spriteBatch, drawParams);
			return false;
		}
	}
}
