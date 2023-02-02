using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Magic_Glove : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Magic Glove");
			Tooltip.SetDefault("5% reduced mana usage\nAutomatically use mana potions when needed\nShoots random magic as you swing\n'May require magical capability'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaultsKeepSlots(ItemID.YoYoGlove);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;

			Item.damage -= 69420;
			Item.DamageType = DamageClass.Magic;
			Item.useTime = 5;
			Item.useAnimation = 14;
			Item.shootSpeed = 14;
			Item.mana = 3;
			Item.UseSound = SoundID.Item4;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.gunGlove = true;
			originPlayer.gunGloveItem = Item;
			player.manaCost -= 0.05f;
			player.manaFlower = true;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.ManaCloak);
			recipe.AddIngredient(ModContent.ItemType<Gun_Glove>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			switch (Main.rand.Next(7)) {
				case 0:
				type = ProjectileID.AmberBolt;
				break;

				case 1:
				type = ProjectileID.MagicMissile;
				break;

				case 2:
				type = ProjectileID.WaterBolt;
				break;

				case 3:
				type = ProjectileID.DemonScythe;
				break;

				case 4:
				type = ProjectileID.WandOfSparkingSpark;
				break;

				case 5:
				type = ProjectileID.ZapinatorLaser;
				break;

				case 6:
				type = ProjectileID.Bee;
				break;
			}
		}
	}
}
