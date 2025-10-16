using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Melee {
    public class Snapthrow : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Yoyo
        ];
        public override void SetStaticDefaults() {
			// These are all related to gamepad controls and don't seem to affect anything else
			ItemID.Sets.Yoyo[Item.type] = true; // Used to increase the gamepad range when using Strings.
			ItemID.Sets.GamepadExtraRange[Item.type] = 10; // Increases the gamepad range. Some vanilla values: 4 (Wood), 10 (Valor), 13 (Yelets), 18 (The Eye of Cthulhu), 21 (Terrarian).
			ItemID.Sets.GamepadSmartQuickReach[Item.type] = true; // Unused, but weapons that require aiming on the screen are in this set.
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CorruptYoyo);
			Item.shoot = ModContent.ProjectileType<Snapthrow_P>();
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 12)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] <= 0;
		}
	}
	public class Snapthrow_P : ModProjectile {
		public override void SetStaticDefaults() {
			// YoyosLifeTimeMultiplier is how long in seconds the yoyo will stay out before automatically returning to the player. 
			// Vanilla values range from 3f (Wood) to 16f (Chik), and defaults to -1f. Leaving as -1 will make the time infinite.
			ProjectileID.Sets.YoyosLifeTimeMultiplier[Type] = 1f;

			// YoyosMaximumRange is the maximum distance the yoyo sleep away from the player. 
			// Vanilla values range from 130f (Wood) to 400f (Terrarian), and defaults to 200f.
			ProjectileID.Sets.YoyosMaximumRange[Type] = 200f;

			// YoyosTopSpeed is top speed of the yoyo Projectile.
			// Vanilla values range from 9f (Wood) to 17.5f (Terrarian), and defaults to 10f.
			ProjectileID.Sets.YoyosTopSpeed[Type] = 15f;
		}
		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;

			Projectile.aiStyle = ProjAIStyleID.Yoyo;

			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1; // All vanilla yoyos have infinite penetration. The number of enemies the yoyo can hit before being pulled back in is based on YoyosLifeTimeMultiplier.
		}
		public override void OnSpawn(IEntitySource source) {
			const float testMagnitude = 2;
			const float snapRange = 16 * 4;
			Vector2 teleportPos = Main.MouseWorld;
			float dist = snapRange * snapRange;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (npc.CanBeChasedBy(Projectile)) {
					Vector2 currentPos = Main.MouseWorld.Clamp(npc.Hitbox);
					float currentDist = (currentPos - Main.MouseWorld).LengthSquared();
					if (currentDist < dist && Collision.IsClearSpotTest(currentPos - Projectile.Size * 0.5f, testMagnitude, Projectile.width, Projectile.height, true, true, checkSlopes: true)) {
						teleportPos = currentPos;
						dist = currentDist;
					}
				}
			}
			if (dist != snapRange * snapRange || Collision.IsClearSpotTest(teleportPos - Projectile.Size * 0.5f, testMagnitude, Projectile.width, Projectile.height, true, true, checkSlopes: true)) {
				Vector2 diff = (teleportPos - Projectile.Center);
				Vector2 direction = diff.SafeNormalize(Vector2.Zero) * 8;
				float maxRange = ProjectileID.Sets.YoyosMaximumRange[Type];
				if (Main.player[Projectile.owner].yoyoString) {
					maxRange = maxRange * 1.25f + 30f;
				}
				Projectile.Center += diff.WithMaxLength(maxRange);
				Projectile.velocity = direction;
				for (int i = 0; i < 5; i++) {
					Dust.NewDust(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						DustID.WhiteTorch
					);
				}
			}
		}
	}
}
