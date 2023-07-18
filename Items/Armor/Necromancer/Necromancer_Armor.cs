using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Necromancer {
	[AutoloadEquip(EquipType.Head)]
	public class Necromancer_Helmet : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Necromancer Crown");
			// Tooltip.SetDefault("25% increased summoning damage");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Summon) += 0.25f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Necromancer_Breastplate>() && legs.type == ModContent.ItemType<Necromancer_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = "Slain enemies provide a temporary boost to all stats\nMana usage is halved when in a Graveyard and artifact minions cost half as much\nEnemies spawn more frequently\n+3 minion slots";
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.artifactManaCost *= 0.5f;
			if (player.ZoneGraveyard) {
				player.manaCost *= 0.5f;
			}
			originPlayer.necroSet = true;
			float killMult = originPlayer.necroSetAmount * 0.002f;
			player.GetAttackSpeed(DamageClass.Summon) += Math.Min(0.1f * killMult, 0.6f);
			player.GetDamage(DamageClass.Summon) += Math.Min(0.1f * killMult, 0.75f);
			if (killMult > 0) {
				player.GetDamage(DamageClass.Summon) += (float)Math.Max(Math.Pow(0.1f * killMult, 0.5f), 0);
			}
			player.lifeRegenCount += (int)Math.Min(4 * killMult, 5);
			player.statDefense += (int)Math.Min(6 * killMult, 13);

			/*godmode dev set:
			float killMult = originPlayer.necroSetAmount * 0.002f;
			player.GetAttackSpeed(DamageClass.Summon) += 0.1f * killMult;
			player.GetDamage(DamageClass.Generic) += 0.1f * killMult;
			player.lifeRegenCount += (int)(killMult * 4);
			player.statDefense += (int)(12 * killMult);
			 */

			player.maxMinions += 3;
		}
		public override void ArmorSetShadows(Player player) {
			//player.armorEffectDrawOutlines = true;
			//player.armorEffectDrawOutlinesForbidden = true;
			//player.armorEffectDrawShadow = true;
			player.armorEffectDrawShadowLokis = true;
			player.armorEffectDrawShadowSubtle = true;
			/*if (Main.rand.NextBool(1)) {
				Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Blood, 0, 0, 165, new Color(80, 50, 50));
				dust.noGravity = true;
				dust.scale *= 1.02f;
			}*/
		}
		public override void UpdateVanitySet(Player player) {
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ChlorophyteBar, 12);
			recipe.AddIngredient(ItemID.DarkShard);
			recipe.AddRecipeGroupWithItem(OriginSystem.CursedFlameRecipeGroupID, showItem: ModContent.ItemType<Black_Bile>(), 7);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Body)]
	public class Necromancer_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Necromancer Breastplate");
			// Tooltip.SetDefault("15% increased summoning attack speed\n+1 minion slot");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 14;
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.maxMinions += 1;
			player.GetAttackSpeed(DamageClass.Summon) += 0.15f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ChlorophyteBar, 24);
			recipe.AddIngredient(ItemID.DarkShard);
			recipe.AddRecipeGroupWithItem(OriginSystem.CursedFlameRecipeGroupID, showItem: ModContent.ItemType<Black_Bile>(), 7);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Necromancer_Greaves : ModItem {
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Necromancer Greaves");
			// Tooltip.SetDefault("25% increased artifact minion damage");
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.defense = 10;
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().artifactDamage += 0.25f;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.ChlorophyteBar, 18);
			recipe.AddIngredient(ItemID.DarkShard);
			recipe.AddRecipeGroupWithItem(OriginSystem.CursedFlameRecipeGroupID, showItem: ModContent.ItemType<Black_Bile>(), 7);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
	}
}
