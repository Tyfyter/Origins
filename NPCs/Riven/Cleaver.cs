using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Cleaver_Head : Cleaver, IRivenEnemy {
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerHead);
			NPC.width = NPC.height = 12;
			NPC.lifeMax = 60;
			NPC.defense = 7;
			NPC.damage = 23;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath23;
			NPC.value = 70;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText("A vicious defender of the Riven Hive, the Cleaver hides in burrows of spug flesh awaiting any trespassers of its territory."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			
		}
		public override void AI() {
			NPC.velocity *= 1.0033f;
		}
		public override void OnSpawn(IEntitySource source) {
			NPC.spriteDirection = Main.rand.NextBool() ? 1 : -1;
			NPC.ai[3] = NPC.whoAmI;
			NPC.realLife = NPC.whoAmI;
			int current = 0;
			int last = NPC.whoAmI;
			int type = ModContent.NPCType<Cleaver_Body>();
			for (int k = 0; k < 32; k++) {
				current = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI);
				Main.npc[current].ai[3] = NPC.whoAmI;
				Main.npc[current].realLife = NPC.whoAmI;
				Main.npc[current].ai[1] = last;
				Main.npc[current].spriteDirection = Main.rand.NextBool() ? 1 : -1;
				Main.npc[last].ai[0] = current;
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, current);
				last = current;
			}
			current = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Cleaver_Tail>(), NPC.whoAmI);
			Main.npc[current].ai[3] = NPC.whoAmI;
			Main.npc[current].realLife = NPC.whoAmI;
			Main.npc[current].ai[1] = last;
			Main.npc[current].spriteDirection = Main.rand.NextBool() ? 1 : -1;
			Main.npc[last].ai[0] = current;
			NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, current);
		}
	}

	internal class Cleaver_Body : Cleaver {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new() {
				Hide = true
			});
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerBody);
			NPC.width = NPC.height = 12;
		}
	}

	internal class Cleaver_Tail : Cleaver {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new() {
				Hide = true
			});
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerTail);
			NPC.width = NPC.height = 12;
		}
	}

	public abstract class Cleaver : Glowing_Mod_NPC {
		public override string GlowTexturePath => Texture;
		public override void AI() {
			if (NPC.realLife > -1) NPC.life = Main.npc[NPC.realLife].active ? NPC.lifeMax : 0;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			NPC current = Main.npc[NPC.realLife > -1 ? NPC.realLife : NPC.whoAmI];
			if (current.life < 0) {
				int skip = Main.rand.Next(2);
				while (current.ai[0] != 0) {
					if (++skip >= 2){
						deathEffect(current);
						skip = 0;
					}
					current = Main.npc[(int)current.ai[0]];
				}
			}
		}
		protected static void deathEffect(NPC npc) {
			OriginExtensions.LerpEquals(
				ref Gore.NewGoreDirect(
					npc.GetSource_Death(),
					npc.position,
					npc.velocity,
					Origins.instance.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4))
				).velocity,
				npc.velocity,
				0.5f
			);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			OriginPlayer.InflictTorn(target, 300, targetSeverity: 1f - 0.9f);
		}
	}
}