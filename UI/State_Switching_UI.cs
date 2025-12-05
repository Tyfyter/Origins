using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PegasusLib;
using PegasusLib.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.UI {
	public abstract class AStateSwtichingUI() : UIState() {
		public abstract void AddState(SwitchableUIState state);
		public abstract void AddStates(params SwitchableUIState[] states);
		public abstract bool IsActive { get; }
		public abstract InterfaceScaleType ScaleType { get; }
	}
	public class State_Switching_UI(params SwitchableUIState[] states) : AStateSwtichingUI() {
		readonly List<SwitchableUIState> states = states.ToList();
		public SwitchableUIState CurrentState { get; private set; }
		public override bool IsActive => CurrentState is not null;
		public override InterfaceScaleType ScaleType => CurrentState.ScaleType;
		public override void AddState(SwitchableUIState state) {
			states.Add(state);
		}
		public override void AddStates(params SwitchableUIState[] states) {
			this.states.AddRange(states);
		}
		public override void Update(GameTime gameTime) {
			CurrentState = null;
			for (int i = 0; i < states.Count; i++) {
				if (!Children.Contains(states[i])) Append(states[i]);
				if (states[i].IsActive()) {
					CurrentState = states[i];
					states[i].Activate();
				} else {
					states[i].Deactivate();
				}
			}
			if (CurrentState is null) return;
			CurrentState.Update(gameTime);
		}
		public override void Draw(SpriteBatch spriteBatch) {
			if (CurrentState is null) return;
			CurrentState.Draw(spriteBatch);
		}
	}
	public class Multistate_Switching_UI(params SwitchableUIState[] states) : AStateSwtichingUI() {
		readonly List<SwitchableUIState> states = states.ToList();
		bool isActive;
		bool optimized;
		public override bool IsActive => isActive;
		public override InterfaceScaleType ScaleType => InterfaceScaleType.UI;
		public override void AddState(SwitchableUIState state) {
			states.Add(state);
		}
		public override void AddStates(params SwitchableUIState[] states) {
			this.states.AddRange(states);
		}
		public override void Update(GameTime gameTime) {
			isActive = false;
			if (optimized.TrySet(true)) OptimizeOrder();
			for (int i = 0; i < states.Count; i++) {
				if (!Children.Contains(states[i])) Append(states[i]);
				if (states[i].IsActive()) {
					isActive = true;
					states[i].Activate();
					states[i].Update(gameTime);
				} else {
					states[i].Deactivate();
				}
			}
		}
		public void OptimizeOrder() {
			states.Sort((a, b) => (a.ScaleType ^ InterfaceScaleType.UI).CompareTo(b.ScaleType ^ InterfaceScaleType.UI));
		}
		public override void Draw(SpriteBatch spriteBatch) {
			SpriteBatchState state = spriteBatch.GetState();
			for (int i = 0; i < states.Count; i++) {
				if (states[i].IsActive()) {
					Matrix transformMatrix;
					switch (states[i].ScaleType) {
						case InterfaceScaleType.Game:
						transformMatrix = Main.GameViewMatrix.ZoomMatrix;
						break;
						case InterfaceScaleType.UI:
						transformMatrix = Main.UIScaleMatrix;
						break;
						default:
						transformMatrix = Matrix.Identity;
						break;
					}
					if (state.transformMatrix != transformMatrix) {
						spriteBatch.Restart(state, transformMatrix: transformMatrix);
						switch (states[i].ScaleType) {
							case InterfaceScaleType.Game:
							PlayerInput.SetZoom_World();
							break;
							case InterfaceScaleType.UI:
							PlayerInput.SetZoom_UI();
							break;
							default:
							PlayerInput.SetZoom_Unscaled();
							break;
						}
					}
					states[i].Draw(spriteBatch);
				}
			}
		}
	}
	public abstract class SwitchableUIState : UIState, ILoadable {
		public void Load(Mod mod) {
			if (OriginSystem.queuedUIStates is null) {
				AddToList();
			} else {
				OriginSystem.queuedUIStates.Add(this);
			}
		}
		public void Unload() { }
		public abstract void AddToList();
		public abstract bool IsActive();
		protected bool isActive = false;
		public sealed override void OnActivate() {
			if (isActive.TrySet(true)) OnEnter();
		}
		public sealed override void OnDeactivate() {
			if (isActive.TrySet(false)) OnExit();
		}
		public virtual InterfaceScaleType ScaleType => InterfaceScaleType.UI;
		public virtual void OnEnter() { }
		public virtual void OnExit() { }
		public override bool ContainsPoint(Vector2 point) => isActive && base.ContainsPoint(point);
		public override void Update(GameTime gameTime) {
			if (isActive) base.Update(gameTime);
		}
		public override void Draw(SpriteBatch spriteBatch) {
			if (isActive) base.Draw(spriteBatch);
		}
	}
	public class StateSwitchingInterface : UserInterface {
		readonly LegacyGameInterfaceLayer layer;
		readonly AStateSwtichingUI state;
		readonly string after;
		public StateSwitchingInterface(string name, bool multi = false, string after = "Vanilla: Inventory") : base() {
			layer = new LegacyGameInterfaceLayer(
				name,
				delegate {
					Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
					return true;
				}
			);
			SetState(state = multi ? new Multistate_Switching_UI() : new State_Switching_UI());
			this.after = after;
		}
		public void AddState(SwitchableUIState state) {
			this.state.AddState(state);
		}
		public void AddStates(params SwitchableUIState[] states) {
			state.AddStates(states);
		}
		public void Insert(List<GameInterfaceLayer> layers) {
			if (!state.IsActive) return;
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals(after));
			if (inventoryIndex != -1) {//error prevention & null check
				layer.ScaleType = state.ScaleType;
				layers.Insert(inventoryIndex + 1, layer);
			}
		}
	}
}