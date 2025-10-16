using Origins.Dev;
using Origins.Items.Materials;
using Origins.Journal;
using Origins.Projectiles.Weapons;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Splashid : ModItem, IElementalItem, IJournalEntrySource {
		static short glowmask;
		public string[] Categories => [
			WikiCategories.Wand,
			WikiCategories.ToxicSource
		];
		public string EntryName => "Origins/" + typeof(Splashid_Entry).Name;
		public class Splashid_Entry : JournalEntry {
			public override string TextKey => "Splashid";
			public override JournalSortIndex SortIndex => new("Brine_Fiend", 5);
		}
		public ushort Element => Elements.Acid;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.staff[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 52;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.reuseDelay = 8;
			Item.mana = 18;
			Item.shoot = ModContent.ProjectileType<Brine_Droplet>();
			Item.value = Item.sellPrice(gold: 2, silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Alkaliphiliac_Tissue>(), 38)
			.AddIngredient(ModContent.ItemType<Brineglow_Item>(), 12)
			.AddIngredient(ModContent.ItemType<Baryte_Item>(), 25)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int a = Main.rand.Next(5, 7);
			//++i < a: increments i and returns true if i < a/2
			//a = Main.rand.Next(5,7): randomize a every loop so ((i-a/2f)/a) returns a random value but maintains a mostly constant spread
			for (int i = 0; ++i < a; a = Main.rand.Next(5, 7)) {
				//((i-a/2f)/a): returns a value based on i between -0.5 and 0.5
				Projectile.NewProjectileDirect(source, position, velocity.RotatedBy(((i - a / 2f) / a) * 0.75), type, damage, knockback, player.whoAmI, 0, 12f).timeLeft += i;
			}
			return false;
		}
	}
}
