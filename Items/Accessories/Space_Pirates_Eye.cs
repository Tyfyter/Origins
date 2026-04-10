using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Layers;
using Origins.Reflection;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics.Shaders;
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
				player =>  Colors.GetIfInRange(player.OriginPlayer().SpacePirateEyeVisualSelection)?.Color ?? Color.Transparent//(Main.timeForVisualEffects % 30 < 15 ? Color.Black : Color.Magenta)
			);
			ArmorIDs.Face.Sets.DrawInFaceUnderHairLayer[Item.faceSlot] = true;

			Colors.Sort();
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
			if (originPlayer.spacePirateEye is null) return;
			if (mode < 0) {
				int lowest = GetPlayerCounts(player);
				if (originPlayer.spacePirateEyePreference > -2) {
					if (originPlayer.spacePirateEyePreference == -1 || counts[originPlayer.spacePirateEyePreference] == lowest) {
						originPlayer.spacePirateEyeSelection = originPlayer.spacePirateEyePreference;
					}
					if (originPlayer.spacePirateEyeSelection == -1) return;
				}
				for (int i = 0; i < counts.Length && originPlayer.spacePirateEyeSelection < 0; i++) {
					if (counts[i] == lowest) {
						originPlayer.spacePirateEyeSelection = i;
						break;
					}
				}
				mode = originPlayer.spacePirateEyeSelection;
			}

			if (originPlayer.spacePirateEyeCooldown > 0) return;
			Vector2 position = player.Center + new Vector2(2 * player.direction, (12 - player.height * 0.5f) * player.gravDir);
			PirateEyeMode eyeMode = Colors[mode];
			if (eyeMode.FindTarget(player, position, out Vector2 targetPos, out Entity targetEntity)) {
				originPlayer.spacePirateEyeCooldown = eyeMode.Cooldown;
				int damage = originPlayer.spacePirateEye.damage;
				float knockback = originPlayer.spacePirateEye.knockBack;
				try {
					originPlayer.spacePirateEye.damage = (int)(damage * eyeMode.DamageMult);
					originPlayer.spacePirateEye.knockBack *= eyeMode.KnockbackMult;
					eyeMode.Shoot(player,
						targetEntity,
						player.GetSource_Accessory(originPlayer.spacePirateEye),
						position,
						eyeMode.GetVelocity(player, targetPos - position, targetEntity),
						eyeMode.Type,
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
		public bool CanRightClickAccessory(Item[] inv, int context, int slot) => Math.Abs(context) != ItemSlot.Context.EquipAccessoryVanity;
		public bool RightClickAccessory(Item[] inv, int context, int slot) {
			OriginSystem.Instance.SpacePirateEyeUI.Activate();
			/*Player player = Main.LocalPlayer;
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
			}*/
			return false;
		}
		public abstract class PirateEyeMode : ModProjectile, IComparable<PirateEyeMode> {
			public sealed override void Load() {
				Colors.Add(this);
				OnLoad();
			}
			public virtual void OnLoad() { }
			public abstract Color Color { get; }
			public virtual float DamageMult => 1;
			public virtual float KnockbackMult => 1;
			public abstract int Cooldown { get; }
			public virtual float Order => Main.rgbToHsl(Color).X;
			public virtual Vector2 GetVelocity(Player player, Vector2 difference, Entity target) => difference.Normalized(out _) * 8;
			public virtual void Shoot(Player player, Entity target, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
				player.SpawnProjectile(source, position, velocity, type, damage, knockback);
			}
			public virtual bool FindTarget(Player player, Vector2 position, out Vector2 targetPos, out Entity targetEntity) => DefaultFindTarget(player, position, out targetPos, out targetEntity);
			protected static bool DefaultFindTarget(Player player, Vector2 position, out Vector2 targetPos, out Entity targetEntity, float maxDist = 16 * 25) {
				Vector2 _targetPos = position;
				Entity _targetEntity = null;
				maxDist *= maxDist;

				bool result = player.DoHoming(FindTarget);
				targetEntity = _targetEntity;
				targetPos = _targetPos;
				return result;
				bool FindTarget(Entity target) {
					Vector2 currentPos = position.Clamp(target.Hitbox);
					if (Math.Sign(position.X - currentPos.X) == player.direction) return false;
					float dist = currentPos.DistanceSQ(position);
					if (dist < maxDist) {
						maxDist = dist;
						_targetEntity = target;
						_targetPos = currentPos;
						return true;
					}
					return false;
				}
			}
			int IComparable<PirateEyeMode>.CompareTo(PirateEyeMode other) => Order.CompareTo(other.Order);
		}
		//AddColor(0x00ffff, new(ModContent.ProjectileType<Magnus_P>()), 60);//#00ffff
		//AddColor(0x009fff, new(ProjectileID.FrostBoltStaff), 60);//#009fff
		//AddColor(0x2000ff, new(ProjectileID.WaterStream, 0.15f, 0.15f), 6);//#2000ff
		//AddColor(0x8000ff, new(1), 60);//#8000ff
		//AddColor(0xdf00ff, new(1), 60);//#df00ff
		//AddColor(0xff00bf, new(1), 60);//#ff00bf
		//AddColor(0xff9ae9, new(1), 60);//#ff9ae9
		//AddColor(0x009700, new(1), 60);//#009700
		//AddColor(0xa74d00, new(ProjectileID.WoodenArrowFriendly), 60);//#a74d00
		public class Laser : PirateEyeMode {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MiniRetinaLaser}";
			public override Color Color => FromHexRGB(0xff0060);//#ff0060
			public override float Order => -2;
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.MiniRetinaLaser);
				Projectile.DamageType = DamageClass.Magic;
				AIType = ProjectileID.MiniRetinaLaser;
			}
		}
		public class Cursed_Flames : PirateEyeMode {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CursedFlameHostile}";
			public override Color Color => FromHexRGB(0x80ff00);//#80ff00
			public override float Order => -1;
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.CursedFlameFriendly);
				AIType = ProjectileID.CursedFlameHostile;
			}
			public override void AI() {
				base.AI();
			}
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.CursedInferno, Main.rand.Next(120, 301));
		}
		public class _Temp_Red : PirateEyeMode {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.SharpTears}";
			public override Color Color => FromHexRGB(0xff0000);
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.SharpTears);
				AIType = ProjectileID.SharpTears;
			}
		}
		public class _Temp_Orange : PirateEyeMode {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Flamelash}";
			public override Color Color => FromHexRGB(0xff6000);
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.Flamelash);
				AIType = ProjectileID.Flamelash;
			}
		}
		public class Ichor_Spray : PirateEyeMode {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.IchorSplash}";
			public override Color Color => FromHexRGB(0xffbf00);
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.DamageType = DamageClass.Magic;
				Projectile.width = 10;
				Projectile.height = 10;
				Projectile.friendly = true;
				Projectile.alpha = 255;
				Projectile.penetrate = 5;
				Projectile.ignoreWater = true;
				Projectile.extraUpdates = 2;
				Projectile.usesIDStaticNPCImmunity = true;
				Projectile.idStaticNPCHitCooldown = 10;
				Projectile.timeLeft = 20;
			}
			public override void AI() {
				for (float i = 0; i < 1; i += 1f / 2) {
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ichor, 0f, 0f, 100);
					dust.position = (dust.position + Projectile.Center) / 2f + Projectile.velocity * i;
					dust.noGravity = true;
					dust.velocity *= 0.1f;
					dust.scale *= (800f - Projectile.ai[0]) / 800f + 0.1f;
				}
			}
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Ichor, Main.rand.Next(60, 181));
			public override bool FindTarget(Player player, Vector2 position, out Vector2 targetPos, out Entity targetEntity) {
				return DefaultFindTarget(player, position, out targetPos, out targetEntity, 16 * 10);
			}
			public override void Shoot(Player player, Entity target, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
				for (int i = 0; i < 6; i++) player.SpawnProjectile(source, position, velocity + Main.rand.NextVector2Circular(2, 2), type, damage, knockback);
			}
		}
		public class _Temp_Yellow : PirateEyeMode {
			public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MedusaHeadRay;
			public override Color Color => FromHexRGB(0xdfff00);
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.DamageType = DamageClass.Magic;
				Projectile.width = 0;
				Projectile.height = 0;
				Projectile.timeLeft = 30;
				Projectile.penetrate = -1;
				Projectile.friendly = true;
				Projectile.tileCollide = false;
			}
			public override bool ShouldUpdatePosition() => false;
			Triangle hitTri;
			public override void AI() {
				Player player = Main.player[Projectile.owner];
				Projectile.position = player.Center + new Vector2(2 * player.direction, (12 - player.height * 0.5f) * player.gravDir);
				Vector2 perp = Projectile.velocity.Perpendicular() * 0.5f;
				hitTri = new(
					Projectile.position,
					Projectile.position + Projectile.velocity + perp,
					Projectile.position + Projectile.velocity - perp
				);
			}
			public override Vector2 GetVelocity(Player player, Vector2 difference, Entity target) => difference.Normalized(out _) * 16 * 15;
			public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => hitTri.Intersects(targetHitbox);
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Midas, Main.rand.Next(180, 481));
			private readonly VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[3];
			public override bool PreDraw(ref Color lightColor) {

				vertices[0].TextureCoordinate = new Vector2(0, 1);
				vertices[1].TextureCoordinate = new Vector2(0, 0);
				vertices[2].TextureCoordinate = new Vector2(1, 1);
													  
				Color color = Color * Projectile.Opacity * 0.05f;
				vertices[0].Color = color;
				vertices[1].Color = color;
				vertices[2].Color = color;

				short[] dices = [0, 1, 2];
				GameShaders.Misc["Origins:Identity"]
				.UseImage0(TextureAssets.MagicPixel)//Extra[ExtrasID.LightDisc]
				.UseSamplerState(SamplerState.LinearClamp)
				.Apply();
				const int count = 5;
				Vector2 perp = Projectile.velocity.Perpendicular().Normalized(out _);
				for (int i = -count; i <= count; i++) {

					vertices[0].Position = new Vector3(hitTri.a + perp * i - Main.screenPosition, 0);//
					vertices[1].Position = new Vector3(hitTri.b + perp * i + Main.rand.NextVector2Circular(4, 4) - Main.screenPosition, 0);//
					vertices[2].Position = new Vector3(hitTri.c + perp * i + Main.rand.NextVector2Circular(4, 4) - Main.screenPosition, 0);//

					Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 3, dices, 0, 2);
				}
				return false;
			}
		}
		public class _Temp_Turquoise : PirateEyeMode {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.PoisonFang}";
			public override Color Color => FromHexRGB(0x00ff9f);
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.PoisonFang);
				AIType = ProjectileID.PoisonFang;
			}
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
					if (PlayerInput.IgnoreMouseInterface) return true;
					if (!GetBox().Contains(Main.MouseScreen)) return true;
					Main.LocalPlayer.mouseInterface = true;
					IgnoreRemainingInterface.Activate();
					for (int i = 0; i < buttons.Length; i++) {
						if (GetButton(i).Contains(Main.MouseScreen) && Main.mouseLeft && Main.mouseLeftRelease && Space_Pirates_Eye.counts[i] == lowest) {
							Main.LocalPlayer.OriginPlayer().spacePirateEyeSelection = i;
							isActive = false;
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
					Main.spriteBatch.Draw(TextureAssets.InventoryBack.Value, GetBox(), Color.Gainsboro);
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
		Rectangle entireBox;
		void EnsureButtons() {
			const int button_size = 14;
			const int padded_size = button_size + 2;
			if (buttons is not null) return;
			buttons = new Rectangle[Space_Pirates_Eye.Colors.Count];
			Vector2 pos = position - new Vector2(padded_size);
			Vector2 min = new(float.PositiveInfinity);
			Vector2 max = new(float.NegativeInfinity);
			for (int i = 0; i < buttons.Length; i++) {
				Min(ref min.X, pos.X);
				Min(ref min.Y, pos.Y);
				Max(ref max.X, pos.X + button_size);
				Max(ref max.Y, pos.Y + button_size);
				buttons[i] = new((int)pos.X, (int)pos.Y, button_size, button_size);
				pos.X += padded_size;
				if (i % 8 == 7) {
					pos.X = position.X - padded_size;
					pos.Y += padded_size;
				}
			}
			const int box_padding = 4;
			entireBox.X = (int)min.X - box_padding;
			entireBox.Y = (int)min.Y - box_padding;
			entireBox.Width = (int)(max.X - min.X) + box_padding * 2;
			entireBox.Height = (int)(max.Y - min.Y) + box_padding * 2;
		}
		Rectangle GetButton(int i) {
			Rectangle button = buttons[i];
			button.X += Math.Min(Main.screenWidth - entireBox.Right, 0);
			button.Y += MainReflection.currentMapHeight.Value;
			return button;
		}
		Rectangle GetBox() {
			Rectangle box = entireBox;
			box.X += Math.Min(Main.screenWidth - entireBox.Right, 0);
			box.Y += MainReflection.currentMapHeight.Value;
			return box;
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
