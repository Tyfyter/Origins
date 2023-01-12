using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Single_Cellular_Nautilus : Glowing_Mod_NPC {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Single-Cellular Nautilus");
            Main.npcFrameCount[NPC.type] = 3;
            SpawnModBiomes = new int[] {
                ModContent.GetInstance<Riven_Hive>().Type
            };
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.Tumbleweed);
            NPC.aiStyle = NPCAIStyleID.Unicorn;
            NPC.lifeMax = 70;
            NPC.defense = 12;
            NPC.damage = 33;
            NPC.width = 38;
            NPC.height = 38;
            NPC.friendly = false;
            NPC.knockBackResist = 1f;
            NPC.frame.Height = 30;
            NPC.value = 200;
        }
		public override void PostAI() {
            NPC.rotation += NPC.velocity.X / 24f;
            NPC.frameCounter++;
            if (NPC.frameCounter >= 24) NPC.frameCounter = 0;
            NPC.frame.Y = ((int)NPC.frameCounter / 8) * 32;
            if (NPC.velocity.X != 0) NPC.spriteDirection = Math.Sign(NPC.velocity.X);
        }
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement(""),
            });
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 1, 1, 3));
        }
        public override void HitEffect(int hitDirection, double damage) {
            //spawn gore if npc is dead after being hit
            if(NPC.life<0) {
                for(int i = 0; i < 3; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)));
                for(int i = 0; i < 6; i++)Gore.NewGore(NPC.GetSource_Death(), NPC.position+new Vector2(Main.rand.Next(NPC.width),Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Meat1"));
            }
        }
    }
}
