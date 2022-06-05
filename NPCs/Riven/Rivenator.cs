using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Rivenator_Head : Rivenator {
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.DiggerHead);
            NPC.lifeMax = 80;
            NPC.defense = 8;
            NPC.damage = 38;
        }
        public override void AI() {
            if(Main.netMode != NetmodeID.MultiplayerClient) {
                if(NPC.ai[0] == 0f) {
                    NPC.spriteDirection = Main.rand.NextBool() ?1:-1;
                    NPC.ai[3] = NPC.whoAmI;
                    NPC.realLife = NPC.whoAmI;
                    int current = 0;
                    int last = NPC.whoAmI;
                    int type = ModContent.NPCType<Rivenator_Body>();
                    for(int k = 0; k < 17; k++) {
                        current = NPC.NewNPC((int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI);
                        Main.npc[current].ai[3] = NPC.whoAmI;
                        Main.npc[current].realLife = NPC.whoAmI;
                        Main.npc[current].ai[1] = last;
                        Main.npc[current].spriteDirection = Main.rand.NextBool() ?1:-1;
                        Main.npc[last].ai[0] = current;
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, current);
                        last = current;
                    }
                    current = NPC.NewNPC((int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Rivenator_Tail>(), NPC.whoAmI);
                    Main.npc[current].ai[3] = NPC.whoAmI;
                    Main.npc[current].realLife = NPC.whoAmI;
                    Main.npc[current].ai[1] = last;
                    Main.npc[current].spriteDirection = Main.rand.NextBool() ?1:-1;
                    Main.npc[last].ai[0] = current;
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, current);
                }
            }
        }
    }

    internal class Rivenator_Body : Rivenator {
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.DiggerBody);
        }
    }

    internal class Rivenator_Tail : Rivenator {
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.DiggerTail);
        }
    }

    public abstract class Rivenator : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Rivenator");
        }

        public override void AI() {
            NPC.life = Main.npc[NPC.realLife].active ? NPC.lifeMax : 0;
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(NPC.life<0) {
                NPC current = Main.npc[NPC.realLife];
                while(current.ai[0]!=0) {
                    deathEffect(current);
                    current = Main.npc[(int)current.ai[0]];
                }
            }
        }
        protected static void deathEffect(NPC npc) {
            Gore.NewGore(npc.position, npc.velocity, Origins.instance.GetGoreSlot("Gores/NPCs/DF3_Gore"));
            //Gore.NewGore(npc.position, npc.velocity, Origins.instance.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
            Gore.NewGore(npc.position, npc.velocity, Origins.instance.GetGoreSlot("Gores/NPCs/DF_Effect_Small"+Main.rand.Next(1,4)));
            //for(int i = 0; i < 3; i++)
        }
    }
}