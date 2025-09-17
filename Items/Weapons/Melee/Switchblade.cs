using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Tools;
using Origins.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Prefixes;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Switchblade_Broadsword : ModItem, ICustomWikiStat, IItemObtainabilityProvider {
        public string[] Categories => [
            "Sword"
        ];
		public override void SetStaticDefaults() {
			PrefixLegacy.ItemSets.SwordsHammersAxesPicks[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.LightsBane);
			Item.damage = 23;
			Item.DamageType = DamageClass.Melee;
			Item.width = 64;
			Item.height = 68;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 19;
			Item.useAnimation = 19;
			Item.knockBack = 5f;
			Item.shoot = ProjectileID.None;
			Item.rare = ItemRarityID.Blue;
			Item.autoReuse = true;
			Item.useTurn = true;
			Item.value = Item.sellPrice(silver: 27);
			Item.glowMask = -1;
		}
		public override bool AltFunctionUse(Player player) {
			int prefix = Item.prefix;
			Item.ChangeItemType(ModContent.ItemType<Switchblade_Shortsword>());
			Item.Prefix(prefix);
			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, player.selectedItem);
			}
			Switchblade_Crit_Type.StartCritTime(player);
			return false;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 10)//only bars used in vanilla evil sword recipes, no scales/samples
			.AddTile(TileID.Anvils)
			.Register();
		}
		public IEnumerable<int> ProvideItemObtainability() {
			yield return ModContent.ItemType<Pincushion_Inactive>();
		}
	}
	public class Switchblade_Shortsword : ModItem, ICustomWikiStat, INoSeperateWikiPage {
		public string[] Categories => [
			"Sword"
		];

		void ICustomWikiStat.ModifyWikiStats(JObject data) {
			data["Name"] = "Switchblade (Retracted)";
		}

		string ICustomWikiStat.CustomStatPath => "Switchblade_Shortsword";

		public override void SetStaticDefaults() {
			PrefixLegacy.ItemSets.SwordsHammersAxesPicks[Type] = true;
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Gladius);
			Item.damage = 23;
			Item.DamageType = DamageClass.Melee;
			Item.width = 46;
			Item.height = 50;
			Item.useTime = 9;
			Item.useAnimation = 18;
			Item.reuseDelay = 5;
			Item.knockBack = 5f;
			Item.shoot = ModContent.ProjectileType<Switchblade_Shortsword_P>();
			Item.shootSpeed = 2;
			Item.rare = ItemRarityID.Blue;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(silver: 27);
		}
		public override bool AltFunctionUse(Player player) {
			int prefix = Item.prefix;
			Item.ChangeItemType(ModContent.ItemType<Switchblade_Broadsword>());
			Item.Prefix(prefix);
			if (Main.netMode != NetmodeID.SinglePlayer) {
				NetMessage.SendData(MessageID.SyncEquipment, -1, -1, null, player.whoAmI, player.selectedItem);
			}
			Switchblade_Crit_Type.StartCritTime(player);
			return false;
		}
	}
	public class Switchblade_Shortsword_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Melee/Switchblade_Shortsword";
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GladiusStab);
			Projectile.timeLeft = 3600;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = 0;
		}
		public float movementFactor {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Vector2 ownerMountedCenter = player.RotatedRelativePoint(player.MountedCenter, true);
			Projectile.direction = player.direction;
			player.heldProj = Projectile.whoAmI;
			Projectile.position.X = ownerMountedCenter.X - (Projectile.width / 2);
			Projectile.position.Y = ownerMountedCenter.Y - (Projectile.height / 2);
			if (!player.frozen) {
				if (movementFactor == 0f) {
					SoundEngine.PlaySound(player.HeldItem.UseSound, Projectile.Center);
					movementFactor = 1f * Projectile.scale;
					Projectile.netUpdate = true;
				}
				movementFactor += 4f * Projectile.scale / player.itemTimeMax;
			}
			Projectile.position += Projectile.velocity * movementFactor;
			if (player.itemTime == 1) {
				Projectile.Kill();
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (projHitbox.Intersects(targetHitbox)) return true;
			Point basePos = projHitbox.Location;
			Vector2 factor = Projectile.velocity * 8.5f * Projectile.scale;
			for (int i = 0; i < 3; i++) {
				projHitbox.Location = new Point(basePos.X + (int)(factor.X * i), basePos.Y + (int)(factor.Y * i));
				if (projHitbox.Intersects(targetHitbox)) return true;
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition - Projectile.velocity,
				null,
				lightColor,
				Projectile.rotation + (Projectile.direction == 1 ? MathHelper.PiOver4 : MathHelper.PiOver4 * 3),
				new Vector2(23 - 8 * Projectile.direction, 36),
				Projectile.scale,
				Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
			return false;
		}
	}
	public class Switchblade_Crit_Type : CritType {
		static int CritDuration => 60 * 3;
		static int SpamProtectionGracePeriod => 30;
		public override LocalizedText Description => base.Description.WithFormatArgs(CritDuration / 60f);
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) => player.GetModPlayer<Switchblade_Crit_Player>().timeSinceSwitch < CritDuration;
		public override float CritMultiplier(Player player, Item item) => 1.4f;
		public override void PreSetup() {
			ForcedCritTypes[ModContent.ItemType<Switchblade_Shortsword>()] = this;
			ForcedCritTypes[ModContent.ItemType<Switchblade_Broadsword>()] = this;
		}
		public static void StartCritTime(Player player) {
			if (!player.TryGetModPlayer(out Switchblade_Crit_Player global)) return;
			if (global.timeSinceSwitch > CritDuration - SpamProtectionGracePeriod) {
				global.timeSinceSwitch = 0;
			}
		}
		class Switchblade_Crit_Player : CritModPlayer {
			public int timeSinceSwitch = CritDuration;
			public override bool IsLoadingEnabled(Mod mod) => ModEnabled;
			public override void ResetEffects() => timeSinceSwitch.Warmup(CritDuration);
		}
	}
}
