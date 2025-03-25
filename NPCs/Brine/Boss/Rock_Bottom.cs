using Microsoft.Xna.Framework.Graphics;
using Origins.CrossMod.Thorium.Items.Weapons.Bard;
using Origins.Items.Accessories;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.LootBags;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.Journal;
using Origins.Tiles.BossDrops;
using Origins.Tiles.Brine;
using Origins.World.BiomeData;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.NPCs.Brine.Boss {
	public class Rock_Bottom : Brine_Pool_NPC, IInteractableNPC {
		public bool NeedsSync => true;
		public override bool CanChat() {
			if (Main.mouseRight && Main.npcChatRelease) {
				Interact();
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					ModPacket packet = Mod.GetPacket();
					packet.Write(Origins.NetMessageType.entity_interaction);
					packet.Write((byte)NPC.whoAmI);
					packet.Send(-1, Main.myPlayer);
				}
			}
			return false;
		}
		public void Interact() {
			NPC.ai[1] = 1;
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 15;
			NPCID.Sets.CantTakeLunchMoney[Type] = false;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
			Mildew_Creeper.FriendlyNPCTypes.Add(Type);
			ModContent.GetInstance<Brine_Pool.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.dontTakeDamage = true;
			NPC.lifeMax = 6000;
			NPC.defense = 24;
			NPC.noGravity = true;
			NPC.width = 76;
			NPC.height = 58;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) => NPC.AnyNPCs(Type) ? 0 : Brine_Pool.SpawnRates.Dead_Guy;
		public override bool PreAI() {
			NPC.velocity *= 0.6f;
			if (NPC.ai[1] == 0) return false;
			switch ((int)NPC.ai[0]) {
				default:
				NPC.ai[0] += 1f / 15;
				break;
			}
			NPC.frame.Y = NPC.frame.Height * (int)NPC.ai[0];
			if (NPC.ai[0] >= Main.npcFrameCount[Type]) {
				NPC.Transform(ModContent.NPCType<Lost_Diver>());
			}
			return false;
		}
	}
}
