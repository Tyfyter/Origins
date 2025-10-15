using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Grenade_Lawnchair : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Launcher
		];
		public static AutoCastingAsset<Texture2D> UseTexture { get; private set; }
		public override void Unload() {
			UseTexture = null;
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				UseTexture = ModContent.Request<Texture2D>(Texture + "_Use");
			}
		}
		public override void SetDefaults() {
			Item.knockBack = 5.75f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 40;
			Item.useTime = 40;
			Item.width = 50;
			Item.height = 14;
			Item.shoot = ProjectileID.Grenade;
			Item.UseSound = SoundID.Item36;
			Item.shootSpeed = 5.35f;
			Item.noMelee = true;
			Item.damage = 10;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.useAmmo = ItemID.Grenade;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
			Item.useLimitPerAnimation = 3;
		}
		public override float UseTimeMultiplier(Player player) => 4f / Item.useTime;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int count = Main.rand.Next(6, 9);
			Vector2 speed = new(velocity.Length() * player.direction, 0);
			damage -= damage / 2;
			for (int i = count; i-- > 0;) {
				Projectile.NewProjectile(source, position, speed.RotatedBy(-2 * (i / (float)count) * player.direction) * Main.rand.NextFloat(0.9f, 1), type, damage, knockback, player.whoAmI);
			}
			return false;
		}
	}
}
