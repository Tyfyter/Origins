using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
    public class Bloodletter : Harpoon_Gun {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bloodletter");
			Tooltip.SetDefault("Uses harpoons as ammo\n83.3% chance not to consume ammo");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.damage = 24;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 4;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.useAnimation = 3;
			Item.useTime = 3;
			Item.reuseDelay = 2;
			Item.width = 58;
			Item.height = 22;
			Item.useAmmo = Harpoon.ID;
			Item.shoot = Harpoon_P.ID;
			Item.shootSpeed = 15.25f;
			Item.UseSound = SoundID.Item11;
			Item.value = Item.buyPrice(gold: 1, silver: 30);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ItemID.CrimtaneBar, 8);
			recipe.AddIngredient(ModContent.ItemType<Harpoon_Gun>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override void OnConsumeAmmo(Item ammo, Player player) {
			if (!Main.rand.NextBool(6)) {
				ammo.stack++;
			} else {
				consume = true;
			}
		}
		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
			target.AddBuff(BuffID.Bleeding, 300);
		}
	}
}
