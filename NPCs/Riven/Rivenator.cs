using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Rivenator_Head : Rivenator {
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.DiggerHead);
            npc.lifeMax = 80;
            npc.defense = 8;
            npc.damage = 38;
        }
        public override void AI() {
            if(Main.netMode != NetmodeID.MultiplayerClient) {
                if(npc.ai[0] == 0f) {
                    npc.spriteDirection = Main.rand.NextBool() ?1:-1;
                    npc.ai[3] = npc.whoAmI;
                    npc.realLife = npc.whoAmI;
                    int current = 0;
                    int last = npc.whoAmI;
                    int type = ModContent.NPCType<Rivenator_Body>();
                    for(int k = 0; k < 17; k++) {
                        current = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, type, npc.whoAmI);
                        Main.npc[current].ai[3] = npc.whoAmI;
                        Main.npc[current].realLife = npc.whoAmI;
                        Main.npc[current].ai[1] = last;
                        Main.npc[current].spriteDirection = Main.rand.NextBool() ?1:-1;
                        Main.npc[last].ai[0] = current;
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, current);
                        last = current;
                    }
                    current = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<Rivenator_Tail>(), npc.whoAmI);
                    Main.npc[current].ai[3] = npc.whoAmI;
                    Main.npc[current].realLife = npc.whoAmI;
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
            npc.CloneDefaults(NPCID.DiggerBody);
        }
    }

    internal class Rivenator_Tail : Rivenator {
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.DiggerTail);
        }
    }

    public abstract class Rivenator : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Rivenator");
        }

        public override void AI() {
            npc.life = Main.npc[npc.realLife].active ? npc.lifeMax : 0;
        }
        public override void HitEffect(int hitDirection, double damage) {
            if(npc.life<0) {
                NPC current = Main.npc[npc.realLife];
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