using Origins.Dev;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Myth {
	[AutoloadEquip(EquipType.Head)]
	public class Mythic_Skull : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            WikiCategories.ArmorSet,
            WikiCategories.ExplosiveBoostGear
        ];
        public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 90);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().explosiveProjectileSpeed += 0.15f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Mythic_Shell>() && legs.type == ModContent.ItemType<Mythic_Leggings>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Mythic");
			ref StatModifier explosiveProjectileSpeed = ref player.GetModPlayer<OriginPlayer>().explosiveProjectileSpeed;
			ref StatModifier explosiveDamage = ref player.GetDamage(DamageClasses.Explosive);
			const float maxSpeed = 16f;
			float speed = Math.Min(player.velocity.Length(), maxSpeed);
			//the literals here are the values that'll be applied when the player is moving at whatever speed is determined by maxSpeed
			//whatever these are is the maximum possible bonus provided by the set bonus
			explosiveDamage += speed / (maxSpeed / 0.2f);
			explosiveDamage.Flat += speed / (maxSpeed / 10f);

			explosiveProjectileSpeed += speed / (maxSpeed / 0.5f);
			explosiveProjectileSpeed.Flat += speed / (maxSpeed / 3f);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Bone, 10)
			.AddIngredient(ItemID.Cloud, 5)
			.AddIngredient(ItemID.Feather, 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public string ArmorSetName => "Mythic_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Mythic_Shell>();
		public int LegsItemID => ModContent.ItemType<Mythic_Leggings>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Mythic_Shell : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.jumpSpeedBoost += 0.45f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Bone, 25)
			.AddIngredient(ItemID.Cloud, 15)
			.AddIngredient(ItemID.Feather, 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Mythic_Leggings : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 4;
			Item.value = Item.sellPrice(silver: 60);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.2f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Bone, 15)
			.AddIngredient(ItemID.Cloud, 10)
			.AddIngredient(ItemID.Feather, 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
