using System.Collections;
using System.Collections.Generic;

#if UNITY
using UnityEngine;
#elif STEREOKIT
using StereoKit;
#endif

using Newtonsoft.Json;

using Instrumental.Schema;

namespace Instrumental.Test
{
	public interface IParentType
	{
		sV3 GetPosition();
		void SetPosition(sV3 position);
		sQuat GetRotation();
		void SetRotation(sQuat rotation);
	}

	public interface ISubTypeA : IParentType
	{
		float GetThreshold();
		void SetThreshold(float threshold);

		sV2 GetDimensions();
		void SetDimensions(sV2 dimensions);
	}

	public interface ISubTypeB : IParentType
	{
		sColor GetColor();
		void SetColor(sColor color);
	}

	// might not need a struct for the parent type - 
	// If it's intended to be fully abstract, then
	// we won't need it, since it can't be implemented, ever.
	//[System.Serializable]
	//public struct ParentType : IParentType
	//{
	//	public sV3 Position;
	//	public sQuat Rotation;

	//	public sV3 GetPosition()
	//	{
	//		return this.Position;
	//	}

	//	public void SetPosition(sV3 position)
	//	{
	//		this.Position = position;
	//	}

	//	public sQuat GetRotation()
	//	{
	//		return this.Rotation;
	//	}

	//	public void SetRotation(sQuat rotation)
	//	{
	//		this.Rotation = rotation;
	//	}
	//}

	[System.Serializable]
	public struct SubTypeA : ISubTypeA
	{
		public sV3 Position;
		public sQuat Rotation;

		public float Threshold;
		public sV2 Dimensions;

		public SubTypeA(sV3 position, sQuat rotation, float threshold, sV2 dimensions)
		{
			this.Position = position;
			this.Rotation = rotation;
			this.Threshold = threshold;
			this.Dimensions = dimensions;
		}

		public sV3 GetPosition()
		{
			return this.Position;
		}

		public void SetPosition(sV3 position)
		{
			this.Position = position;
		}

		public sQuat GetRotation()
		{
			return this.Rotation;
		}

		public void SetRotation(sQuat rotation)
		{
			this.Rotation = rotation;
		}

		public float GetThreshold()
		{
			return Threshold;
		}

		public void SetThreshold(float threshold)
		{
			this.Threshold = threshold;
		}

		public sV2 GetDimensions()
		{
			return Dimensions;
		}

		public void SetDimensions(sV2 dimensions)
		{
			this.Dimensions = dimensions;
		}
	}

	[System.Serializable]
	public struct SubTypeB : ISubTypeB
	{
		public sV3 Position;
		public sQuat Rotation;

		public float Threshold;
		public sV2 Dimensions;

		public sColor Color;

		public SubTypeB(sV3 position, sQuat rotation, float threshold, sV2 dimensions,
			sColor color)
		{
			this.Position = position;
			this.Rotation = rotation;
			this.Threshold = threshold;
			this.Dimensions = dimensions;
			this.Color = color;
		}

		public sV3 GetPosition()
		{
			return this.Position;
		}

		public void SetPosition(sV3 position)
		{
			this.Position = position;
		}

		public sQuat GetRotation()
		{
			return this.Rotation;
		}

		public void SetRotation(sQuat rotation)
		{
			this.Rotation = rotation;
		}

		public float GetThreshold()
		{
			return Threshold;
		}

		public void SetThreshold(float threshold)
		{
			this.Threshold = threshold;
		}

		public sV2 GetDimensions()
		{
			return Dimensions;
		}

		public void SetDimensions(sV2 dimensions)
		{
			this.Dimensions = dimensions;
		}

		public sColor GetColor()
		{
			return this.Color;
		}

		public void SetColor(sColor color)
		{
			this.Color = color;
		}
	}

	[System.Serializable]
	public struct TypeContainer
	{
		public IParentType[] items;
	}

#if UNITY
	public class PolyInterfaceSerializationTest : MonoBehaviour
	{
		[SerializeField] bool generateData;
		[SerializeField] bool doSerializationTest;
		[SerializeField] bool doDeserializationTest;
		[SerializeField] string openFilePath;
		[SerializeField] string saveFilePath;

		TypeContainer container;

		void GenerateData()
		{
			container = new TypeContainer();

			SubTypeA itemA = new SubTypeA(
				new sV3(1, 1.5f, 2),
				(sQuat)Quaternion.identity,
				0.7f, new sV2(1.9f, 0.9f));

			SubTypeB itemB = new SubTypeB(
				new sV3(0.65f, 1f, 1.75f),
				(sQuat)(Quaternion.LookRotation(Vector3.right, Vector3.forward)),
				0.5f, new sV2(1.3f, 1.1f),
				new sColor(1, 1, 1, 0.5f));

			container.items = new IParentType[2];
			container.items[0] = itemA;
			container.items[1] = itemB;
		}

		void DoSerializationTest()
		{
			// maybe generate data again? just to be sure
			//GenerateData();

			// save serialized container to file in path
			string jsonString = JsonConvert.SerializeObject(container, Formatting.Indented);

			// try saving to json


			// do we try saving as scriptable object as well as json?
		}

		void DoDeserializationTest()
		{

		}

		private void OnDrawGizmos()
		{
			if(generateData)
			{
				generateData = false;
				GenerateData();
			}

			if(doSerializationTest)
			{
				doSerializationTest = false;
				DoSerializationTest();
			}

			if(doDeserializationTest)
			{
				doDeserializationTest = false;
				DoDeserializationTest();
			}
		}
	}
#endif
}