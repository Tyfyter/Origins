using Microsoft.Xna.Framework;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo.Canisters {
	public class RocketsAsCanisters : GlobalItem {
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.useAmmo == ModContent.ItemType<Resizable_Mine_Wood>();
		public override bool? CanChooseAmmo(Item weapon, Item ammo, Player player) {
			if (ammo.ammo == AmmoID.Rocket) {
				return CanisterGlobalItem.ItemToCanisterID.ContainsKey(ammo.type);
			}
			return null;
		}
		public override void SetStaticDefaults() {
			//uncomment, then insert outer & inner colors
			/*
			new Rocket_Dummy_Canister(new(, , false), 128).Register(ItemID.RocketI);
			new Rocket_Dummy_Canister(new(, , false), 128, 3).Register(ItemID.RocketII);
			new Rocket_Dummy_Canister(new(, , false), 200).Register(ItemID.RocketIII);
			new Rocket_Dummy_Canister(new(, , false), 200, 5).Register(ItemID.RocketIV);
			new Rocket_Dummy_Canister(new(, , false), 250).Register(ItemID.MiniNukeI);
			new Rocket_Dummy_Canister(new(, , false), 250, 7).Register(ItemID.MiniNukeII);

			new Cluster_Rocket_Dummy_Canister(new(, , true), ProjectileID.ClusterFragmentsI).Register(ItemID.ClusterRocketI);
			new Cluster_Rocket_Dummy_Canister(new(, , true), ProjectileID.ClusterFragmentsII).Register(ItemID.ClusterRocketII);

			new Liquid_Rocket_Dummy_Canister(new(, , true), DelegateMethods.SpreadDry).Register(ItemID.DryRocket);
			new Liquid_Rocket_Dummy_Canister(new(, , true), DelegateMethods.SpreadWater).Register(ItemID.WetRocket);
			new Liquid_Rocket_Dummy_Canister(new(, , true), DelegateMethods.SpreadLava).Register(ItemID.LavaRocket);
			new Liquid_Rocket_Dummy_Canister(new(, , true), DelegateMethods.SpreadHoney).Register(ItemID.HoneyRocket);
			*/
		}
	}
	public class Rocket_Dummy_Canister(CanisterData canisterData, int explosionSize, int tileDestructionRadius = 0) : ICanisterAmmo {
		public void Register(int itemType) {
			GetCanisterData.Ammo = this;
			CanisterGlobalItem.RegisterCanister(itemType, GetCanisterData);
		}
		public CanisterData GetCanisterData => canisterData;
		public virtual void AI(Projectile projectile, bool child) { }
		public virtual void OnKill(Projectile projectile, bool child) {
			ExplosiveGlobalProjectile.DoExplosion(projectile, explosionSize);
			if (tileDestructionRadius > 0) {
				Vector2 center = projectile.Center;
				int i = (int)(center.X / 16);
				int j = (int)(center.Y / 16);
				projectile.ExplodeTiles(
					center,
					tileDestructionRadius,
					i - tileDestructionRadius,
					i + tileDestructionRadius,
					j - tileDestructionRadius,
					j + tileDestructionRadius,
					projectile.ShouldWallExplode(center, tileDestructionRadius, i - tileDestructionRadius, i + tileDestructionRadius, j - tileDestructionRadius, j + tileDestructionRadius)
				);
			}
		}
	}
	public class Cluster_Rocket_Dummy_Canister(CanisterData canisterData, int fragmentType, int explosionSize = 128, int tileDestructionRadius = 0) : Rocket_Dummy_Canister(canisterData, explosionSize, tileDestructionRadius) {
		public override void OnKill(Projectile projectile, bool child) {
			base.OnKill(projectile, child);
			if (projectile.owner != Main.myPlayer) return;
			float startingAngle = Main.rand.NextFloat(MathHelper.TwoPi);
			for (float i = 0f; i < 1f; i += 1f / 6f) {
				float angle = startingAngle + i * MathHelper.TwoPi;
				Vector2 velocity = GeometryUtils.Vec2FromPolar(Main.rand.NextFloat(4f, 6f), angle);
				velocity += Vector2.UnitY * -1f;
				Projectile.NewProjectileDirect(
					projectile.GetSource_Death(),
					projectile.Center,
					velocity,
					fragmentType,
					projectile.damage / 2,
					0f
				).timeLeft -= Main.rand.Next(30);
			}
		}
	}
	public class Liquid_Rocket_Dummy_Canister(CanisterData canisterData, Utils.TileActionAttempt tileAction, int explosionSize = 48) : Rocket_Dummy_Canister(canisterData, explosionSize) {
		public override void AI(Projectile projectile, bool child) {
			projectile.timeLeft = 1;
		}
		public override void OnKill(Projectile projectile, bool child) {
			base.OnKill(projectile, child);
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks(projectile.Center.ToTileCoordinates(), 3f, tileAction);
			}
		}
	}
}
