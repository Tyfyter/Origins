using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Graphics;
using Origins.Items.Weapons.Magic;
using Origins.World.BiomeData;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled.Boss {
	public class DA_Body_Part : ModNPC, IOutlineDrawer, IDefiledEnemy, IOnHitByNPC {
		const string bodyPartsPath = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Split_";
		static PegasusLib.AutoLoadingAsset<Texture2D> torsoPath = bodyPartsPath + "Torso";
		static PegasusLib.AutoLoadingAsset<Texture2D> armPath = bodyPartsPath + "Arm";
		static PegasusLib.AutoLoadingAsset<Texture2D> leg1Path = bodyPartsPath + "Leg1";
		static PegasusLib.AutoLoadingAsset<Texture2D> leg2Path = bodyPartsPath + "Leg2";
		static PegasusLib.AutoLoadingAsset<Texture2D> shoulderPath = bodyPartsPath + "Shoulder";
		static PegasusLib.AutoLoadingAsset<Texture2D> torsoGlowPath = bodyPartsPath + "Torso_Glow";

		static PegasusLib.AutoLoadingAsset<Texture2D> armGlowPath = bodyPartsPath + "Arm_Glow";
		static PegasusLib.AutoLoadingAsset<Texture2D> leg1GlowPath = bodyPartsPath + "Leg1_Glow";
		static PegasusLib.AutoLoadingAsset<Texture2D> leg2GlowPath = bodyPartsPath + "Leg2_Glow";
		static PegasusLib.AutoLoadingAsset<Texture2D> shoulderGlowPath = bodyPartsPath + "Shoulder_Glow";
		static PegasusLib.AutoLoadingAsset<Texture2D> RightArmPath = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Right_Arm";
		static PegasusLib.AutoLoadingAsset<Texture2D> RightArmGlowPath = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Right_Arm_Glow";
		static PegasusLib.AutoLoadingAsset<Texture2D> LeftArmPath = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Left_Arm";
		static PegasusLib.AutoLoadingAsset<Texture2D> LeftArmGlowPath = "Origins/NPCs/Defiled/Boss/Defiled_Amalgamation_Left_Arm_Glow";
		Part PartType => (Part)(int)NPC.ai[0];
		Defiled_Amalgamation DA;
		int maxFrames = -1;
		int currentFrame = 0;
		int frameHeight = 0;
		public override string Texture => bodyPartsPath + "Torso";
		public AssimilationAmount? Assimilation => 0.03f;
		public static int DifficultyMult => Main.masterMode ? 3 : (Main.expertMode ? 2 : 1);
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			Defiled_Amalgamation.Minions.Add(Type);
		}
		public override void SetDefaults() {
			NPC.friendly = false;
			NPC.aiStyle = -1;
			NPC.width = NPC.height = 32;
			NPC.lifeMax = 1000;
			NPC.noGravity = true;
			NPC.knockBackResist = 0L;
			NPC.noTileCollide = true;
			NPC.damage = 0;
			NPC.defense = 12;
			NPC.HitSound = Origins.Sounds.DefiledHurt.WithPitchRange(0f, 0.25f);
			NPC.DeathSound = Origins.Sounds.DefiledKill.WithPitchRange(-1f, -0.75f);
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			ContentSamples.NpcBestiaryRarityStars[Type] = 3;
		}
		public enum Part : byte {
			torso = 0,
			arm = 1,
			leg1 = 2,
			leg2 = 3,
			shoulder = 4
		}

		public void SetupPart() {
			switch (PartType) {
				// only the torso can take damage
				case Part.torso:
				maxFrames = 4;
				NPC.frame = new Rectangle(0, 0, 80, 60);
				frameHeight = 60;
				break;

				case Part.arm:
				maxFrames = 4;
				NPC.frame = new Rectangle(0, 0, 20, 50);
				frameHeight = 50;
				break;

				case Part.leg1:
				maxFrames = 3;
				NPC.frame = new Rectangle(0, 0, 40, 90);
				frameHeight = 90;
				NPC.noTileCollide = false;
				NPC.noGravity = false;

				break;

				case Part.leg2:
				maxFrames = 3;
				NPC.frame = new Rectangle(0, 0, 38, 380 / 5);
				frameHeight = 380 / 5;
				NPC.noTileCollide = false;
				NPC.noGravity = false;
				break;

				case Part.shoulder:
				maxFrames = 5;
				NPC.frame = new Rectangle(0, 0, 40, 40);
				frameHeight = 46;
				break;
			}

			NPC.GivenName = Language.GetOrRegister("Mods.Origins.NPCs.DA_Body_Part.DisplayNameFormattable").Format(Language.GetOrRegister("Mods.Origins.NPCs.DA_Body_Part." + PartType));
		}
		public override void OnSpawn(IEntitySource source) {
			if (Main.npc[(int)NPC.ai[1]].ModNPC is Defiled_Amalgamation DA) {
				this.DA = DA;
				NPC.realLife = DA.NPC.whoAmI;
			}
			SetupPart();
		}
		ref float Timer => ref NPC.ai[3];
		public DrawData[] OutlineDrawDatas => [
			new(RightArmPath, NPC.Center, new Rectangle(0, (384 / 4) * currentFrame, 30, ((384 / 4))), Color.White, NPC.rotation - MathHelper.PiOver2, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0)
		];

		public int OutlineSteps => 4;

		public float OutlineOffset => 15 * ((Timer - 60f) / 60f);

		public float Mana { get => 1; set { } }

		public override void AI() {
			if (DA is null) {
				NPC.active = false;
				return;
			}
			NPC.lifeMax = DA.NPC.lifeMax;
			NPC.life = DA.NPC.life;
			NPC.target = Main.maxPlayers;
			NPC.TargetClosest(true);
			if (!NPC.HasValidTarget) {
				NPC.position.Y += 10;
				NPC.EncourageDespawn(2);
				return;
			}

			if (++NPC.frameCounter % 7 == 0)
				if (++currentFrame >= maxFrames - 1)
					currentFrame = 0;

			NPC.GivenName = Language.GetOrRegister("Mods.Origins.NPCs.DA_Body_Part.DisplayNameFormattable").Format(Language.GetOrRegister("Mods.Origins.NPCs.DA_Body_Part." + PartType));

			NPC.ai[2]++;
			if (NPC.ai[2] < 200) {
				NPC.Center = Vector2.Lerp(NPC.Center, NPC.targetRect.Center() +
					new Vector2(MathF.Sin(DA.time * 0.1f + (MathHelper.TwoPi * 0.2f * NPC.ai[0])) * 400,
					MathF.Cos(DA.time * 0.1f + (MathHelper.TwoPi * 0.2f * NPC.ai[0])) * 200), NPC.ai[2] / 160f);
				NPC.velocity = Vector2.Zero;
				NPC.spriteDirection = NPC.targetRect.Center().X > NPC.Center.X ? -1 : 1;

				return;
			}

			NPC.damage = DA.NPC.damage / 2;

			//regroup
			if (DA.NPC.ai[0] is not Defiled_Amalgamation.state_split_amalgamation_start and not Defiled_Amalgamation.state_split_amalgamation_active) {
				NPC.active = false;
				return;
			}
			if (DA.NPC.ai[1] >= Defiled_Amalgamation.SplitDuration) {
				float progress = (DA.NPC.ai[1] - Defiled_Amalgamation.SplitDuration) / Defiled_Amalgamation.SplitRegroupDuration;
				NPC.Center = Vector2.Lerp(NPC.Center, DA.NPC.Center, progress);
				NPC.damage = 0;

				if (progress >= 1) {
					NPC.active = false;
				}

				return;
			}

			NPC.spriteDirection = NPC.velocity.X > 0 ? -1 : NPC.velocity.X < 0 ? 1 : NPC.spriteDirection;

			switch (PartType) {

				case Part.leg1:
				case Part.leg2:
				LegsAI();
				break;
				case Part.arm:
				ArmAI();
				break;
				case Part.shoulder:
				ShoulderAI();
				break;
				case Part.torso:
				TorsoAI();
				break;
			}

			switch (PartType) {
				default:
				NPC.width = NPC.frame.Height;
				NPC.height = NPC.frame.Height;
				break;

				case Part.leg1:
				NPC.width = 40;
				NPC.height = 75;
				NPC.frame = new Rectangle(0, 0, 40, 90);
				break;

				case Part.leg2:
				NPC.width = 50;
				NPC.height = 76;
				NPC.frame = new Rectangle(0, 0, 38, 380 / 5);
				break;
			}
		}
		public void TorsoAI() {
			NPC.velocity = NPC.Center.DirectionTo(NPC.targetRect.Center()) * 3;
		}

		public void ShoulderAI() {
			Timer++;
			NPC.velocity *= 0.97f;
			if (Timer < 120) {

				NPC.rotation += MathHelper.Lerp(0, 1f, Timer / 120);
			}

			if (Timer == 120) {
				NPC.velocity = NPC.Center.DirectionTo(NPC.targetRect.Center()) * 20;
				NPC.rotation = NPC.velocity.ToRotation();
			}

			if (Timer == 160) {
				NPC.velocity = Vector2.Zero;
				NPC.ai[3] = 0;
			}
		}
		bool Charging => NPC.aiAction == 1;
		public void ArmAI() {
			Timer++;
			NPC.velocity.Y = MathF.Sin(Timer * 0.05f) * 2;
			Point coords = Utils.ToTileCoordinates(NPC.Center);
			int tileType = WorldGen.TileType(coords.X, coords.Y);
			if (tileType != -1 && Main.tileSolid[tileType] && !Main.tileSolidTop[tileType]) {

				NPC.Center = Vector2.Lerp(NPC.Center, NPC.targetRect.Center() - new Vector2(0, 200), 0.2f);
			}
			void PickNextAction() {
				Timer = 0;
				NPC.aiAction = Main.rand.Next(3);
				NPC.netUpdate = true;
			}
			switch (NPC.aiAction) {
				case 0: {
					if (Timer >= 60) {
						SoundEngine.PlaySound(Origins.Sounds.RivenBass.WithPitch(-1f).WithVolume(0.4f), NPC.Center);
						SoundEngine.PlaySound(Origins.Sounds.EnergyRipple.WithVolume(1.4f), NPC.Center);
						if (Main.netMode != NetmodeID.MultiplayerClient) {
							//center projectile removed for classic mode to make it sometimes better to stay still to dodge arm attacks
							if (Main.expertMode) Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.rotation.ToRotationVector2() * 15, ModContent.ProjectileType<DA_Arc_Bolt>(), (NPC.damage + (5 * DifficultyMult)) / 2, 0, -1, -1, -1, -1);
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.rotation.ToRotationVector2().RotatedBy(0.1f) * 15, ModContent.ProjectileType<DA_Arc_Bolt>(), (NPC.damage + (5 * DifficultyMult)) / 2, 0, -1, -1, -1, -1);
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.rotation.ToRotationVector2().RotatedBy(-0.1f) * 15, ModContent.ProjectileType<DA_Arc_Bolt>(), (NPC.damage + (5 * DifficultyMult)) / 2, 0, -1, -1, -1, -1);
							PickNextAction();
						}
					}
					NPC.Center = Vector2.Lerp(NPC.Center - new Vector2(5, 0).RotatedBy(NPC.rotation), NPC.Center + new Vector2(10, 0).RotatedBy(NPC.rotation), Timer / 60f);
					NPC.rotation = NPC.rotation.AngleTowards(NPC.targetRect.Center().DirectionFrom(NPC.Center).ToRotation(), 0.02f);
					break;
				}
				case 1: {
					Vector2 diff = NPC.targetRect.Center() - NPC.Center;
					float dist = diff.Length();
					// A constant rate of turning covers a lot more distance at longer ranges, this compensates for that
					float distFactor = dist == 0 ? 1 : Math.Min((16 * 20) / dist, 1);
					// Makes the turning slow down the closer it gets to firing
					float slowdownFactor = 1 - MathF.Pow(((Timer - 60) / 60), 2);
					NPC.rotation = NPC.rotation.AngleTowards(diff.ToRotation(), 0.05f * distFactor * slowdownFactor);

					if (Timer >= 120 && Main.netMode != NetmodeID.MultiplayerClient) {
						// DA_Flan.tick_motion is used here because it's used to set the max length 
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.rotation.ToRotationVector2() * DA_Flan.tick_motion, ModContent.ProjectileType<DA_Flan>(), (NPC.damage + (5 * DifficultyMult)) / 2, 0, -1, 0, 0, 0);
						PickNextAction();
					}
					break;
				}
				case 2: {
					if (Timer >= 60) {
						SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(-0.6f, -0.4f), NPC.Center);
						if (Main.netMode != NetmodeID.MultiplayerClient) {
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.rotation.ToRotationVector2() * 25, ModContent.ProjectileType<Low_Signal_Hostile>(), NPC.damage, 0, -1, 0, 0, 0);
							PickNextAction();
						}
					}
					NPC.Center = Vector2.Lerp(NPC.Center - new Vector2(5, 0).RotatedBy(NPC.rotation), NPC.Center + new Vector2(10, 0).RotatedBy(NPC.rotation), Timer / 60f);
					NPC.rotation = NPC.rotation.AngleTowards(NPC.targetRect.Center().DirectionFrom(NPC.Center).ToRotation(), 0.02f);
					break;
				}
			}
		}
		public void LegsAI() {
			CheckTrappedCollision();
			if (NPC.noTileCollide) {
				NPC.Center += (NPC.targetRect.Center() - NPC.Center).WithMaxLength(6);
				NPC.GravityMultiplier *= 0;
				return;
			}

			if (NPC.collideY || NPC.velocity.Y == 0) {
				NPC.velocity.X = 0;
				Timer++;
			}

			if (Timer >= 20 && (NPC.collideY || NPC.collideX || NPC.velocity.X == 0)) {
				SoundEngine.PlaySound(SoundID.Item174.WithPitchRange(0.5f, 0.75f), NPC.Center);
				SoundEngine.PlaySound(SoundID.NPCHit38.WithPitchRange(-2f, -1.75f).WithVolume(0.1f), NPC.Center);
				Timer = 0;
				float highSpeed = 7 + ContentExtensions.DifficultyDamageMultiplier * 2;
				float lowSpeed = 4 + ContentExtensions.DifficultyDamageMultiplier;
				if (PartType == Part.leg1) {
					NPC.velocity = NPC.targetRect.X > NPC.Center.X ? new Vector2(highSpeed, -lowSpeed) : new Vector2(-highSpeed, -lowSpeed);
				} else {
					highSpeed += 2;
					NPC.velocity = NPC.targetRect.X > NPC.Center.X ? new Vector2(lowSpeed, -highSpeed) : new Vector2(-lowSpeed, -highSpeed);
				}
			}

			NPC.rotation = NPC.velocity.Y * NPC.spriteDirection * 0.05f;
		}
		int trappedTime = 0;
		public void CheckTrappedCollision() {
			if (NPC.position.Y > Main.UnderworldLayer * 16 && NPC.HasValidTarget) {
				NPC.noTileCollide = false;
				trappedTime = 60;
				return;
			}
			Rectangle hitbox = NPC.Hitbox;
			int inflateAmount = NPC.noTileCollide.ToDirectionInt() * 4;
			hitbox.Inflate(inflateAmount, inflateAmount);
			if (!hitbox.OverlapsAnyTiles()) {
				if (trappedTime <= 0) NPC.noTileCollide = false;
			} else {
				trappedTime = 60;
			}
			if (NPC.collideX || NPC.collideY) {
				trappedTime += 2;
				if (trappedTime > 60) {
					NPC.noTileCollide = true;
					NPC.collideX = NPC.collideY = false;
				}
			} else if (trappedTime > 0) {
				trappedTime--;
			}
		}
		/*public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (NPC.ai[2] < 200 || NPC.ai[2] >= Defiled_Amalgamation.SplitDuration) {
				modifiers.FinalDamage /= projectile.penetrate > 0 ? Math.Min(projectile.maxPenetrate, 5f) : 5f;
			}
		}*/
		public override void FindFrame(int frameHeight) {
			if (PartType == Part.leg1 || PartType == Part.leg2) {
				if (NPC.velocity.Y < 0)
					currentFrame = 3;
				else if (NPC.velocity.Y > 0)
					currentFrame = 4;
			}
			NPC.frame.Y = currentFrame * ((this.frameHeight * (maxFrames - 1)) / (maxFrames - 1));
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = torsoPath;
			Texture2D glowTexture = torsoGlowPath;
			switch (PartType) {

				case Part.leg1:
				texture = leg1Path;
				glowTexture = leg1GlowPath;
				break;
				case Part.leg2:
				texture = leg2Path;
				glowTexture = leg2GlowPath;

				break;
				case Part.shoulder:
				texture = shoulderPath;
				glowTexture = shoulderGlowPath;
				// assumes that the maximum amount of frames is same as the wings 
				spriteBatch.Draw(LeftArmPath, NPC.Center - Main.screenPosition, new Rectangle(0, (384 / 4) * currentFrame, 30, ((384 / 4))), drawColor, NPC.rotation - MathHelper.PiOver2, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(LeftArmGlowPath, NPC.Center - Main.screenPosition, new Rectangle(0, (384 / 4) * currentFrame, 30, ((384 / 4))), Color.White, NPC.rotation - MathHelper.PiOver2, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				break;
				case Part.arm:
				texture = armPath;
				glowTexture = armGlowPath;
				if (Timer > 60 && NPC.ai[2] < Defiled_Amalgamation.SplitDuration)
					this.DrawOutline();
				// assumes that the maximum amount of frames is same as the wings 
				spriteBatch.Draw(RightArmPath, NPC.Center - Main.screenPosition, new Rectangle(0, (384 / 4) * currentFrame, 30, ((384 / 4))), drawColor, NPC.rotation - MathHelper.PiOver2, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				spriteBatch.Draw(RightArmGlowPath, NPC.Center - Main.screenPosition, new Rectangle(0, (384 / 4) * currentFrame, 30, ((384 / 4))), Color.White, NPC.rotation - MathHelper.PiOver2, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
				break;
			}

			spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
			spriteBatch.Draw(glowTexture, NPC.Center - Main.screenPosition, NPC.frame, Color.White, NPC.rotation, NPC.frame.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

			if (Charging && Timer > 60 && NPC.ai[2] < Defiled_Amalgamation.SplitDuration) {
				Defiled_Spike_Indicator.Draw(
					[NPC.Center, NPC.Center + NPC.rotation.ToRotationVector2() * DA_Flan.tick_motion * ProjectileID.Sets.TrailCacheLength[ModContent.ProjectileType<DA_Flan>()]],
					[NPC.rotation, NPC.rotation + MathHelper.Pi],
					MathHelper.Lerp(15, 1, (Timer - 60f) / 60f),
					0.02f,
					0.5f
				);
			}

			return false;
		}
		public Color? SetOutlineColor(float progress) => Color.Lerp(Color.Green, Color.Purple, progress);
		#region i-frame sharing
		public override bool? CanBeHitByProjectile(Projectile projectile) {
			if (projectile.usesLocalNPCImmunity) {
				if (projectile.localNPCImmunity[NPC.realLife] == 0) return null;
				return false;
			}
			if (projectile.usesIDStaticNPCImmunity) {
				if (Projectile.perIDStaticNPCImmunity[projectile.type][NPC.realLife] < Main.GameUpdateCount) return null;
				return false;
			}
			if (projectile.penetrate != 1) {
				if (Main.npc[NPC.realLife].immune[projectile.owner] <= 0) return null;
				return false;
			}
			return null;
		}
		public override bool? CanBeHitByItem(Player player, Item item) {
			if (Main.npc[NPC.realLife].immune[player.whoAmI] <= 0) return null;
			return false;
		}
		public override bool CanBeHitByNPC(NPC attacker) {
			if (Main.npc[NPC.realLife].immune[Main.maxPlayers] <= 0) return true;
			return false;
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			if (projectile.usesLocalNPCImmunity) {
				projectile.localNPCImmunity[NPC.realLife] = projectile.localNPCImmunity[NPC.whoAmI];
			} else if (projectile.usesIDStaticNPCImmunity) {
				Projectile.perIDStaticNPCImmunity[projectile.type][NPC.realLife] = Projectile.perIDStaticNPCImmunity[projectile.type][NPC.whoAmI];
			} else if (projectile.penetrate != 1) {
				Main.npc[NPC.realLife].immune[projectile.owner] = NPC.immune[projectile.owner];
			}
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			Main.npc[NPC.realLife].immune[player.whoAmI] = NPC.immune[player.whoAmI];
		}
		public void OnHitByNPC(NPC attacker, NPC.HitInfo hit) {
			Main.npc[NPC.realLife].immune[Main.maxPlayers] = NPC.immune[Main.maxPlayers];
		}
		#endregion
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)NPC.aiAction);
			writer.Write((short)NPC.realLife);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.aiAction = reader.ReadByte();
			NPC.realLife = reader.ReadInt16();
		}
	}
	public class DA_Arc_Bolt : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Infusion_P";

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 15;
			ProjectileID.Sets.TrailingMode[Type] = 3;
		}
		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 300;
			Projectile.tileCollide = false;
		}

		public override void AI() {
			if (Projectile.ai[1] != -1)
				Projectile.Center = Projectile.Center.RotatedBy((MathHelper.PiOver2 * MathF.Sign(Projectile.ai[0])) / (60f), Main.npc[(int)Projectile.ai[1]].Center);

			Projectile.rotation = Projectile.oldPosition.DirectionTo(Projectile.Center).ToRotation() + MathHelper.PiOver2;
		}

		public override bool PreDraw(ref Color lightColor) {
			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return false;
			}
			Origins.shaderOroboros.Capture();
			DrawDefiledBolt(Projectile.oldPos, Projectile.oldRot, Projectile.timeLeft / 300f);
			Origins.shaderOroboros.DrawContents(renderTarget, Color.White, Main.GameViewMatrix.EffectMatrix);
			Origins.shaderOroboros.Reset(default);
			Vector2 center = renderTarget.Size() * 0.5f;

			TangelaVisual.DrawAntiGray(new(renderTarget,
				center,
				null,
				Color.White,
				0,
				center,
				Vector2.One / Main.GameViewMatrix.Zoom,
				SpriteEffects.None
			));

			return false;
		}
		private static readonly VertexStrip vertexStrip = new();
		public void DrawDefiledBolt(Vector2[] positions, float[] rotations, float progress) {
			MiscShaderData shader = GameShaders.Misc["Origins:DefiledLaser2"];
			shader.UseColor(Color.Black);
			shader.UseShaderSpecificData(new Vector4(progress, 0, 0, 0));
			Main.graphics.GraphicsDevice.Textures[3] = TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion].Value;
			Main.graphics.GraphicsDevice.SamplerStates[3] = SamplerState.LinearWrap;
			shader.Apply();
			Vector2 visualOffset = Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.Zero) * 8;
			vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, (p) => Color.White, (p) => MathHelper.Lerp(MathHelper.Lerp(80, 0, 1f - p), 1, MathF.Sqrt(p)), visualOffset - Main.screenPosition, false);
			vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}
		public override void OnKill(int timeLeft) {
			if (renderTarget is not null) {
				TangelaVisual.SendRenderTargetForDisposal(ref renderTarget);
				Main.OnResolutionChanged -= Resize;
			}
		}
		internal RenderTarget2D renderTarget;
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			renderTarget.Dispose();
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
	public class DA_Flan : ModProjectile, ITangelaHaver {

		public const int tick_motion = 8;
		public override string Texture => "Origins/Projectiles/Weapons/Seam_Beam_P";
		public override void SetStaticDefaults() {
			const int max_length = 1200;
			ProjectileID.Sets.TrailCacheLength[Type] = max_length / tick_motion;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = max_length + 16;
			Origins.HomingEffectivenessMultiplier[Type] = 25f;
			OriginsSets.Projectiles.DuplicationAIVariableResets[Type].second = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 25;
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.Inflate(2, 2);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			for (int i = 1; i < Projectile.ai[1] && i < Projectile.oldPos.Length; i++) {
				Vector2 pos = Projectile.oldPos[^i];
				if (pos == default) {
					break;
				} else if (OriginExtensions.Recentered(projHitbox, pos).Intersects(targetHitbox)) {
					return true;
				}
			}
			return null;
		}
		protected Vector2? target = null;
		protected int startupDelay = 2;
		protected float randomArcing = 0.3f;
		public override void AI() {
			SoundEngine.PlaySound(Origins.Sounds.defiledKillAF.WithPitchRange(-1f, -0.2f).WithVolume(0.2f), Projectile.Center);
			SoundEngine.PlaySound(SoundID.Item60.WithPitchRange(-1f, -0.2f).WithVolume(0.6f), Projectile.Center);

			target ??= Projectile.Center + Projectile.velocity * 25 * (10 - Projectile.ai[2]);
			if (Projectile.numUpdates == -1 && ++Projectile.ai[2] >= 20) {
				Projectile.Kill();
				return;
			}
			if (Projectile.ai[0] != 1) {
				if ((Projectile.numUpdates + 1) % 5 == 0 && startupDelay <= 0) {
					float speed = Projectile.velocity.Length();
					if (speed != 0) Projectile.velocity = (target.Value - Projectile.Center).SafeNormalize(Projectile.velocity / speed).RotatedByRandom(randomArcing) * speed;
				}
				if (startupDelay > 0) {

					startupDelay--;
				} else {
					if (++Projectile.ai[1] > ProjectileID.Sets.TrailCacheLength[Type]) {
						StopMovement();
					} else {
						int index = (int)Projectile.ai[1];
						Projectile.oldPos[^index] = Projectile.Center;
						Projectile.oldRot[^index] = Projectile.velocity.ToRotation();
					}
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Vector2 direction = oldVelocity.SafeNormalize(default);
			if (direction != default) {
				float[] samples = new float[3];
				Collision.LaserScan(
					Projectile.Center,
					direction,
					5,
					32,
					samples
				);
				if (samples.Average() > tick_motion * 0.5f) {
					Projectile.Center += direction * tick_motion;
					int index = Math.Min((int)++Projectile.ai[1], Projectile.oldPos.Length);
					Projectile.oldPos[^index] = Projectile.Center;
					Projectile.oldRot[^index] = oldVelocity.ToRotation();
				}
			}
			StopMovement();
			return false;
		}
		protected void StopMovement() {
			Projectile.velocity = Vector2.Zero;
			Projectile.ai[0] = 1;
			Projectile.extraUpdates = 0;
		}
		public int? TangelaSeed { get; set; }
		public override bool PreDraw(ref Color lightColor) {
			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return false;
			}
			Origins.shaderOroboros.Capture();
			Nerve_Flan_P_Drawer.Draw(Projectile);
			Origins.shaderOroboros.DrawContents(renderTarget, Color.White, Main.GameViewMatrix.EffectMatrix);
			Origins.shaderOroboros.Reset(default);
			Vector2 center = renderTarget.Size() * 0.5f;
			TangelaVisual.DrawTangela(
				this,
				renderTarget,
				center,
				null,
				0,
				center,
				Vector2.One / Main.GameViewMatrix.Zoom,
				SpriteEffects.None,
				Main.screenPosition
			);
			return false;
		}
		public override void OnKill(int timeLeft) {
			if (renderTarget is not null) {
				TangelaVisual.SendRenderTargetForDisposal(ref renderTarget);
				Main.OnResolutionChanged -= Resize;
			}
		}
		internal RenderTarget2D renderTarget;
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			renderTarget.Dispose();
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
}