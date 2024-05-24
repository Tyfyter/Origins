using AltLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Weapons.Demolitionist;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Ammo.Canisters {
	#region global stuff
	public class CanisterGlobalItem : GlobalItem {
		public static Dictionary<int, int> ItemToCanisterID { get; private set; } = [];
		public static Dictionary<Type, int> TypeToCanisterID { get; private set; } = [];
		public static List<CanisterData> CanisterDatas { get; private set; } = [];
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			if (entity.ModItem is ICanisterAmmo canister) {
				if (!ItemToCanisterID.TryGetValue(entity.type, out _)) {
					ItemToCanisterID.Add(entity.type, CanisterDatas.Count);
					TypeToCanisterID.Add(entity.ModItem.GetType(), CanisterDatas.Count);
					CanisterDatas.Add(canister.GetCanisterData with { WhatAmI = CanisterDatas.Count, Ammo = canister });
				}
				return true;
			}
			return false;
		}
		public static int GetCanisterType(int type) {
			return ItemToCanisterID.TryGetValue(type, out int canisterID) ? canisterID : -1;
		}
		public static int GetCanisterType(Type type) {
			return TypeToCanisterID.TryGetValue(type, out int canisterID) ? canisterID : - 1;
		}
	}
	public interface ICanisterAmmo {
		CanisterData GetCanisterData { get; }
		void AI(Projectile projectile, bool child) { }
		public void OnKill(Projectile projectile, bool child) => CanisterGlobalProjectile.DefaultExplosion(projectile, child);
	}
	public record class CanisterData(Color OuterColor, Color InnerColor, int WhatAmI = -999, ICanisterAmmo Ammo = null);
	public class CanisterGlobalProjectile : GlobalProjectile {
		public override bool InstancePerEntity => true;
		ICanisterProjectile canister;
		int canisterID;
		CanisterData canisterData;
		public int CanisterID { 
			get => canisterID;
			set {
				canisterID = value;
				if (canisterID == -1) return;
				canisterData = CanisterGlobalItem.CanisterDatas[value];
			}
		}
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			if (entity.ModProjectile is ICanisterProjectile) {
				return true;
			}
			return false;
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			canister = projectile.ModProjectile as ICanisterProjectile;
			if (source is EntitySource_ItemUse_WithAmmo ammoSource) {
				CanisterID = CanisterGlobalItem.GetCanisterType(ammoSource.AmmoItemIdUsed);
			}
		}
		public override void AI(Projectile projectile) {
			canisterData?.Ammo?.AI(projectile, false);
		}
		public override void OnKill(Projectile projectile, int timeLeft) {
			canisterData?.Ammo?.OnKill(projectile, false);
		}
		public override bool PreDraw(Projectile projectile, ref Color lightColor) {
			Vector2 origin = canister.OuterTexture.Value.Size() * 0.5f;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				canister.InnerTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.InnerColor,
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				canister.OuterTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			return false;
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			binaryWriter.Write((int)CanisterID);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			canister = projectile.ModProjectile as ICanisterProjectile;
			CanisterID = binaryReader.ReadInt32();
		}
		public static void DefaultExplosion(Projectile projectile, bool child) {
			if (child) return;
			projectile.penetrate = -1;
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 96;
			projectile.height = 96;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
			ExplosiveGlobalProjectile.DealSelfDamage(projectile);
			ExplosiveGlobalProjectile.ExplosionVisual(projectile, true, sound: SoundID.Item62);
		}
	}
	public class CanisterChildGlobalProjectile : GlobalProjectile {
		public override bool InstancePerEntity => true;
		int canisterID;
		CanisterData canisterData;
		public int CanisterID {
			get => canisterID;
			set {
				canisterID = value;
				if (canisterID == -1) return;
				canisterData = CanisterGlobalItem.CanisterDatas[value];
			}
		}
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			if (entity.ModProjectile is ICanisterChildProjectile) {
				return true;
			}
			return false;
		}
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile parentProj && parentProj.TryGetGlobalProjectile(out CanisterGlobalProjectile parent)) {
				CanisterID = parent.CanisterID;
			}
		}
		public override void AI(Projectile projectile) {
			canisterData?.Ammo?.AI(projectile, true);
		}
		public override void OnKill(Projectile projectile, int timeLeft) {
			canisterData?.Ammo?.OnKill(projectile, true);
		}
	}
	public interface ICanisterProjectile {
		public const string base_texture_path = "Origins/Items/Weapons/Ammo/Canisters/";
		public abstract AutoLoadingAsset<Texture2D> OuterTexture { get; }
		public abstract AutoLoadingAsset<Texture2D> InnerTexture { get; }
	}
	public interface ICanisterChildProjectile {}
	#endregion global stuff
	public class Coolant_Canister : ModItem, ICanisterAmmo, ICustomWikiStat {
		static short glowmask;
		public CanisterData GetCanisterData => new(new(99, 206, 236), new(178, 255, 255));
		public string[] Categories => [
			"Canistah"
		];
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RocketI);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.useStyle = ItemUseStyleID.None;
			Item.damage = 30;
			Item.ammo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shootSpeed = 4.1f;
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(silver: 3, copper: 2);
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 10)
			.AddIngredient(ItemID.Fireblossom)
			.AddRecipeGroup(RecipeGroupID.IronBar, 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public void OnKill(Projectile projectile, bool child) {
			if (child) return;
			Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, Vector2.Zero, ProjectileID.StardustGuardianExplosion, 0, 0, projectile.owner, -1, 1);
		}
	}
	public class Napalm_Canister : ModItem, ICanisterAmmo, ICustomWikiStat {
		static short glowmask;
		public CanisterData GetCanisterData => new(new(211, 194, 182), new(255, 163, 68));
		public string[] Categories => [
			"Canistah"
		];
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RocketI);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.useStyle = ItemUseStyleID.None;
			Item.damage = 30;
			Item.ammo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shootSpeed = 4.1f;
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(silver: 3, copper: 2);
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 10)
			.AddIngredient(ItemID.Fireblossom)
			.AddRecipeGroup(RecipeGroupID.IronBar, 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public void OnKill(Projectile projectile, bool child) {
			if (child) return;
			int damage = projectile.damage;
			projectile.damage = (int)(projectile.damage * 0.75f);
			projectile.knockBack = 16f;
			projectile.position = projectile.Center;
			projectile.width = projectile.height = 52;
			projectile.Center = projectile.position;
			projectile.Damage();
			ExplosiveGlobalProjectile.DealSelfDamage(projectile);
			projectile.damage = damage;
			if (projectile.type == Thermite_Canister_P.ID) {
				Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 0, 0, projectile.owner, -1, 1);
			} else {
				ExplosiveGlobalProjectile.ExplosionVisual(projectile, true, sound: SoundID.Item62);
			}
			for (int i = 0; i < 5; i++) {
				Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, (projectile.velocity / 2) + GeometryUtils.Vec2FromPolar((i / Main.rand.NextFloat(5, 7)) * MathHelper.TwoPi, Main.rand.NextFloat(2, 4)), ModContent.ProjectileType<Napalm_P>(), (int)(projectile.damage * 0.65f), 0, projectile.owner);
			}
		}
	}
	public class Napalm_P : ModProjectile, ICanisterChildProjectile {
		public override string Texture => "Origins/Projectiles/Ammo/Napalm_Pellet_P";

		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.friendly = true;
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.aiStyle = 1;
			Projectile.penetrate = 25;
			Projectile.timeLeft = Main.rand.Next(300, 451);
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 15;
		}
		public override void AI() {
			float v = 0.75f + (float)(0.125f * (Math.Sin(Projectile.timeLeft / 5f) + 2 * Math.Sin(Projectile.timeLeft / 60f)));
			Lighting.AddLight(Projectile.Center, v, v * 0.5f, 0);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = height = 2;
			fallThrough = true;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.ai[0] == 0f) {
				Projectile.ai[0] = 1f;
				Projectile.aiStyle = 0;
				//Projectile.tileCollide = false;
				//Projectile.position+=Vector2.Normalize(oldVelocity)*2;
			}
			Projectile.velocity *= 0.9f;
			//Projectile.velocity = Vector2.Zero;
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire, Main.rand.Next(300, 451));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.OnFire, Main.rand.Next(300, 451));
		}
		public override Color? GetAlpha(Color lightColor) {
			int v = 200 + (int)(25 * (Math.Sin(Projectile.timeLeft / 5f) + Math.Sin(Projectile.timeLeft / 60f)));
			return new Color(v + 20, v + 25, v - 90, 0);
		}
	}
	public class Cursed_Canister : ModItem, ICanisterAmmo, ICustomWikiStat {
		static short glowmask;
		public CanisterData GetCanisterData => new(new(190, 81, 216), new(126, 255, 65));
		public string[] Categories => [
			"Canistah"
		];
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RocketI);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.useStyle = ItemUseStyleID.None;
			Item.damage = 30;
			Item.ammo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shootSpeed = 4.1f;
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(silver: 3, copper: 2);
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 5)
			.AddIngredient(ItemID.CursedFlame)
			.AddRecipeGroup(AltLibrary.Common.Systems.RecipeGroups.CobaltBars, 5)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public void OnKill(Projectile projectile, bool child) {
			if (child) return;
			int damage = projectile.damage;
			projectile.damage = (int)(projectile.damage * 0.75f);
			projectile.knockBack = 16f;
			projectile.position = projectile.Center;
			projectile.width = projectile.height = 96;
			projectile.Center = projectile.position;
			projectile.Damage();
			ExplosiveGlobalProjectile.DealSelfDamage(projectile);
			projectile.damage = damage;
			ExplosiveGlobalProjectile.ExplosionVisual(projectile, true, sound: SoundID.Item62, fireDustAmount: 0);
			Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, default, Lingering_Cursed_Flames_P.ID, (int)(projectile.damage * 0.65f), 0, projectile.owner);
		}
	}
	public class Lingering_Cursed_Flames_P : ModProjectile, ICanisterChildProjectile, IIsExplodingProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.friendly = true;
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 25;
			Projectile.timeLeft = 180;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 15;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}
		public override void AI() {
			for (int i = 0; i < Projectile.width / 4; i++) {
				Dust.NewDust(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.CursedTorch
				);
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.CursedInferno, Main.rand.Next(300, 451));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.CursedInferno, Main.rand.Next(300, 451));
		}
		public bool IsExploding() => true;
	}
	public class Starfuze : ModItem, ICustomWikiStat {
		static short glowmask;
		public string[] Categories => new string[] {
			"Canistah"
		};
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WoodenArrow);
			Item.maxStack = 999;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Cyan;
			Item.glowMask = glowmask;
		}
	}
}
