using System.Collections;
using System.Collections.Generic;

using Instrumental.Core.Math;

namespace Instrumental.Core
{
	public struct PoseIC /*: System.IEquatable<PoseIC>*/
	{
		public Vect3 position;
		public Quatn rotation;

		public PoseIC(Vect3 position, Quatn rotation)
		{
			this.position = position;
			this.rotation = rotation;
		}

		public static readonly PoseIC identity = new PoseIC(Vect3.zero, Quatn.identity);

		public Vect3 forward
		{
			get
			{
				return rotation * Vect3.forward;
			}
		}

		public Vect3 right
		{
			get
			{
				return rotation * Vect3.right;
			}
		}

		public Vect3 up
		{
			get
			{
				return rotation * Vect3.up;
			}
		}

		//public bool Equals(PoseIC other)
		//{
		//	return position == other.position && rotation == other.rotation;
		//}
	}
}