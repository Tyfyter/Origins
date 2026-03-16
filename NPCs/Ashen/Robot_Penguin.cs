using Origins.Dev;
using Origins.Projectiles;
using PegasusLib.Networking;
using System.IO;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Ashen {
	public class Robot_Penguin : Glowing_Mod_NPC, IAshenEnemy, IWikiNPC {
		public Rectangle DrawRect => new(0, 4, 32, 40);
		public int AnimationFrames => 24;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 3;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.CrimsonPenguin);
			NPC.damage = 21;
			NPC.lifeMax = 75;
			NPC.value = 500f;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			AnimationType = NPCID.CrimsonPenguin;
			AIType = NPCID.CrimsonPenguin;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon
			});
		}
		public override bool PreAI() {
			if (NPC.ai[0] > 0) {
				DoExplosion();
				if (NPC.ai[0] == 1) NPC.active = false;
				return false;
			}
			return true;
		}
		public void DoExplosion() {
			if (NPC.ai[0] > 0) {
				Vector2 oldCenter = NPC.Center;
				NPC.Size = new(64);
				NPC.Center = oldCenter;
				NPC.velocity = default;
				NPC.noGravity = true;
				NPC.dontTakeDamage = true;
				NPC.chaseable = false;
				NPC.immortal = true;
				if (NPC.ai[0]-- == 3) ExplosiveGlobalProjectile.ExplosionVisual(NPC.Hitbox, SoundID.Item14);
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			new Explode_Critter_Action(NPC).Perform();
		}
		public override void OnKill() {
			NPC.ai[0] = 3;
			DoExplosion();
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.PedguinHat, 150));
			npcLoot.Add(ItemDropRule.Common(ItemID.PedguinShirt, 150));
			npcLoot.Add(ItemDropRule.Common(ItemID.PedguinPants, 150));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore1");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore2");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore3");
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore4");
				for (int i = 0; i < 2; i++) {
					Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
				}
			} else if (Main.rand.NextBool(5)) {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, "Gores/NPCs/Ashen_Gore" + Main.rand.Next(1, 5));
			}
		}
	}
	public record class Explode_Critter_Action(NPC Npc) : SyncedAction {
		public override bool ServerOnly => true;
		public Explode_Critter_Action() : this(default(NPC)) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			Npc = Main.npc[reader.ReadInt16()]
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write(Npc.whoAmI);
		}
		protected override void Perform() {
			if (Npc.ai[0] == 0) Npc.ai[0] = 3;
		}
	}
}
