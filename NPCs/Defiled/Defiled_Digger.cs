using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled {
    public class Defiled_Digger_Head : Defiled_Digger {
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
                    int type = ModContent.NPCType<Defiled_Digger_Body>();
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
                    current = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<Defiled_Digger_Tail>(), npc.whoAmI);
                    Main.npc[current].ai[3] = npc.whoAmI;
                    Main.npc[current].realLife = npc.whoAmI;
                    Main.npc[current].ai[1] = last;
                    Main.npc[current].spriteDirection = Main.rand.NextBool() ?1:-1;
                    Main.npc[last].ai[0] = current;
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, current);
                }
            }
        }

        /*public override void Init() {
			base.Init();
			head = true;
		}

		private int attackCounter;
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(attackCounter);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			attackCounter = reader.ReadInt32();
		}

		public override void CustomBehavior() {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				if (attackCounter > 0) {
					attackCounter--;
				}

				Player target = Main.player[npc.target];
				if (attackCounter <= 0 && Vector2.Distance(npc.Center, target.Center) < 200 && Collision.CanHit(npc.Center, 1, 1, target.Center, 1, 1)) {
					Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitX);
					direction = direction.RotatedByRandom(MathHelper.ToRadians(10));

					int projectile = Projectile.NewProjectile(npc.Center, direction * 1, ProjectileID.ShadowBeamHostile, 5, 0, Main.myPlayer);
					Main.projectile[projectile].timeLeft = 300;
					attackCounter = 500;
					npc.netUpdate = true;
				}
			}
		}*/
    }

    internal class Defiled_Digger_Body : Defiled_Digger {
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.DiggerBody);
        }
    }

    internal class Defiled_Digger_Tail : Defiled_Digger {
        public override void SetDefaults() {
            npc.CloneDefaults(NPCID.DiggerTail);
        }
    }

    public abstract class Defiled_Digger : ModNPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Digger");
        }

        public override void AI() {
            NPC head = Main.npc[npc.realLife];
            npc.life = head.active ? npc.lifeMax : 0;
            npc.immune = Main.npc[npc.realLife].immune;
        }
        public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
            if(npc.realLife!=npc.whoAmI) {
                if(projectile.usesLocalNPCImmunity) {
                    projectile.localNPCImmunity[npc.realLife] = projectile.localNPCHitCooldown;
                    projectile.localNPCImmunity[npc.whoAmI] = 0;
                } else {
                    Main.npc[npc.realLife].immune[projectile.owner] = npc.immune[projectile.owner];
                }
            }
        }
        public override bool? CanBeHitByProjectile(Projectile projectile) {
            if(npc.realLife==npc.whoAmI)return null;
            if((projectile.usesLocalNPCImmunity?projectile.localNPCImmunity[npc.realLife]:Main.npc[npc.realLife].immune[projectile.owner])>0) {
                return false;
            }
            return null;
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