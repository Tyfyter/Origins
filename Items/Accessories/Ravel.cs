using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Dusts;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Ravel : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Movement"
		};
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			ID = Type;
            glowmask = Origins.AddGlowMask(this);
        }
        static short glowmask;
        public override void SetDefaults() {
			Item.DefaultToAccessory();
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 5);
			Item.shoot = ModContent.MountType<Ravel_Mount>();//can't use mountType because that'd make it fit in the mount slot
			Item.hasVanityEffects = true;
            Item.glowMask = glowmask;
        }
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.ravelEquipped = true;
			originPlayer.vanityRavel = Type;
			const int down = 0;
			bool toggle = player.controlDown && player.releaseDown && player.doubleTapCardinalTimer[down] < 15;
			bool inOtherRavel = false;
			if (player.mount.Type == Item.shoot) {
				UpdateRaveled(player);
			} else {
				inOtherRavel = Ravel_Mount.RavelMounts.Contains(player.mount.Type);
			}
			if (toggle || inOtherRavel) ToggleRavel(player);
		}
		public void ToggleRavel(Player player) {
			bool animated = OriginClientConfig.Instance.AnimatedRavel;
			if (player.mount.Type == Item.shoot) {
				if (animated) {
					player.mount._idleTime = -Ravel_Mount.transformAnimationFrames;
					player.mount._idleTimeNext = 0;
				} else {
					for (int i = 0; i < 40; i++) {
						Dust.NewDustDirect(player.position, player.width, player.height, DustID.AncientLight).velocity *= 0.4f;
					}
					player.mount.Dismount(player);
				}
			} else {
				player.mount.SetMount(Item.shoot, player);
				player.mount._idleTime = animated ? 0 : Ravel_Mount.transformAnimationFrames;
				if (!animated) {
					for (int i = 0; i < 40; i++) {
						Dust.NewDustDirect(player.position, player.width, player.height, DustID.AncientLight).velocity *= 0.4f;
					}
				}
			}
		}
		public override void UpdateVanity(Player player) {
			player.GetModPlayer<OriginPlayer>().vanityRavel = Type;
		}
		protected virtual void UpdateRaveled(Player player) {
			player.blackBelt = true;
		}
		public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) {
			return incomingItem.ModItem is not Ravel || equippedItem.ModItem is not Ravel;
		}
		public override bool CanEquipAccessory(Player player, int slot, bool modded) {
			if (slot > Player.InitialAccSlotCount + player.extraAccessorySlots + 3) {
				return true;
			}
			for (int i = 0; i < Player.InitialAccSlotCount + player.extraAccessorySlots; i++) {
				if (i + 3 == slot) continue;
				if (player.armor[i + 3].ModItem is Ravel) {
					return false;
				}
			}
			return true;
		}
	}
	public class Ravel_Mount : ModMount {
		public const int transformAnimationFrames = 3;
		public const float transformCounterSpeed = 1.75f;
		public const float transformCounterMax = 4f;
		public override string Texture => "Origins/Items/Accessories/Ravel";
		public static int ID { get; private set; } = -1;
		public static AutoCastingAsset<Texture2D> TransformTexture { get; private set; }
		protected virtual void SetID() {
			MountData.buff = ModContent.BuffType<Ravel_Mount_Buff>();
			ID = Type;
		}
		protected internal static HashSet<int> RavelMounts { get; private set; }
		public override void Unload() {
			RavelMounts = null;
			TransformTexture = null;
		}
		public override void SetStaticDefaults() {
			// Movement
			MountData.jumpHeight = 8; // How high the mount can jump.
			MountData.acceleration = 0.19f; // The rate at which the mount speeds up.
			MountData.jumpSpeed = 8f; // The rate at which the player and mount ascend towards (negative y velocity) the jump height when the jump button is pressed.
			MountData.blockExtraJumps = true; // Determines whether or not you can use a double jump (like cloud in a bottle) while in the mount.
			MountData.constantJump = false; // Allows you to hold the jump button down.
			MountData.fallDamage = 0.5f; // Fall damage multiplier.
			MountData.runSpeed = 8f; // The speed of the mount
			MountData.dashSpeed = 8f; // The speed the mount moves when in the state of dashing.
			MountData.flightTimeMax = 0; // The amount of time in frames a mount can be in the state of flying.
			MountData.heightBoost = -22;
			MountData.spawnDust = No_Dust.ID;

			// Misc
			MountData.fatigueMax = 0;
			//MountData.buff = Ravel_Mount_Buff.ID; // The ID number of the buff assigned to the mount.

			MountData.totalFrames = 1; // Amount of animation frames for the mount
			MountData.playerYOffsets = new int[] { -22 };
			(RavelMounts ??= new()).Add(Type);
			if (!Main.dedServ && TransformTexture.Value is null) {
				TransformTexture = Mod.Assets.Request<Texture2D>("Items/Accessories/Ravel_Morph");
			}
			SetID();
		}
		public override void UpdateEffects(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.mountOnly = true;
			originPlayer.ravel = true;
		}
		public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {
			OriginPlayer originPlayer = mountedPlayer.GetModPlayer<OriginPlayer>();
			const float factor = 10f / 12f;
			if (originPlayer.ceilingRavel) {
				mountedPlayer.mount._frameCounter -= velocity.X * factor;
			} else {
				mountedPlayer.mount._frameCounter += velocity.X * factor;
				if (originPlayer.collidingX) {
					mountedPlayer.mount._frameCounter -= velocity.Y * originPlayer.oldXSign * factor;
				}
			}
			if (mountedPlayer.mount._idleTime < transformAnimationFrames && (mountedPlayer.mount._frameExtraCounter += transformCounterSpeed) >= transformCounterMax) {
				mountedPlayer.mount._frameExtraCounter -= transformCounterMax;
				if (mountedPlayer.mount._idleTime == 0 && mountedPlayer.mount._idleTimeNext == 0) {
					mountedPlayer.mount.Dismount(mountedPlayer);
				}
				mountedPlayer.mount._idleTime += 1;
			}
			return false;
		}
		public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow) {
			//playerDrawData.Clear();
			DrawData item;
			int transformFrame = Math.Abs(drawPlayer.mount._idleTime);
			if (transformFrame < transformAnimationFrames) {
				texture = TransformTexture;
				drawOrigin = new Vector2(16, 34);
				frame = texture.Frame(1, 3, 0, transformFrame);
				item = new DrawData(texture, drawPosition, frame, Color.Lerp(drawColor, new Color(225, 225, 225, 225), 0.75f), rotation, drawOrigin, drawScale, 0, 0);
				playerDrawData.Add(item);
				return false;
			}
			rotation = drawPlayer.mount._frameCounter * 0.1f;
			int vanityRavel = drawPlayer.GetModPlayer<OriginPlayer>().vanityRavel;
			if (vanityRavel < 0) return false;
			Main.instance.LoadItem(vanityRavel);
			texture = Terraria.GameContent.TextureAssets.Item[vanityRavel].Value;
			drawOrigin = new Vector2(12, 12);
			item = new DrawData(texture, drawPosition, null, drawColor, rotation, drawOrigin, drawScale, 0, 0);
			item.shader = Mount.currentShader;
			playerDrawData.Add(item);
			return false;
		}
	}
	public class Ravel_Mount_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/Ravel_Generic_Buff";
		protected virtual int MountID => ModContent.MountType<Ravel_Mount>();
		public override void SetStaticDefaults() {

			BuffID.Sets.BasicMountData[Type] = new BuffID.Sets.BuffMountData() {
				mountID = MountID
			};
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.changeSize = true;
			originPlayer.targetWidth = 20;
			originPlayer.targetHeight = 20;
		}
	}
}
