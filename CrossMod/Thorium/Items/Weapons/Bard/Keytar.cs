using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Tiles.Other;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ThoriumMod;
using ThoriumMod.Empowerments;
using ThoriumMod.Items;
using ThoriumMod.Projectiles.Bard;

namespace Origins.CrossMod.Thorium.Items.Weapons.Bard {
	#region without thorium
	public class Keytar : ModItem {
		public static int ID { get; internal set; }
		public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("ThoriumMod");
		public static SoundStyle BassSound = new("Origins/Sounds/Custom/KeytarBass", SoundType.Sound);
		public static SoundStyle SynthSound = new("Origins/Sounds/Custom/KeytarSynth", SoundType.Sound);
		public static SoundStyle LeadSound = new("Origins/Sounds/Custom/KeytarLead", SoundType.Sound);
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			ID = Type;
		}
		public static void SetSharedDefaults(Item item, out int cost) {
			item.damage = 52;
			item.DamageType = DamageClass.Generic;
			item.knockBack = 1f;
			item.useAnimation = 16;
			item.useTime = 16;
			item.useStyle = ItemUseStyleID.Shoot;
			item.noMelee = true;
			item.shootSpeed = 18;
			item.value = Item.sellPrice(gold: 2);
			item.rare = ItemRarityID.Pink;
			cost = 20;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Busted_Servo>(17)
			.AddIngredient<Rotor>(8)
			.AddIngredient<Watered_Down_Keytar>()
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
		public static TooltipLine ModeTooltip => new(
			Origins.instance,
			"ModeIndicator",
			GetModeText(Main.LocalPlayer, out Color color)
		) {
			OverrideColor = color
		};
		public static string GetModeText(Player player, out Color color) {
			int keytarMode = player.OriginPlayer().keytarMode;
			switch (keytarMode) {
				default:
				color = new(0.25f, 0.25f, 0.75f);
				break;
				case 1:
				color = new(0.75f, 0.25f, 0.25f);
				break;
				case 2:
				color = new(0.85f, 0.75f, 0.25f);
				break;
			}
			return Language.GetTextValue("Mods.Origins.Items.Keytar.Mode" + keytarMode);
		}
		public static void CycleMode(Player player) {
			if (!player.ItemAnimationJustStarted) {
				if (player.controlUseItem || player.controlUseTile && player.OriginPlayer().releaseAltUse) {
					player.itemAnimation = 0;
					player.itemTime = 0;
				}
				return;
			}
			ref int keytarMode = ref player.OriginPlayer().keytarMode;
			keytarMode = (keytarMode + 1) % 3;
			if (player.whoAmI == Main.myPlayer) {
				string text = GetModeText(player, out Color color);
				CombatText.NewText(
					player.Hitbox with { Height = 0 },
					color,
					text + "!",
					keytarMode == 2
				);
			}
		}
		public static void AddTooltips(List<TooltipLine> tooltips, int tagIndex = 1) {
			tooltips.Insert(tagIndex, ModeTooltip);
			int index = tooltips.FindIndex(l => l.Name == "Tooltip0");
			for (int i = 0; i < 3; i++) {
				string text = Language.GetTextValue("Mods.Origins.Items.Keytar.Modes." + i);
				if (i == OriginPlayer.LocalOriginPlayer?.keytarMode) text = "-" + text;
				tooltips.Insert(index++, new(Origins.instance, "ModeIndicator", text));
			}
			//
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			AddTooltips(tooltips);
		}
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) CycleMode(player);
			switch (player.OriginPlayer().keytarMode) {
				default:
				Item.UseSound = SynthSound;
				break;
				case 1:
				Item.UseSound = BassSound;
				break;
				case 2:
				Item.UseSound = LeadSound;
				break;
			}
			SoundEngine.PlaySound(Item.UseSound.Value.WithPitchOffset(
				Math.Min(((Main.MouseWorld - player.Center) / new Vector2(Main.screenWidth * 0.4f, Main.screenHeight * 0.4f)).Length(), 1) * 2 - 1
			), player.Center);
			return null;
		}
		public override Vector2? HoldoutOffset() => new Vector2(-6, 0);
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyManaCost(Player player, ref float reduce, ref float mult) {
			if (player.altFunctionUse == 2) mult *= 0;
		}
		public override void ModifyWeaponCrit(Player player, ref float crit) {
			if (player.OriginPlayer().keytarMode == 2) crit *= 3;
		}
		public override bool CanShoot(Player player) => player.altFunctionUse != 2;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			switch (player.OriginPlayer().keytarMode) {
				case 1:
				type = ModContent.ProjectileType<Keytar_Bass>();
				break;
				case 2:
				type = ModContent.ProjectileType<Keytar_Lead>();
				break;
			}
		}
		public override void SetDefaults() {
			SetSharedDefaults(Item, out Item.mana);
			Item.shoot = ModContent.ProjectileType<Keytar_Synth>();
		}
		public static void ProjectileAI(ModProjectile proj, float r, float g, float b, int frame_time = 5) {
			proj.Projectile.rotation = proj.Projectile.velocity.ToRotation() + MathHelper.PiOver2 * (proj.Projectile.direction - 1);
			proj.Projectile.spriteDirection = proj.Projectile.direction;
			if (++proj.Projectile.frameCounter >= frame_time && ++proj.Projectile.frame >= Main.projFrames[proj.Type]) proj.Projectile.frame = 0;
			const int HalfSpriteWidth = 32 / 2;
			const int HalfSpriteHeight = 32 / 2;

			int HalfProjWidth = proj.Projectile.width / 2;
			int HalfProjHeight = proj.Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			proj.DrawOriginOffsetX = 0;
			proj.DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			proj.DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
			Lighting.AddLight(proj.Projectile.Center, r, g, b);
		}
		public static bool ProjectileTileCollideStyle(ref int width, ref int height) {
			width = 24;
			height = 24;
			return true;
		}
	}
	public class Keytar_Synth : ModProjectile {
		public const int frame_time = 5;
		public const int frame_count = 1;
		public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("ThoriumMod");
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = frame_count;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Generic;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.friendly = true;
		}
		public override void AI() {
			Keytar.ProjectileAI(this, 0.25f, 0.25f, 0.75f);
		}
		public static void OnHitNPC(NPC target) {
			target.AddBuff(BuffID.Venom, Main.rand.Next(180, 301));
			if (Main.rand.NextBool(4)) target.AddBuff(Toxic_Shock_Debuff.ID, Main.rand.Next(180, 301));
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OnHitNPC(target);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => Keytar.ProjectileTileCollideStyle(ref width, ref height);
	}
	public class Keytar_Bass : ModProjectile {
		public const int frame_time = 5;
		public const int frame_count = 1;
		public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("ThoriumMod");
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = frame_count;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Generic;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.friendly = true;
		}
		public override void AI() {
			Keytar.ProjectileAI(this, 0.75f, 0.25f, 0.25f);
		}
		public static void OnHitNPC(NPC target) {
			if (target.wet && Main.rand.NextBool(4)) target.AddBuff(Cavitation_Debuff.ID, Main.rand.Next(180, 301));
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OnHitNPC(target);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => Keytar.ProjectileTileCollideStyle(ref width, ref height);
	}
	public class Keytar_Lead : ModProjectile {
		public const int frame_time = 5;
		public const int frame_count = 1;
		public override bool IsLoadingEnabled(Mod mod) => !ModLoader.HasMod("ThoriumMod");
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = frame_count;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Generic;
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.friendly = true;
		}
		public override void AI() {
			Keytar.ProjectileAI(this, 0.85f, 0.75f, 0.25f);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => Keytar.ProjectileTileCollideStyle(ref width, ref height);
	}
	#endregion without thorium
	#region with thorium
	[ExtendsFromMod("ThoriumMod")]
	public class Keytar_Thorium : BardItem, ICustomWikiStat {
		public override string Name => base.Name[..^"_Thorium".Length];
		public override BardInstrumentType InstrumentType => BardInstrumentType.Electronic;
		public override void SetStaticDefaults() {
			Empowerments.AddInfo<EmpowermentProlongation>(2);
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			Keytar.ID = Type;
		}
		public override void SetBardDefaults() {
			Keytar.SetSharedDefaults(Item, out int cost);
			Item.shoot = ModContent.ProjectileType<Keytar_Synth_Thorium>();
			InspirationCost = cost / 10;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Busted_Servo>(17)
			.AddIngredient<Rotor>(8)
			.AddIngredient<Watered_Down_Keytar_Thorium>()
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
		public override void BardUseAnimation(Player player) {
			if (player.altFunctionUse == 2) {
				ItemID.Sets.SkipsInitialUseSound[Type] = false;
				player.itemRotation = 0;
			} else {
				ItemID.Sets.SkipsInitialUseSound[Type] = true;
				switch (player.OriginPlayer().keytarMode) {
					default:
					Item.UseSound = Keytar.SynthSound;
					break;
					case 1:
					Item.UseSound = Keytar.BassSound;
					break;
					case 2:
					Item.UseSound = Keytar.LeadSound;
					break;
				}
			}
		}
		public override void BardModifyTooltips(List<TooltipLine> tooltips) {
			Keytar.AddTooltips(tooltips, 2);
		}
		public override Vector2? HoldoutOffset() => new Vector2(-6, 0);
		public override bool AltFunctionUse(Player player) => true;
		public override void ModifyInspirationCost(Player player, ref int cost) {
			if (player.altFunctionUse == 2) cost = 0;
		}
		public override void GetInstrumentCrit(ThoriumPlayer bard, ref float crit) {
			if (bard.Player.OriginPlayer().keytarMode == 2) crit *= 2;
		}
		public override bool? BardUseItem(Player player) {
			if (player.altFunctionUse == 2) Keytar.CycleMode(player);
			return base.BardUseItem(player);
		}
		/*public override bool CanPlayInstrument(Player player) {
			return base.CanPlayInstrument(player);
		}*/
		public override bool CanApplyEmpowerment(Player player, Player target, int group, int empowerment) => player.altFunctionUse != 2;
		public override bool CanShoot(Player player) => player.altFunctionUse != 2;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			switch (player.OriginPlayer().keytarMode) {
				case 1:
				type = ModContent.ProjectileType<Keytar_Bass_Thorium>();
				break;
				case 2:
				type = ModContent.ProjectileType<Keytar_Lead_Thorium>();
				break;
			}
		}
		public string CustomStatPath => WikiPageExporter.GetWikiName(this) + "_Thorium";
	}
	[ExtendsFromMod("ThoriumMod")]
	public class Keytar_Synth_Thorium : BardProjectile {
		public override string Name => base.Name[..^"_Thorium".Length];
		public override BardInstrumentType InstrumentType => BardInstrumentType.Electronic;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = Keytar_Synth.frame_count;
		}
		public override void SetBardDefaults() {
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.friendly = true;
		}
		public override void AI() {
			Keytar.ProjectileAI(this, 0.25f, 0.25f, 0.75f);
		}
		public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Keytar_Synth.OnHitNPC(target);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => Keytar.ProjectileTileCollideStyle(ref width, ref height);
	}
	[ExtendsFromMod("ThoriumMod")]
	public class Keytar_Bass_Thorium : BardProjectile {
		public override string Name => base.Name[..^"_Thorium".Length];
		public override BardInstrumentType InstrumentType => BardInstrumentType.Electronic;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = Keytar_Bass.frame_count;
		}
		public override void SetBardDefaults() {
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.friendly = true;
		}
		public override void AI() {
			Keytar.ProjectileAI(this, 0.75f, 0.25f, 0.25f);
		}
		public override void BardOnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Keytar_Bass.OnHitNPC(target);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => Keytar.ProjectileTileCollideStyle(ref width, ref height);
	}
	[ExtendsFromMod("ThoriumMod")]
	public class Keytar_Lead_Thorium : BardProjectile {
		public override string Name => base.Name[..^"_Thorium".Length];
		public override BardInstrumentType InstrumentType => BardInstrumentType.Electronic;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = Keytar_Bass.frame_count;
		}
		public override void SetBardDefaults() {
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.friendly = true;
		}
		public override void AI() {
			Keytar.ProjectileAI(this, 0.85f, 0.75f, 0.25f);
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => Keytar.ProjectileTileCollideStyle(ref width, ref height);
	}
	#endregion with thorium
}