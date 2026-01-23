using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Core;
using Origins.Dev;
using Origins.Items.Weapons.Summoner;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Crimson {
	public class Crimbrain : ModNPC, IWikiNPC {
		public Rectangle DrawRect => new(0, 36, 34, 28);
		public int AnimationFrames => 32;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public static float KnockbackMultiplier => 1.5f;
		public static float Friction => 0.97f;
		public static float AccelMult => 0.99f;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
			CrimsonGlobalNPC.NPCTypes.Add(Type);
			AssimilationLoader.AddNPCAssimilation<Crimson_Assimilation>(Type, 0.04f);
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new(0, -16),
				PortraitPositionYOverride = -32
			};
			AprilFoolsTextures.AddNPC(this);
		}
		public override void SetDefaults() {
			NPC.aiStyle = Terraria.ID.NPCAIStyleID.AncientVision;
			NPC.lifeMax = 30;
			NPC.defense = 4;
			NPC.damage = 25;
			NPC.width = 34;
			NPC.height = 28;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = KnockbackMultiplier;
			NPC.value = 75;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown || spawnInfo.PlayerSafe) return 0;
			if (spawnInfo.PlayerFloorY > Main.worldSurface + 50 || spawnInfo.SpawnTileY >= Main.worldSurface - 50) return 0;
			if (!spawnInfo.Player.ZoneCrimson) return 0;
			return 0.07f * (spawnInfo.Player.ZoneSkyHeight ? 2 : 1);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			target.AddBuff(BuffID.Confused, 20);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCrimson
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Vertebrae));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Brainy_Staff>(), 17));
		}
		public override void AI() {
			if (!NPC.HasValidTarget) NPC.direction = Math.Sign(NPC.velocity.X);
			NPC.spriteDirection = NPC.direction;
			NPC.knockBackResist = KnockbackMultiplier;
			NPC.velocity = Vector2.Lerp(NPC.oldVelocity, NPC.velocity, AccelMult);
			NPC.velocity *= Friction;
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 7) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 28) % 112, 34, 26);
				NPC.frameCounter = 0;
			}
		}
	}
}
