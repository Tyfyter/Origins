using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
    public class Riven_Splitter : Harpoon_Gun, ICustomWikiStat {
		static short glowmask;
        public string[] Categories => new string[] {
            "HarpoonGun"
        };
        public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.damage = 30;
			Item.DamageType = DamageClass.Ranged;
			Item.knockBack = 4;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.useAnimation = 3;
			Item.useTime = 3;
			Item.reuseDelay = 2;
			Item.width = 56;
			Item.height = 26;
			Item.useAmmo = Harpoon.ID;
			Item.shoot = Harpoon_P.ID;
			Item.shootSpeed = 18.75f;
			Item.UseSound = SoundID.Item11;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.autoReuse = true;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 9);
			recipe.AddIngredient(ModContent.ItemType<Riven_Carapace>(), 3);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
		public override Vector2? HoldoutOffset() => new Vector2(-8, 0);
		public override void OnConsumeAmmo(Item ammo, Player player) {
			if (!Main.rand.NextBool(8)) {
				ammo.stack++;
			} else {
				consume = true;
			}
		}
	}
}
