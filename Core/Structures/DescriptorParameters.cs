using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Origins.Core.Structures;
public class IntParameter : DescriptorParameter {
	public override ParameterElement CreateElement(Structure structure, string value) => new IntElement(int.Parse(value));
	public class IntElement(int value) : ParameterElement {
		public int Value { get; set; } = value;
		public override string GetParameterValue() => Value.ToString();
		public override void Draw(SpriteBatch spriteBatch, Rectangle dimensions) {
			throw new System.NotImplementedException();
		}
	}
}
public class FloatParameter : DescriptorParameter {
	public override ParameterElement CreateElement(Structure structure, string value) => new FloatElement(float.Parse(value));
	public class FloatElement(float value) : ParameterElement {
		public float Value { get; set; } = value;
		public override string GetParameterValue() => Value.ToString();
		public override void Draw(SpriteBatch spriteBatch, Rectangle dimensions) {
			throw new System.NotImplementedException();
		}
	}
}
public class RoomParameter : DescriptorParameter {
	public override ParameterElement CreateElement(Structure structure, string value) => new RoomElement(structure, structure.Rooms.FirstOrDefault(r => r.Identifier == value));
	public class RoomElement(Structure structure, ARoom room) : ParameterElement {
		public ARoom Room { get; set; } = room;
		public override string GetParameterValue() => Room.Identifier;
		public override void Draw(SpriteBatch spriteBatch, Rectangle dimensions) {
			throw new System.NotImplementedException();
		}
	}
}
