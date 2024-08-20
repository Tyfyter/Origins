using Origins.Dev;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Necromancer {
    [AutoloadEquip(EquipType.Head)]
	public class Necromancer_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            "HardmodeArmorSet",
            "SummonBoostGear"
        ];
        public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Summon) += 0.25f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Necromancer_Breastplate>() && legs.type == ModContent.ItemType<Necromancer_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Necromancer");
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
		public string ArmorSetName => "Necromancer_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Necromancer_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Necromancer_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Necromancer_Breastplate : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 14;
			Item.value = Item.sellPrice(gold: 5);
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
	public class Necromancer_Greaves : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 10;
			Item.value = Item.sellPrice(gold: 5);
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
