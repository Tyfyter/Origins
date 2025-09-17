using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.Misc;
using Origins.NPCs;
using Origins.Projectiles;
using Origins.Tiles.Brine;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Liquid;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Items.Weapons.Ammo.Canisters {
	#region global stuff
	public class CanisterGlobalItem : GlobalItem {
		public static Dictionary<int, int> ItemToCanisterID { get; private set; } = [];
		public static Dictionary<Type, int> TypeToCanisterID { get; private set; } = [];
		public static List<CanisterData> CanisterDatas { get; private set; } = [];
		public static Dictionary<int, int> LauncherToProjectile { get; private set; } = [];
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			if (entity.ModItem is ICanisterAmmo canister) {
				CanisterData data = canister.GetCanisterData;
				data.Ammo = canister;
				int type = RegisterCanister(entity.type, data);
				if (type != -1) TypeToCanisterID.Add(entity.ModItem.GetType(), type);
				return true;
			}
			return false;
		}
		public override void Unload() => LauncherToProjectile = null;
		public override bool? CanBeChosenAsAmmo(Item ammo, Item weapon, Player player) {
			if (LauncherToProjectile.ContainsKey(weapon.type)) return true;
			return null;
		}
		public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
			if (LauncherToProjectile.TryGetValue(weapon.type, out int proj)) {
				type = proj;
			} else {
				type = weapon.shoot;
			}
		}
		public static int GetCanisterType(int type) {
			return ItemToCanisterID.TryGetValue(type, out int canisterID) ? canisterID : -1;
		}
		public static int GetCanisterType(Type type) {
			return TypeToCanisterID.TryGetValue(type, out int canisterID) ? canisterID : - 1;
		}
		public static void RegisterForLauncher(int launcher, int projectile) {
			LauncherToProjectile.Add(launcher, projectile);
		}
		public static int RegisterCanister(int ammo, CanisterData data) {
			if (!ItemToCanisterID.TryGetValue(ammo, out _)) {
				ItemToCanisterID.Add(ammo, CanisterDatas.Count);
				data.WhatAmI = CanisterDatas.Count;
				CanisterDatas.Add(data);
				return data.WhatAmI;
			}
			return -1;
		}
	}
	public interface ICanisterAmmo : ICustomWikiStat {
		CanisterData GetCanisterData { get; }
		void AI(Projectile projectile, bool child) { }
		void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone, bool child) { }
		string[] ICustomWikiStat.Categories => [
			"Canistah"
		];
		public void OnKill(Projectile projectile, bool child) {
			if (!child && projectile.ModProjectile is ICanisterProjectile canister) {
				canister.DefaultExplosion(projectile);
			}
		}
	}
	public class CanisterData(Color outerColor, Color innerColor, bool hasSpecialEffect = true, int whatAmI = -999, ICanisterAmmo ammo = null) {
		public Color OuterColor { get; set; } = outerColor;
		public Color InnerColor { get; set; } = innerColor;
		public bool HasSpecialEffect => hasSpecialEffect;
		public int WhatAmI { get; internal set; } = whatAmI;
		public ICanisterAmmo Ammo { get; internal set; } = ammo;
	}
	public class CanisterGlobalProjectile : GlobalProjectile {
		public float gravityMultiplier = 1f;
		public override bool InstancePerEntity => true;
		ICanisterProjectile canister;
		int canisterID;
		public CanisterData CanisterData { get; private set; }
		public int CanisterID {
			get => canisterID;
			set {
				canisterID = value;
				if (canisterID == -1) return;
				CanisterData = CanisterGlobalItem.CanisterDatas[value];
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
				if (CanisterID == -1) {
					if (CanisterData is null) CanisterID = 0;
				}
			} else if (source is EntitySource_Parent parentSource && parentSource.Entity is Projectile proj && proj.TryGetGlobalProjectile(out CanisterGlobalProjectile parentCanister)) {
				CanisterID = parentCanister.CanisterID;
			}
		}
		public override void AI(Projectile projectile) {
			CanisterData?.Ammo?.AI(projectile, false);
		}
		public override void OnKill(Projectile projectile, int timeLeft) {
			CanisterData?.Ammo?.OnKill(projectile, false);
		}
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			CanisterData?.Ammo?.OnHitNPC(projectile, target, hit, damageDone, false);
		}
		public override bool PreDraw(Projectile projectile, ref Color lightColor) {
			canister.CustomDraw(projectile, CanisterData, lightColor);
			return false;
		}
		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			binaryWriter.Write((int)CanisterID);
		}
		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			canister = projectile.ModProjectile as ICanisterProjectile;
			CanisterID = binaryReader.ReadInt32();
		}
		public static void DefaultExplosion(Projectile projectile, bool child, int fireDustType = DustID.Torch, int size = 96) {
			if (child) return;
			ExplosiveGlobalProjectile.DoExplosion(projectile, size, sound: SoundID.Item62, fireDustType: fireDustType);
		}
	}
	public class CanisterChildGlobalProjectile : GlobalProjectile {
		public float gravityMultiplier = 1f;
		public override bool InstancePerEntity => true;
		int canisterID;
		public CanisterData CanisterData { get; private set; }
		bool isVisual = false;
		public int CanisterID {
			get => canisterID;
			set {
				canisterID = value;
				if (canisterID == -1) return;
				CanisterData = CanisterGlobalItem.CanisterDatas[value];
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
			if (CanisterID == -1) CanisterID = 0;
			isVisual = ((ICanisterChildProjectile)projectile.ModProjectile).IsVisual;
		}
		public override void AI(Projectile projectile) {
			if (!isVisual) {
				CanisterData?.Ammo?.AI(projectile, true);
			}
		}
		public override void OnKill(Projectile projectile, int timeLeft) {
			if (!isVisual) {
				CanisterData?.Ammo?.OnKill(projectile, true);
			}
		}
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			CanisterData?.Ammo?.OnHitNPC(projectile, target, hit, damageDone, true);
		}
	}
	public interface ICanisterProjectile {
		public const string base_texture_path = "Origins/Items/Weapons/Ammo/Canisters/";
		public abstract AutoLoadingAsset<Texture2D> OuterTexture { get; }
		public abstract AutoLoadingAsset<Texture2D> InnerTexture { get; }
		public void DefaultExplosion(Projectile projectile, int fireDustType = DustID.Torch, int size = 96) => CanisterGlobalProjectile.DefaultExplosion(projectile, false, fireDustType: fireDustType, size: size);
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			Vector2 origin = OuterTexture.Value.Size() * 0.5f;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				InnerTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.InnerColor,
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				OuterTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
		}
	}
	public interface ICanisterChildProjectile {
		bool IsVisual => false;
	}
	public static class CanisterExtensions {
		public static void DoGravity<T>(this T self, float value) where T : ModProjectile, ICanisterProjectile {
			self.Projectile.velocity.Y += value * self.Projectile.GetGlobalProjectile<CanisterGlobalProjectile>().gravityMultiplier;
		}
		public static bool InheritImmunityFrames(this Projectile parent, int childType, out int[] iFrames) {
			Projectile childTemplate = ContentSamples.ProjectilesByType[childType];
			int[] _iFrames = [];
			iFrames = _iFrames;
			Action<int> applyIFrames;
			if (childTemplate.usesLocalNPCImmunity) {
				iFrames = new int[childTemplate.localNPCImmunity.Length];
				applyIFrames = index => _iFrames[index] = childTemplate.localNPCHitCooldown;
			} else if (childTemplate.usesIDStaticNPCImmunity) {
				applyIFrames = index => Projectile.perIDStaticNPCImmunity[childType][index] = Main.GameUpdateCount + (uint)childTemplate.idStaticNPCHitCooldown;
			} else {
				applyIFrames = index => Main.npc[index].immune[parent.owner] = Math.Max(Main.npc[index].immune[parent.owner], 10);
			}
			if (parent.usesLocalNPCImmunity) {
				for (int i = 0; i < parent.localNPCImmunity.Length; i++) {
					if (parent.localNPCImmunity[i] != 0) {
						applyIFrames(i);
					}
				}
			} else if (parent.usesIDStaticNPCImmunity) {
				for (int i = 0; i < Projectile.perIDStaticNPCImmunity[parent.type].Length; i++) {
					if (Projectile.IsNPCIndexImmuneToProjectileType(parent.type, i)) {
						applyIFrames(i);
					}
				}
			} else {
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc.immune[parent.owner] > 0) {
						applyIFrames(npc.whoAmI);
					}
				}
			}
			return iFrames.Length != 0;
		}
	}
	#endregion global stuff
	public class Coolant_Canister : ModItem, ICanisterAmmo, ICustomWikiStat {
		static short glowmask;
		public CanisterData GetCanisterData => new(new(99, 206, 236), new(178, 255, 255));
		public bool? Hardmode => true;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(38);
			Item.glowMask = glowmask;
			Item.shootSpeed = 0.3f;
			Item.knockBack = 3.6f;
			Item.value = Item.sellPrice(silver: 8, copper: 80);
			Item.rare = ItemRarityID.Pink;
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 200)
			.AddRecipeGroup(AltLibrary.Common.Systems.RecipeGroups.CobaltBars, 100)
			.AddIngredient(ItemID.FrostCore)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public void OnKill(Projectile projectile, bool child) {
			if (!child && projectile.ModProjectile is ICanisterProjectile canister) {
				canister.DefaultExplosion(projectile, fireDustType: DustID.IceTorch, 166);
			}
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone, bool child) {
			target.AddBuff(BuffID.Frostburn2, Main.rand.Next(child ? 180 : 120, child ? 241 : 181));
		}
	}
	public class Napalm_Canister : ModItem, ICanisterAmmo, ICustomWikiStat {
		static short glowmask;
		public CanisterData GetCanisterData => new(new(211, 194, 182), new(255, 163, 68));
		public string[] Categories => [
			"Canistah"
		];
		public bool? Hardmode => false;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(30);
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(silver: 3, copper: 2);
			Item.rare = ItemRarityID.Orange;
			Item.ArmorPenetration += 5;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 50)
			.AddIngredient(ItemID.Fireblossom)
			.AddRecipeGroup(RecipeGroupID.IronBar, 10)
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
			int napalm = ModContent.ProjectileType<Napalm_P>();
			projectile.InheritImmunityFrames(napalm, out _);
			for (int i = 0; i < 5; i++) {
				Projectile.NewProjectile(
					projectile.GetSource_FromThis(),
					projectile.Center,
					(projectile.velocity / 2) + GeometryUtils.Vec2FromPolar((i / Main.rand.NextFloat(5, 7)) * MathHelper.TwoPi, Main.rand.NextFloat(2, 4)),
					napalm,
					(int)(projectile.damage * 0.65f),
					0,
					projectile.owner
				);
			}
		}
	}
	public class Napalm_P : ModProjectile, ICanisterChildProjectile {
		public override string Texture => "Origins/Projectiles/Ammo/Napalm_Pellet_P";

		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
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
		public bool? Hardmode => true;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(27);
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(silver: 3, copper: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 10)
			.AddIngredient(ItemID.CursedFlame)
			.AddRecipeGroup(AltLibrary.Common.Systems.RecipeGroups.CobaltBars, 10)
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
			projectile.InheritImmunityFrames(Lingering_Cursed_Flames_P.ID, out _);
			Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, default, Lingering_Cursed_Flames_P.ID, (int)(projectile.damage * 0.65f), 0, projectile.owner);
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone, bool child) {
			target.AddBuff(BuffID.CursedInferno, Main.rand.Next(child ? 180 : 120, child ? 241 : 181));
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
			Projectile.timeLeft = 120;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 15;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}
		public override void AI() {
			for (int i = 0; i < Projectile.width / 4; i++) {
				Dust dust = Dust.NewDustDirect(
					Projectile.Center + Main.rand.NextVector2Circular(67.5f, 67.5f) + Vector2.UnitY * 12,
					0,
					0,
					DustID.CursedTorch
				);
				//dust.noGravity = true;
				dust.velocity.Y -= 2;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			return Projectile.Center.Clamp(targetHitbox).DistanceSQ(Projectile.Center) < 67.5f * 67.5f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.CursedInferno, Main.rand.Next(120, 181));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(BuffID.CursedInferno, Main.rand.Next(120, 181));
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding() => true;
	}
	public class Ichor_Canister : ModItem, ICanisterAmmo, ICustomWikiStat {
		static short glowmask;
		public CanisterData GetCanisterData => new(new(215, 104, 94), new(247, 253, 158));
		public bool? Hardmode => true;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(22);
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(silver: 3, copper: 2);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 50)
			.AddIngredient(ItemID.Ichor)
			.AddRecipeGroup(AltLibrary.Common.Systems.RecipeGroups.CobaltBars, 10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone, bool child) {
			target.AddBuff(BuffID.Ichor, (child ? 7 : 4) * (projectile.penetrate >= 0 ? 2 : 1) * 60);
		}
		public void OnKill(Projectile projectile, bool child) {
			if (!child && projectile.ModProjectile is ICanisterProjectile canister) {
				canister.DefaultExplosion(projectile, DustID.IchorTorch);
			}
		}
	}
	public class Bile_Canister : ModItem, ICanisterAmmo, ICustomWikiStat {
		static short glowmask;
		public CanisterData GetCanisterData => new(new(239, 235, 233), new(70, 19, 66));
		public bool? Hardmode => true;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(30);
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(silver: 3, copper: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 50)
			.AddIngredient<Black_Bile>()
			.AddRecipeGroup(AltLibrary.Common.Systems.RecipeGroups.CobaltBars, 10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone, bool child) {
			target.AddBuff(ModContent.BuffType<Rasterized_Debuff>(), child ? 18 : 12);
		}
		public void OnKill(Projectile projectile, bool child) {
			if (child) return;
			projectile.InheritImmunityFrames(Bile_Canister_Explosion.ID, out _);
			Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.oldPosition + ContentSamples.ProjectilesByType[projectile.type].Size / 2 + projectile.velocity, default, Bile_Canister_Explosion.ID, (int)(projectile.damage * 0.75f), 0, projectile.owner);
		}
	}
	public class Bile_Canister_Explosion : ModProjectile, IIsExplodingProjectile, ICanisterChildProjectile {
		//public override string Texture => typeof(Bile_Dart_Aura).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.hide = false;
			Projectile.width = Projectile.height = 72;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 15;
			Projectile.tileCollide = false;
			Projectile.scale = 1.5f;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62, fireDustAmount: 0);
				Projectile.ai[0] = 1;
			}
			ExplosiveGlobalProjectile.DealSelfDamage(Projectile);
			Projectile.scale = Math.Min(Projectile.scale * 1.2f - 0.3f, Projectile.scale - 0.01f);
			if (Projectile.scale <= 0) Projectile.Kill();
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			int inflation = (int)(hitbox.Width * (Projectile.scale / 1.5f - 1) * 0.5f);
			hitbox.Inflate(inflation, inflation);
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Mask_Rasterize.QueueProjectile(Projectile.whoAmI)) return false;
			Vector2 screenCenter = Main.ScreenSize.ToVector2() * 0.5f;
			Main.spriteBatch.Draw(
				TextureAssets.Projectile[ID].Value,
				(Projectile.Center - Main.screenPosition - screenCenter) * Main.GameViewMatrix.Zoom + screenCenter,
				null,
				new Color(
					1f,
					1f,
				0f),
				0,
				new Vector2(36),
				Projectile.scale * Main.GameViewMatrix.Zoom.X,
				0,
			0);
			return false;
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding() => true;
	}
	public class Alkahest_Canister : ModItem, ICanisterAmmo, ICustomWikiStat, ITornSource {
		public float Severity => 0.35f;
		static short glowmask;
		public CanisterData GetCanisterData => new(new(61, 164, 196), new(255, 254, 156));
		public bool? Hardmode => true;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(30);
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(silver: 3, copper: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 50)
			.AddIngredient<Alkahest>()
			.AddRecipeGroup(AltLibrary.Common.Systems.RecipeGroups.CobaltBars, 10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone, bool child) {
			OriginGlobalNPC.InflictTorn(target, 240, 180, Severity, source: Main.player[projectile.owner].GetModPlayer<OriginPlayer>());
		}
		public void OnKill(Projectile projectile, bool child) {
			if (child) return;
			if (projectile.ModProjectile is ICanisterProjectile canister) {
				canister.DefaultExplosion(projectile);
			}
			projectile.InheritImmunityFrames(Alkahest_Canister_Droplet.ID, out _);
			for (int i = 0; i < 6; i++) {
				Projectile.NewProjectile(
					projectile.GetSource_FromThis(),
					projectile.Center,
					Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(0.9f, 0.1f) + projectile.velocity * 0.25f,
					Alkahest_Canister_Droplet.ID,
					(int)(projectile.damage * 0.35f),
					0,
					projectile.owner
				);
			}
		}
	}
	public class Alkahest_Canister_Droplet : ModProjectile, ICanisterChildProjectile {
		public override string Texture => typeof(Bile_Dart_Aura).GetDefaultTMLName();
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.hide = true;
			Projectile.width = Projectile.height = 4;
			Projectile.friendly = true;
			Projectile.penetrate = 2;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 15;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 180;
		}
		public override void AI() {
			Projectile.velocity.Y += 0.08f;
			Lighting.AddLight(Projectile.Center, 0.78f, 0.75f, 0.2f);
			Dust.NewDustPerfect(Projectile.Center, 228, Projectile.velocity * 0.95f, 100, new Color(0, 255, 0), Projectile.scale).noGravity = true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 180, 180, 0.25f, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.timeLeft -= 2;
			return false;
		}
	}
	public class Alkaline_Canister : ModItem, ICanisterAmmo, ICustomWikiStat {
		static short glowmask;
		public CanisterData GetCanisterData => new(new(110, 240, 197), new(94, 255, 182));
		public bool? Hardmode => true;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
			PegasusLib.Sets.ItemSets.InflictsExtraDebuffs[Type] = [Toxic_Shock_Debuff.ID];
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(30);
			Item.glowMask = glowmask;
			Item.value = Item.sellPrice(silver: 3, copper: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 50)
			.AddIngredient<Brineglow_Item>()
			.AddRecipeGroup(AltLibrary.Common.Systems.RecipeGroups.CobaltBars, 10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone, bool child) {
			target.AddBuff(Toxic_Shock_Debuff.ID, 300);
		}
		public void OnKill(Projectile projectile, bool child) {
			if (child) return;
			if (projectile.ModProjectile is ICanisterProjectile canister) {
				canister.DefaultExplosion(projectile);
			}
			int projType = ModContent.ProjectileType<Projectiles.Weapons.Brine_Droplet>();
			projectile.InheritImmunityFrames(projType, out _);
			for (int i = 0; i < 6; i++) {
				Projectile.NewProjectile(
					projectile.GetSource_FromThis(),
					projectile.Center,
					Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(0.9f, 0.1f) + projectile.velocity * 0.25f,
					projType,
					(int)(projectile.damage * 0.15f),
					0,
					projectile.owner
				);
			}
		}
	}
	public class Discharge_Canister : ModItem, ICanisterAmmo, ICustomWikiStat {
		public CanisterData GetCanisterData => new(new(207, 110, 21), new(100, 222, 242));
		public bool? Hardmode => false;
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(26);
			Item.value = Item.sellPrice(silver: 3, copper: 2);
			Item.rare = ItemRarityID.Orange;
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 50)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient<Felnum_Bar>(3)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public void OnKill(Projectile projectile, bool child) {
			if (child) return;
			if (projectile.ModProjectile is ICanisterProjectile canister) {
				canister.DefaultExplosion(projectile);
			}
			SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(2), projectile.Center);
			int t = ModContent.ProjectileType<Felnum_Shock_Grenade_Shock>();
			projectile.InheritImmunityFrames(t, out _);
			for (int i = Main.rand.Next(2); i < 3; i++) {
				Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center, Vector2.Zero, t, (int)(projectile.damage * 0.5f), 6, projectile.owner);
			}
		}
	}
	public class Bee_Canister : ModItem, ICanisterAmmo, ICustomWikiStat {
		public CanisterData GetCanisterData => new(new(212, 206, 37), new(38, 2, 44));
		public bool? Hardmode => false;
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(15);
			Item.value = Item.sellPrice(silver: 3, copper: 2);
			Item.rare = ItemRarityID.Orange;
			Item.ArmorPenetration += 15;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 50)
			.AddIngredient(ItemID.BeeWax)
			.AddRecipeGroup(AltLibrary.Common.Systems.RecipeGroups.SilverBars, 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public void OnKill(Projectile projectile, bool child) {
			if (child) return;
			if (projectile.ModProjectile is ICanisterProjectile canister) {
				canister.DefaultExplosion(projectile);
			}
			for (int i = 0; i < 4; i++) {
				Projectile bee = Projectile.NewProjectileDirect(
					projectile.GetSource_FromThis(),
					projectile.Center,
					Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(0.9f, 0.1f) + projectile.velocity * 0.25f,
					Main.player[projectile.owner].beeType(),
					(int)(projectile.damage * 0.15f),
					0,
					projectile.owner
				);
				projectile.InheritImmunityFrames(bee.type, out _); // bee type is procedurally unknowable, so we have to just do this for every bee type that gets spawned
				bee.usesIDStaticNPCImmunity = true;
				bee.idStaticNPCHitCooldown = 8;
			}
		}
	}
	public class Aether_Canister : ModItem, ICanisterAmmo, ICustomWikiStat {
		public CanisterData GetCanisterData => new(new(182, 194, 211), new(100, 222, 242));
		public bool? Hardmode => false;
		internal static readonly FrameCachedValue<Color> color = new(() => new(LiquidRenderer.GetShimmerBaseColor(0, 0)));
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 199;
		}
		public override void SetDefaults() {
			Item.DefaultToCanister(26);
			Item.value = Item.sellPrice(silver: 3, copper: 2);
			Item.rare = ItemRarityID.Orange;
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 50)
			.AddIngredient(ItemID.ExplosivePowder, 50)
			.AddIngredient(ModContent.ItemType<Aetherite_Bar>(), 3)
			.Register();
		}
		public void OnKill(Projectile projectile, bool child) {
			if (child) return;
			if (projectile.ModProjectile is ICanisterProjectile canister) {
				canister.DefaultExplosion(projectile, DustID.ShimmerTorch);
			}
			if (Main.myPlayer != projectile.owner) return;
			int starType = ModContent.ProjectileType<Aether_Canister_P>();
			projectile.InheritImmunityFrames(starType, out _);
			for (int i = 0; i < 5; i++) {
				Projectile.NewProjectile(
					projectile.GetSource_Death(),
					projectile.Center,
					(projectile.velocity / 2) + GeometryUtils.Vec2FromPolar(Main.rand.NextFloat(3, 6), (i / Main.rand.NextFloat(5, 7)) * MathHelper.TwoPi),
					starType,
					(int)(projectile.damage * 0.65f),
				0);
			}
		}
		public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone, bool child) {
			if (NPCID.Sets.ShimmerTransformToNPC[target.type] != -1 || NPCID.Sets.ShimmerTransformToItem[target.type] != -1 || NPCID.Sets.ShimmerTownTransform[target.type]) {
				target.AddBuff(BuffID.Shimmer, 91);
			}
		}
		public void AI(Projectile projectile, bool child) {
			projectile.velocity *= 0.99f;
			if (child) {
				CanisterChildGlobalProjectile childGlobal = projectile.GetGlobalProjectile<CanisterChildGlobalProjectile>();
				childGlobal.gravityMultiplier = 0;
				return;
			}
			CanisterGlobalProjectile global = projectile.GetGlobalProjectile<CanisterGlobalProjectile>();
			global.CanisterData.InnerColor = color.Value;
			global.gravityMultiplier = 0;
			if (Main.myPlayer == projectile.owner) {
				if (Main.rand.NextBool(2)) projectile.timeLeft--;
				if (projectile.timeLeft % (10 * projectile.MaxUpdates) == 0) {
					Projectile.NewProjectile(
						projectile.GetSource_FromAI(),
						projectile.Center,
						Main.rand.NextVector2Circular(1, 1),
						ModContent.ProjectileType<Aether_Canister_P>(),
						(int)(projectile.damage * 0.65f),
					0);
				}
			}
		}
	}
	public class Aether_Canister_P : ModProjectile, ICanisterChildProjectile {
		public override string Texture => "Origins/Projectiles/Ammo/Aether_Star";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.friendly = true;
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.aiStyle = -1;
			Projectile.penetrate = 25;
			Projectile.alpha = Main.rand.Next(180, 256);
			Projectile.timeLeft = Main.rand.Next(300, 451);
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 15;
			(Projectile.localAI[0], Projectile.localAI[1], Projectile.localAI[2]) = CanisterGlobalItem.CanisterDatas[CanisterGlobalItem.GetCanisterType(typeof(Aether_Canister))].InnerColor.ToVector3();
		}
		public override void AI() {
			float v = 0.75f + (float)(0.125f * (Math.Sin(Projectile.timeLeft / 5f) + 2 * Math.Sin(Projectile.timeLeft / 60f)));
			Lighting.AddLight(Projectile.Center, Projectile.localAI[0] * v, Projectile.localAI[1] * v, Projectile.localAI[2] * v);
			Projectile.velocity.Y -= 0.1f;
			Projectile.velocity *= 0.997f;
			Projectile.ai[0]++;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = height = 2;
			fallThrough = true;
			return true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			//target.AddBuff(BuffID.Shimmer, 91);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) => Projectile.ai[0] > 10;
		public override Color? GetAlpha(Color lightColor) {
			float v = 0.75f + (float)(0.125f * (Math.Sin(Projectile.timeLeft / 5f) + 2 * Math.Sin(Projectile.timeLeft / 60f)));
			return new(Projectile.localAI[0] * v, Projectile.localAI[1] * v, Projectile.localAI[2] * v, Projectile.alpha * v / 255f);
		}
	}
}
