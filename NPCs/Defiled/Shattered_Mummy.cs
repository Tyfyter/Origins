using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Defiled
{
    public class Shattered_Mummy : ModNPC, IDefiledEnemy {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 4;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 180;
			NPC.defense = 18;
			NPC.knockBackResist = 0.5f;
			NPC.damage = 60;
			NPC.width = 20;
			NPC.height = 44;
			NPC.value = 700;
			NPC.friendly = false;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.value = 700;
			SpawnModBiomes = new int[] {
				ModContent.GetInstance<Defiled_Wastelands_Desert>().Type
			};
		}
		public int MaxMana => 100;
		public int MaxManaDrain => 20;
		public float Mana { get; set; }
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			this.DrainMana(target);
		}
		public void Regenerate(out int lifeRegen) {
			int factor = Main.rand.RandomRound((180f / NPC.life) * 8);
			lifeRegen = factor;
			Mana -= factor / 180f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText("The {$Defiled_Wastelands} did not struggle to rapidly break down this fiend. It clunks around the {$Defiled} sands in search of any trespassers."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.DarkShard, 10));
			npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Megaphone, 100));
			npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Blindfold, 100));
			npcLoot.Add(ItemDropRule.Common(ItemID.MummyMask, 75));
			npcLoot.Add(ItemDropRule.Common(ItemID.MummyShirt, 75));
			npcLoot.Add(ItemDropRule.Common(ItemID.MummyPants, 75));
		}
		public override void AI() {
			if (Main.rand.NextBool(800)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle, NPC.Center);
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.FaceTarget();
				NPC.spriteDirection = NPC.direction;
			}
			//increment frameCounter every frame and run the following code when it exceeds 7 (i.e. run the following code every 8 frames)
			if (++NPC.frameCounter > 7) {
				//add frame height (with buffer) to frame y position and modulo by frame height (with buffer) multiplied by walking frame count
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 50) % 200, 32, 48);
				//reset frameCounter so this doesn't trigger every frame after the first time
				NPC.frameCounter = 0;
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
			//spawn gore if npc is dead after being hit
			if (NPC.life < 0) {
				for (int i = 0; i < 3; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF3_Gore"));
				for (int i = 0; i < 6; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4)));
			}
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Mana);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Mana = reader.ReadSingle();
		}
	}
}
