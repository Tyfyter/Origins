using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
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
    public class Barnacle_Mound : ModNPC {
        public virtual string GlowTexturePath => Texture + "_Glow";
        private Asset<Texture2D> _glowTexture;
        public Texture2D GlowTexture => (_glowTexture ??= (ModContent.RequestIfExists<Texture2D>(GlowTexturePath, out var asset) ? asset : null))?.Value;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Barnacle Mound");
            Main.npcFrameCount[NPC.type] = 1;
            SpawnModBiomes = new int[] {
                ModContent.GetInstance<Riven_Hive>().Type
            };
        }
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
        }
		public override void AI() {
			if (++NPC.ai[0] > (Main.masterMode ? 420 : (Main.expertMode? 540 : 600))) {
                int type = ModContent.NPCType<Amoeba_Bugger>();
				for (int i = Main.rand.Next(5, 8); i-->0;) {
                    NPC.NewNPCDirect(NPC.GetSource_FromAI(), (int)NPC.position.X, (int)NPC.position.Y, type, ai0: Main.rand.NextFloat(-4, 4), ai1: Main.rand.NextFloat(-4, 4));
				}
                NPC.ai[0] = 0;
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
        }
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("Barnacle mounds act as a self-contained ecosystem of microbes, buggers, and the Riven of course."),
            });
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Amebic_Gel>(), 1, 1, 3));
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
    }
}
