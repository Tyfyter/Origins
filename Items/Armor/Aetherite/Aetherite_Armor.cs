using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Magic;
using Origins.NPCs.MiscB.Shimmer_Construct;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Aetherite {
	[AutoloadEquip(EquipType.Head)]
	public class Aetherite_Wreath : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public static int HeadSlot { get; private set; }
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
			HeadSlot = Item.headSlot;
		}
		public override void SetDefaults() {
			Item.defense = 8;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.projectileSpeedBoost += 0.15f;
			originPlayer.meleeScaleMultiplier *= 1.15f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Aetherite_Robe>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Aetherite").SubstituteKeybind(Keybindings.TriggerSetBonus);
			player.OriginPlayer().setActiveAbility = SetActiveAbility.aetherite_armor;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Aetherite_Bar>(12)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public string ArmorSetName => "Aetherite_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Aetherite_Robe>();
		public int LegsItemID => 0;
	}
	[AutoloadEquip(EquipType.Body)]
	public class Aetherite_Robe : ModItem, INoSeperateWikiPage {
		int legsID;
		public override void Load() {
			legsID = EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, name: $"{Name}_{EquipType.Legs}");
		}
		public override void SetDefaults() {
			Item.defense = 8;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Generic) += 0.24f;
			player.maxMinions += 1;
			if (!player.controlDown) player.gravity *= 0.8f;
			player.jumpSpeedBoost += 2f;
		}
		public override void SetMatch(bool male, ref int equipSlot, ref bool robes) {
			robes = true;
			equipSlot = legsID;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Aetherite_Bar>(24)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Aetherite_Aura_P : ModProjectile, IPreDrawSceneProjectile, ITriggerSCBackground {
		public override string Texture => "Terraria/Images/Misc/StarDustSky/Planet";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.NeedsUUID[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.scale = 0;
		}
		Stack<Vector2> positions = new();
		public static float MaxRadius = 312f;
		public override void AI() {
			Projectile.timeLeft = 2;
			if (Projectile.ai[0] == 0) {
				if (Projectile.scale == 0 && Projectile.IsLocallyOwned()) {
					Rectangle area = new Rectangle(0, 0, (int)(MaxRadius * 2), (int)(MaxRadius * 2)).Recentered(Projectile.position);
					List<Vector2> newPositions = OriginExtensions.PoissonDiskSampling(Main.rand, area, v => v.WithinRange(Projectile.position, MaxRadius), 16 * 15);
					newPositions.Sort((a, b) => b.DistanceSQ(Projectile.position).CompareTo(a.DistanceSQ(Projectile.position)));
					for (int i = 0; i < newPositions.Count; i++) {
						positions.Push(newPositions[i]);
					}
				}
				if (MathUtils.LinearSmoothing(ref Projectile.scale, 1, 1 / 60f)) {
					Projectile.ai[0] = 1;
					Projectile.netUpdate = true;
				}
			} else if (++Projectile.ai[0] > 600) {
				if (MathUtils.LinearSmoothing(ref Projectile.scale, 0, 1 / 60f)) Projectile.Kill();
			}
			float radius = MaxRadius * Projectile.scale;
			if (positions.TryPeek(out Vector2 next) && next.WithinRange(Projectile.position, radius)) {
				SoundEngine.PlaySound(SoundID.Item88, Projectile.Center);
				SoundEngine.PlaySound(SoundID.Item91.WithPitchRange(1.65f, 1.8f).WithVolume(0.75f), Projectile.Center);
				Projectile.SpawnProjectile(null,
					positions.Pop(),
					Vector2.Zero,
					Friendly_Shimmer_Landmine.ID,
					30,
					1,
					Projectile.projUUID
				);
			}
			foreach (Player player in Main.ActivePlayers) {
				if (player.Hitbox.IsWithin(Projectile.position, radius)) {
					player.AddBuff(Weak_Shimmer_Debuff.ID, 5, true);
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) => false;

		public void PreDrawScene() {
			if (SC_Phase_Three_Underlay.DrawnMaskSources.Add(Projectile)) {
				Texture2D circle = TextureAssets.Projectile[Type].Value;
				SC_Phase_Three_Underlay.DrawDatas.Add(new(
					circle,
					Projectile.position - Main.screenPosition,
					null,
					Color.White
				) {
					origin = circle.Size() * 0.5f,
					scale = Vector2.One * Projectile.scale
				});
				SC_Phase_Three_Underlay.AddMinLightArea(Projectile.position, (MaxRadius + 32) * Projectile.scale);
			}
		}
	}
	public class Friendly_Shimmer_Landmine : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowRodBullet;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 3;
			ProjectileID.Sets.TrailCacheLength[Type] = 30;
			ProjectileID.Sets.NoLiquidDistortion[Type] = true;
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Generic;
			Projectile.friendly = true;
			Projectile.timeLeft = 60 * 15;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.tileCollide = false;
			Projectile.scale = 0.85f;
		}
		public override void AI() {
			float GetTargetOpacity() {
				Projectile owner = Main.projectile.GetIfInRange(Projectile.GetByUUID(Projectile.owner, Projectile.ai[0]));
				if (owner?.active ?? false) {
					float radius = Aetherite_Aura_P.MaxRadius * owner.scale;
					if (Projectile.Center.WithinRange(owner.position, radius)) {
						return 1;
					} else if (Projectile.Center.WithinRange(owner.position, Aetherite_Aura_P.MaxRadius)) {
						return 0;
					}
					return -1;
				} else {
					return -1;
				}
			}
			float smoothSpeed = 0.1f;
			if (Projectile.localAI[0] == 0) {
				smoothSpeed = 1f;
				Projectile.localAI[0] = 1;
				Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
				Vector2 pos = Main.player[Projectile.owner].MountedCenter;
				float rotation = (pos - Projectile.position).ToRotation();
				for (int i = 0; i < Projectile.oldPos.Length; i++) {
					Projectile.oldPos[i] = Vector2.Lerp(Projectile.position, pos, i / (float)Projectile.oldPos.Length);
					Projectile.oldRot[i] = rotation;
				}
			}
			if (Projectile.timeLeft < 15) {
				Projectile.Opacity = Projectile.timeLeft / 15f;
			} else {
				float targetOpacity = GetTargetOpacity();
				if (targetOpacity == -1) {
					Projectile.active = false;
				} else {
					float opacity = Projectile.Opacity;
					MathUtils.LinearSmoothing(ref opacity, targetOpacity, smoothSpeed);
					Projectile.Opacity = opacity;
				}
			}
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			int direction = Math.Sign(target.Center.X - Projectile.Center.X);
			if (direction == 0) direction = Main.rand.NextBool().ToDirectionInt();
			modifiers.HitDirectionOverride = direction;
			modifiers.KnockbackImmunityEffectiveness *= 0.8f;
			modifiers.Knockback.Base += 6;
		}
		public override bool PreDraw(ref Color lightColor) {
			Shimmerstar_Staff_P.DrawShimmerstar(Projectile);
			return false;
		}
	}
}
