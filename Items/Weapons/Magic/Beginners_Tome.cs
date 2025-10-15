using Origins.Dev;
using Origins.Journal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	public class Beginners_Tome : ModItem, IJournalEntrySource, ICustomWikiStat {
        public string[] Categories => [
			WikiCategories.UsesBookcase,
			WikiCategories.SpellBook,
			WikiCategories.LoreItem
		];
		public string EntryName => "Origins/" + typeof(Beginners_Tome_Entry).Name;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 16;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.mana = 8;
			Item.shoot = ModContent.ProjectileType<Beginner_Spell>();
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.Book)
			.AddIngredient(ItemID.FallenStar)
			.AddIngredient(ItemID.WandofSparking)
			.AddTile(TileID.Bookcases)
			.Register();
		}
		public class Beginners_Tome_Entry : JournalEntry {
			public override string TextKey => "Beginners_Tome";
			public override JournalSortIndex SortIndex => new("The_Dungeon", 2);
		}
	}
	public class Beginner_Spell : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_125";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RubyBolt);
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
		}
		public override void AI() {
			Dust dust = Main.dust[Terraria.Dust.NewDust(Projectile.Center, 0, 0, DustID.GemRuby, 0f, 0f, 0, new Color(255, 0, 0), 1f)];
			dust.noGravity = true;
			dust.velocity /= 2;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.extraUpdates == 0) {
				return true;
			} else {
				Projectile.extraUpdates = 0;
				if (Projectile.velocity.Y != oldVelocity.Y) {
					Projectile.velocity.Y = 0f - oldVelocity.Y;
				}
				if (Projectile.velocity.X != oldVelocity.X) {
					Projectile.velocity.X = 0f - oldVelocity.X;
				}
			}
			return false;
		}
	}
}
