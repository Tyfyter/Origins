using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Weapons.Magic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Magic_Pain_Glove : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Combat"
		};
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 24);
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightPurple;
			Item.damage -= 8;
			Item.DamageType = DamageClass.Magic;
			Item.useTime = 5;
			Item.useAnimation = 14;
			Item.shootSpeed += 1;
			Item.mana = 3;
			Item.UseSound = SoundID.Item4;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.gunGlove = true;
			originPlayer.gunGloveItem = Item;
			player.manaCost -= 0.08f;
			player.manaFlower = true;
            player.GetAttackSpeed(DamageClass.Melee) += 0.12f;
			player.autoReuseGlove = true;
        }
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.FeralClaws);
			recipe.AddIngredient(ModContent.ItemType<Magic_Glove>());
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			switch (Main.rand.Next(8)) {
				case 0:
				type = ModContent.ProjectileType<Amber_Of_Embers_P>();
				break;

				case 1:
				type = ProjectileID.RainbowRodBullet;
				break;

				case 2:
				type = ModContent.ProjectileType<Bled_Out_Staff_P>();
                break;

				case 3:
				type = ProjectileID.Flames;
                break;

				case 4:
				type = ModContent.ProjectileType<Seam_Beam_Beam>();
                break;

				case 5:
				type = ModContent.ProjectileType<Laser_Tag_Laser>();
                break;

				case 6:
				type = ProjectileID.LostSoulFriendly;
				break;

                case 7:
                type = ProjectileID.ShadowBeamFriendly;
                break;
            }
		}
	}
}
