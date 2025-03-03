using CalamityMod.Graphics.Renderers;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Weapons.Crossmod;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.Brine.Boss {
	[AutoloadBossHead]
	public class Lost_Diver : Brine_Pool_NPC {
		internal static IItemDropRule normalDropRule;
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
		}
		public override void Unload() {
			normalDropRule = null;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.damage = 14;
			NPC.lifeMax = 25000;
			NPC.defense = 26;
			NPC.aiStyle = 0;
			NPC.width = 20;
			NPC.height = 42;
			NPC.knockBackResist = 0f;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-0.8f);
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.value = 0;//Item.buyPrice(gold: 5);
		}
		public AIModes AIMode {
			get => (AIModes)NPC.aiAction;
			set => NPC.aiAction = (int)value;
		}
		public int HeldProjectile {
			get => (int)NPC.ai[3];
			set => NPC.ai[3] = value;
		}
		public AIModes LastMode {
			get => (AIModes)(int)NPC.localAI[3];
			set => NPC.localAI[3] = (int)value;
		}
		public override bool CanTargetPlayer(Player player) => true;
		public override bool CheckTargetLOS(Vector2 target) => !NPC.wet || base.CheckTargetLOS(target);
		public override float RippleTargetWeight(float magnitude, float distance) => 0;
		public override bool? CanFallThroughPlatforms() => NPC.wet || NPC.targetRect.Bottom > NPC.BottomLeft.Y;
		public override void AI() {
			float difficultyMult = ContentExtensions.DifficultyDamageMultiplier;
			DoTargeting();
			Vector2 direction = NPC.DirectionTo(TargetPos);
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
			} else if (NPC.velocity.Y != 0f) {
				bodyFrame.Y = bodyFrame.Height * 5;
			} else {
				if (NPC.wet) NPC.frameCounter = 0;
				bodyFrame.Y = 0;
			}
			if (NPC.wet) {

			} else {
				swimTime = 0;
				if (NPC.collideY && Math.Abs(NPC.velocity.X) > 2) {
					NPC.velocity.X *= 0.98f;
				}
				if (Math.Abs(direction.X) >= 0.1f) {
					if (Math.Abs(NPC.velocity.Y) < 0.01f && Math.Sign(direction.X) != -Math.Sign(NPC.velocity.X)) {
						int xDirection = Math.Sign(direction.X);
						NPC.frameCounter += 1;
						while (NPC.frameCounter > 8.0) {
							NPC.frameCounter -= 8.0;
							legFrame.Y += legFrame.Height;
						}
						if (legFrame.Y < legFrame.Height * 7) {
							legFrame.Y = legFrame.Height * 19;
						} else if (legFrame.Y > legFrame.Height * 19) {
							legFrame.Y = legFrame.Height * 7;
						}
						if (legFrame.Y / legFrame.Height is 9 or 17) NPC.velocity.X = xDirection * 0.01f;
						else NPC.velocity.X = xDirection * 0.7f;
					}
				}
				NPC.velocity.Y += 0.4f;
				Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
				Collision.StepDown(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			}
			bool useItemRotFrame = false;
			heldItemType = -1;
			startMode:
			switch (AIMode) {
				default:
				case AIModes.Idle:
				NPC.ai[0] += 0.75f + (0.25f * difficultyMult);
				if (NPC.ai[0] > 120 && NPC.HasValidTarget) {
					HeldProjectile = -1;
					WeightedRandom<AIModes> rand = new(Main.rand);
					void AddMode(AIModes mode, double weight) {
						if (mode == LastMode) weight *= 0.3f;
						rand.Add(mode, weight);
					}
					AddMode(AIModes.Idle, 0);
					AddMode(AIModes.Depth_Charge, 1);
					AddMode(AIModes.Torpedo_Tube, 1);
					AddMode(AIModes.Mildew_Whip, NPC.life > NPC.lifeMax / 2 ? 0 : 1);
					AIMode = rand.Get();
					LastMode = AIMode;
					NPC.ai[0] = 0;
					NPC.netUpdate = true;
					goto startMode;
				}
				break;
				case AIModes.Depth_Charge:
				if (HeldProjectile < 0) {
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						HeldProjectile = Projectile.NewProjectileDirect(
							NPC.GetSource_FromAI(),
							NPC.Center,
							direction.RotatedBy(NPC.direction * -0.5f) * 8,
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
						Projectile.NewProjectileDirect(
							NPC.GetSource_FromAI(),
							NPC.Center,
							direction * 8,
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
					NPC.ai[0] = 0;
					break;
				}
				heldItemType = ModContent.ItemType<Torpedo_Tube>();
				useItemRotFrame = true;
				NPC.ai[0]++;
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
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			normalDropRule = ItemDropRule.OneFromOptionsNotScalingWithLuck(1,
				ModContent.ItemType<Boat_Rocker>(),
				ModContent.ItemType<Depth_Charge>(),
				ModContent.ItemType<Torpedo_Tube>(),
				ModContent.ItemType<Mildew_Whip>(),
				Watered_Down_Keytar.ID
			).WithOnSuccess(ItemDropRule.Common(ModContent.ItemType<Lost_Diver_Helmet>(), 10)
			.WithOnSuccess(ItemDropRule.Common(ModContent.ItemType<Lost_Diver_Chest>()))
			.WithOnSuccess(ItemDropRule.Common(ModContent.ItemType<Lost_Diver_Greaves>()))
			);
			//These need to be in the mildew carrion's loot, it's realistically simplest to make the two separate NPCs
			/*npcLoot.Add(new DropBasedOnExpertMode(
				normalDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ModContent.ItemType<Lost_Diver_Bag>(), 1, 1, 1, null)
			));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Faith_Beads>(), 4));
			npcLoot.Add(new DropInstancedPerClient(ModContent.ItemType<Crown_Jewel>()));*/
		}
		public int heldItemType;
		public int swimTime;
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
			Color headColor = Lighting.GetColorClamped((int)NPC.Center.X / 16, (int)(NPC.position.Y + NPC.height * 0.25) / 16, Color.White);
			Color bodyColor = Lighting.GetColorClamped((int)NPC.Center.X / 16, (int)(NPC.position.Y + NPC.height * 0.75) / 16, Color.White);
			Color legColor = Lighting.GetColorClamped((int)NPC.Center.X / 16, (int)(NPC.position.Y + NPC.height * 0.75) / 16, Color.White);
			if (NPC.IsABestiaryIconDummy) {
				headColor = Color.White;
				bodyColor = Color.White;
				legColor = Color.White;
			}

			#region composite data
			Vector2 vector = new Vector2((int)(NPC.position.X - (bodyFrame.Width / 2) + (NPC.width / 2)), (int)(NPC.position.Y + NPC.height - bodyFrame.Height + 4f)) + new Vector2(bodyFrame.Width / 2, bodyFrame.Height / 2) - screenPos;
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
			Vector2 vec = NPC.position + new Vector2(NPC.width / 2, NPC.height - bodyFrame.Height / 2) + new Vector2(0f, 4f) - screenPos;
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
				new Vector2((int)(NPC.position.X - (bodyFrame.Width / 2) + (NPC.width / 2)), (int)(NPC.position.Y + NPC.height - bodyFrame.Height + 4f)) + headVect - screenPos,
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
					new Vector2((int)(NPC.position.X + NPC.width * 0.5f - (NPC.direction * 2) + vector9.X), (int)(NPC.Center.Y - itemDrawFrame.Height * 0.5f + vector9.Y)) - screenPos,
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
			Boss_Tracker.Instance.downedLostDiver = true;
			NetMessage.SendData(MessageID.WorldData);
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
