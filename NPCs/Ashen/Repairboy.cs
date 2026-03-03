using Microsoft.Xna.Framework.Graphics;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Ashen {
	public class Repairboy : ModNPC, IAshenEnemy {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 4;
			NPCID.Sets.UsesNewTargetting[Type] = true;
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 45;
			NPC.defense = 8;
			NPC.damage = 18;
			NPC.width = 40;
			NPC.height = 26;
			NPC.noGravity = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Ashen_Biome>().Type,
			];
		}
		void MoveTowards(Vector2 targetPos) {
			const float acceleration = 0.35f;
			const float target_dist_min = 16;
			const float target_dist_max = 32;
			Vector2 movementDir = NPC.velocity.Normalized(out float speed);
			Vector2 targetDir = (targetPos - NPC.Center).Normalized(out float dist);
			if (dist < target_dist_min) {
				NPC.velocity -= targetDir * acceleration;
			} else if (dist > target_dist_max) {
				float speedInRightDir = Vector2.Dot(movementDir, targetDir) * speed;
				float predictedTime = (float.Sqrt(2 * (dist - target_dist_max) * -acceleration + speedInRightDir * speedInRightDir) + speedInRightDir) / acceleration;
				if (!float.IsNaN(predictedTime)) {
					NPC.velocity -= targetDir * acceleration;
				} else {
					NPC.velocity += targetDir * acceleration;
				}
			}
			if (dist < target_dist_max) {
				if (NPC.ai[0].CycleUp(30)) {
					NPC.velocity += new Vector2(targetDir.Y, -targetDir.X) * Main.rand.NextBool().ToDirectionInt();
					NPC.netUpdate = true;
				}
			} else NPC.ai[0] = 0;
			NPC.direction = targetDir.X < 0 ? -1 : 1;
		}
		public void TargetClosest(bool resetTarget = true, bool faceTarget = true) {
			if (resetTarget) {
				NPC.target = -1;
				NPC.targetRect = NPC.Hitbox;
			}
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.All, null, target => target.ModNPC is IAshenEnemy && target.type != Type && target.life < target.lifeMax);
			if (searchResults.FoundTarget) {
				NPC.target = searchResults.NearestTargetIndex;
				NPC.targetRect = searchResults.NearestTargetHitbox;
				if (NPC.ShouldFaceTarget(ref searchResults) && faceTarget) NPC.FaceTarget();
			}
		}
		public override void AI() {
			TargetClosest();
			MoveTowards(NPC.Center.Clamp(NPC.targetRect));
			NPC.velocity *= 0.97f;
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects effects = NPC.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				NPC.Center - screenPos,
				NPC.frame,
				drawColor,
				0,
				new Vector2(32, 21).Apply(effects, NPC.frame.Size()),
				NPC.scale,
				effects,
			0);
			return false;
		}
	}
}
