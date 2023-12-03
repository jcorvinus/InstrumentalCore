using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Instrumental.Overlay
{
    public class InputDataSources : MonoBehaviour
    {
		public class LiveSource
		{
			DataSource source;
			bool dataBool;
			float dataFloat;
			Vector2 dataVec2;

			public DataSource Source { get { return source; } }

			public LiveSource(DataSource source)
			{
				this.source = source;
			}

			public bool GetBool()
			{
				return dataBool;
			}

			public float GetFloat()
			{
				return dataFloat;
			}

			public Vector2 GetVec2()
			{
				return dataVec2;
			}

			public void SetDataBool(bool value)
			{
				dataBool = value;
			}

			public void SetDataFloat(float value)
			{
				dataFloat = value;
			}

			public void SetDataVec2(Vector2 value)
			{
				dataVec2 = value;
			}
		}

		private static Dictionary<string, LiveSource> liveDataSources;
		public static Dictionary<string, LiveSource> LiveDataSources { get { return liveDataSources; } }

		[System.Serializable]
		public struct DataSource
		{
			public DataType Type;
			public string Name;
			public string Guid;
		}

		// need a serialized array of these
		// would be nice to have a better inspector
		// but we don't have one
		// can it be a dictionary?
		[SerializeField] DataSource[] dataSources;

		private void Awake()
		{
			if (liveDataSources == null) liveDataSources = new Dictionary<string, LiveSource>();

			for (int i = 0; i < dataSources.Length; i++)
			{
				DataSource dataSource = dataSources[i];
				if (!liveDataSources.ContainsKey(dataSource.Guid))
				{
					LiveSource liveSource = new LiveSource(dataSource);
					liveDataSources.Add(dataSource.Guid, liveSource);
				}
				else
				{
					Debug.LogError(string.Format("Duplicate data source for GUID {0}, source names {1} + {2}",
						dataSource.Guid, LiveDataSources[dataSource.Guid].Source.Name, dataSource.Name));
				}
			}
		}

		public int GetIndexForDataSource(string name)
		{
			int index = -1;

			for(int i=0; i < dataSources.Length; i++)
			{
				if(dataSources[i].Name == name)
				{
					return i;
				}
			}

			return index;
		}
		
		public void SetData(int sourceIndex, object value)
		{ 
			if (value is bool)
			{
				liveDataSources[dataSources[sourceIndex].Guid].SetDataBool((bool)value);
			}
			else if (value is float)
			{
				liveDataSources[dataSources[sourceIndex].Guid].SetDataFloat((float)value);
			}
			else if (value is Vector2)
			{
				liveDataSources[dataSources[sourceIndex].Guid].SetDataVec2((Vector2)value);
			}
			else
			{
				Debug.LogError("SetData was fed incompatible type");
			}
		}
	}
}