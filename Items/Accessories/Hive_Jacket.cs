using Origins.Dev;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Front)]
	public class Hive_Jacket : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat
		];
		public override void SetStaticDefaults() {
			OriginsSets.Armor.Front.DrawsInNeckLayer[Item.frontSlot] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(34, 30);
			Item.damage = 81;
			Item.knockBack = 4f;
			Item.defense = 1;
			Item.shoot = ModContent.ProjectileType<Hive_Jacket_Explosion>();
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 1);
			Item.ArmorPenetration += 3;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			OriginPlayer oP = player.OriginPlayer();
			player.endurance += (1 - player.endurance) * 0.05f;
			oP.flakJacketItem = Item;
			oP.hiveJacket = true;
		}
		public override bool WeaponPrefix() => true;
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.HoneyComb)
			.AddIngredient<Flak_Jacket>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Hive_Jacket_Explosion : ExplosionProjectile {
		public override DamageClass DamageType => DamageClasses.Explosive;
		public override int Size => 80;
		public override bool DealsSelfDamage => false;
		public override void SetStaticDefaults() {
			OriginsSets.Projectiles.NoMultishot[Type] = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				Player owner = Main.player[Projectile.owner];
				int count = 2 + Main.rand.Next(3) + (owner.strongBees && Main.rand.NextBool()).ToInt();
				float rot = MathHelper.TwoPi / count;
				for (int i = count; i > 0; i--) {
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Projectile.Center,
						GeometryUtils.Vec2FromPolar(5, rot * i + Main.rand.NextFloat(0.3f) * Main.rand.NextBool().ToDirectionInt()) + Main.rand.NextVector2Unit(),
						owner.beeType(),
						owner.beeDamage(Projectile.damage / 3),
						owner.beeKB(Projectile.knockBack * 0.05f),
						Projectile.owner
					);
				}
			}
			base.AI();
		}
	}
}
