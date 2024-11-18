using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Tiles.Other;
using PegasusLib.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Chat;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Origins.UI.Event {
	public class Laser_Tag_Hud : SwitchableUIState {
		public AutoLoadingAsset<Texture2D> texture = typeof(Laser_Tag_Hud).GetDefaultTMLName();
		public override void AddToList() => OriginSystem.Instance.SetBonusHUD.AddState(this);
		public override bool IsActive() {
			if (OriginPlayer.LocalOriginPlayer is null) return false;
			if (OriginPlayer.LocalOriginPlayer.laserTagVest) {
				return true;
			}
			return false;
		}
		public Laser_Tag_Hud() : base() {
			OverrideSamplerState = SamplerState.PointClamp;
		}
		Laser_Tag_Summary_Panel summaryPanel;
		UIHideWrapper teamsListWrapper;
		UI_Hidden_Supporting_List teamsList;
		Laser_Tag_Player_List playersList;
		IHideableElement ActiveScoreList => Laser_Tag_Console.LaserTagRules.Teams ? teamsListWrapper : playersList;
		public override void OnInitialize() {
			UIElement topPanelWrapper = new() {
				HAlign = 0.5f,
				Width = new(250, 0.1f),
				Height = new(0, 1)
			};
			Append(topPanelWrapper);

			//summary
			summaryPanel = new() {
				Width = new(0, 1),
				Height = new(32, 0),
			};
			topPanelWrapper.Append(summaryPanel);

			//teams
			teamsList = [
				new Laser_Tag_Team_UIElement(1),
				new Laser_Tag_Team_UIElement(2),
				new Laser_Tag_Team_UIElement(3),
				new Laser_Tag_Team_UIElement(4),
				new Laser_Tag_Team_UIElement(5)
			];
			teamsList.Width.Set(0, 1);
			teamsList.Height.Set(0, 1);
			teamsList.ListPadding = 0;
			teamsList.OnUpdate += el => {
				if (el is UIList list) list.Height.Set(list.GetTotalHeight(), 0);
			};
			teamsListWrapper = new() {
				HAlign = 0.5f,
				Width = new(0, 1),
				Height = new(0, 1),
			};
			teamsListWrapper.Append(teamsList);
			topPanelWrapper.Append(teamsListWrapper);

			topPanelWrapper.Append(playersList = new Laser_Tag_Player_List {
				Width = new(0, 1),
				Height = new(0, 1),
			});

			Append(new Laser_Tag_Health_Pips());
		}
		public override void Update(GameTime gameTime) {
			if (PlayerInput.Triggers.JustPressed.Down) {
				RemoveAllChildren();
				OnInitialize();
			}
			if (summaryPanel.IsMouseHovering || teamsList.IsMouseHovering || playersList.ContainsPoint(Main.MouseScreen)) {
				summaryPanel.Hidden = true;
				if (ActiveScoreList.Hidden) {
					if (Laser_Tag_Console.LaserTagRules.Teams) {
						teamsList.UpdateOrder();
						teamsList.Recalculate();
					}
				}
				ActiveScoreList.Hidden = false;
			} else {
				summaryPanel.Hidden = false;
				teamsListWrapper.Hidden = true;
				playersList.Hidden = true;
			}
			base.Update(gameTime);
		}
	}
	public class UIHideWrapper : UIElement, IHideableElement {
		public bool isHidden = false;
		public bool Hidden { get => isHidden; set => isHidden = value; }
		public override bool ContainsPoint(Vector2 point) => !isHidden && base.ContainsPoint(point);
		public override void Draw(SpriteBatch spriteBatch) {
			if (!isHidden) base.Draw(spriteBatch);
		}
	}
	public class Laser_Tag_Summary_Panel : UI_Hideable_Panel {
		public override void OnInitialize() {
			base.OnInitialize();
			this.BackgroundColor = new(67, 67, 80);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			Rectangle scissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
			SpriteBatchState state = spriteBatch.GetState();
			spriteBatch.Restart(state, rasterizerState: new RasterizerState {
				CullMode = CullMode.None,
				ScissorTestEnable = true
			});
			spriteBatch.GraphicsDevice.ScissorRectangle = GetDimensions().ToRectangle().Scaled(Main.UIScale);
			try {
				Vector2 textPos = GetInnerDimensions().Center();
				TextSnippet[] text = ChatManager.ParseMessage(Laser_Tag_Console.LaserTagRules.GetGameSummaryText(GetInnerDimensions().Width), Color.White).ToArray();
				Vector2 textSize = default;
				Vector2 sizeMult = new(1, 1f / text.Length);
				for (int i = 0; i < text.Length; i++) {
					if (!text[i].UniqueDraw(true, out Vector2 snippetSize, spriteBatch)) {
						snippetSize = FontAssets.MouseText.Value.MeasureString(text[i].Text);
					}
					textSize += snippetSize * sizeMult;
				}
				textPos.X -= textSize.X * 0.5f;
				textPos.Y -= textSize.Y * 0.2f;
				ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					FontAssets.MouseText.Value,
					text,
					textPos,
					0,
					Color.White,
					Color.Black * 0.75f,
					textSize * Vector2.UnitY * 0.2f,
					Vector2.One,
					out _
				);
			} finally {
				spriteBatch.End();
				spriteBatch.GraphicsDevice.RasterizerState = state.rasterizerState;
				spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
				spriteBatch.Begin(state);
			}
		}
	}
	public class Laser_Tag_Team_UIElement : UI_Hideable_Panel {
		public readonly int team;
		public Laser_Tag_Team_UIElement(int team) {
			Width.Set(0, 1);
			Height.Set(32, 0);
			BackgroundColor = Main.teamColor[team];
			this.team = team;
		}
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			this.Hidden = Laser_Tag_Console.LaserTagTeamPlayers[team] <= 0;
			if (this.IsMouseHovering) Main.LocalPlayer.mouseInterface = true;
		}
		public override int CompareTo(object obj) {
			if (obj is not Laser_Tag_Team_UIElement other) return 0;
			if (Laser_Tag_Console.LaserTagRules.IsElimination) return -Comparer<int>.Default.Compare(Laser_Tag_Console.LaserTagTeamPlayers[team], Laser_Tag_Console.LaserTagTeamPlayers[other.team]);
			return -Comparer<int>.Default.Compare(Laser_Tag_Console.LaserTagTeamPoints[team], Laser_Tag_Console.LaserTagTeamPoints[other.team]);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			Rectangle scissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
			SpriteBatchState state = spriteBatch.GetState();
			spriteBatch.Restart(state, rasterizerState: new RasterizerState {
				CullMode = CullMode.None,
				ScissorTestEnable = true
			});
			spriteBatch.GraphicsDevice.ScissorRectangle = GetDimensions().ToRectangle().Scaled(Main.UIScale);
			try {
				Rectangle bounds = GetInnerDimensions().ToRectangle();
				Vector2 pointsPos = bounds.Right();
				string points = Language.GetTextValue("Mods.Origins.Laser_Tag.Hud_Points", Laser_Tag_Console.LaserTagTeamPoints[team]);
				Vector2 pointsSize = FontAssets.MouseText.Value.MeasureString(points);
				pointsPos.X -= pointsSize.X;
				ChatManager.DrawColorCodedString(
					spriteBatch,
					FontAssets.MouseText.Value,
					points,
					pointsPos,
					Color.Black,
					0,
					pointsSize * Vector2.UnitY * 0.4f,
					Vector2.One
				);
				if (Laser_Tag_Console.LaserTagRules.IsElimination) {
					Vector2 headPos = bounds.TopLeft();
					foreach (Player player in Main.ActivePlayers) {
						if (player.team == team && player.OriginPlayer().laserTagVestActive) {
							int direction = player.direction;
							Rectangle headFrame = player.headFrame;
							Rectangle hairFrame = player.hairFrame;
							Rectangle bodyFrame = player.bodyFrame;
							Rectangle legFrame = player.legFrame;
							try {
								player.direction = 1;
								player.headFrame.Y = 0;
								player.hairFrame.Y = 0;
								player.bodyFrame.Y = 0;
								player.legFrame.Y = 0;
								Main.PlayerRenderer.DrawPlayerHead(
									Main.Camera,
									player,
									headPos + new Vector2(8, -2),
									scale: 0.85f
								);
							} finally {
								player.direction = direction;
								player.headFrame = headFrame;
								player.hairFrame = hairFrame;
								player.bodyFrame = bodyFrame;
								player.legFrame = legFrame;
							}
							headPos.X += 30;
							if (headPos.X > pointsPos.X - 30) break;
						}
					}
				}
			} finally {
				spriteBatch.End();
				spriteBatch.GraphicsDevice.RasterizerState = state.rasterizerState;
				spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
				spriteBatch.Begin(state);
			}
		}
	}
	public class Laser_Tag_Player_List : UI_Hidden_Supporting_List, IHideableElement {
		UIList list;
		public bool Hidden { get; set; }
		public override void OnInitialize() {
			list = [];
			list.Width.Set(0, 1);
			list.Height.Set(0, 1);
			Append(list);
		}
		public override void Update(GameTime gameTime) {
			if (Hidden) return;
			base.Update(gameTime);
			list.Clear();
			List<(int points, Player player)> players = [];
			foreach (Player player in Main.ActivePlayers) {
				OriginPlayer originPlayer = player.OriginPlayer();
				if (originPlayer.laserTagVest) players.Add((originPlayer.laserTagPoints, player));
			}
			players = players.OrderBy(v => v.points).ToList();
			for (int i = 0; i < players.Count; i++) {
				list.Add(new Laser_Tag_Player_UIElement(players[i].player));
			}
			list.RecalculateChildren();
			list.Height.Set(list.GetTotalHeight(), 0);
			RecalculateChildren();
		}
		public override bool ContainsPoint(Vector2 point) => !Hidden && (list?.ContainsPoint(point) ?? false);
		public override void Draw(SpriteBatch spriteBatch) {
			if (Hidden) return;
			base.Draw(spriteBatch);
		}
	}
	public class Laser_Tag_Player_UIElement : UI_Hideable_Panel {
		public readonly Player player;
		public readonly OriginPlayer originPlayer;
		public Laser_Tag_Player_UIElement(Player player) {
			Width.Set(0, 1);
			Height.Set(32, 0);
			this.player = player;
			this.originPlayer = this.player.OriginPlayer();
			BackgroundColor = !Laser_Tag_Console.LaserTagRules.IsElimination || originPlayer.laserTagVestActive ? new Color(63, 82, 151) * 0.7f : new Color(70, 79, 101) * 0.7f;
		}
		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			if (this.IsMouseHovering) Main.LocalPlayer.mouseInterface = true;
		}
		public override int CompareTo(object obj) {
			if (obj is not Laser_Tag_Player_UIElement other) return 0;
			if (Laser_Tag_Console.LaserTagRules.IsElimination && (originPlayer.laserTagVestActive != other.originPlayer.laserTagVestActive)) {
				return other.originPlayer.laserTagVestActive.ToInt() - originPlayer.laserTagVestActive.ToInt();
			}
			return -Comparer<int>.Default.Compare(originPlayer.laserTagPoints, other.originPlayer.laserTagPoints);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			base.DrawSelf(spriteBatch);
			Rectangle scissorRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
			SpriteBatchState state = spriteBatch.GetState();
			spriteBatch.Restart(state, rasterizerState: new RasterizerState {
				CullMode = CullMode.None,
				ScissorTestEnable = true
			});
			spriteBatch.GraphicsDevice.ScissorRectangle = GetDimensions().ToRectangle().Scaled(Main.UIScale);
			try {
				Rectangle bounds = GetInnerDimensions().ToRectangle();
				Vector2 pointsPos = bounds.Right();
				string points = Language.GetTextValue("Mods.Origins.Laser_Tag.Hud_Points", originPlayer.laserTagPoints);
				Vector2 pointsSize = FontAssets.MouseText.Value.MeasureString(points);
				pointsPos.X -= pointsSize.X;
				ChatManager.DrawColorCodedString(
					spriteBatch,
					FontAssets.MouseText.Value,
					points,
					pointsPos,
					Color.Black,
					0,
					pointsSize * Vector2.UnitY * 0.4f,
					Vector2.One
				);
				Vector2 namePos = GetInnerDimensions().ToRectangle().Left();
				TextSnippet[] text = ChatManager.ParseMessage($"[head:{player.whoAmI}] {player.name}", Color.White).ToArray();
				Vector2 textSize = default;
				Vector2 sizeMult = new(1, 1f / text.Length);
				for (int i = 0; i < text.Length; i++) {
					if (!text[i].UniqueDraw(true, out Vector2 snippetSize, spriteBatch)) {
						snippetSize = FontAssets.MouseText.Value.MeasureString(text[i].Text);
					}
					textSize += snippetSize * sizeMult;
				}
				namePos.Y -= textSize.Y * 0.2f;
				ChatManager.DrawColorCodedStringWithShadow(
					spriteBatch,
					FontAssets.MouseText.Value,
					text,
					namePos,
					0,
					Color.White,
					Color.Black * 0.75f,
					textSize * Vector2.UnitY * 0.2f,
					Vector2.One,
					out _
				);
			} finally {
				spriteBatch.End();
				spriteBatch.GraphicsDevice.RasterizerState = state.rasterizerState;
				spriteBatch.GraphicsDevice.ScissorRectangle = scissorRectangle;
				spriteBatch.Begin(state);
			}
		}
	}
	public class Laser_Tag_Health_Pips : UIElement {
		public override void Draw(SpriteBatch spriteBatch) {
			if (Laser_Tag_Console.LaserTagRules.HP <= 1) return;
			SpriteBatchState state = spriteBatch.GetState();
			Matrix matrix = Main.GameViewMatrix.ZoomMatrix;
			spriteBatch.Restart(state, transformMatrix: matrix);
			try {
				Player player = Main.LocalPlayer;
				OriginPlayer originPlayer = player.OriginPlayer();
				Color color = Main.teamColor[player.team];
				color.A = (byte)(color.A * 0.7f);
				Color hurtColor = Color.DarkSlateGray;
				hurtColor.A = (byte)(hurtColor.A * 0.7f);
				Vector2 pos;
				Vector2 move;
				switch (LaserTagConfig.Instance.HealthPipPlacement) {
					default:
					case Laser_Tag_Health_Pip_Placement.Bottom:
					pos = player.Bottom + new Vector2(0, LaserTagConfig.Instance.HealthPipOffset);
					move = new(player.width / (float)Laser_Tag_Console.LaserTagRules.HP, 0);
					break;

					case Laser_Tag_Health_Pip_Placement.Left:
					pos = player.Left - new Vector2(LaserTagConfig.Instance.HealthPipOffset, 0);
					move = new(0, -player.height / (float)Laser_Tag_Console.LaserTagRules.HP);
					break;

					case Laser_Tag_Health_Pip_Placement.Right:
					pos = player.Right + new Vector2(LaserTagConfig.Instance.HealthPipOffset, 0);
					move = new(0, -player.height / (float)Laser_Tag_Console.LaserTagRules.HP);
					break;

					case Laser_Tag_Health_Pip_Placement.Top:
					pos = player.Top - new Vector2(0, LaserTagConfig.Instance.HealthPipOffset);
					move = new(player.width / (float)Laser_Tag_Console.LaserTagRules.HP, 0);
					break;

					case Laser_Tag_Health_Pip_Placement.Back:
					if (player.direction == 1) goto case Laser_Tag_Health_Pip_Placement.Left;
					goto case Laser_Tag_Health_Pip_Placement.Right;

					case Laser_Tag_Health_Pip_Placement.Front:
					if (player.direction == -1) goto case Laser_Tag_Health_Pip_Placement.Left;
					goto case Laser_Tag_Health_Pip_Placement.Right;
				}
				if (LaserTagConfig.Instance.HealthPipDirectionInverted) move = -move;
				pos.Y += player.gfxOffY;
				pos = (pos.Floor() - move * (Laser_Tag_Console.LaserTagRules.HP - 1) * 0.5f - Main.screenPosition);
				Rectangle frame = new(0, 0, 2, 2);
				for (int i = 0; i < Laser_Tag_Console.LaserTagRules.HP; i++) {
					spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos, frame, i < originPlayer.laserTagHP ? color : hurtColor, 0, Vector2.One, 1f, SpriteEffects.None, 0);
					pos += move;
				}
			} finally {
				spriteBatch.Restart(state);
			}
		}
	}
	public enum Laser_Tag_Health_Pip_Placement : int {
		Bottom,
		Left,
		Right,
		Top,
		Back,
		Front
	}
}
