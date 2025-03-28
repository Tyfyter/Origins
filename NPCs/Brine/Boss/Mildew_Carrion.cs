using Microsoft.Xna.Framework.Graphics;
using Origins.CrossMod.Thorium.Items.Weapons.Bard;
using Origins.Items.Accessories;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.LootBags;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.Journal;
using Origins.Misc;
using Origins.Tiles.BossDrops;
using PegasusLib;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using static Origins.Misc.Physics;
using static Origins.NPCs.Brine.Mildew_Creeper;

namespace Origins.NPCs.Brine.Boss {
	public class Lost_Diver_Transformation : ModNPC, IJournalEntrySource {
		public class Mildew_Carrion_Entry : JournalEntry {
			public override string TextKey => "Mildew_Carrion";
		}
		public string EntryName => "Origins/" + typeof(Mildew_Carrion_Entry).Name;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 10;
			NPCID.Sets.CantTakeLunchMoney[Type] = false;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.dontTakeDamage = true;
			NPC.lifeMax = 6000;
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
	public class Mildew_Carrion : Brine_Pool_NPC {
		internal static IItemDropRule normalDropRule;
		public override bool AggressivePathfinding => true;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() {
				//CustomTexturePath = "Origins/NPCs/Brine/Boss/Rock_Bottom", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(0f, 0f),
				PortraitPositionXOverride = 0f,
				PortraitPositionYOverride = 0f
			};
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
		}
		public override void Unload() {
			normalDropRule = null;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.boss = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 58;
			NPC.lifeMax = 6000;
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
		public override bool CanTargetNPC(NPC other) => other.type != NPCID.TargetDummy && NPC.WithinRange(other.Center, 16 * 400) && CanHitNPC(other);
		public override bool CanHitNPC(NPC target) => !Mildew_Creeper.FriendlyNPCTypes.Contains(target.type);
		public override bool CheckTargetLOS(Vector2 target) => !NPC.wet || base.CheckTargetLOS(target);
		public override float RippleTargetWeight(float magnitude, float distance) => 0;
		public override bool? CanFallThroughPlatforms() => NPC.wet || NPC.targetRect.Bottom > NPC.BottomLeft.Y;
		public override bool PreAI() {
			float difficultyMult = ContentExtensions.DifficultyDamageMultiplier;
			DoTargeting();
			Vector2 targetVelocity = Vector2.Zero;
			if (NPC.HasPlayerTarget) {
				targetVelocity = Main.player[NPC.target].velocity;
			} else if (NPC.HasNPCTarget) {
				targetVelocity = Main.npc[NPC.TranslatedTargetIndex].velocity;
			}
			Vector2 differenceFromTarget = TargetPos - NPC.Center;
			float distanceFromTarget = differenceFromTarget.Length();
			Vector2 direction = differenceFromTarget / distanceFromTarget;
			startMode:
			switch (AIMode) {
				default:
				case AIModes.Idle:
				NPC.ai[0] += 0.65f + (0.35f * difficultyMult);
				if (NPC.ai[0] > 120 && NPC.HasValidTarget) {
					HeldProjectile = -1;
					WeightedRandom<AIModes> rand = new(Main.rand);
					void AddMode(AIModes mode, double weight) {
						if (mode == LastMode) weight *= 0.3f;
						rand.Add(mode, weight);
					}
					float rangeFactor = Math.Max(0, distanceFromTarget / (15 * 16) - 1);
					AddMode(AIModes.Idle, 1);
					AIMode = rand.Get();
					LastMode = AIMode;
					NPC.netUpdate = true;
					goto startMode;
				}
				break;
			}
			if (Main.netMode != NetmodeID.MultiplayerClient && ++NPC.ai[3] > 90) {
				for (int i = 2; i > 0; i--) {
					Vector2 tentacleDir = direction.RotatedBy(Main.rand.NextFloat(0.1f, 0.5f) * Main.rand.NextBool().ToDirectionInt()) * 16;
					NPC.NewNPC(
						NPC.GetSource_FromAI(),
						(int)NPC.Center.X,
						(int)NPC.Center.Y,
						ModContent.NPCType<Mildew_Carrion_Tentacle>(),
						0,
						tentacleDir.X,
						tentacleDir.Y,
						0,
						NPC.whoAmI
					);
					NPC.ai[3] = 0;
				}
			}
			NPC.velocity *= 0.97f;
			return false;
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
			}
		}
		public override void OnKill() {
			Boss_Tracker.Instance.downedLostDiver = true;
			NetMessage.SendData(MessageID.WorldData);
		}
		public enum AIModes {
			Idle
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)NPC.aiAction);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.aiAction = reader.ReadByte();
		}
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
		public override void SetStaticDefaults() {
			NPCID.Sets.CantTakeLunchMoney[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
		}
		public override void SetDefaults() {
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 58;
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
		public override bool PreAI() {
			NPC owner = Owner;
			Vector2 targetPos = (owner.ModNPC as Mildew_Carrion)?.TargetPos ?? owner.Center;
			switch ((int)NPC.ai[0]) {
				case 0: {
					if (--NPC.ai[2] <= 0 && NPC.Hitbox.OverlapsAnyTiles(false)) {
						NPC.velocity = Vector2.Zero;
						NPC.ai[0] = 1;
					}
					break;
				}
				case 1: {
					NPC.velocity = Vector2.Zero;
					Vector2 targetDirection = owner.DirectionTo(targetPos);
					Vector2 direction = owner.DirectionTo(NPC.Center);
					if (Vector2.Dot(targetDirection, direction) < -0.1f) {
						NPC.life = 0;
					}
					if (NPC.life > 0) owner.velocity += direction * 0.1f;
					break;
				}
			}
			return false;
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2 diff = Owner.Center - NPC.Center;
			float dist = diff.Length();
			if (NPC.localAI[0] == 0) NPC.localAI[0] = Main.rand.Next(1, ushort.MaxValue);
			FastRandom rand = new((int)NPC.localAI[0]);
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Rectangle frame = texture.Frame(verticalFrames: 5, frameY: 4);
			drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
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
			diff /= dist / (frame.Height - 8);
			Vector2 pos = NPC.Center;
			while (dist > frame.Height) {
				frame.Y = rand.Next(4) * frame.Height;
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
				dist -= frame.Height - 8;
			}
			return false;
			Color GetColor(Vector2 position) {
				if (NPC.IsABestiaryIconDummy) return Color.White;
				Color npcColor = Lighting.GetColor(position.ToTileCoordinates());
				NPCLoader.DrawEffects(NPC, ref npcColor);
				return NPC.GetNPCColorTintedByBuffs(npcColor);
			}
		}
	}
}
