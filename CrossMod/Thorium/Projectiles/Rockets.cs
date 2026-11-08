using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ammo;
using Origins.NPCs.Ashen;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod.Items.BossThePrimordials.Omni;
using ThoriumMod.Items.Donate;
using ThoriumMod.Items.Illumite;
using ThoriumMod.Items.RangedItems;
using ThoriumMod.Projectiles;

namespace Origins.CrossMod.Thorium.Projectiles {
	#region Base Class
	#region The Javelin
	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Rocket_Javelin(ModItem item, Color? overlayColor = null) : JavelinPro {
		[field: CloneByReference]
		public ModItem Item { get; } = item;
		public virtual int TileDestructionRadius { get; } = -1;
		public override string Name => Item.Name + "_Javelin";
		public override string Texture => typeof(Rocket_Javelin).GetDefaultTMLName();
		public virtual AutoLoadingTexture Overlay => Texture + "_Overlay";
		public virtual Color OverlayColor => overlayColor ?? Color.White;
		public override LocalizedText DisplayName => Language.GetText($"Mods.ThoriumMod.Projectiles.JavelinPro.DisplayName");
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.GetOrAdd(ModContent.ItemType<TheJavelin>()).Add(Item.Type, Type);
			Projectile.SetupTripwireValues(16);
			Defensive_Turret.TargetProjectilesLow[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
		}
		public override void AI() {
			Projectile.ai[0] = 0;
			if (Projectile.timeLeft < 292 && Item is IRocketItem rocket) rocket.AI(Projectile, false);
			base.AI();
		}
		public override void PostAI() {
			if (Projectile.frameCounter++ > 3) {
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % 3;
			}
		}
		public override void OnKill(int timeLeft) {
			Projectile.ai[0] = 0f;
			ExplosionRadius = TileDestructionRadius + 1;
			base.OnKill(timeLeft);
			SpecialOnKill(timeLeft);
		}
		public virtual void SpecialOnKill(int timeLeft) { }
		public override bool PreDraw(ref Color lightColor) {
			Vector2 pos = Projectile.Center - Main.screenPosition;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(1, 3, 0, Projectile.frame);
			SpriteEffects effects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			Main.spriteBatch.DrawGlowingNPCPart(
				texture,
				Overlay,
				pos,
				frame,
				lightColor,
				lightColor.MultiplyRGB(OverlayColor),
				Projectile.rotation,
				frame.Size() * 0.5f,
				Projectile.scale,
				effects
			);
			return false;
		}
	}
	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Rocket_Javelin<TRocket>(ModItem item, Color? overlayColor = null) : Rocket_Javelin(item, overlayColor) where TRocket : IRocketItem {
		public override int TileDestructionRadius => TRocket.TileDestructionRadius;
	}

	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Liquid_Rocket_Javelin(ModItem item) : Rocket_Javelin(item) {
		public new BaseLiquidRocket Item = item as BaseLiquidRocket;
		public override AutoLoadingTexture Overlay => Texture + "_Liquid_Overlay";
		public override Color OverlayColor => Item.OverlayColor;
		public override void SpecialOnKill(int timeLeft) {
			BaseLiquidRocketP.BaseLiquidExplosiveKill(Projectile, Item.LiquidType, Item.DustType, Item.DustColor, spreadSize: 5f);
		}
	}
	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Liquid_Rocket_Javelin<TRocket>(ModItem item) : Liquid_Rocket_Javelin(item) where TRocket : IRocketItem {
		public override int TileDestructionRadius => TRocket.TileDestructionRadius;
	}
	#endregion
	#region Illumite Barrager
	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Rocket_Illumite(ModItem item, Color? overlayColor = null) : IllumiteRocketPro {
		[field: CloneByReference]
		public ModItem Item { get; } = item;
		public virtual int TileDestructionRadius { get; } = -1;
		public override string Name => Item.Name + "_Illumite";
		public override string Texture => typeof(Rocket_Illumite).GetDefaultTMLName();
		public virtual AutoLoadingTexture Overlay => Texture + "_Overlay";
		public virtual Color OverlayColor => overlayColor ?? Color.White;
		public override LocalizedText DisplayName => Language.GetText($"Mods.ThoriumMod.Projectiles.IllumiteRocketPro.DisplayName");
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.GetOrAdd(ModContent.ItemType<IllumiteBarrage>()).Add(Item.Type, Type);
			Projectile.SetupTripwireValues(48, 1);
			Defensive_Turret.OohShiny[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
		}
		public override void AI() {
			Projectile.ai[0] = 0;
			if (Item is IRocketItem rocket) rocket.AI(Projectile, false);
			base.AI();
		}
		public override void OnKill(int timeLeft) {
			Projectile.ai[0] = 0f;
			explosion = TileDestructionRadius;
			base.OnKill(timeLeft);
			SpecialOnKill(timeLeft);
		}
		public virtual void SpecialOnKill(int timeLeft) { }
		public override bool PreDraw(ref Color lightColor) {
			Vector2 pos = Projectile.Center - Main.screenPosition;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			SpriteEffects effects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			Main.spriteBatch.DrawGlowingNPCPart(
				texture,
				Overlay,
				pos,
				null,
				lightColor,
				OverlayColor,
				Projectile.rotation,
				texture.Size() * 0.5f,
				Projectile.scale,
				effects
			);
			Main.EntitySpriteDraw(Overlay, pos, null, OverlayColor * 0.35f * Projectile.Opacity, Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, effects);
			return false;
		}
		public override void PostDraw(Color lightColor) { }
	}
	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Rocket_Illumite<TRocket>(ModItem item, Color? overlayColor = null) : Rocket_Illumite(item, overlayColor) where TRocket : IRocketItem {
		public override int TileDestructionRadius => TRocket.TileDestructionRadius;
	}

	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Liquid_Rocket_Illumite(ModItem item) : Rocket_Illumite(item) {
		public new BaseLiquidRocket Item = item as BaseLiquidRocket;
		public override AutoLoadingTexture Overlay => Texture + "_Liquid_Overlay";
		public override Color OverlayColor => Item.OverlayColor;
		public override void SpecialOnKill(int timeLeft) {
			BaseLiquidRocketP.BaseLiquidExplosiveKill(Projectile, Item.LiquidType, Item.DustType, Item.DustColor);
		}
	}
	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Liquid_Rocket_Illumite<TRocket>(ModItem item) : Liquid_Rocket_Illumite(item) where TRocket : IRocketItem {
		public override int TileDestructionRadius => TRocket.TileDestructionRadius;
	}
	#endregion
	#region Micro Launcher
	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Rocket_Micro(ModItem item, Color? overlayColor = null) : MicroRocketPro {
		[field: CloneByReference]
		public ModItem Item { get; } = item;
		public virtual int TileDestructionRadius { get; } = -1;
		public override string Name => Item.Name + "_Micro";
		public override string Texture => typeof(Rocket_Micro).GetDefaultTMLName();
		public virtual AutoLoadingTexture Overlay => Texture + "_Overlay";
		public virtual Color OverlayColor => overlayColor ?? Color.White;
		public override LocalizedText DisplayName => Language.GetText($"Mods.ThoriumMod.Projectiles.MicroRocketPro.DisplayName");
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 2;
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.GetOrAdd(ModContent.ItemType<MicroLauncher>()).Add(Item.Type, Type);
			Projectile.SetupTripwireValues(36, 1);
			Defensive_Turret.TargetProjectilesLow[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
		}
		public override void AI() {
			Projectile.ai[0] = 0;
			if (Item is IRocketItem rocket) rocket.AI(Projectile, false);
			base.AI();
		}
		public override void PostAI() {
			if (Projectile.frameCounter++ > 3) {
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % 2;
			}
		}
		public override void OnKill(int timeLeft) {
			Projectile.ai[0] = 0f;
			ExplosionRadius = TileDestructionRadius - 1;
			base.OnKill(timeLeft);
			SpecialOnKill(timeLeft);
		}
		public virtual void SpecialOnKill(int timeLeft) { }
		public override bool PreDraw(ref Color lightColor) {
			Vector2 pos = Projectile.Center - Main.screenPosition;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(1, 2, 0, Projectile.frame);
			SpriteEffects effects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			Main.spriteBatch.DrawGlowingNPCPart(
				texture,
				Overlay,
				pos,
				frame,
				lightColor,
				lightColor.MultiplyRGB(OverlayColor),
				Projectile.rotation,
				frame.Size() * 0.5f,
				Projectile.scale,
				effects
			);
			return false;
		}
	}
	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Rocket_Micro<TRocket>(ModItem item, Color? overlayColor = null) : Rocket_Micro(item, overlayColor) where TRocket : IRocketItem {
		public override int TileDestructionRadius => TRocket.TileDestructionRadius;
	}

	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Liquid_Rocket_Micro(ModItem item) : Rocket_Micro(item) {
		public new BaseLiquidRocket Item = item as BaseLiquidRocket;
		public override AutoLoadingTexture Overlay => Texture + "_Liquid_Overlay";
		public override Color OverlayColor => Item.OverlayColor;
		public override void SpecialOnKill(int timeLeft) {
			BaseLiquidRocketP.BaseLiquidExplosiveKill(Projectile, Item.LiquidType, Item.DustType, Item.DustColor, spreadSize: 1);
		}
	}
	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Liquid_Rocket_Micro<TRocket>(ModItem item) : Liquid_Rocket_Micro(item) where TRocket : IRocketItem {
		public override int TileDestructionRadius => TRocket.TileDestructionRadius;
	}
	#endregion
	#region Buffalo Launcher
	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Rocket_Buffalo(ModItem item) : BuffaloLauncherPro {
		[field: CloneByReference]
		public ModItem Item { get; } = item;
		public virtual int TileDestructionRadius { get; } = -1;
		public override string Name => Item.Name + "_Buffalo";
		public override string Texture => $"{typeof(Rocket_Buffalo).GetDefaultTMLName()}_{Item.Name.Replace("_Rocket", null)}";
		public override LocalizedText DisplayName => Language.GetText($"Mods.ThoriumMod.Projectiles.BuffaloLauncherPro.DisplayName");
		protected override bool CloneNewInstances => true;
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches.GetOrAdd(ModContent.ItemType<BuffaloLauncher>()).Add(Item.Type, Type);
			Projectile.SetupTripwireValues(48, 1);
			Defensive_Turret.TargetProjectilesLow[Type] = true;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
		}
		public override void AI() {
			Projectile.ai[0] = 0;
			if (Item is IRocketItem rocket) rocket.AI(Projectile, false);
			base.AI();
		}
		public override void OnKill(int timeLeft) {
			Projectile.ai[0] = 0f;
			explosion = TileDestructionRadius + 1;
			base.OnKill(timeLeft);
			SpecialOnKill(timeLeft);
		}
		public virtual void SpecialOnKill(int timeLeft) { }
	}
	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Rocket_Buffalo<TRocket>(ModItem item) : Rocket_Buffalo(item) where TRocket : IRocketItem {
		public override int TileDestructionRadius => TRocket.TileDestructionRadius;
	}

	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Liquid_Rocket_Buffalo(ModItem item) : Rocket_Buffalo(item) {
		public new BaseLiquidRocket Item = item as BaseLiquidRocket;
		public override void SpecialOnKill(int timeLeft) {
			BaseLiquidRocketP.BaseLiquidExplosiveKill(Projectile, Item.LiquidType, Item.DustType, Item.DustColor, spreadSize: 5f);
		}
	}
	[ExtendsFromMod("ThoriumMod"), Autoload(false)]
	public class Liquid_Rocket_Buffalo<TRocket>(ModItem item) : Liquid_Rocket_Buffalo(item) where TRocket : IRocketItem {
		public override int TileDestructionRadius => TRocket.TileDestructionRadius;
	}
	#endregion
	#endregion

	[ExtendsFromMod("ThoriumMod")]
	public static class ThoriumExtraRockets {
		public static void AddExtraRockets(this Mod mod, ModItem item, Color? overlayColor = null) {
			if (item is BaseLiquidRocket) {
				mod.AddContent(new Liquid_Rocket_Javelin(item));
				mod.AddContent(new Liquid_Rocket_Illumite(item));
				mod.AddContent(new Liquid_Rocket_Micro(item));
				mod.AddContent(new Liquid_Rocket_Buffalo(item));
			} else {
				mod.AddContent(new Rocket_Javelin(item, overlayColor));
				mod.AddContent(new Rocket_Illumite(item, overlayColor));
				mod.AddContent(new Rocket_Micro(item, overlayColor));
				mod.AddContent(new Rocket_Buffalo(item));
			}
		}
		public static void AddExtraRockets<TRocket>(this Mod mod, ModItem item, Color? overlayColor = null) where TRocket : IRocketItem {
			if (item is BaseLiquidRocket) {
				mod.AddContent(new Liquid_Rocket_Javelin<TRocket>(item));
				mod.AddContent(new Liquid_Rocket_Illumite<TRocket>(item));
				mod.AddContent(new Liquid_Rocket_Micro<TRocket>(item));
				mod.AddContent(new Liquid_Rocket_Buffalo<TRocket>(item));
			} else {
				mod.AddContent(new Rocket_Javelin<TRocket>(item, overlayColor));
				mod.AddContent(new Rocket_Illumite<TRocket>(item, overlayColor));
				mod.AddContent(new Rocket_Micro<TRocket>(item, overlayColor));
				mod.AddContent(new Rocket_Buffalo<TRocket>(item));
			}
		}
	}
}
