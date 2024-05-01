using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.NPCs.Critters;
using Origins.World.BiomeData;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Barnacle_Mound : ModNPC, IRivenEnemy {
		public virtual string GlowTexturePath => Texture + "_Glow";
		private Asset<Texture2D> _glowTexture;
		public Texture2D GlowTexture => (_glowTexture ??= (ModContent.RequestIfExists<Texture2D>(GlowTexturePath, out var asset) ? asset : null))?.Value;
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BloodJelly);
			NPC.aiStyle = 0;
			NPC.noGravity = true;
			NPC.lifeMax = 90;
			NPC.defense = 18;
			NPC.damage = 0;
			NPC.width = 24;
			NPC.height = 24;
			NPC.knockBackResist = 0;
			NPC.value = 100;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 1, 3, 8));
        }
		public override void OnKill() {
			if (Main.rand.NextBool(4, 7)) {
				int type = ModContent.NPCType<Amoeba_Buggy>();
				for (int i = Main.rand.Next(1, 3); i-- > 0;) {
					NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.position.X, (int)NPC.position.Y, type);
				}
			}
		}
		public override void AI() {
			NPC.TargetClosest(faceTarget: false);
			if (NPC.HasValidTarget && ++NPC.ai[0] > (Main.masterMode ? 420 : (Main.expertMode ? 540 : 600))) {
				int type = ModContent.NPCType<Amoeba_Bugger>();
				NPC.ai[0] = 0;
				for (int i = Main.rand.Next(4, 7); i-- > 0;) {
					NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.position.X, (int)NPC.position.Y, type, ai0: Main.rand.NextFloat(-4, 4), ai1: Main.rand.NextFloat(-4, 4));
					NPC.ai[0] -= 30;
				}
			}
			NPC.position = NPC.oldPosition;
			NPC.velocity = Vector2.Zero;
		}
		public override void OnSpawn(IEntitySource source) {
			const float offsetLen = 10;
			NPC.velocity = new Vector2(0, 1);
			Vector2 offset = new Vector2(0, offsetLen);
			for (int i = 0; i < 10; i++) {
				if (Framing.GetTileSafely(NPC.Center + new Vector2(0, 16 * i)).HasSolidTile()) {
					NPC.velocity = new Vector2(0, 1);
					NPC.rotation = 0;
					offset = new Vector2(0, offsetLen);
					break;
				} else if (Framing.GetTileSafely(NPC.Center + new Vector2(0, -16 * i)).HasSolidTile()) {
					NPC.velocity = new Vector2(0, -1);
					NPC.rotation = MathHelper.Pi;
					offset = new Vector2(0, -offsetLen);
					break;
				} else if (Framing.GetTileSafely(NPC.Center + new Vector2(-16 * i, 0)).HasSolidTile()) {
					NPC.velocity = new Vector2(-1, 0);
					NPC.rotation = MathHelper.PiOver2;
					offset = new Vector2(-offsetLen, 0);
					break;
				} else if (Framing.GetTileSafely(NPC.Center + new Vector2(16 * i, 0)).HasSolidTile()) {
					NPC.velocity = new Vector2(1, 0);
					NPC.rotation = -MathHelper.PiOver2;
					offset = new Vector2(offsetLen, 0);
					break;
				}
			}
			int tries = 0;
			while (!Framing.GetTileSafely(NPC.Center + offset + NPC.velocity).HasSolidTile()) {
				NPC.position += NPC.velocity;
				if (++tries > 160) break;
			}
			NPC.velocity = Vector2.Zero;
			NPC.oldVelocity = Vector2.Zero;
			NPC.oldPosition = NPC.position;
			NPC.netUpdate = true;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Riven_Hive.SpawnRates.LandEnemyRate(spawnInfo, true) * Riven_Hive.SpawnRates.Barnacle;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText("Barnacle mounds act as a self-contained ecosystem of microbes, buggers, and the Riven of course."),
			});
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2 halfSize = new Vector2(GlowTexture.Width / 2, GlowTexture.Height / Main.npcFrameCount[NPC.type]);
			Vector2 position = NPC.Center + new Vector2(0, 12).RotatedBy(NPC.rotation);
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				position - screenPos,
				NPC.frame,
				drawColor,
				NPC.rotation,
				halfSize,
				NPC.scale,
				SpriteEffects.None,
			0);
			if (GlowTexture is not null) {
				spriteBatch.Draw(
					GlowTexture,
					position - screenPos,
					NPC.frame,
					Color.White,
					NPC.rotation,
					halfSize,
					NPC.scale,
					SpriteEffects.None,
				0);
			}
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life < 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(1, 4));
			}
		}
	}
}
