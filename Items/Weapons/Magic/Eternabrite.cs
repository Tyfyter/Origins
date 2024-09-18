using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Microsoft.Xna.Framework;
using Origins.Reflection;

namespace Origins.Items.Weapons.Magic {
    public class Eternabrite : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "UsesBookcase",
            "SpellBook"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Flamethrower);
			Item.damage = 28;
			Item.DamageType = DamageClass.Magic;
			Item.shoot = ModContent.ProjectileType<Eternabrite_P>();
			Item.mana = 14;
			Item.useAmmo = AmmoID.None;
			Item.noUseGraphic = false;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.knockBack = 8;
			Item.shootSpeed = 14f;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item82;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Book)
			.AddIngredient(ItemID.HellstoneBar, 12)
			.AddIngredient(ItemID.WandofSparking)
			.AddTile(TileID.Bookcases)
			.Register();
		}
    }
	public class Eternabrite_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Flames;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Flames);
			Projectile.DamageType = DamageClass.Magic;
			AIType = ProjectileID.Flames;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire, 240);
		}
		public override bool PreDraw(ref Color lightColor) {
			MainReflection.DrawProj_Flamethrower(Projectile);
			return false;
		}
	}
}
