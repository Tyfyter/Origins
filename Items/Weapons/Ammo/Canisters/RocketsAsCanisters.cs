using Origins.Dusts;
using Origins.Liquids;
using Origins.Projectiles;
using Origins.Projectiles.Weapons;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo.Canisters {
	public class FixRocketShootType : GlobalItem {
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.ammo == AmmoID.Rocket;
		public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
			if (weapon.useAmmo == ModContent.ItemType<Resizable_Mine_Wood>()) {
				type = weapon.shoot;
			}
		}
	}
	public class RocketsAsCanisters : GlobalItem {
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.useAmmo == ModContent.ItemType<Resizable_Mine_Wood>();
		public override bool? CanChooseAmmo(Item weapon, Item ammo, Player player) {
			if (ammo.ammo == AmmoID.Rocket) {
				return CanisterGlobalItem.ItemToCanisterID.ContainsKey(ammo.type);
			}
			return null;
		}
		public override void SetStaticDefaults() {
			static CanisterData Canister(uint outer, uint inner, bool special = false) {
				static Color FromHex(uint value) {
					return new((int)((0xff0000 & value) >> 16), (int)((0x00ff00 & value) >> 8), (int)((0x0000ff & value) >> 0));
				}
				return new(FromHex(outer), FromHex(inner), special);
			}
			new Rocket_Dummy_Canister(Canister(0xc3c3c3, 0xed1c24), 128).Register(ItemID.RocketI);
			new Rocket_Dummy_Canister(Canister(0x4b4b4b, 0x63c45e), 128, 3).Register(ItemID.RocketII);
			new Rocket_Dummy_Canister(Canister(0xc3c3c3, 0x066bff), 200).Register(ItemID.RocketIII);
			new Rocket_Dummy_Canister(Canister(0x4b4b4b, 0xecc800), 200, 5).Register(ItemID.RocketIV);
			new Rocket_Dummy_Canister(Canister(0xea9427, 0x82af7b), 250).Register(ItemID.MiniNukeI);
			new Rocket_Dummy_Canister(Canister(0xed1c24, 0xfec214), 250, 7).Register(ItemID.MiniNukeII);

			new Cluster_Rocket_Dummy_Canister(Canister(0x82af7b, 0xc76812, true), ProjectileID.ClusterFragmentsI).Register(ItemID.ClusterRocketI);
			new Cluster_Rocket_Dummy_Canister(Canister(0xc3c3c3, 0xecc800, true), ProjectileID.ClusterFragmentsII).Register(ItemID.ClusterRocketII);

			new Liquid_Rocket_Dummy_Canister(Canister(0xeaeaea, 0xeaeaea, true), DelegateMethods.SpreadDry, 3.5f).Register(ItemID.DryRocket);
			new Liquid_Rocket_Dummy_Canister(Canister(0xc3c3c3, 0x5698ff, true), DelegateMethods.SpreadWater).Register(ItemID.WetRocket);
			new Liquid_Rocket_Dummy_Canister(Canister(0x655dc5, 0xf59300, true), DelegateMethods.SpreadLava).Register(ItemID.LavaRocket);
			new Liquid_Rocket_Dummy_Canister(Canister(0x9b7c40, 0xfec214, true), DelegateMethods.SpreadHoney).Register(ItemID.HoneyRocket);
			new Liquid_Rocket_Dummy_Canister(Canister(0x0A0A0A, 0x3B2B1F, true), BaseLiquidRocketP.SpreadLiquid<Oil, Black_Smoke_Dust>()).Register(ModContent.ItemType<Oil_Rocket>()); // TODO: select better colors
		}
	}
	public class Rocket_Dummy_Canister(CanisterData canisterData, int explosionSize, int tileDestructionRadius = 0) : ICanisterAmmo {
		public readonly CanisterData canisterData = canisterData;
		public readonly int explosionSize = explosionSize;
		public readonly int tileDestructionRadius = tileDestructionRadius;
		public void Register(int itemType) {
			GetCanisterData.Ammo = this;
			CanisterGlobalItem.RegisterCanister(itemType, GetCanisterData);
		}
		public CanisterData GetCanisterData => canisterData;
		public virtual void AI(Projectile projectile, bool child) { }
		public virtual void OnKill(Projectile projectile, bool child) {
			if (projectile.ModProjectile is ICanisterProjectile canister) {
				canister.DefaultExplosion(projectile);
			} else {
				ExplosiveGlobalProjectile.DoExplosion(projectile, explosionSize);
			}
			SoundEngine.PlaySound(SoundID.Item14, projectile.Center);
			int tileDestructionRadius = this.tileDestructionRadius;
			if (tileDestructionRadius > 0) {
				tileDestructionRadius *= 16;
				tileDestructionRadius = (int)projectile.GetGlobalProjectile<ExplosiveGlobalProjectile>().modifierBlastRadius
					.CombineWith(Main.player[projectile.owner].GetModPlayer<OriginPlayer>().explosiveBlastRadius)
					.Scale(0.5f)
					.ApplyTo(tileDestructionRadius);
				tileDestructionRadius /= 16;
				Vector2 center = projectile.Center;
				int i = (int)(center.X / 16);
				int j = (int)(center.Y / 16);
				int minI = Math.Max(i - tileDestructionRadius, 0);
				int maxI = Math.Min(i + tileDestructionRadius, Main.maxTilesX);
				int minJ = Math.Max(j - tileDestructionRadius, 0);
				int maxJ = Math.Min(j + tileDestructionRadius, Main.maxTilesY);
				projectile.ExplodeTiles(
					center,
					tileDestructionRadius,
					minI,
					maxI,
					minJ,
					maxJ,
					projectile.ShouldWallExplode(center, tileDestructionRadius, minI, maxI, minJ, maxJ)
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
	public class Liquid_Rocket_Dummy_Canister(CanisterData canisterData, Utils.TileActionAttempt tileAction, float liquidSize = 3f, int explosionSize = 48) : Rocket_Dummy_Canister(canisterData, explosionSize) {
		public override void AI(Projectile projectile, bool child) {
			if (projectile.wet) projectile.timeLeft = 1;
		}
		public override void OnKill(Projectile projectile, bool child) {
			base.OnKill(projectile, child);
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				Point pos = projectile.Center.ToTileCoordinates();
				Tile tile = Main.tile[pos.X, pos.Y];
				if (tile != null && tile.HasTile && tile.BlockType == BlockType.Solid) {
					Vector2 offsetCenter = projectile.Center;
					if (projectile.velocity == Vector2.Zero) {
						while (tile != null && tile.HasTile && tile.BlockType == BlockType.Solid) {
							Point offsetPos = offsetCenter.ToTileCoordinates();
							tile = Main.tile[pos.X, pos.Y];
							offsetCenter += GeometryUtils.Vec2FromPolar(4, projectile.rotation + MathHelper.PiOver2);
							pos = offsetPos;
						}
					} else {
						offsetCenter -= projectile.velocity;
						pos = offsetCenter.ToTileCoordinates();
					}
				}
				float liquidRadius = projectile.GetGlobalProjectile<ExplosiveGlobalProjectile>().modifierBlastRadius
					.CombineWith(Main.player[projectile.owner].GetModPlayer<OriginPlayer>().explosiveBlastRadius)
					.ApplyTo(liquidSize);
				projectile.Kill_DirtAndFluidProjectiles_RunDelegateMethodPushUpForHalfBricks(pos, liquidRadius, tileAction);
			}
		}
	}
}
