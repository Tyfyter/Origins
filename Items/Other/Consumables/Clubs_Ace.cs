﻿using Origins.Items.Accessories;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Clubs_Ace : ModItem {
		
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ManaCrystal);
			Item.rare = ItemRarityID.Cyan;
			Item.UseSound = SoundID.Item139.WithPitch(0.2f);
			Item.maxStack = 1;
		}
		public override bool CanUseItem(Player player) {
			return false; // (player.name == "Pandora" && !NPC.AnyNPCs(ModContent.NPCType<SCP_Portal>())); spawns a portal that has a random chance to give item, npc, or cause event
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				SoundEngine.PlaySound(
					new SoundStyle("Origins/Sounds/Custom/Defiled_Idle3") {
						Pitch = -1,
						Volume = 0.66f
					}, player.itemLocation
				);
			}
			return true;
		}
		public override void AddRecipes() {
			//Recipe.Create(Type)
			//.AddIngredient(ModContent.ItemType<No_U_Card>())
			//.AddCondition(player.name == "Pandora")
			//.Register();
		}
	}
}
