using System.Collections.Generic;

namespace RBush
{
	public interface ISpatialIndex<out T>
	{
		IEnumerable<T> Search();
		IEnumerable<T> Search(in Envelope boundingBox);
	}
}