using Microsoft.Xna.Framework;
using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Magic_Glove : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.MagicBoostAcc
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 24);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
			Item.damage = 8;
			Item.DamageType = DamageClass.Magic;
			Item.useTime = 5;
			Item.useAnimation = 14;
			Item.shootSpeed = 3;
			Item.mana = 3;
			Item.UseSound = SoundID.Item4;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.gunGlove = true;
			originPlayer.gunGloveItem = Item;
			player.manaCost -= 0.08f;
			player.manaFlower = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.ManaCloak)
			.AddIngredient(ModContent.ItemType<Gun_Glove>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
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
		public override bool MagicPrefix() => false;
	}
}
