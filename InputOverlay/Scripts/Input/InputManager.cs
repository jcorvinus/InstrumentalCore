using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// network stuff
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Instrumental.Controls;
using Instrumental.Interaction.VirtualJoystick;

// debug request stuff
using Valve.VR;
using System.Text;

namespace Instrumental.Overlay
{
    [System.Serializable]
    public enum DataType
	{
        None=0,
        Bool=1,
        Float=2,
        Vec2=3
	}

    [System.Serializable]
	public struct InputData
	{
        public DataType Type;
        public string Name;
	}

    [System.Serializable]
	public struct InputHookup
	{ 
        public InputData Data;
        public Button Button; // come up with an abstract interface for this later
		public Joystick joystick;
		public Snap Snap;
	}

	// controller data and controller emulation data
	// are temporary hacks to get things running as quickly as possible
	public struct ControllerData
	{
		public int a; // lower case names breaking with
		public int b; // C# .NET convention
		public float trigger; // because that's how they were specified
		public float grip; // in the input diagram
		public int thumbstick_active;
		public float thumbstick_x;
		public float thumbstick_y;
		public int system;
	}

	public struct ControllerEmulationData
	{
		public ControllerData Left;
		public ControllerData Right;
		public int through_shoulder_aim;
	}

	public class InputManager : MonoBehaviour
    {
        [SerializeField] int port;
        [SerializeField] InputHookup[] hookups;

		[SerializeField] bool printOutput;
		[SerializeField] bool useJsonSend = true;
		[SerializeField] bool refrainFromDebugSend = false;

		LensedValue<bool> useAim = new LensedValue<bool>(false);

		ControllerEmulationData previousGeneratedData;

		public LensedValue<bool> UseAim { get { return useAim; } }

		#region networking stuff
		TcpClient client;
		bool isConnected = false;
		string payload;
		bool hasPayload = false;
		bool shuttingDown = false;
		Thread netThread;

		void StartClient()
		{
			IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, port);
			client = new TcpClient();

			ThreadStart threadStart = new ThreadStart(MessageThread);
			netThread = new Thread(threadStart);
			netThread.Start();
		}

		void MessageThread()
		{
			try
			{
				IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, port);

				Debug.Log("Trying to connect client");
				client.Connect(endpoint);

				while (!client.Connected)
				{

				}

				Debug.Log("Client has connected!");

				Stream stream = client.GetStream(); // not allowed on non-connected sockets. Might need to re-order calls
				StreamWriter writer = new StreamWriter(stream);
				writer.AutoFlush = true;

				while(!client.Connected)
				{
					Thread.Sleep(100);
				}

				isConnected = true;
				Debug.Log("Client connected " + client.Client.RemoteEndPoint);

				while(client.Connected && !shuttingDown)
				{
					if(hasPayload)
					{
						Debug.Log("Sending payload");
						writer.Write(payload);
						hasPayload = false;
					}
				}

				Debug.Log("Disconnecting client");
				isConnected = false;
				client.Close();
			}
			catch (System.Exception exception)
			{
				Debug.LogError(exception.Message + exception.StackTrace);
			}
			finally
			{
				Debug.Log("Disconnecting client");
				isConnected = false;
				client.Close();
			}
		}
		#endregion

		// debug request stuff
		uint m_leapDevice = OpenVR.k_unTrackedDeviceIndexInvalid;

		private void OnDisable()
		{
			shuttingDown = true;
		}

		// Start is called before the first frame update
		void Start()
        {
			if(!useJsonSend)
			{
				// find the device to send to
				for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
				{
					CVRSystem m_vrSystem = OpenVR.System;
					ETrackedPropertyError l_propertyError = ETrackedPropertyError.TrackedProp_Success;
					ulong l_property = m_vrSystem.GetUint64TrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_VendorSpecific_Reserved_Start, ref l_propertyError);
					if ((l_propertyError == ETrackedPropertyError.TrackedProp_Success) && (l_property == 0x4C4D6F74696F6E))
					{
						m_leapDevice = i;
						break;
					}
				}
			}
			else
			{
				StartClient();
			}
        }

		private void Update()
		{
			
		}

		void JsonSend(ControllerEmulationData controllerEmulationData)
		{
			// set our payload to the string
			// mark the payload as full
			string emulationDataString = JsonConvert.SerializeObject(controllerEmulationData);
			payload = emulationDataString;
			hasPayload = true;

			if (printOutput) Debug.Log(payload);
		}

		ControllerEmulationData GetControllerData()
		{
			ControllerEmulationData controllerEmulationData = new ControllerEmulationData();

			// send all of our data
			foreach (InputHookup hookup in hookups)
			{
				if (hookup.Data.Name == "Menu") // replace these string lookups with hashes at some point
				{
					controllerEmulationData.Left.b = (hookup.Button.Runtime.IsPressed) ? 1 : 0; // no way to get runtime values right now.
				}
				else if (hookup.Data.Name == "AppSpecific1")
				{
					controllerEmulationData.Left.a = (hookup.Button.Runtime.IsPressed) ? 1 : 0;
				}
				else if (hookup.Data.Name == "SystemDashboard")
				{
					controllerEmulationData.Left.system = (hookup.Button.Runtime.IsPressed) ? 1 : 0;
				}
				else if (hookup.Data.Name == "LeftJoystick")
				{
					controllerEmulationData.Left.thumbstick_active =
						(hookup.joystick.isActiveAndEnabled) ? 1 : 0;
					Vector2 leftJoystickInput = hookup.joystick.Value;

					controllerEmulationData.Left.thumbstick_x = leftJoystickInput.x;
					controllerEmulationData.Left.thumbstick_y = leftJoystickInput.y;
				}
				else if (hookup.Data.Name == "Snap")
				{
					bool isActive = (hookup.Snap.IsSnapLeft || hookup.Snap.IsSnapRight);
					controllerEmulationData.Right.thumbstick_active =
						isActive ? 1 : 0;

					if (isActive)
					{
						controllerEmulationData.Right.thumbstick_x = (hookup.Snap.IsSnapLeft) ? -1 : 1;
					}
					else
					{
						controllerEmulationData.Right.thumbstick_x = 0;
					}
				}
			}

			controllerEmulationData.through_shoulder_aim = (useAim.GetValue() ? 1 : 0);

			return controllerEmulationData;
		}

		//public enum ButtonState
		//{
		//	None = 0,
		//	Touched,
		//	Clicked
		//}

		const string noneState = "none";
		const string touchedState = "touched";
		const string clickedState = "clicked";

		void DebugRequestSend(ControllerEmulationData controllerEmulationData, ControllerEmulationData previousControllerData)
		{
			// we need to update the inputs to include the button/axis prefix
			// also include ButtonState
			if(controllerEmulationData.Left.b != previousGeneratedData.Left.b)
			{
				SendDebugRequest(string.Format("input left button b {0}", (controllerEmulationData.Left.b == 1) ? clickedState : noneState));
			}

			if(controllerEmulationData.Left.a != previousGeneratedData.Left.a)
			{
				SendDebugRequest(string.Format("input left button a {0}", (controllerEmulationData.Left.a == 1) ? clickedState : noneState));
			}

			if(controllerEmulationData.Left.system != previousGeneratedData.Left.system)
			{
				SendDebugRequest(string.Format("input left button system {0}", (controllerEmulationData.Left.system == 1) ? clickedState : noneState));
			}

			// need to look into button state for thumbsticks and maybe everything else
			// On axes it looks like there is both a clicked and touch state.
			// on driver_leap it looks like click/touch is a distance based thing,
			// which is pretty fascinating. Probably not worth completely replicating
			// but I can see it being useful for the case of having click bound to something interesting
			// like gesture enable/disable
			// GetButtonState.ToString().ToLower - lowercase form of enum name
			if(controllerEmulationData.Left.thumbstick_x != previousGeneratedData.Left.thumbstick_x ||
				controllerEmulationData.Left.thumbstick_y != previousGeneratedData.Left.thumbstick_y)
			{
				SendDebugRequest(string.Format("input left axis thumbstick {0} {1} {2}",
					(controllerEmulationData.Left.thumbstick_active == 1) ? touchedState : noneState,
					controllerEmulationData.Left.thumbstick_x,
					controllerEmulationData.Left.thumbstick_y));
			}

			// update to include sending the right thumbstick axis stuff
			if(controllerEmulationData.Right.thumbstick_x != previousGeneratedData.Right.thumbstick_x ||
				controllerEmulationData.Right.thumbstick_y != previousGeneratedData.Right.thumbstick_y)
			{
				SendDebugRequest(string.Format("input right axis thumbstick {0} {1} {2}",
					(controllerEmulationData.Right.thumbstick_active == 1) ? touchedState : noneState,
					controllerEmulationData.Right.thumbstick_x,
					controllerEmulationData.Right.thumbstick_y));
			}
		}

		void SendDebugRequest(string message)
		{
			StringBuilder messageBuilder = new StringBuilder(32);
			Debug.Log("DriverDebugRequest: " + message);
			OpenVR.Debug.DriverDebugRequest(m_leapDevice, message, messageBuilder, 32);
		}

		private void LateUpdate()
		{
			ControllerEmulationData controllerEmulationData = GetControllerData();

			if(useJsonSend)
			{
				JsonSend(controllerEmulationData);
			}
			else
			{
				if(!refrainFromDebugSend) DebugRequestSend(controllerEmulationData, previousGeneratedData);
			}

			previousGeneratedData = controllerEmulationData;
		}
	}
}