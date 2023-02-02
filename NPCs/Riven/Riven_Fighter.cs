using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Riven_Fighter : Glowing_Mod_NPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Riven Protoform");
			Main.npcFrameCount[NPC.type] = 5;
			SpawnModBiomes = new int[] {
				ModContent.GetInstance<Riven_Hive>().Type
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 81;
			NPC.defense = 10;
			NPC.damage = 33;
			NPC.width = 36;
			NPC.height = 40;
			NPC.friendly = false;
			NPC.value = 90;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				new FlavorTextBestiaryInfoElement("The first creature born in the abominable Riven Hive. It is a very agile protector of its home."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 1, 1, 3));
		}
		public override bool PreAI() {
			NPC.ai[0] += Main.rand.NextFloat(0, 1);
			if (NPC.ai[0] > 210 && NPC.collideY) {
				switch (NPC.aiAction) {
					case 0:
					NPC.aiAction = (int)(Main.GlobalTimeWrappedHourly % 2 + 1);
					if (NPC.aiAction == 2) {
						NPC.velocity = ((NPC.GetTargetData().Center - new Vector2(0, 16) - NPC.Center) * new Vector2(1, 4)).WithMaxLength(12);
						NPC.collideX = false;
						NPC.collideY = false;
					}
					break;
					case 1:
					case 2:
					NPC.aiAction = 0;
					break;
				}
				NPC.ai[0] = 0;
			}
			if (NPC.aiAction != 0) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 40) % 160, 36, 40);
				NPC.frameCounter = 0;
				if (NPC.aiAction == 1) {
					NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X + NPC.direction * 0.15f, -6, 6);
					NPC.ai[0] += NPC.collideX ? 9 : 3;
				} else if (NPC.collideX || NPC.collideY) {
					NPC.ai[0] = 300;
				}
				return false;

			}
			return true;
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.FaceTarget();
				NPC.spriteDirection = NPC.direction;
			}
			//increment frameCounter every frame and run the following code when it exceeds 7 (i.e. run the following code every 8 frames)
			if (NPC.collideY && ++NPC.frameCounter > 7) {
				//add frame height (with buffer) to frame y position and modulo by frame height (with buffer) multiplied by walking frame count
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 40) % 160, 36, 40);
				//reset frameCounter so this doesn't trigger every frame after the first time
				NPC.frameCounter = 0;
			}
		}
		public override void HitEffect(int hitDirection, double damage) {
			//spawn gore if npc is dead after being hit
			if (NPC.life < 0) {
				for (int i = 0; i < 3; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)));
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Meat" + Main.rand.Next(1, 4)));
			}
			NPC.frame = new Rectangle(0, 160, 36, 40);
			NPC.frameCounter = 0;
		}
	}
}
