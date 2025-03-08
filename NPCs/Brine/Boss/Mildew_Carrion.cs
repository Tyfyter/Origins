using Origins.Items.Accessories;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.LootBags;
using Origins.Items.Weapons.Crossmod;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using System;
using Terraria;
using Terraria.GameContent.ItemDropRules;
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
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
		}
		public override void SetDefaults() {
			NPC.dontTakeDamage = true;
			NPC.lifeMax = 6000;
			NPC.noGravity = true;
			NPC.width = 76;
			NPC.height = 58;
		}
		public override bool PreAI() {
			NPC.spriteDirection = (int)-NPC.ai[1];
			NPC.velocity = Vector2.Zero;
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
	[AutoloadBossHead]
	public class Mildew_Carrion : Brine_Pool_NPC {
		internal static IItemDropRule normalDropRule;
		public override bool AggressivePathfinding => true;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.CantTakeLunchMoney[Type] = false;
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
					AddMode(AIModes.Idle, 0 + rangeFactor);
					AIMode = rand.Get();
					LastMode = AIMode;
					NPC.netUpdate = true;
					goto startMode;
				}
				break;
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
			npcLoot.Add(new DropBasedOnExpertMode(
				normalDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ModContent.ItemType<Lost_Diver_Bag>(), 1, 1, 1, null)
			));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Faith_Beads>(), 4));
			npcLoot.Add(new Crown_Jewel_Drop());
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
		}
	}
}
