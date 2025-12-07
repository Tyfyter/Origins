using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
namespace Origins.Items.Weapons.Demolitionist {
	public class Heartache_Dynamite : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Dynamite);
			Item.damage = 85;
			Item.shoot = ModContent.ProjectileType<Heartache_Dynamite_P>();
			Item.shootSpeed *= 1.75f;
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			if (ModLoader.TryGetMod("HolidayLib", out Mod holidayLib)) {
				Func<bool> Day(string name) => (Func<bool>)holidayLib.Call("GETACTIVELOOKUP", name);
				Func<object[], object> _addHoliday = (Func<object[], object>)holidayLib.Call("GETFUNC", "ADDHOLIDAY");
				Func<bool> AddHoliday(params object[] args) {
					if (_addHoliday(args) is Exception e) throw e;
					return Day((string)args[0]);
				}
				const string name = "Saint Valentine's Day";
				Func<bool> holiday = AddHoliday(name, new DateTime(2012, Holiday_Hair_Dye.February, 14));
				Recipe.Create(Type, 12)
				.AddIngredient(ItemID.Dynamite, 12)
				.AddIngredient(ItemID.LifeCrystal)
				.AddCondition(Language.GetOrRegister("Mods.Origins.Conditions.Holiday").WithFormatArgs(name), () => holiday())
				.AddTile(TileID.Anvils)
				.Register();
				Recipe.Create(Type, 8)
				.AddIngredient(ItemID.Dynamite, 8)
				.AddIngredient(ItemID.LifeCrystal)
				.AddCondition(Language.GetOrRegister("Mods.Origins.Conditions.NotHoliday").WithFormatArgs(name), () => !holiday())
				.AddTile(TileID.Anvils)
				.Register();
			} else {
				Recipe.Create(Type, 8)
				.AddIngredient(ItemID.Dynamite, 8)
				.AddIngredient(ItemID.LifeCrystal)
				.AddTile(TileID.Anvils)
				.Register();
			}
		}
	}
	public class Heartache_Dynamite_P : ModProjectile {
		public override string Texture => typeof(Heartache_Dynamite).GetDefaultTMLName();
		public override LocalizedText DisplayName => Language.GetOrRegister($"Mods.Origins.Items.{nameof(Heartache_Dynamite)}.DisplayName");
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
			Origins.MagicTripwireDetonationStyle[Type] = 1;
			ProjectileID.Sets.Explosive[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Dynamite);
			Projectile.timeLeft = 360;
			Projectile.friendly = false;
			AIType = ProjectileID.Dynamite;
			DrawOriginOffsetY = -16;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.life <= 0) {
				int i = 0;
				while (i < 2 && Main.rand.NextBool(2 + i)) {
					i++;
				}
				bool scattered = i > 1;
				for (; i > 0; i--) {
					CommonCode.DropItem(target.Hitbox, target.GetSource_Loot(), ItemID.Heart, 1, scattered);
				}
			}
		}
		public override void PrepareBombToBlow() {
			Projectile.Resize(250, 250);
			Projectile.ai[1] = 0f;
			Projectile.friendly = true;
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile.Hitbox, SoundID.Item14);
			const int rad = 7;
			Vector2 center = Projectile.Center;
			int i = (int)(center.X / 16);
			int j = (int)(center.Y / 16);
			Projectile.ExplodeTiles(
				center,
				rad,
				i - rad,
				i + rad,
				j - rad,
				j + rad,
				Projectile.ShouldWallExplode(center, rad, i - rad, i + rad, j - rad, j + rad)
			);
		}
	}
}
