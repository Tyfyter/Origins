using System.Numerics;

namespace Origins.Core {
	public interface ISummable<TSelf> : IAdditionOperators<TSelf, TSelf, TSelf> where TSelf : ISummable<TSelf> { }
}
