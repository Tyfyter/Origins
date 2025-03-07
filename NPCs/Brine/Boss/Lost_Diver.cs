using CalamityMod.Graphics.Renderers;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Weapons.Crossmod;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using PegasusLib;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Brine.Boss {
	[AutoloadBossHead]
	public class Lost_Diver : Brine_Pool_NPC {
		public override bool AggressivePathfinding => true;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.CantTakeLunchMoney[Type] = false;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() {
				//CustomTexturePath = "Origins/NPCs/Brine/Boss/Rock_Bottom", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(0f, 0f),
				PortraitPositionXOverride = 0f,
				PortraitPositionYOverride = 0f
			};
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 58;
			NPC.lifeMax = 25000;
			NPC.defense = 26;
			NPC.aiStyle = 0;
			NPC.width = 20;
			NPC.height = 42;
			NPC.knockBackResist = 0f;
			NPC.HitSound = SoundID.NPCHit4.WithPitchRange(-0.8f, -0.4f);
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 0;//Item.buyPrice(gold: 5);
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
		public override bool CanTargetNPC(NPC other) => other.type != NPCID.TargetDummy && NPC.WithinRange(other.Center, 16 * 400) && CanHitNPC(other);
		public override bool CanHitNPC(NPC target) => !Mildew_Creeper.FriendlyNPCTypes.Contains(target.type);
		public override bool CheckTargetLOS(Vector2 target) => !NPC.wet || base.CheckTargetLOS(target);
		public override float RippleTargetWeight(float magnitude, float distance) => 0;
		public override bool? CanFallThroughPlatforms() => NPC.wet || NPC.targetRect.Bottom > NPC.BottomLeft.Y;
		public override void AI() {
			float difficultyMult = ContentExtensions.DifficultyDamageMultiplier;
			DoTargeting();
			Vector2 targetVelocity = Vector2.Zero;
			bool enraged = false;
			if (NPC.HasPlayerTarget) {
				Player player = Main.player[NPC.target];
				enraged = NPC.wet && !player.wet;
				targetVelocity = player.velocity;
			} else if (NPC.HasNPCTarget) {
				NPC npcTarget = Main.npc[NPC.TranslatedTargetIndex];
				enraged = NPC.wet && !npcTarget.wet;
				targetVelocity = npcTarget.velocity;
			}
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
			if (NPC.wet) {
				NPC.velocity *= 0.96f;
				if (TargetPos != default) {
					const float fast_speed = 0.6f;
					float slow_speed = CanSeeTarget ? 0.2f : 0.4f;
					float fast_distance = 16 * 20;
					float slow_distance = CanSeeTarget ? 16 * 15 : 0;
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
					AddMode(AIModes.Idle, 0 + rangeFactor);
					AddMode(AIModes.Boat_Rocker, 1 + (Main.expertMode ? rangeFactor * 0.5f : 0));
					AddMode(AIModes.Depth_Charge, 1 - rangeFactor);
					AddMode(AIModes.Torpedo_Tube, 1);
					AddMode(AIModes.Mildew_Whip, NPC.life > NPC.lifeMax / 2 ? 0 : (1 - rangeFactor));
					AIMode = rand.Get();
					LastMode = AIMode;
					NPC.netUpdate = true;
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
							60,
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
							60,
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
						float speed = enraged ? 12 : 8;
						Projectile.NewProjectileDirect(
							NPC.GetSource_FromAI(),
							NPC.Center,
							direction * speed,
							ModContent.ProjectileType<Lost_Diver_Torpedo_Tube>(),
							60,
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
							60,
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
			Vector2 frontShoulderPosition = frontArmPosition;
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
				new Vector2((int)(position.X - (legFrame.Width / 2) + (NPC.width / 2)), (int)(position.Y + NPC.height - legFrame.Height + 4f)) + legVect - screenPos,
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
				Vector2 vector9 = new Vector2(0f, itemDrawFrame.Height / 2);
				Vector2 vector10 = new(10, itemDrawFrame.Height * 0.5f);
				ItemLoader.HoldoutOffset(1, heldItemType, ref vector10);
				int num12 = (int)vector10.X;
				vector9.Y = vector10.Y;
				Vector2 origin7 = new Vector2(-num12, itemDrawFrame.Height / 2);
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
			NPC.NewNPCDirect(NPC.GetSource_Death(), NPC.Center, ModContent.NPCType<Lost_Diver_Transformation>(), ai1: NPC.direction).Center = NPC.Center;
		}
		public enum AIModes {
			Idle,
			Boat_Rocker,
			Depth_Charge,
			Torpedo_Tube,
			Mildew_Whip,
		}
	}
}
