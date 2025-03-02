using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Armor.Fiberglass;
using Origins.Items.Armor.Vanity.BossMasks;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.LootBags;
using Origins.Items.Weapons.Crossmod;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.LootConditions;
using Origins.NPCs.Brine;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine.Boss {
	[AutoloadBossHead]
	public class Lost_Diver : Brine_Pool_NPC {
		internal static IItemDropRule normalDropRule;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.CantTakeLunchMoney[Type] = false;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new() {
				Position = new Vector2(0f, -32f),
				PortraitPositionXOverride = 0f,
				PortraitPositionYOverride = -32f
			};
		}
		public override void Unload() {
			normalDropRule = null;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.boss = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.damage = 14;
			NPC.lifeMax = 25000;
			NPC.defense = 26;
			NPC.aiStyle = 0;
			NPC.width = NPC.height = 68;
			NPC.knockBackResist = 0.1f;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.value = Item.buyPrice(gold: 5);
		}
		public override void AI() {
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			normalDropRule = ItemDropRule.OneFromOptionsNotScalingWithLuck(1,
				ModContent.ItemType<Boat_Rocker>(),
				ModContent.ItemType<Depth_Charge>(),
				ModContent.ItemType<Torpedo_Tube>(),
				ModContent.ItemType<Mildew_Whip>(),
				Watered_Down_Keytar.ID
			).WithOnSuccess(ItemDropRule.Common(ModContent.ItemType<Lost_Diver_Helmet>(), 10)
			.WithOnSuccess(ItemDropRule.Common(ModContent.ItemType<Lost_Diver_Chest>()))
			.WithOnSuccess(ItemDropRule.Common(ModContent.ItemType<Lost_Diver_Greaves>()))
			);

			npcLoot.Add(new DropBasedOnExpertMode(
				normalDropRule,
				new DropLocalPerClientAndResetsNPCMoneyTo0(ModContent.ItemType<Lost_Diver_Bag>(), 1, 1, 1, null)
			));
			npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Faith_Beads>(), 4));
			npcLoot.Add(new DropInstancedPerClient(ModContent.ItemType<Crown_Jewel>()));
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
			}
		}
		public override void OnKill() {
			Boss_Tracker.Instance.downedLostDiver = true;
			NetMessage.SendData(MessageID.WorldData);
		}
	}
}
