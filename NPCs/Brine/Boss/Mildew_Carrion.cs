using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.CrossMod.Thorium.Items.Weapons.Bard;
using Origins.Gores.NPCs;
using Origins.Items.Accessories;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.LootBags;
using Origins.Items.Vanity.BossMasks;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.Journal;
using Origins.Misc;
using Origins.Music;
using Origins.Tiles.BossDrops;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Creative;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.Brine.Boss {
	public class Lost_Diver_Transformation : ModNPC {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 10;
			NPCID.Sets.CantTakeLunchMoney[Type] = false;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.dontTakeDamage = true;
			NPC.lifeMax = Mildew_Carrion.GetMaxLife();
			NPC.defense = 24;
			NPC.noGravity = true;
			NPC.width = 76;
			NPC.height = 58;
			NPC.BossBar = ModContent.GetInstance<Boss_Bar_LD_Transformation>();
			NPC.npcSlots = 200;
		}
		public override bool PreAI() {
			NPC.spriteDirection = (int)-NPC.ai[1];
			NPC.velocity *= 0.92f;
			if (NPC.ai[0] < 2) {
				NPC.ai[0] += 1f / 15;
			} else {
				NPC.ai[0] += 1f / 5;
			}
			NPC.frame.Y = NPC.frame.Height * (int)NPC.ai[0];
			if (NPC.ai[0] >= Main.npcFrameCount[Type]) {
				NPC.Transform(ModContent.NPCType<Mildew_Carrion>());
			}
			return false;
		}
		public static AutoLoadingAsset<Texture2D> normalTexture = typeof(Lost_Diver_Transformation).GetDefaultTMLName();
		public static AutoLoadingAsset<Texture2D> afTexture = typeof(Lost_Diver_Transformation).GetDefaultTMLName() + "_AF";
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (OriginsModIntegrations.CheckAprilFools()) {
				TextureAssets.Npc[Type] = afTexture;
			} else {
				TextureAssets.Npc[Type] = normalTexture;
			}
		}
	}
	public class Boss_Bar_LD_Transformation : ModBossBar {
		internal static float lastLostDiverMaxHealth = -1;
		AutoLoadingAsset<Texture2D> barTextureLD = typeof(Boss_Bar_LD).GetDefaultTMLName();
		AutoLoadingAsset<Texture2D> barTextureMC = typeof(Boss_Bar_MC).GetDefaultTMLName();
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			if (Lost_Diver.HeadID == -1) return null;
			return TextureAssets.NpcHeadBoss[Lost_Diver.HeadID];
		}
		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
			float progress = npc.ai[0] / Main.npcFrameCount[npc.type];
			float inverseProgress = MathF.Pow(1 - progress, 0.5f);
			(_, Vector2 barCenter, Texture2D iconTexture, Rectangle iconFrame, Color iconColor, float life, float lifeMax, float shield, float shieldMax, float iconScale, bool showText, Vector2 textOffset) = drawParams;
			life = npc.lifeMax * MathF.Pow(progress, 0.5f);
			lifeMax = MathHelper.Lerp(lastLostDiverMaxHealth, npc.lifeMax, progress);
			Point barSize = new Point(456, 22); //Size of the bar
			Point topLeftOffset = new Point(32, 24); //Where the top left of the bar starts
			int frameCount = 6;

			Rectangle bgFrame = barTextureLD.Frame(verticalFrames: frameCount, frameY: 3);
			Color bgColor = Color.White * 0.2f;

			int scale = (int)(barSize.X * life / lifeMax);
			scale -= scale % 2;
			Rectangle barFrame = barTextureLD.Frame(verticalFrames: frameCount, frameY: 2);
			barFrame.X += topLeftOffset.X;
			barFrame.Y += topLeftOffset.Y;
			barFrame.Width = 2;
			barFrame.Height = barSize.Y;

			Rectangle tipFrame = barTextureLD.Frame(verticalFrames: frameCount, frameY: 1);
			tipFrame.X += topLeftOffset.X;
			tipFrame.Y += topLeftOffset.Y;
			tipFrame.Width = 2;
			tipFrame.Height = barSize.Y;

			Rectangle barPosition = Utils.CenteredRectangle(barCenter, barSize.ToVector2());
			Vector2 barTopLeft = barPosition.TopLeft();
			Vector2 topLeft = barTopLeft - topLeftOffset.ToVector2();

			// Background
			spriteBatch.Draw(barTextureLD, topLeft, bgFrame, bgColor * inverseProgress, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			// Bar itself
			Vector2 stretchScale = new Vector2(scale / barFrame.Width, 1f);
			Color barColor = Color.White * inverseProgress;
			spriteBatch.Draw(barTextureLD, barTopLeft, barFrame, barColor, 0f, Vector2.Zero, stretchScale, SpriteEffects.None, 0f);

			// Tip
			spriteBatch.Draw(barTextureLD, barTopLeft + new Vector2(scale - 2, 0f), tipFrame, barColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			// Frame
			Rectangle frameFrame = barTextureLD.Frame(verticalFrames: frameCount, frameY: 0);
			spriteBatch.Draw(barTextureLD, topLeft, frameFrame, barColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			barColor = Color.White * progress;
			// Background
			spriteBatch.Draw(barTextureMC, topLeft, bgFrame, bgColor * progress, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			// Bar itself
			spriteBatch.Draw(barTextureMC, barTopLeft, barFrame, barColor * progress, 0f, Vector2.Zero, stretchScale, SpriteEffects.None, 0f);

			// Tip
			spriteBatch.Draw(barTextureMC, barTopLeft + new Vector2(scale - 2, 0f), tipFrame, barColor * progress, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			// Frame
			spriteBatch.Draw(barTextureMC, topLeft, frameFrame, Color.White * progress, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			// Icon
			Vector2 iconOffset = new Vector2(4f, 20f);
			Vector2 iconSize = new Vector2(26f, 28f);
			// The vanilla method with the shieldCurrent parameter, which is used only by the lunar pillars, uses iconSize = iconFrame.Size() instead, which have a size of 26x30,
			// causing a slight vertical offset that is barely noticeable. Considering that the non-shieldCurrent method is the more general one, let's keep it like this
			// (changing that using the lunar pillar code will cause many other icons to be offset instead) --direwolf420
			Vector2 iconPos = iconOffset + iconSize / 2f;
			// iconFrame Centered around iconPos
			spriteBatch.Draw(iconTexture, topLeft + iconPos, iconFrame, iconColor, 0f, iconFrame.Size() / 2f, iconScale, SpriteEffects.None, 0f);

			BigProgressBarHelper.DrawHealthText(spriteBatch, barPosition, textOffset, life, lifeMax);
			return false;
		}
	}
	[AutoloadBossHead]
	public class Mildew_Carrion : Brine_Pool_NPC, IJournalEntrySource<Mildew_Carrion_Entry>, IMinions {
		internal static IItemDropRule normalDropRule;

		public static List<int> Minions = [];
		List<int> IMinions.BossMinions => Minions;
		public override bool AggressivePathfinding => true;

		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() {
				//CustomTexturePath = "Origins/NPCs/Brine/Boss/Rock_Bottom", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(0f, 0f),
				PortraitPositionXOverride = 0f,
				PortraitPositionYOverride = 0f
			};
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			Origins.RasterizeAdjustment[Type] = (8, 0.05f, 0.8f);
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
		}
		public override void Unload() {
			normalDropRule = null;
		}
		public static int GetMaxLife() {
			return (int)(15120 + 2160 * ContentExtensions.DifficultyDamageMultiplier);
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.boss = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 30;
			NPC.lifeMax = GetMaxLife();
			NPC.defense = 18;
			NPC.aiStyle = 0;
			NPC.width = 76;
			NPC.height = 58;
			NPC.knockBackResist = 0f;
			NPC.HitSound = SoundID.Item127;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = Item.buyPrice(gold: 5);
			NPC.npcSlots = 200;
			NPC.BossBar = ModContent.GetInstance<Boss_Bar_MC>();
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
		public override bool CanTargetNPC(NPC other) => false;//!OriginsSets.NPCs.TargetDummies[other.type] && NPC.WithinRange(other.Center, 16 * 400) && CanHitNPC(other);
		public override bool CanHitNPC(NPC target) => !Mildew_Creeper.FriendlyNPCTypes.Contains(target.type);
		public override bool CheckTargetLOS(Vector2 target) => true;
		public override float RippleTargetWeight(float magnitude, float distance) => 0;
		public override bool? CanFallThroughPlatforms() => NPC.wet || NPC.targetRect.Bottom > NPC.BottomLeft.Y;
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override bool PreAI() {
			float difficultyMult = ContentExtensions.DifficultyDamageMultiplier;
			DoTargeting();
			Vector2 differenceFromTarget = TargetPos - NPC.Center;
			float distanceFromTarget = differenceFromTarget.Length();
			Vector2 direction = differenceFromTarget / distanceFromTarget;
			int tendrilType = ModContent.NPCType<Mildew_Carrion_Tendril>();
			NPC.rotation = direction.ToRotation() + MathHelper.PiOver2;
			startMode:
			switch (AIMode) {
				default:
				case AIModes.Idle:
				NPC.ai[0] += 0.65f + (0.35f * difficultyMult);
				if (NPC.ai[0] > 150 && NPC.HasValidTarget) {
					HeldProjectile = -1;
					WeightedRandom<AIModes> rand = new(Main.rand);
					void AddMode(AIModes mode, double weight) {
						if (mode == LastMode) weight *= 0.3f;
						rand.Add(mode, weight);
					}
					AddMode(AIModes.Idle, 0);
					if (CollisionExt.CanHitRay(NPC.Center, TargetPos)) AddMode(AIModes.Spores, 1);
					int tendrilCountMax = 9 + (int)(difficultyMult);
					int tendrilCount = 0;
					foreach (NPC other in Main.ActiveNPCs) {
						if (other.type == tendrilType && ++tendrilCount >= tendrilCountMax) break;
					}
					if (tendrilCount < tendrilCountMax) AddMode(AIModes.SpawnTendril, 1 - (tendrilCount / (float)tendrilCountMax));
					AIMode = rand.Get();
					LastMode = AIMode;
					NPC.netUpdate = true;
					goto startMode;
				}
				break;
				case AIModes.Spores: {
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						for (int i = Main.rand.RandomRound(difficultyMult * 0.5f + 0.75f); i > 0; i--) {
							Vector2 sporeDirection = direction.RotatedByRandom(0.4f) * (11 + difficultyMult) * Main.rand.NextFloat(0.8f, 1f);
							Projectile.NewProjectile(
								NPC.GetSource_FromAI(),
								NPC.Center,
								sporeDirection,
								Main.rand.Next(Mildew_Carrion_Spore.types),
								30 + (int)(11 * difficultyMult),
								2
							);
						}
					}
					AIMode = AIModes.Idle;
					break;
				}
				case AIModes.SpawnTendril: {
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						for (int i = Main.rand.RandomRound(difficultyMult + 0.5f); i > 0; i--) {
							Vector2 tentacleDir = direction.RotatedBy(Main.rand.NextFloat(0f, MathHelper.TwoPi)) * 4;
							NPC.NewNPC(
								NPC.GetSource_FromAI(),
								(int)NPC.Center.X,
								(int)NPC.Center.Y,
								tendrilType,
								0,
								tentacleDir.X,
								tentacleDir.Y,
								0,
								NPC.whoAmI
							);
						}
					}
					AIMode = AIModes.Idle;
					break;
				}
			}
			if (Main.netMode != NetmodeID.MultiplayerClient && ++NPC.localAI[2] > 150) {
				int side = Main.rand.NextBool().ToDirectionInt();
				int tentacleType = ModContent.NPCType<Mildew_Carrion_Tentacle>();
				int presentTentacleCount = (int)Math.Ceiling(NPC.localAI[1] / Mildew_Carrion_Tentacle.DestructionCooldown);
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc.type == tentacleType && npc.localAI[1] == 0) presentTentacleCount++;
				}
				for (int i = Main.rand.RandomRound(1.5f + difficultyMult); i > 0; i--) {
					if (presentTentacleCount++ >= 3 + difficultyMult) break;
					Vector2 tentacleDir = direction.RotatedBy(Main.rand.NextFloat(0.2f, 0.5f) * side) * (14 + difficultyMult);
					Vector2 pos = NPC.Center;
					Rectangle hitbox = new((int)pos.X - 8, (int)pos.Y - 8, 16, 16);
					int intangibleSteps = 1;
					while (hitbox.OverlapsAnyTiles(false) && intangibleSteps < 120) {
						intangibleSteps++;
						pos += tentacleDir;
						hitbox.X = (int)pos.X - 8;
						hitbox.Y = (int)pos.Y - 8;
					}
					NPC.NewNPC(
						NPC.GetSource_FromAI(),
						(int)NPC.Center.X,
						(int)NPC.Center.Y,
						tentacleType,
						0,
						tentacleDir.X,
						tentacleDir.Y,
						intangibleSteps,
						NPC.whoAmI
					);
					side = -side;
				}
				NPC.localAI[2] = 0;
			}
			if (NPC.localAI[1] > 0) NPC.localAI[1]--;
			NPC.velocity *= 0.97f;
			return false;
		}
		int lifeRegenCount = 0;
		public override void PostAI() {
			float minutesToDie = 4 + ContentExtensions.DifficultyDamageMultiplier;
			lifeRegenCount -= Main.rand.RandomRound((NPC.lifeMax * 2) / (minutesToDie * 60));
			while (lifeRegenCount <= -120) {
				lifeRegenCount += 120;
				int num14 = NPC.whoAmI;
				if (NPC.realLife >= 0) {
					num14 = NPC.realLife;
				}
				if (!Main.npc[num14].immortal) {
					Main.npc[num14].life--;
				}
			}
			NPC.scale = 0.95f + NPC.GetLifePercent() * 0.15f;
			if (NPC.life <= 0 && !NPC.immortal) {
				NPC.life = 1;
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					NPC.StrikeInstantKill();
				}
			}
		}
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			float factor = (NPC.scale - 1) * 0.5f;
			npcHitbox.Inflate((int)(npcHitbox.Width * factor), (int)(npcHitbox.Height * factor));
			return base.ModifyCollisionData(victimHitbox, ref immunityCooldownSlot, ref damageMultiplier, ref npcHitbox);
		}
		public override void BossLoot(ref string name, ref int potionType) {
			potionType = ItemID.GreaterHealingPotion;
		}
		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
			float terriblyPlacedHookMult = 1;
			if (Main.GameModeInfo.IsJourneyMode) {
				CreativePowers.DifficultySliderPower power = CreativePowerManager.Instance.GetPower<CreativePowers.DifficultySliderPower>();
				if (power != null && power.GetIsUnlocked()) {
					if (power.StrengthMultiplierToGiveNPCs > 2) {
						terriblyPlacedHookMult /= 3;
					} else if (power.StrengthMultiplierToGiveNPCs > 1) {
						terriblyPlacedHookMult /= 2;
					}
				}
			}
			NPC.lifeMax = (int)(GetMaxLife() * balance * terriblyPlacedHookMult);
		}
		public override void FindFrame(int frameHeight) {
			NPC.DoFrames(6);
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

			normalDropRule.OnSuccess(ItemDropRule.Common(TrophyTileBase.ItemType<Lost_Diver_Trophy>(), 10));
			npcLoot.Add(new DropBasedOnExpertMode(
				normalDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ModContent.ItemType<Lost_Diver_Bag>(), 1, 1, 1, null)
			));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Faith_Beads>(), 4));
			npcLoot.Add(new Crown_Jewel_Drop());
			npcLoot.Add(ItemDropRule.MasterModeCommonDrop(RelicTileBase.ItemType<Lost_Diver_Relic>()));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-15, -10).RotatedBy(NPC.rotation),
					Vector2.Zero,
					$"Gores/NPC/{nameof(Mildew_Carrion)}_Gore1"
				);
				Origins.instance.SpawnGoreByName(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-1, -3).RotatedBy(NPC.rotation),
					Vector2.Zero,
					$"Gores/NPC/{nameof(Mildew_Carrion)}_Gore2"
				);
			}
		}
		public override void OnKill() {
			Boss_Tracker.Instance.downedLostDiver = true;
			NetMessage.SendData(MessageID.WorldData);
		}
		public enum AIModes {
			Idle,
			Spores,
			SpawnTendril
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)NPC.aiAction);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.aiAction = reader.ReadByte();
		}
		public static AutoLoadingAsset<Texture2D> normalTexture = typeof(Mildew_Carrion).GetDefaultTMLName();
		public static AutoLoadingAsset<Texture2D> afTexture = typeof(Mildew_Carrion).GetDefaultTMLName() + "_AF";
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (OriginsModIntegrations.CheckAprilFools()) {
				TextureAssets.Npc[Type] = afTexture;
			} else {
				TextureAssets.Npc[Type] = normalTexture;
			}
		}
	}
	public class Mildew_Carrion_Entry : JournalEntry {
		public override JournalSortIndex SortIndex => new("Brine_Pool_And_Lost_Diver", 9);
	}
	public class Boss_Bar_MC : ModBossBar {
		int bossHeadIndex = -1;
		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			if (bossHeadIndex == -1) return null;
			return TextureAssets.NpcHeadBoss[bossHeadIndex];
		}
		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
			bossHeadIndex = npc.GetBossHeadTextureIndex();
			BossBarLoader.DrawFancyBar_TML(spriteBatch, drawParams);
			return false;
		}
	}
	public class Mildew_Carrion_Tentacle : ModNPC {
		public static int DestructionCooldown => 300;
		public override void SetStaticDefaults() {
			NPCID.Sets.SpecificDebuffImmunity[Type][Toxic_Shock_Debuff.ID] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<Rasterized_Debuff>()] = true;
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			Mildew_Carrion.Minions.Add(Type);
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
		}
		public override void SetDefaults() {
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 0;
			NPC.lifeMax = 200;
			NPC.defense = 18;
			NPC.aiStyle = 0;
			NPC.width = 16;
			NPC.height = 16;
			NPC.knockBackResist = 0f;
			NPC.HitSound = SoundID.Item127;
			NPC.DeathSound = SoundID.NPCDeath1;
		}
		public NPC Owner {
			get {
				int index = (int)NPC.ai[3];
				if (Main.npc.IndexInRange(index) && Main.npc[index].ModNPC is Mildew_Carrion) return Main.npc[index];
				NPC.life = 0;
				return NPC;
			}
		}
		public override void OnSpawn(IEntitySource source) {
			NPC.velocity = new(NPC.ai[0], NPC.ai[1]);
			NPC.ai[0] = 0;
			NPC.ai[1] = 0;
			NPC.netUpdate = true;
		}
		Physics.Chain chain;
		public override bool PreAI() {
			NPC owner = Owner;
			Vector2 targetPos = (owner.ModNPC as Mildew_Carrion)?.TargetPos ?? owner.Center;
			switch ((int)NPC.ai[0]) {
				case 0: {
					if (--NPC.ai[2] <= 0 && (NPC.Hitbox.OverlapsAnyTiles(false) || NPC.ai[2] < -180)) {
						NPC.velocity = Vector2.Zero;
						NPC.ai[0] = 1;
					}
					break;
				}
				case 1: {
					NPC.chaseable = false;
					NPC.velocity = Vector2.Zero;
					Vector2 targetDirection = owner.DirectionTo(targetPos);
					Vector2 direction = owner.DirectionTo(NPC.Center);
					if (Vector2.Dot(targetDirection, direction) < ContentExtensions.DifficultyDamageMultiplier * 0.25f - 0.35f) {
						NPC.localAI[1] += 1f / 181;
					}
					owner.velocity += direction * 0.1f;
					break;
				}
			}
			if (NPC.localAI[1] > 0) {
				if (!Main.dedServ && chain is null) {
					Physics.Gravity[] gravity = [
						new Physics.ConstantGravity(Vector2.UnitY * 0.04f),
					];
					List<Physics.Chain.Link> links = [];
					Vector2 anchor = NPC.Center;
					anchor.Y += 8;
					const float spring = 0.1f;
					Vector2 diff = Owner.Center - NPC.Center;
					float dist = diff.Length();
					Texture2D texture = TextureAssets.Npc[Type].Value;
					Rectangle frame = texture.Frame(verticalFrames: 5, frameY: 4);
					Gore.NewGoreDirect(
						NPC.GetSource_Death(),
						NPC.Center,
						Vector2.Zero,
						ModContent.GoreType<Mildew_Carrion_Tentacle_Gore>()
					).frame = 4;
					diff /= dist / (frame.Height - 8);
					Vector2 pos = NPC.Center;
					links.Add(new(pos, default, frame.Height - 8, gravity, drag: 0.93f, spring: spring));
					while (dist > frame.Height) {
						pos += diff;
						links.Add(new(pos, default, frame.Height - 8, gravity, drag: 0.93f, spring: spring));
						dist -= frame.Height - 8;
					}
					chain = new Physics.Chain() {
						anchor = new Physics.WorldAnchorPoint(anchor),
						links = links.ToArray()
					};
				}
				NPC.dontTakeDamage = true;
				NPC.localAI[1] += 1f / 181;
			} else {
				NPC.rotation = (Owner.Center - NPC.Center).ToRotation() + MathHelper.PiOver2;
			}
			if (NPC.localAI[1] >= 1 || !owner.active) NPC.life = 0;
			return false;
		}
		public override void OnKill() {
			if (NPC.localAI[1] < 1) Owner.localAI[1] += DestructionCooldown;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0 && NPC.localAI[1] < 0.5f) {
				Vector2 diff = Owner.Center - NPC.Center;
				float dist = diff.Length();
				if (NPC.localAI[0] == 0) NPC.localAI[0] = Main.rand.Next(1, ushort.MaxValue);
				FastRandom rand = new((int)NPC.localAI[0]);
				Texture2D texture = TextureAssets.Npc[Type].Value;
				if (texture.Height == 0) return;
				Rectangle frame = texture.Frame(verticalFrames: 5, frameY: 4);
				Gore.NewGoreDirect(
					NPC.GetSource_Death(),
					NPC.Center,
					Vector2.Zero,
					ModContent.GoreType<Mildew_Carrion_Tentacle_Gore>()
				).frame = 4;
				diff /= dist / (frame.Height - 8);
				Vector2 pos = NPC.Center;
				while (dist > frame.Height) {
					pos += diff;
					Gore.NewGoreDirect(
						NPC.GetSource_Death(),
						pos,
						Vector2.Zero,
						ModContent.GoreType<Mildew_Carrion_Tentacle_Gore>()
					).frame = (byte)rand.Next(4);
					dist -= frame.Height - 8;
				}
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2 diff = Owner.Center - NPC.Center;
			float dist = diff.Length();
			if (NPC.localAI[0] == 0) NPC.localAI[0] = Main.rand.Next(1, ushort.MaxValue);
			FastRandom rand = new((int)NPC.localAI[0]);
			Texture2D texture = TextureAssets.Npc[Type].Value;
			if (texture.Height == 0) return false;
			Rectangle frame = texture.Frame(verticalFrames: 5, frameY: 4);
			drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
			float rotation = NPC.rotation;
			float sqrtDecay = MathF.Pow(1 - NPC.localAI[1], 0.5f);
			Color decayColor = new(1 - NPC.localAI[1], 1 - NPC.localAI[1], 1 - NPC.localAI[1], sqrtDecay); 
			Main.EntitySpriteDraw(
				texture,
				NPC.Center - screenPos,
				frame,
				drawColor.MultiplyRGBA(decayColor),
				rotation,
				frame.Size() * 0.5f,
				1,
				SpriteEffects.None
			);
			if (chain is not null) {
				for (int i = 0; i < 3; i++) chain.UpdateWithCollision();
				for (int i = 0; i < chain.links.Length - 1; i++) {
					frame.Y = rand.Next(4) * frame.Height;
					Vector2 pos = chain.links[i].position;
					Vector2 dir = chain.links[i + 1].position - pos;
					Main.EntitySpriteDraw(
						texture,
						pos + dir * 0.5f - screenPos,
						frame,
						GetColor(pos).MultiplyRGBA(decayColor),
						dir.ToRotation() + MathHelper.PiOver2,
						frame.Size() * 0.5f,
						1,
						SpriteEffects.None
					);
				}
			} else {
				diff /= dist / (frame.Height - 8);
				Vector2 pos = NPC.Center;
				if (frame.Height < 8) return false;
				while (dist > frame.Height) {
					frame.Y = rand.Next(4) * frame.Height;
					Main.EntitySpriteDraw(
						texture,
						pos - screenPos,
						frame,
						GetColor(pos).MultiplyRGBA(decayColor),
						rotation,
						frame.Size() * 0.5f,
						1,
						SpriteEffects.None
					);
					pos += diff;
					dist -= frame.Height - 8;
				}
			}
			return false;
			Color GetColor(Vector2 position) {
				if (NPC.IsABestiaryIconDummy) return Color.White;
				Color npcColor = Lighting.GetColor(position.ToTileCoordinates());
				NPCLoader.DrawEffects(NPC, ref npcColor);
				return NPC.GetNPCColorTintedByBuffs(npcColor);
			}
		}
		public static AutoLoadingAsset<Texture2D> normalTexture = typeof(Mildew_Carrion_Tentacle).GetDefaultTMLName();
		public static AutoLoadingAsset<Texture2D> afTexture = typeof(Mildew_Carrion_Tentacle).GetDefaultTMLName() + "_AF";
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (OriginsModIntegrations.CheckAprilFools()) {
				TextureAssets.Npc[Type] = afTexture;
			} else {
				TextureAssets.Npc[Type] = normalTexture;
			}
		}
	}
	public class Mildew_Carrion_Tendril : ModNPC {
		public override void SetStaticDefaults() {
			NPCID.Sets.SpecificDebuffImmunity[Type][Toxic_Shock_Debuff.ID] = true;
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			Mildew_Carrion.Minions.Add(Type);
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
		}
		public override void SetDefaults() {
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 26;
			NPC.lifeMax = 150;
			NPC.defense = 18;
			NPC.aiStyle = 0;
			NPC.width = 16;
			NPC.height = 16;
			NPC.knockBackResist = 1f;
			NPC.HitSound = SoundID.Item127;
			NPC.DeathSound = SoundID.NPCDeath1;
		}
		public NPC Owner {
			get {
				int index = (int)NPC.ai[3];
				if (Main.npc.IndexInRange(index) && Main.npc[index].ModNPC is Mildew_Carrion) return Main.npc[index];
				NPC.life = 0;
				return NPC;
			}
		}
		public override void OnSpawn(IEntitySource source) {
			NPC.velocity = new(NPC.ai[0], NPC.ai[1]);
			NPC.netUpdate = true;
		}
		public override bool PreAI() {
			NPC owner = Owner;
			Vector2 ownerDiff = owner.Center - NPC.Center;
			float ownerDist = ownerDiff.Length();
			float distFactor = ownerDist - 16 * 8;
			float distDir = Math.Sign(distFactor);
			distFactor *= distDir;
			distFactor = Math.Max(1, 1 / MathF.Pow(distFactor, 0.7f));
			distFactor *= 0.1f;
			NPC.velocity += ownerDiff * (distFactor + 0.1f) * distDir * 0.01f;

			if (distDir < 0) {
				ownerDiff /= ownerDist;
				foreach (NPC other in Main.ActiveNPCs) {
					if (other.type == Type && other.whoAmI != NPC.whoAmI) {
						Vector2 otherDiff = NPC.Center - other.Center;
						float otherDist = otherDiff.Length();
						if (otherDist < 1) continue;
						Vector2 nextVel = NPC.velocity + (otherDiff / MathF.Pow(otherDist, 1.1f)) * 1f;
						if (nextVel.LengthSquared() < 16 * 16) {
							NPC.velocity = nextVel;
						}
					}
				}
			}
			float speedSquared = NPC.velocity.LengthSquared();
			if (speedSquared > 16 * 16) NPC.velocity = NPC.velocity.WithMaxLength(16);
			else if (speedSquared > 8 * 8) NPC.velocity *= 0.93f;
			if (!owner.active) NPC.life = 0;
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				/*Vector2 diff = Owner.Center - NPC.Center;
				float dist = diff.Length();
				if (NPC.localAI[0] == 0) NPC.localAI[0] = Main.rand.Next(1, ushort.MaxValue);
				FastRandom rand = new((int)NPC.localAI[0]);
				Texture2D texture = TextureAssets.Npc[Type].Value;
				Rectangle frame = texture.Frame(verticalFrames: 5, frameY: 4);
				Gore.NewGoreDirect(
					NPC.GetSource_Death(),
					NPC.Center,
					Vector2.Zero,
					ModContent.GoreType<Mildew_Carrion_Tentacle_Gore>()
				).frame = 4;
				diff /= dist / (frame.Height - 8);
				Vector2 pos = NPC.Center;
				while (dist > frame.Height) {
					pos += diff;
					Gore.NewGoreDirect(
						NPC.GetSource_Death(),
						pos,
						Vector2.Zero,
						ModContent.GoreType<Mildew_Carrion_Tentacle_Gore>()
					).frame = (byte)rand.Next(4);
					dist -= frame.Height - 8;
				}*/
			}
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.ScalingArmorPenetration += Brine_Pool_NPC.ScalingArmorPenetrationToCompensateForTSNerf;
		}
		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2 diff = Owner.Center - NPC.Center;
			float dist = diff.Length();
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:Beam"];
			FastRandom rand = new(NPC.whoAmI);
			float uTime = (float)Main.timeForVisualEffects * (rand.Next(10) * 0.1f + 0.1f);
			int length = (int)(dist / 10);
			if (length == 0) return false;
			float[] rot = new float[length];
			Vector2[] pos = new Vector2[length];
			Color[] colors = new Color[length];
			Vector2 step = diff / length;
			for (int i = 0; i < length; i++) {
				pos[i] = NPC.Center + step * i + new Vector2(step.Y, -step.X) * MathF.Sin((i * rand.Next(1, 10) * 0.05f) + NPC.whoAmI + uTime) * (rand.Next(10) * 0.03f + 0.2f);
				colors[i] = Lighting.GetColor(pos[i].ToTileCoordinates());
			}
			for (int i = 1; i < length; i++) {
				rot[i] = (pos[i] - pos[i - 1]).ToRotation();
			}
			Asset<Texture2D> texture = TextureAssets.Npc[Type];
			miscShaderData.UseImage0(texture);
			miscShaderData.UseShaderSpecificData(texture.UVFrame());
			float endLength = 1f / length;
			miscShaderData.Shader.Parameters["uLoopData"].SetValue(new Vector2(
				10f / 30f,
				endLength
			));
			miscShaderData.Apply();
			_vertexStrip.PrepareStrip(pos, rot, progress => float.IsNaN(progress) ? Color.Red : colors[(int)(progress * (colors.Length - 1))], _ => 3, -Main.screenPosition, length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			return false;
			/*
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Rectangle frame = texture.Frame(verticalFrames: 3, frameY: 2);
			//drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
			float rotation = diff.ToRotation() + MathHelper.PiOver2;
			Main.EntitySpriteDraw(
				texture,
				NPC.Center - screenPos,
				frame,
				drawColor,
				rotation,
				frame.Size() * 0.5f,
				1,
				SpriteEffects.None
			);
			diff /= dist / (frame.Height - 2);
			Vector2 pos = NPC.Center;
			frame.Y = frame.Height;
			while (dist > frame.Height) {
				pos += diff;
				Main.EntitySpriteDraw(
					texture,
					pos - screenPos,
					frame,
					GetColor(pos),
					rotation,
					frame.Size() * 0.5f,
					1,
					SpriteEffects.None
				);
				dist -= frame.Height - 2;
			}
			return false;
			Color GetColor(Vector2 position) {
				if (NPC.IsABestiaryIconDummy) return Color.White;
				Color npcColor = Lighting.GetColor(position.ToTileCoordinates());
				NPCLoader.DrawEffects(NPC, ref npcColor);
				return NPC.GetNPCColorTintedByBuffs(npcColor);
			}
			*/
		}
	}
	public class MC_Music_Scene_Effect : BossMusicSceneEffect {
		public override int Music => Origins.Music.MildewCarrion;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			npcIDs[ModContent.NPCType<Lost_Diver_Transformation>()] = true;
			npcIDs[ModContent.NPCType<Mildew_Carrion>()] = true;
		}
	}
}
