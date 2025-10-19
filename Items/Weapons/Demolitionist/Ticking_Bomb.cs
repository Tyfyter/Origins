using Microsoft.Xna.Framework.Graphics;
using Origins.UI;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Origins.Items.Weapons.Demolitionist {
	public class Ticking_Bomb : ModItem {
		public static float DamageMult => 1 + ModContent.GetInstance<Ticking_Explosives_UI>().TotalSeconds * 0.125f;
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatCountAsBombsForDemolitionistToSpawn[Type] = true;
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Bomb);
			Item.damage = 65;
			Item.shootSpeed *= 1.5f;
			Item.value = 1000;
			Item.shoot = ModContent.ProjectileType<Ticking_Bomb_P>();
			Item.ammo = ItemID.Bomb;
			Item.rare = ItemRarityID.Orange;
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			for (int i = 0; i < tooltips.Count; i++) {
				if (tooltips[i].Name == "Tooltip0") {
					tooltips[i].Text = string.Format(tooltips[i].Text, ModContent.GetInstance<Ticking_Explosives_UI>().TotalSeconds, DamageMult);
					break;
				}
			}
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			damage *= DamageMult;
		}
	}
	public class Ticking_Explosives_UI : ItemModeHUD {
		AutoLoadingAsset<Texture2D> texture = typeof(Ticking_Explosives_UI).GetDefaultTMLName();
		public override bool IsActive() => Main.LocalPlayer.HeldItem.ModItem is Ticking_Bomb;
		public float rotation = float.Epsilon;
		public bool dragging = true;
		public int seconds = 0;
		public float TotalSeconds {
			get {
				float value = seconds + rotation / MathHelper.TwoPi;
				if (rotation == 0) value += 1;
				return value;
			}
		}
		public override bool ShouldToggle() {
			switch (PlayerInput.CurrentInputMode) {
				default:
				case InputMode.Keyboard or InputMode.KeyboardUI:
				return base.ShouldToggle();

				case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
				return Main.mouseRight != isShowing;
			}
		}
		public float RotationControl {
			get {
				switch (PlayerInput.CurrentInputMode) {
					default:
					case InputMode.Keyboard or InputMode.KeyboardUI:
					return (Main.MouseScreen - activationPosition).ToRotation();

					case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
					return PlayerInput.GamepadThumbstickRight.ToRotation();
				}
			}
		}
		public bool IsHovering {
			get {
				switch (PlayerInput.CurrentInputMode) {
					default:
					case InputMode.Keyboard or InputMode.KeyboardUI:
					return new Rectangle((int)activationPosition.X - 8, (int)activationPosition.Y - 48, 16, 56).Contains(Main.MouseScreen.RotatedBy(-rotation, activationPosition));

					case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
					return true;
				}
				
			}
		}
		public bool IsTryingToDrag {
			get {
				switch (PlayerInput.CurrentInputMode) {
					default:
					case InputMode.Keyboard or InputMode.KeyboardUI:
					return Main.mouseLeft && Main.mouseLeftRelease;

					case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
					return PlayerInput.GamepadThumbstickRight != default;
				}
			}
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			Rectangle frame = texture.Value.Frame(verticalFrames: 3, sizeOffsetY: -2);
			switch (PlayerInput.CurrentInputMode) {
				case InputMode.XBoxGamepad or InputMode.XBoxGamepadUI:
				activationPosition = (Main.LocalPlayer.Top - Vector2.UnitY * 64).ToScreenPosition();
				break;
			}
			spriteBatch.Draw(
				texture,
				activationPosition,
				frame,
				Color.White,
				0,
				frame.Size() * 0.5f,
				1,
				0,
			0);
			frame.Y += frame.Height + 2;
			spriteBatch.Draw(
				texture,
				activationPosition,
				frame,
				Color.White,
				rotation,
				frame.Size() * 0.5f,
				1,
				0,
			0);
			if (dragging || IsHovering) {
				Main.LocalPlayer.mouseInterface = true;
				frame.Y += frame.Height + 2;
				spriteBatch.Draw(
					texture,
					activationPosition,
					frame,
					Main.OurFavoriteColor,
					rotation,
					frame.Size() * 0.5f,
					1,
					0,
				0);
				if (IsTryingToDrag) dragging = true;
			}
			if (dragging) {
				if (!Main.mouseLeft) dragging = false;
				float oldRotation = rotation;
				rotation = RotationControl + MathHelper.PiOver2;
				rotation = MathHelper.WrapAngle(rotation - MathHelper.Pi) + MathHelper.Pi;
				float fullRotFactor = MathHelper.WrapAngle(oldRotation + MathHelper.Pi) - MathHelper.WrapAngle(rotation + MathHelper.Pi);
				if (seconds <= 0 && oldRotation < MathHelper.TwoPi - 4 && rotation > 4) rotation = float.Epsilon;
				if (Math.Abs(fullRotFactor) > 4 && oldRotation != float.Epsilon) {
					seconds += Math.Sign(fullRotFactor);
					if (seconds < 0) {
						seconds = 0;
						rotation = 0;
					} else if (seconds > 11) {
						seconds = 11;
						rotation = 0;
					} else {
						SoundEngine.PlaySound(SoundID.MenuTick);
					}
				}
			}
			string text;
			switch ((int)TotalSeconds) {
				default:
				return;
				case 1:
				text = "I";
				break;
				case 2:
				text = "II";
				break;
				case 3:
				text = "III";
				break;
				case 4:
				text = "IV";
				break;
				case 5:
				text = "V";
				break;
				case 6:
				text = "VI";
				break;
				case 7:
				text = "VII";
				break;
				case 8:
				text = "VIII";
				break;
				case 9:
				text = "IX";
				break;
				case 10:
				text = "X";
				break;
				case 11:
				text = "XI";
				break;
				case 12:
				text = "XII";
				break;
			}
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.CombatText[0].Value,
				text,
				activationPosition - Vector2.UnitY * 48,
				Color.White,
				0,
				FontAssets.CombatText[0].Value.MeasureString(text) * new Vector2(0.5f, 1),
				Vector2.One
			);
		}
	}
	public class Ticking_Bomb_P : ModProjectile {
		public override string Texture => typeof(Ticking_Bomb).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 0;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.friendly = false;
			Projectile.timeLeft = 60 * 20;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[2] = (int)(60 * ModContent.GetInstance<Ticking_Explosives_UI>().TotalSeconds);
		}
		public override void AI() {
			if (Projectile.timeLeft > 3 && --Projectile.ai[2] <= 0) Projectile.timeLeft = 3;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.type = ProjectileID.Bomb;
			return true;
		}
		public override void OnKill(int timeLeft) {
			Projectile.friendly = true;
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
		}
	}
}
