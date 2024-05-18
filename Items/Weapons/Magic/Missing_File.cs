using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.UI;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.Audio;
using Origins.Projectiles.Weapons;
using Terraria.DataStructures;
using Origins.Graphics;
using Origins.Items.Other.Dyes;
using Terraria.Graphics.Shaders;

namespace Origins.Items.Weapons.Magic {
	[LegacyName("Defiled_Dungeon_Chest_Placeholder_Item")]
	public class Missing_File : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"OtherMagic"
		];
		public static int ID { get; private set; }
		public override void SetStaticDefaults() => ID = Type;
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrystalVileShard);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Item.shoot = ProjectileID.None;
			Item.damage = 250;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.shootSpeed = 0;
			Item.useStyle = ItemHoldStyleID.HoldUp;
			Item.knockBack = 0;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Yellow;
			Item.mana = 18;
		}
	}
	public class Missing_File_UI : SwitchableUIState {
		public override void AddToList() => OriginSystem.Instance.ItemUseHUD.AddState(this);
		public override bool IsActive() => Main.LocalPlayer.HeldItem.type == Missing_File.ID;
		readonly List<MissingFileTarget> targets = [];
		readonly AutoLoadingAsset<Texture2D> texture = typeof(Missing_File_UI).GetDefaultTMLName();
		public static bool drawingMissingFileUI = true;
		public static Color currentNPCColor;
		public override void Draw(SpriteBatch spriteBatch) {
			static bool IsInvalidNPC(NPC npc) {
				switch (npc.type) {
					case NPCID.CultistBoss or NPCID.CultistBossClone or NPCID.CultistDevote or NPCID.CultistArcherBlue or NPCID.CultistArcherWhite:
					return !npc.active;
				}
				return !npc.CanBeChasedBy(typeof(Missing_File_UI));
			}
			if (targets.Count == 0) {
				HashSet<int> realNPCs = [];
				const int margin = 64;
				Rectangle screenArea = new(margin, margin, Main.screenWidth - margin * 2, Main.screenHeight - margin * 2);
				foreach (NPC npc in Main.ActiveNPCs) {
					if (IsInvalidNPC(npc)) continue;
					if (realNPCs.Add(npc.type)) {
						targets.Add(new(
							npc.type,
							Main.rand.NextVector2FromRectangle(screenArea),
							Main.rand.NextFloat(4),
							false
						));
					}
				}
				if (targets.Count == 0) return;
				NPC fakeTarget;
				List<NPC> npcTypes = ContentSamples.NpcsByNetId.Values.ToList();
				for (int i = 0; i < 4; i++) {
					int tries = 0;
					do {
						fakeTarget = Main.rand.Next(npcTypes);
						if (++tries > 100) break;
					} while (IsInvalidNPC(fakeTarget) || realNPCs.Contains(fakeTarget.type));
					targets.Add(new(
						fakeTarget.type,
						Main.rand.NextVector2FromRectangle(screenArea),
						Main.rand.NextFloat(4),
						true
					));
				}
			} else {
				try {
					drawingMissingFileUI = true;
					GraphicsUtils.drawingEffect = true;
					bool anyReal = false;
					Color normalColor = Color.White * 0.8f;
					Color hoverColor = Color.White;
					ArmorShaderData shader = GameShaders.Armor.GetSecondaryShader(Rasterized_Dye.ShaderID, null);
					for (int i = 0; i < targets.Count; i++) {
						MissingFileTarget target = targets[i];
						bool hovered = Main.MouseScreen.DistanceSQ(target.Position) < 32 * 32;
						if (hovered) {
							currentNPCColor = hoverColor;
						} else {
							currentNPCColor = normalColor;
						}
						target.Frame = (target.Frame + 0.1f) % 4;
						spriteBatch.Draw(
							texture,
							target.Position,
							texture.Frame(verticalFrames: 4, frameY: (int)target.Frame),
							currentNPCColor,
							0,
							new(16),
							2,
							0,
						0);

					}
					Origins.currentScreenTarget = null;
					Origins.shaderOroboros.Capture(spriteBatch);
					Main.UIScaleMatrix.Decompose(out Vector3 _scale, out Quaternion _, out Vector3 _);
					Vector2 scale = new(1 / (_scale.X * _scale.X), 1 / (_scale.Y * _scale.Y)); // why does the scale have to be squared?
					for (int i = 0; i < targets.Count; i++) {
						MissingFileTarget target = targets[i];
						NPC npc;
						if (target.Fake) {
							if (NPC.AnyNPCs(target.Type)) {
								targets.Clear();
								break;
							}
							npc = ContentSamples.NpcsByNetId[target.Type];
						} else {
							if (!NPC.AnyNPCs(target.Type)) {
								targets.RemoveAt(i--);
								break;
							}
							npc = ContentSamples.NpcsByNetId[target.Type];
							anyReal = true;
						}
						bool hovered = Main.MouseScreen.DistanceSQ(target.Position) < 32 * 32;
						if (hovered) {
							currentNPCColor = hoverColor;
							Main.instance.MouseText(npc.TypeName);
						} else {
							currentNPCColor = hoverColor;
						}
						Vector2 position = npc.Size * 0.5f - (target.Position * scale);// * scale;
						Main.instance.DrawNPCDirect(spriteBatch, npc, true, position);
						Main.instance.DrawNPCDirect(spriteBatch, npc, false, position);
						if (hovered && Main.LocalPlayer.ItemAnimationJustStarted) {
							if (target.Real) {
								Item item = Main.LocalPlayer.HeldItem;
								IEntitySource source = Main.LocalPlayer.GetSource_ItemUse(item);
								int damage = Main.LocalPlayer.GetWeaponDamage(item);
								foreach (NPC targetNPC in Main.ActiveNPCs) {
									if (targetNPC.type == target.Type) {
										SoundEngine.PlaySound(SoundID.Meowmere, targetNPC.Center);
										Projectile.NewProjectile(
											source,
											targetNPC.Center,
											default,
											ModContent.ProjectileType<Defiled_Spike_Explosion>(),
											damage,
											0,
											Main.myPlayer,
											5
										);
									}
								}
							}
							targets.Clear();
							break;
						}
					}
					shader.Shader.Parameters["uOffset"].SetValue(-Main.LocalPlayer.velocity.WithMaxLength(4) * 2);
					Origins.shaderOroboros.Stack(shader, null);
					Origins.shaderOroboros.Release();
					if (!anyReal) targets.Clear();
				} finally {
					drawingMissingFileUI = false;
					GraphicsUtils.drawingEffect = false;
					if (Origins.shaderOroboros.Capturing) Origins.shaderOroboros.Reset(default);
				}
			}
		}
		public class MissingFileTarget(int type, Vector2 position, float frame, bool fake) {
			public int Type => type;
			public Vector2 Position => position;
			public float Frame {
				get => frame;
				set => frame = value;
			}
			public bool Fake => fake;
			public bool Real => !fake;
		}
	}
}
