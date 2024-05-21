using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Weapons.Demolitionist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

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
		void OnKill(Projectile projectile, bool child) { }
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
			Item.shoot = ModContent.ProjectileType<Napalm_Canister_P>();
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
			Item.shoot = ModContent.ProjectileType<Napalm_Canister_P>();
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
