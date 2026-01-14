using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Dev;
using Origins.Items.Weapons.Magic;
using Origins.Layers;
using Origins.Reflection;
using Origins.UI;
using System;
using System.Buffers;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Space_Pirates_Eye : ModItem, IRightClickableAccessory {
		public static List<PirateEyeMode> Colors { get; } = [];
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask(EquipType.Face, Item.faceSlot,
				$"{Texture}_{EquipType.Face}_Glow",
				player => Colors.GetIfInRange(player.OriginPlayer().spacePirateEyeSelection).Color
			);
			ArmorIDs.Face.Sets.DrawInFaceUnderHairLayer[Item.faceSlot] = true;

			AddColor(0xff0060, new(ProjectileID.MiniRetinaLaser), 60, (v, t) => v.Normalized(out _) * 8);//#ff0060
			AddColor(0x80ff00, new(ProjectileID.CursedDartFlame), 60, (v, t) => v.Normalized(out _) * 8);//#80ff00
			AddColor(0xff0000, new(ProjectileID.SharpTears), 60, (v, t) => v.Normalized(out _) * 8);//#ff0000
			AddColor(0xff6000, new(ProjectileID.Flamelash), 60, (v, t) => v.Normalized(out _) * 8);//#ff6000
			AddColor(0xffbf00, new(ProjectileID.IchorSplash), 60, (v, t) => v.Normalized(out _) * 8);//#ffbf00
			AddColor(0xdfff00, new(1), 60, (v, t) => v.Normalized(out _) * 8);//#dfff00
			AddColor(0x00ff9f, new(ProjectileID.PoisonFang), 60, (v, t) => v.Normalized(out _) * 8);//#00ff9f
			AddColor(0x00ffff, new(ModContent.ProjectileType<Magnus_P>()), 60, (v, t) => v.Normalized(out _) * 8);//#00ffff
			AddColor(0x009fff, new(ProjectileID.FrostBoltStaff), 60, (v, t) => v.Normalized(out _) * 8);//#009fff
			AddColor(0x2000ff, new(ProjectileID.WaterStream, 0.15f, 0.15f), 6, (v, t) => v.Normalized(out _) * 8);//#2000ff
			AddColor(0x8000ff, new(1), 60, (v, t) => v.Normalized(out _) * 8);//#8000ff
			AddColor(0xdf00ff, new(1), 60, (v, t) => v.Normalized(out _) * 8);//#df00ff
			AddColor(0xff00bf, new(1), 60, (v, t) => v.Normalized(out _) * 8);//#ff00bf
			AddColor(0xff9ae9, new(1), 60, (v, t) => v.Normalized(out _) * 8);//#ff9ae9
			AddColor(0x009700, new(1), 60, (v, t) => v.Normalized(out _) * 8);//#009700
			AddColor(0xa74d00, new(ProjectileID.WoodenArrowFriendly), 60, (v, t) => v.Normalized(out _) * 8);//#a74d00
			Array.Resize(ref counts, Colors.Count);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(34, 20);
			Item.damage = 60;
			Item.DamageType = DamageClass.Magic;
			Item.knockBack = 3;
			Item.rare = ItemRarityID.LightRed;
			Item.master = true;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().spacePirateEye = Item;
		}
		public static void UpdateEye(Player player, int mode) {
			if (mode == -1) return;
			OriginPlayer originPlayer = player.OriginPlayer();
			if (mode < 0) {
				int lowest = GetPlayerCounts(player);
				for (int i = 0; i < counts.Length; i++) {
					if (counts[i] == lowest) {
						originPlayer.spacePirateEyeSelection = mode = i;
						break;
					}
				}
			}

			if (originPlayer.spacePirateEyeCooldown > 0) return;
			Vector2 position = player.Center + new Vector2(2 * player.direction, (12 - player.height * 0.5f) * player.gravDir);
			Vector2 targetPos = position;
			Entity targetEntity = null;
			float maxDist = 16 * 25;
			maxDist *= maxDist;
			bool FindTarget(Entity target) {
				Vector2 currentPos = position.Clamp(target.Hitbox);
				if (Math.Sign(position.X - currentPos.X) == player.direction) return false;
				float dist = currentPos.DistanceSQ(position);
				if (dist < maxDist) {
					maxDist = dist;
					targetEntity = target;
					targetPos = currentPos;
					return true;
				}
				return false;
			}
			if (player.DoHoming(FindTarget)) {
				(_, SpecialAttackStats stats, originPlayer.spacePirateEyeCooldown, Func<Vector2, Entity, Vector2> vel) = Colors[mode];
				int damage = originPlayer.spacePirateEye.damage;
				float knockback = originPlayer.spacePirateEye.knockBack;
				try {
					originPlayer.spacePirateEye.damage = (int)(damage * stats.DamageMult);
					originPlayer.spacePirateEye.knockBack *= stats.KnockbackMult;
					player.SpawnProjectile(
						player.GetSource_Accessory(originPlayer.spacePirateEye),
						position,
						vel(targetPos - position, targetEntity),
						stats.ProjectileType,
						player.GetWeaponDamage(originPlayer.spacePirateEye),
						player.GetWeaponKnockback(originPlayer.spacePirateEye)
					);
				} finally {
					originPlayer.spacePirateEye.damage = damage;
					originPlayer.spacePirateEye.knockBack = knockback;
				}
			}
		}
		internal static int[] counts = [];
		/// <returns>the lowest count</returns>
		public static int GetPlayerCounts(Player forPlayer) {
			Array.Clear(counts);
			foreach (Player player in Main.ActivePlayers) {
				if (player == forPlayer) continue;
				int color = player.OriginPlayer().spacePirateEyeSelection;
				if (color < 0) continue;
				counts[color]++;
			}
			int lowest = int.MaxValue;
			for (int i = 0; i < counts.Length; i++) {
				Min(ref lowest, counts[i]);
			}
			counts[(int)(Main.timeForVisualEffects / 30) % counts.Length] = 1; //just to demo what it looks like when a color is taken
			return lowest;
		}
		static void AddColor(uint hexColor, SpecialAttackStats stats, int cooldown, Func<Vector2, Entity, Vector2> velocity) {
			Colors.Add(new(hexColor, stats, cooldown, velocity));
		}
		public record struct SpecialAttackStats(int ProjectileType, float DamageMult = 1, float KnockbackMult = 1);
		public record struct PirateEyeMode(Color Color, SpecialAttackStats Stats, int Cooldown, Func<Vector2, Entity, Vector2> Velocity) {
			public PirateEyeMode(uint hexColor, SpecialAttackStats stats, int cooldown, Func<Vector2, Entity, Vector2> velocity) : this(FromHexRGB(hexColor), stats, cooldown, velocity) { }
		}
		public bool CanRightClickAccessory(Item[] inv, int context, int slot) => true;
		public bool RightClickAccessory(Item[] inv, int context, int slot) {
			OriginSystem.Instance.SpacePirateEyeUI.Activate();
			return false;
			Player player = Main.LocalPlayer;
			OriginPlayer originPlayer = player.OriginPlayer();
			int lowest = GetPlayerCounts(player);
			if (ItemSlot.ShiftInUse) {
				for (int i = originPlayer.spacePirateEyeSelection - 1; i >= 0; i--) {
					if (counts[i] == lowest) {
						originPlayer.spacePirateEyeSelection = i;
						return false;
					}
				}
				for (int i = counts.Length - 1; i > originPlayer.spacePirateEyeSelection; i++) {
					if (counts[i] == lowest) {
						originPlayer.spacePirateEyeSelection = i;
						return false;
					}
				}
				return false;
			}
			for (int i = originPlayer.spacePirateEyeSelection + 1; i < counts.Length; i++) {
				if (counts[i] == lowest) {
					originPlayer.spacePirateEyeSelection = i;
					return false;
				}
			}
			for (int i = 0; i < originPlayer.spacePirateEyeSelection; i++) {
				if (counts[i] == lowest) {
					originPlayer.spacePirateEyeSelection = i;
					return false;
				}
			}
			return false;
		}
	}
	public class SpacePirateEyeInterface : UserInterface {
		readonly LegacyGameInterfaceLayer interactionLayer;
		readonly LegacyGameInterfaceLayer displayLayer;
		bool isActive = false;
		Vector2 position;
		static int lowest = 0;
		public SpacePirateEyeInterface() : base() {
			interactionLayer = new LegacyGameInterfaceLayer(
				"Origins: Space Pirate's Eye UI Interaction",
				delegate {
					lowest = Space_Pirates_Eye.GetPlayerCounts(Main.LocalPlayer);
					EnsureButtons();
					for (int i = 0; i < buttons.Length; i++) {
						if (GetButton(i).Contains(Main.MouseScreen)) {
							Main.LocalPlayer.mouseInterface = true;
							IgnoreRemainingInterface.Activate();
							if (Main.mouseLeft && Main.mouseLeftRelease && Space_Pirates_Eye.counts[i] == lowest) {
								Main.LocalPlayer.OriginPlayer().spacePirateEyeSelection = i;
								isActive = false;
							}
						}
					}
					return true;
				},
				InterfaceScaleType.UI
			);
			displayLayer = new LegacyGameInterfaceLayer(
				"Origins: Space Pirate's Eye UI",
				delegate {
					EnsureButtons();
					Texture2D texture = TextureAssets.MagicPixel.Value;
					for (int i = 0; i < buttons.Length; i++) {
						Rectangle button = GetButton(i);
						Color color = Space_Pirates_Eye.Colors[i].Color;
						if (Space_Pirates_Eye.counts[i] != lowest) {
							color = color.Desaturate(0.5f);
						} else if (button.Contains(Main.MouseScreen)) {
							Main.spriteBatch.Draw(texture, button, (color.R * 0.375f + color.G * 0.5f + color.B * 0.125f) > 128 ? Color.Black : Color.White);
							button.Inflate(-2, -2);
						}
						Main.spriteBatch.Draw(texture, button, color);
						if (Space_Pirates_Eye.counts[i] != lowest) {
							color = color.Desaturate(0f) * 0.5f;
							color.A = 255;
							color = Color.Black;
							button.Inflate(-3, -3);
							Main.spriteBatch.Draw(texture, button, color);
						}
					}
					return true;
				},
				InterfaceScaleType.UI
			);
		}
		Rectangle[] buttons;
		void EnsureButtons() {
			if (buttons is not null) return;
			buttons = new Rectangle[Space_Pirates_Eye.Colors.Count];
			Vector2 pos = position - new Vector2(16);
			for (int i = 0; i < buttons.Length; i++) {
				buttons[i] = new((int)pos.X, (int)pos.Y, 14, 14);
				pos.X += 16;
				if (i % 8 == 7) {
					pos.X = position.X - 16;
					pos.Y += 16;
				}
			}
		}
		Rectangle GetButton(int i) {
			Rectangle button = buttons[i];
			button.X += Math.Min(Main.screenWidth - buttons[^1].Right, 0);
			button.Y += MainReflection.currentMapHeight.Value;
			return button;
		}
		public void Activate() {
			isActive = true;
			position = AccessorySlotLoaderMethods.CurrentSlotPosition;
			position += Vector2.One * 52 * 0.5f * Main.inventoryScale;
			position.Y -= MainReflection.currentMapHeight.Value;
			buttons = null;
		}
		public void Insert(List<GameInterfaceLayer> layers) {
			if (!isActive) return;
			if (!Main.playerInventory || Main.LocalPlayer.OriginPlayer().spacePirateEye is null) isActive = false;
			if (!isActive) return;
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex != -1) {//error prevention & null check
				interactionLayer.ScaleType = InterfaceScaleType.UI;
				layers.Insert(inventoryIndex + 1, displayLayer);
				layers.Insert(inventoryIndex, interactionLayer);
			}
		}
	}
}
