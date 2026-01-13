using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Acrid_Handcannon : ModItem, ICustomWikiStat {
        public string[] Categories => [
			WikiCategories.ToxicSource
        ];
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			ID = Type;
		}
		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 72;
			Item.crit = 7;
			Item.width = 56;
			Item.height = 26;
			Item.useTime = 57;
			Item.useAnimation = 57;
			Item.shoot = ModContent.ProjectileType<Acrid_Slug_P>();
			Item.useAmmo = ModContent.ItemType<Metal_Slug>();
			Item.knockBack = 8f;
			Item.shootSpeed = 12f;
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = Origins.Sounds.Krunch.WithPitch(-0.25f);
			Item.autoReuse = true;
            Item.ArmorPenetration += 6;
        }
		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.IllegalGunParts, 2)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 18)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == Metal_Slug_P.ID) type = Item.shoot;
            Vector2 offset = (velocity.RotatedBy(MathHelper.PiOver2 * -player.direction) * 5) / velocity.Length();
            position += offset;
        }
	}
	public class Acrid_Slug_P : Metal_Slug_P {
		public override string Texture => "Origins/Projectiles/Ammo/Acrid_Slug_P";
	}
	/*public override void OnKill(int timeLeft) {
		int t = ModContent.ProjectileType<Acid_Shot>();
		for (int i = Main.rand.Next(3); i < 6; i++) Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Main.rand.NextVector2Unit() * 4) + (Projectile.velocity / 8), t, Projectile.damage / 8, 6, Projectile.owner, ai1: -0.5f).scale = 0.85f;
	} */
}
