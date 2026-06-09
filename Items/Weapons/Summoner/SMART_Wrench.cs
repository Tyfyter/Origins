using CalamityMod.NPCs.TownNPCs;
using Fargowiltas.Items.Ammos;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Items.Weapons.Summoner.Minions;
using Origins.Projectiles;
using Origins.Tiles.Other;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class SMART_Wrench : ModItem, ISometimesUseCanisters {
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileFallback[Type] = ItemID.RocketLauncher;
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				if (proj.TryGetGlobalProjectile(out FriendlyGlobalProjectile friendlyGlobal)) {
					friendlyGlobal.DamageTypeOverride = proj.CountsAsClass(DamageClasses.Explosive) ? DamageClasses.ExplosiveVersion[DamageClass.Summon] : DamageClass.Summon;
					friendlyGlobal.forceMinionShot = true;
				}
			});
		}
		public override void SetDefaults() {
			Item.damage = 40;
			Item.knockBack = 1;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 18;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.buffType = ModContent.BuffType<Smart_Turret_Buff>();
			Item.shoot = ModContent.ProjectileType<Smart_Turret>();
			Item.shootSpeed = 1;// used as a multiplier
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item44;
			Item.noMelee = true;
		}
		public override void AddRecipes() => Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 15)
			.AddIngredient(ModContent.ItemType<Power_Core>())
			.AddIngredient(ModContent.ItemType<Rotor>(), 5)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		public override bool CanUseItem(Player player) {
			Item.useAmmo = AmmoID.None;
			if (player.ownedProjectileCounts[Item.shoot] < 2) return true;
			ProjectileID.Sets.MinionCannotBeFreed[Item.shoot] = true;
			return true;
		}
		public override bool CanShoot(Player player) {
			Item.useAmmo = AmmoID.None;
			return true;
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Item.useAmmo = Smart_Turret.UseAmmoType(Main.LocalPlayer);
			return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.ownedProjectileCounts[Item.shoot] >= 2) type = Smart_Turret_Counter.ID;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			player.SpawnMinionOnCursor(source, player.whoAmI, type, Item.damage, knockback);
			return false;
		}
	}
	public class Smart_Turret_Buff : MinionBuff {
		public override string Texture => "Origins/Buffs/Smart_Turret_Buff";
		public override IEnumerable<int> ProjectileTypes() => [
			ModContent.ProjectileType<Smart_Turret>(),
			ModContent.ProjectileType<Smart_Turret_Counter>()
		];
		protected override void SetBuffFlag(Player player) => player.OriginPlayer().smartTurret = true;
	}
}
namespace Origins.Items.Weapons.Summoner.Minions {
	public class Smart_Turret : MinionBase {
		static readonly AutoLoadingTexture skateTexture = typeof(Smart_Turret).GetDefaultTMLName("_Skate");
		public readonly struct Bullets : IFiringMode {
			float IFiringMode.SortIndex => 0;
			public int ShootTime => 9;
			public int BaseVelocity => 8;
			public int UseAmmo => AmmoID.Bullet;
			static readonly AutoLoadingTexture texture = typeof(Smart_Turret).GetDefaultTMLName("_Head");
			public void Draw(Projectile turret, Vector2 pivot, Color lightColor, SpriteEffects effects) {
				Rectangle frame = texture.Frame(
					verticalFrames: 8,
					frameY: (int)((turret.ai[1] / ShootTime + (turret.localAI[2] % 2)) * 4) % 8
				);
				Main.EntitySpriteDraw(
					texture,
					pivot,
					frame,
					lightColor,
					turret.rotation,
					effects.ApplyToOrigin(new(14, 8), frame),
					turret.scale,
					effects
				);
			}
		}
		public readonly struct Canisters : IFiringMode {
			float IFiringMode.SortIndex => 1;
			public int ShootTime => 30;
			public int BaseVelocity => 14;
			public int UseAmmo => ModContent.ItemType<Resizable_Mine_Wood>();
			static readonly AutoLoadingTexture texture = typeof(Smart_Turret).GetDefaultTMLName("_Head_Canista");
			static readonly AutoLoadingTexture glowTexture = typeof(Smart_Turret).GetDefaultTMLName("_Head_Canista_Glow");
			public void Draw(Projectile turret, Vector2 pivot, Color lightColor, SpriteEffects effects) {
				DrawData data = new(
					texture,
					pivot,
					null,
					lightColor,
					turret.rotation,
					effects.ApplyToOrigin(new(10, 8), texture.Value.Bounds),
					turret.scale,
					effects
				);
				Main.EntitySpriteDraw(data);
				data.texture = glowTexture;
				data.color = Color.White;
				Main.EntitySpriteDraw(data);
			}
			public bool Fire(Player owner, Projectile turret, Item fromItem, in TargetingData target) {
				if (IFiringMode.IsBlocked(turret, in target)) return false;
				if (!owner.PickAmmo(fromItem, out _, out float speed, out int damage, out float knockBack, out int usedAmmo)) return false;
				Vector2 gunPos = turret.Center - new Vector2(9 * turret.direction, 20);
				SoundEngine.PlaySound(SoundID.Item61, gunPos);
				turret.SpawnProjectile(
					new EntitySource_ItemUse_WithAmmo(owner, fromItem, usedAmmo),
					gunPos,
					(target.Center - gunPos).Normalized(out _) * speed,
					ModContent.ProjectileType<Smart_Turret_Canister_P>(),
					damage,
					knockBack
				);
				return true;
			}
		}
		public readonly struct Rockets : IFiringMode {
			float IFiringMode.SortIndex => 2;
			public int ShootTime => 20;
			public int BaseVelocity => 8;
			public int UseAmmo => AmmoID.Rocket;
			static readonly AutoLoadingTexture texture = typeof(Smart_Turret).GetDefaultTMLName("_Head_Misil");
			static readonly AutoLoadingTexture glowTexture = typeof(Smart_Turret).GetDefaultTMLName("_Head_Misil_Glow");
			public void Draw(Projectile turret, Vector2 pivot, Color lightColor, SpriteEffects effects) {
				DrawData data = new(
					texture,
					pivot,
					null,
					lightColor,
					turret.rotation,
					effects.ApplyToOrigin(new(10, 18), texture.Value.Bounds),
					turret.scale,
					effects
				);
				Main.EntitySpriteDraw(data);
				data.texture = glowTexture;
				data.color = Color.White;
				Main.EntitySpriteDraw(data);
			}
		}
		public static int UseAmmoType(Player player) => firingModes[GetUpgradeCount(player)].UseAmmo;
		public static int GetUpgradeCount(Player player) => player.ownedProjectileCounts[Smart_Turret_Counter.ID];
		public int UpgradeCount => GetUpgradeCount(Owner);
		protected IFiringMode FiringMode => firingModes[UpgradeCount];
		Item fromItem;
		public override bool AutomaticRotationAndDirection => false;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.projFrames[Type] = 12;
			Origins.tickers.Add(new SetCanBeFreed(Type));
		}
		readonly struct SetCanBeFreed(int type) : ITicker {
			void ITicker.Tick() => ProjectileID.Sets.MinionCannotBeFreed[type] = false;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.width = 46;
			Projectile.height = 48;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.ignoreWater = false;
			Projectile.manualDirectionChange = true;
			Projectile.netImportant = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse { Item: Item item }) fromItem = item;
		}
		public override void MoveTowardsTarget() {
			if (fromItem is null || fromItem.ModItem is not SMART_Wrench) {
				Projectile.Kill();
				return;
			}
			Vector2 directionToIdlePosition = (Projectile.Center.Clamp(RestRegion) - Projectile.Center).Normalized(out float distanceToIdlePosition);
			if (Projectile.IsLocallyOwned()) {
				if (distanceToIdlePosition > 600) {
					if (distanceToIdlePosition > 2000) {
						Projectile.Center = RestRegion.Center();
						Projectile.velocity *= 0.1f;
						Projectile.netUpdate = true;
					} else {
						Projectile.ai[2] = 1;
						Projectile.netUpdate = true;
					}
				}
			}
			if (Projectile.ai[2] == 1) {
				Projectile.localAI[1] = 300;
				float speed = 16 * SpeedModifier;
				float inertia = 12f;
				Min(ref speed, distanceToIdlePosition);
				Vector2 direction = directionToIdlePosition * speed;
				Projectile.velocity = (Projectile.velocity * (inertia - 1) + direction) / inertia;
				if (Math.Abs(directionToIdlePosition.X * distanceToIdlePosition) > 1) Projectile.direction = Math.Sign(directionToIdlePosition.X);
				else Projectile.direction = Owner.direction;
				Projectile.tileCollide = false;
				if (Projectile.localAI[0].CycleUp(6, 1)) Projectile.frame = (Projectile.frame + 1) % 18;
				if (distanceToIdlePosition > 64 || Projectile.Hitbox.OverlapsAnyTiles()) return;
				if (!Collision.WetCollision(Projectile.position, Projectile.width, Projectile.height + 16)) {
					Rectangle floorbox = Projectile.Hitbox;
					floorbox.Offset(0, Projectile.height);
					floorbox.Height = 16 * 4;
					if (!floorbox.OverlapsAnyTiles(false)) return;
				}
				Projectile.ai[2] = 0;
				Projectile.netUpdate = true;
				Projectile.frame = 0;
			}
			Projectile.tileCollide = true;
			float walkSpeed = 0.1f * SpeedModifier;
			const float walkDrag = 0.95f;
			Projectile.minionSlots = UpgradeCount > 0 ? float.BitDecrement(1) : 1;
			Rectangle target = targetingData.targetHitbox;
			bool shouldWalk = false;
			if (targetingData.HasTarget) {
				Vector2 diff = target.Center() - Projectile.Center;
				Projectile.direction = Math.Sign(diff.X);
				diff += new Vector2(9 * Projectile.direction, 20);
				if (OriginExtensions.AngularSmoothing(ref Projectile.rotation, diff.ToRotation(), 0.15f) && Projectile.ai[1].CycleDown(FiringMode.ShootTime, SpeedModifier) && Projectile.IsLocallyOwned()) {
					using ScopedOverride<int> _a = fromItem.useAmmo.ScopedOverride(FiringMode.UseAmmo);
					using ScopedOverride<int> _s = fromItem.shoot.ScopedOverride(ProjectileID.None);
					using ScopedOverride<float> _ = fromItem.shootSpeed.ScopedOverride(fromItem.shootSpeed * FiringMode.BaseVelocity);
					if (FiringMode.Fire(Owner, Projectile, fromItem, in targetingData)) {
						Projectile.localAI[2]++;
					} else {
						Projectile.ai[1] = 0;
					}
				}
			} else {
				target = RestRegion;
				target.Inflate(0, 16 * 100);
				Projectile.direction = Math.Sign(target.Center().X - Projectile.Center.X);
				shouldWalk = !target.Intersects(Projectile.Hitbox);
				OriginExtensions.AngularSmoothing(ref Projectile.rotation, MathHelper.PiOver2 - 1.6f * Projectile.direction, 0.05f);
				Projectile.ai[0] = -2;
			}
			if (shouldWalk) {
				float dir = Projectile.velocity.X * Projectile.direction;
				if (dir > 0) {
					if (Projectile.localAI[0].CycleUp(6, dir)) Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
				} else if (dir < 0) {
					if (Projectile.localAI[0].CycleDown(6, dir)) Projectile.frame = (Projectile.frame + Main.projFrames[Type] - 1) % Main.projFrames[Type];
				}
				Projectile.velocity.X += walkSpeed * Projectile.direction;
			} else if (Projectile.frame is not 0 and not 6 && Projectile.localAI[0].CycleUp(8)) Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
			Projectile.velocity.X *= walkDrag;
			Projectile.velocity.Y += 0.4f;
			Projectile.spriteDirection = Projectile.direction;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X) {
				int dir = Math.Sign(oldVelocity.X);
				Vector2 collisionPos = (Projectile.Bottom + new Vector2(18 * dir, 0));
				if (Framing.GetTileSafely(collisionPos.ToTileCoordinates()).HasFullSolidTile() && !Framing.GetTileSafely((collisionPos - new Vector2(0, 12)).ToTileCoordinates()).HasFullSolidTile()) {
					Projectile.velocity.Y = -5;
				}
			}
			return false;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			Collision.StepDown(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
			Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
			fallThrough = targetingData.targetHitbox.Y > Projectile.BottomLeft.Y && (!targetingData.HasTarget || !CollisionExt.CanHitRay(Projectile.Center, targetingData.targetHitbox.Center()));
			width = 38;
			height = 36;
			hitboxCenterFrac = new(0.5f + 2f * Projectile.direction / width, 0.5f - 6f / height);
			return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			int frameCount = Main.projFrames[Type];
			if (Projectile.ai[2] != 0) {
				texture = skateTexture;
				frameCount = 18;
			}
			SpriteEffects spriteEffects = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Rectangle frame = texture.Frame(verticalFrames: frameCount, frameY: Projectile.frame);
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				frame,
				lightColor,
				0,
				(spriteEffects ^ SpriteEffects.FlipHorizontally).ApplyToOrigin(new(23, 29), frame),
				Projectile.scale,
				spriteEffects
			);

			FiringMode.Draw(Projectile, Projectile.Center - Main.screenPosition - new Vector2(9 * Projectile.direction, 20), lightColor, spriteEffects switch {
				SpriteEffects.FlipHorizontally => SpriteEffects.FlipVertically,
				_ => SpriteEffects.None,
			});
			return false;
		}
		public override ref bool HasBuff(Player player) => ref player.OriginPlayer().smartTurret;
		static readonly ClampedList<IFiringMode> firingModes = [];
		protected interface IFiringMode : IAutoload<IFiringMode.LoadImpl>, IComparable<IFiringMode> {
			protected float SortIndex { get; }
			int ShootTime { get; }
			int BaseVelocity { get; }
			int UseAmmo { get; }
			int IComparable<IFiringMode>.CompareTo(IFiringMode other) => SortIndex.CompareTo(other.SortIndex);
			public sealed static bool IsBlocked(Projectile turret, in TargetingData target) => !CollisionExt.CanHitRay(turret.Center, target.Center);
			void Draw(Projectile turret, Vector2 pivot, Color lightColor, SpriteEffects effects);
			bool Fire(Player owner, Projectile turret, Item fromItem, in TargetingData target) {
				if (IsBlocked(turret, in target)) return false;
				if (!owner.PickAmmo(fromItem, out int projType, out float speed, out int damage, out float knockBack, out int usedAmmo)) return false;
				Vector2 gunPos = turret.Center - new Vector2(9 * turret.direction, 20);
				SoundEngine.PlaySound(Origins.Sounds.EnergyRipple.WithPitch(1f).WithVolume(0.25f), gunPos);
				SoundEngine.PlaySound(SoundID.Item26.WithPitchRange(1.2f, 1.28f).WithVolume(0.1f), gunPos);
				SoundEngine.PlaySound(SoundID.Item35.WithPitchRange(0.2f, 0.3f).WithVolume(0.2f), gunPos);
				turret.SpawnProjectile(
					new EntitySource_ItemUse_WithAmmo(owner, fromItem, usedAmmo),
					gunPos,
					(target.Center - gunPos).Normalized(out _) * speed,
					projType,
					damage,
					knockBack
				);
				return true;
			}
			struct LoadImpl : IAutoloader {
				public static void Autoload(Mod mod, Type type) => firingModes.InsertOrdered((IFiringMode)Activator.CreateInstance(type));
			}
		}
		readonly struct ClampedList<T>(IList<T> list) : IList<T> {
			public ClampedList() : this([]) { }
			public T this[int index] { get => list[int.Clamp(index, 0, Count - 1)]; set => list[int.Clamp(index, 0, Count - 1)] = value; }
			public int Count => list.Count;
			public bool IsReadOnly => list.IsReadOnly;
			public void Add(T item) => list.Add(item);
			public void Clear() => list.Clear();
			public bool Contains(T item) => list.Contains(item);
			public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
			public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
			public int IndexOf(T item) => list.IndexOf(item);
			public void Insert(int index, T item) => list.Insert(index, item);
			public bool Remove(T item) => list.Remove(item);
			public void RemoveAt(int index) => list.RemoveAt(index);
			IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)list).GetEnumerator();
		}
	}
	public class Smart_Turret_Canister_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingTexture outerTexture = ICanisterProjectile.base_texture_path + "Canister_Outer";
		public static AutoLoadingTexture innerTexture = ICanisterProjectile.base_texture_path + "Canister_Inner";
		public AutoLoadingTexture OuterTexture => outerTexture;
		public AutoLoadingTexture InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Summon];
			Projectile.timeLeft = 420;
			Projectile.penetrate = 1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 5;
		}
		public override void AI() {
			Projectile.velocity.Y -= 0.2f;
			this.DoGravity(0.2f);
		}
	}
	public class Smart_Turret_Counter : ModProjectile {
		public override string Texture => base.Texture.Replace("_Counter", null); // this one actually doesn't need a texture
		public override void SetStaticDefaults() {
			// This is necessary for right-click targeting
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;
			// This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			OriginsSets.Projectiles.SupportsRealSpeedBuffs[Type] = static (_, _) => { };
			ID = Type;
		}
		public static int ID { get; private set; }
		public override void SetDefaults() {
			Projectile.netImportant = true;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.timeLeft = 60;
			Projectile.aiStyle = ProjAIStyleID.DesertTigerBall;
			Projectile.hide = true;
		}
		public override bool PreAI() {
			Player player = Main.player[Projectile.owner];
			if (player.ownedProjectileCounts[Projectile.type] > 1 && Projectile.localAI[0] == 0f) {
				Projectile.localAI[0] = 1f;
				SoundEngine.PlaySound(in SoundID.AbigailUpgrade, Projectile.Center);
			}
			ref bool smartTurret = ref player.OriginPlayer().smartTurret;
			if (player.dead) smartTurret = false;
			else if (smartTurret) Projectile.timeLeft = 2;
			return true;
		}
	}
}
