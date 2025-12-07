namespace Origins.Core.Structures;
public class MaxRoomCount : BreakDescriptor.Kind {
	protected override Accumulator<StructureInstance, bool> Create(string[] parameters) => Create(int.Parse(parameters[0]));
	public static Accumulator<StructureInstance, bool> Create(int maxRooms) => (StructureInstance instance, ref bool output) => 
	output |= instance.rooms.Length > maxRooms;
}