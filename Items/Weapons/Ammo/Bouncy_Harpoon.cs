using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
namespace Origins.Items.Weapons.Ammo {
	public class Bouncy_Harpoon : ModItem, ICustomWikiStat {
		public static int ID { get; private set; }
        public string[] Categories => [
            "Harpoon"
        ];
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = 99;
			Item.shoot = Bouncy_Harpoon_P.ID;
			Item.ammo = Harpoon.ID;
			Item.value = Item.sellPrice(silver: 4);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 8)
			.AddRecipeGroup(RecipeGroupID.IronBar, 8)
			.AddIngredient(ItemID.PinkGel)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(Type, 8)
            .AddIngredient(ItemID.PinkGel)
            .AddIngredient(ModContent.ItemType<Harpoon>(), 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Bouncy_Harpoon_P : Harpoon_P {
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.penetrate = 17;
			if (Projectile.ai[1] == 1) {
				Projectile.maxPenetrate = 1;
			}
		}
		public override void AI() {
			if (Projectile.ai[0] == 1 && Projectile.maxPenetrate == 1) {
				Projectile.aiStyle = 1;
				Projectile.velocity = Projectile.oldVelocity;
				Projectile.tileCollide = true;
				Vector2 diff = Main.player[Projectile.owner].itemLocation - Projectile.Center;
				SoundEngine.PlaySound(SoundID.Item10, Projectile.Center + diff / 2);
				float len = diff.Length() * 0.25f;
				diff /= len;
				Vector2 pos = Projectile.Center;
				for (int i = 0; i < len; i++) {
					Dust.NewDust(pos - new Vector2(2), 4, 4, DustID.Stone, Scale: 0.75f);
					pos += diff;
				}
			}
			if (Projectile.penetrate == 1) {
				Projectile.penetrate--;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.penetrate = Projectile.maxPenetrate == 1 ? 2 : -1;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (--Projectile.penetrate <= 1) return true;
			Projectile.velocity = oldVelocity *
				new Vector2(Projectile.velocity.X == oldVelocity.X ? 1 : -1, Projectile.velocity.Y == oldVelocity.Y ? 1 : -1);
			return false;
		}
	}
}
