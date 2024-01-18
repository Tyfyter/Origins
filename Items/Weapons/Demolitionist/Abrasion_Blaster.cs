using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Abrasion_Blaster : ModItem {
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				proj.extraUpdates += 2;
			});
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SniperRifle);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 29;
			Item.crit = 14;
			Item.useAnimation = 32;
			Item.useTime = 16;
			Item.shoot = ModContent.ProjectileType<Dreikan_Shot>();
			Item.ammo = ModContent.ItemType<Scrap>();
			Item.reuseDelay = 6;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.LightPurple;
		}
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Biocomponent10>(), 35);
            recipe.AddIngredient(ModContent.ItemType<Scrap>(), 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == ProjectileID.Bullet) type = Item.shoot;
			SoundEngine.PlaySound(SoundID.Item40, position);
			SoundEngine.PlaySound(SoundID.Item36.WithVolume(0.75f), position);
		}
	}
}
