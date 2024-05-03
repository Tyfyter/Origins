using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;
using static Microsoft.Xna.Framework.MathHelper;

namespace Origins.Items.Weapons.Demolitionist {
	public class Nova_Swarm : ModItem, ICustomWikiStat {
		public const float rocket_scale = 0.85f;
        public string[] Categories => new string[] {
            "Launcher"
        };
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[Type] = AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher];
			ID = Type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ProximityMineLauncher);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 110;
			Item.noMelee = true;
			Item.width = 60;
			Item.height = 30;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Red;
			Item.autoReuse = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 18)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (velocity != default) position += Vector2.Normalize(velocity) * 54;
			velocity = velocity.RotatedByRandom(0.15f);
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-7, -4);
		}
	}
}
