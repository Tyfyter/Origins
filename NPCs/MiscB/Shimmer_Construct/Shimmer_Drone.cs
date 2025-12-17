using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscB.Shimmer_Construct {
	public class Shimmer_Drone : ModNPC {
		public override void SetStaticDefaults() {
			NPCID.Sets.ShimmerTransformToNPC[NPCID.ServantofCthulhu] = Type;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() {
				Position = new(1.5f, 7)
			};
			ContentSamples.NpcBestiaryRarityStars[Type] = 3;
			NPCID.Sets.DontDoHardmodeScaling[Type] = true;
			Shimmer_Construct.Minions.Add(Type);
			AprilFoolsTextures.AddNPC(this);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.ServantofCthulhu);
			NPC.damage = 22;
			NPC.lifeMax = 30;
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.HitSound = SoundID.DD2_CrystalCartImpact.WithPitch(1f).WithVolume(0.5f);
			NPC.DeathSound = SoundID.Item101.WithVolume(0.6f);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Caverns
			);
		}
		public override void OnSpawn(IEntitySource source) => NPC.ai[1] = -2;
		public override void AI() {
			Vector2 targetPos = default;
			if (NPC.ai[0] == 0) {
				switch (NPC.ai[1]) {
					case -1:
					NPC.TargetClosest();
					NPC.ai[0] = 1;
					break;

					case -2:
					NPC.ai[1] = NPC.FindFirstNPC(ModContent.NPCType<Shimmer_Construct>());
					if (NPC.ai[1] == -1) goto case -1;
					break;
				}
				if (NPC.ai[1] == -1) return;
				NPC.targetRect = Main.npc[(int)NPC.ai[1]].Hitbox;
				targetPos = NPC.targetRect.Center();

				Vector2 diffToOwner = targetPos - NPC.Center;
				float dist = diffToOwner.Length();
				bool passed = dist < 16 * 4 && (dist <= 0 || Vector2.Dot(diffToOwner / dist, NPC.ai[2].ToRotationVector2()) < 0);
				if (passed) {
					NPC.ai[0] = 1;
					NPC owner = Main.npc[(int)NPC.ai[1]];
					if (owner.active) {
						NPC.target = owner.target;
						NPC.targetRect = Main.player[NPC.target].Hitbox;
						targetPos = NPC.targetRect.Center();
						NPC.ai[2] = (targetPos - NPC.Center).ToRotation();
					} else {
						NPC.ai[1] = -1;
					}
				}
			} else {
				targetPos = NPC.targetRect.Center();

				if (Vector2.Dot(NPC.DirectionTo(targetPos), NPC.ai[2].ToRotationVector2()) < 0) {
					NPC.ai[0] = 0;
					if (NPC.ai[1] != -1) {
						NPC.targetRect = Main.npc[(int)NPC.ai[1]].Hitbox;
						targetPos = NPC.targetRect.Center();
					}
				}
			}
			NPC.velocity += NPC.DirectionTo(targetPos) * 0.5f;
			NPC.velocity = NPC.velocity.SafeNormalize(default) * 8;
			NPC.spriteDirection = Math.Sign(NPC.velocity.X);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(1 * NPC.direction, -6).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Shimmer_Thing1")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(7 * NPC.direction, 3).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Shimmer_Thing_Medium1")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-6 * NPC.direction, -8).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Shimmer_Thing_Medium2")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(9 * NPC.direction, -13).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Shimmer_Thing2")
				);
			}
		}
		public override Color? GetAlpha(Color drawColor) => Color.White * NPC.Opacity;
	}
}
