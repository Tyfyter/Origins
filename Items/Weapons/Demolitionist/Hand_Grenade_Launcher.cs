using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using ShootAction = System.Action<Terraria.Player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo, Microsoft.Xna.Framework.Vector2, Microsoft.Xna.Framework.Vector2, int, int, float>;

namespace Origins.Items.Weapons.Demolitionist {
	[ReinitializeDuringResizeArrays]
	public class Hand_Grenade_Launcher : ModItem, IBroken {
		public static string BrokenReason => "beenade alt-fire needs balancing";

		public static ShootAction[] AltFireAction = ProjectileID.Sets.Factory.CreateNamedSet($"{nameof(Hand_Grenade_Launcher)}_{nameof(AltFireAction)}")
		.RegisterCustomSet<ShootAction>(null);
		public static float[] AltUseTimeMultiplier = ProjectileID.Sets.Factory.CreateNamedSet($"{nameof(Hand_Grenade_Launcher)}_{nameof(AltUseTimeMultiplier)}")
		.RegisterFloatSet(1);
		public static float[] AltAnimationMultiplier = ProjectileID.Sets.Factory.CreateNamedSet($"{nameof(Hand_Grenade_Launcher)}_{nameof(AltAnimationMultiplier)}")
		.RegisterFloatSet(1);
		public static int?[] AltUseCount = ProjectileID.Sets.Factory.CreateNamedSet($"{nameof(Hand_Grenade_Launcher)}_{nameof(AltUseCount)}")
		.RegisterCustomSet<int?>(null);
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			SetupVanillaAltFires();
		}
		public override void SetDefaults() {
			Item.DefaultToLauncher(16, 50, 44, 18);
			Item.shoot = ProjectileID.Grenade;
			Item.useAmmo = ItemID.Grenade;
			Item.shootSpeed = 5f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
			Item.consumeAmmoOnLastShotOnly = true;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool? CanChooseAmmo(Item ammo, Player player) {
			if (player.altFunctionUse == 2 && AltFireAction[ammo.shoot] is null) return false;
			return base.CanChooseAmmo(ammo, player);
		}
		static int selectedProjType;
		public override float UseTimeMultiplier(Player player) {
			if (player.altFunctionUse == 2) return AltUseTimeMultiplier[selectedProjType];
			return base.UseTimeMultiplier(player);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2 && AltFireAction[type] is ShootAction shootAction) {
				shootAction(player, source, position, velocity, type, damage, knockback);
				return false;
			}
			return true;
		}
		class Hand_Grenade_Launcher_Tooltip : GlobalItem {
			public override bool AppliesToEntity(Item entity, bool lateInstantiation) => lateInstantiation && entity.ammo == ItemID.Grenade;
			public override void PickAmmo(Item weapon, Item ammo, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback) {
				weapon.useLimitPerAnimation = AltUseCount[type];
				if (player.altFunctionUse == 2 && player.ItemUsesThisAnimation == 0) {
					player.itemAnimation = player.itemAnimationMax = (int)(player.itemAnimationMax * AltAnimationMultiplier[type]);
				}
				selectedProjType = type;
			}
			public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
				if (Main.LocalPlayer?.HeldItem?.ModItem is not Hand_Grenade_Launcher || AltFireAction[item.shoot] is null) return;
				void InsertTooltip(ref int i) {
					tooltips.Insert(i + 1, new(Mod, "CanAltFireTooltip", Language.GetTextValue("Mods.Origins.Items.Hand_Grenade_Launcher.CanAltFireTooltip")));
					i = 0;
				}
				for (int i = tooltips.Count - 1; i >= 0; i--) {
					switch (tooltips[i].Name) {
						case "Ammo":
						case "Consumable":
						case "Material":
						InsertTooltip(ref i);
						break;
						default:
						if (tooltips[i].Name.StartsWith("Tooltip")) {
							InsertTooltip(ref i);
							break;
						}
						if (i < tooltips.Count - 1 && !tooltips[i].Name.StartsWith("Prefix") && tooltips[i + 1].Name.StartsWith("Prefix")) {
							i--;
							InsertTooltip(ref i);
							break;
						}
						break;
					}
				}
			}
		}
		static void SetupVanillaAltFires() {
			#region beenades
			AltUseTimeMultiplier[ProjectileID.Beenade] = 0.1f;
			AltAnimationMultiplier[ProjectileID.Beenade] = 0.5f;
			AltUseCount[ProjectileID.Beenade] = 4;
			AltFireAction[ProjectileID.Beenade] = (player, source, position, velocity, type, damage, knockback) => {
				position += velocity.SafeNormalize(Vector2.Zero);
				for (int i = Main.rand.Next(2); ++i < 6;) {
					type = player.beeType();
					damage = player.beeDamage(damage)/2;
					knockback = player.beeKB(knockback);
					Projectile.NewProjectileDirect(source, position, velocity.RotatedByRandom(0.1 * i) * Main.rand.NextFloat(0.4f, 0.8f), type, damage, knockback, player.whoAmI);
				}
			};
			#endregion beenades
			#region happy grenades
			AltUseTimeMultiplier[ProjectileID.PartyGirlGrenade] = 0.1f;
			AltUseCount[ProjectileID.PartyGirlGrenade] = 4;
			AltFireAction[ProjectileID.PartyGirlGrenade] = (player, source, position, velocity, type, damage, knockback) => {
				position += velocity.SafeNormalize(Vector2.Zero);
				for (int i = Main.rand.Next(2); ++i < 12;) {
					Projectile.NewProjectileDirect(source, position, velocity.RotatedByRandom(0.1 * (i % 8)) * Main.rand.NextFloat(0.5f, 1f), ModContent.ProjectileType<Happy_Grenade_Confetti>(), (int)(damage * 0.4f), 0, player.whoAmI);
				}
			};
			#endregion happy grenades
		}
		public class Happy_Grenade_Confetti : ModProjectile, IIsExplodingProjectile, IBroken {
			public override string Texture => "Terraria/Images/Dust";
			public bool IsExploding => Projectile.timeLeft <= 0;
			public static string BrokenReason => "needs balancing";
			public override void SetStaticDefaults() {
				Origins.MagicTripwireRange[Type] = 32;
			}
			public override void SetDefaults() {
				Projectile.DamageType = DamageClasses.ThrownExplosive;
				Projectile.width = 5;
				Projectile.height = 5;
				Projectile.timeLeft = 10 * 60;
				Projectile.friendly = true;
				Projectile.appliesImmunityTimeOnSingleHits = true;
				Projectile.usesIDStaticNPCImmunity = true;
				Projectile.idStaticNPCHitCooldown = 5;
			}
			public override void OnSpawn(IEntitySource source) {
				Projectile.scale = Projectile.ai[0] = 1f + Main.rand.Next(-20, 21) * 0.01f;
			}
			public override void AI() {
				Projectile.velocity *= 0.98f;
				if (Projectile.velocity.Y < 1f) Projectile.velocity.Y += 0.05f;

				if (Projectile.scale < Projectile.ai[0] * 1.5f) Projectile.scale += 0.009f;
				Projectile.rotation -= Projectile.velocity.X * 0.4f;

				if (Projectile.velocity.X > 0f) Projectile.rotation += 0.005f;
				else Projectile.rotation -= 0.005f;

				Projectile.rotation += Projectile.velocity.X * 0.5f;
				Projectile.velocity.X *= 0.99f;
			}
			public override bool OnTileCollide(Vector2 oldVelocity) {
				Projectile.velocity *= 0.9f;
				return false;
			}
			public override void OnKill(int timeLeft) {
				ExplosiveGlobalProjectile.DoExplosion(Projectile, 128, sound: SoundID.Item14, fireDustAmount: 4, smokeDustAmount: 6, smokeGoreAmount: Main.rand.NextBool(5).ToInt());
			}
			public override bool PreDraw(ref Color lightColor) {
				if (Projectile.localAI[0] == 0) {
					Projectile.localAI[0] = Main.rand.Next(DustID.Confetti_Blue, DustID.Confetti_Yellow + 1);
					Projectile.localAI[1] = Main.rand.Next(3) + 3;
				}
				Texture2D tex = TextureAssets.Projectile[Type].Value;
				Rectangle frame = tex.Frame(
					100,
					12,
					(int)Projectile.localAI[0] % 100,
					(int)Projectile.localAI[1]
				);

				Main.EntitySpriteDraw(tex,
					Projectile.Center - Main.screenPosition,
					frame,
					lightColor,
					Projectile.rotation,
					frame.Size() * 0.5f,
					Projectile.scale,
					SpriteEffects.None
				);
				return false;
			}
		}
	}
}
