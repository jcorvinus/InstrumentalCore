using System.Collections;
using System.Collections.Generic;

using Instrumental.Core.Math;

namespace Instrumental.Core
{
	public struct PoseIC /*: System.IEquatable<PoseIC>*/
	{
		public Vect3 Position;
		public Quatn Rotation;

		public PoseIC(Vect3 position, Quatn rotation)
		{
			Position = position;
			Rotation = rotation;
		}

		public static readonly PoseIC Identity = new PoseIC(Vect3.zero, Quatn.identity);

		public Vect3 forward
		{
			get
			{
				return Rotation * Vect3.forward;
			}
		}

		public Vect3 right
		{
			get
			{
				return Rotation * Vect3.right;
			}
		}

		public Vect3 up
		{
			get
			{
				return Rotation * Vect3.up;
			}
		}

		//public bool Equals(PoseIC other)
		//{
		//	return Position == other.Position && Rotation == other.Rotation;
		//}
	}
}