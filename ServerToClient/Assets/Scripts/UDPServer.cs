using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using PimDeWitte.UnityMainThreadDispatcher;

public class UDPServer : MonoBehaviour
{
	UIController controller;
	private UdpClient udpClient;
	private IPEndPoint remoteEndPoint;
	private string[] splitData = null;
	private float lastSendTime = 0f;
	private bool clientDisconnected = false;
	private Thread receiveThread;
	private bool isRunning = true;
	private void Awake()
	{
		controller = GameObject.Find("Panel").GetComponent<UIController>();
	}
	// Start is called before the first frame update
	void Start()
    {
		//Application.targetFrameRate = 60;
		udpClient = new UdpClient(7777);
		udpClient.EnableBroadcast = true;
		remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, 8888);

		receiveThread = new Thread(new ThreadStart(ReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
	}
    // Update is called once per frame
    void Update()
    {
		if(Time.time - lastSendTime >= 0.016f && controller.loadingStart)
		{
			if (controller.message!= null && controller.data != null)
			{
				udpClient.Send(controller.data, controller.data.Length, remoteEndPoint);
				controller.message = null;
				controller.data = null;
			}
			else
			{
				for (int i = controller.savedModel.Count - 1; i >= 0; i--)
				{
					GameObject obj = controller.savedModel[i];

					if (obj == null)
					{
						controller.savedModel.RemoveAt(i);
						continue;
					}
					SendObjectData(obj);
				}
			}
			lastSendTime = Time.time;
		}
	}
	void ReceiveData()
	{
		while (isRunning)
		{
			try
			{
				if (udpClient.Client == null)
				{
					Debug.Log("UdpClient has been closed, stopping receive loop.");
					break;
				}
				byte[] receivedData = udpClient.Receive(ref remoteEndPoint);
				string receivedMessage = Encoding.UTF8.GetString(receivedData);
				splitData = receivedMessage.Split(',');
				UnityMainThreadDispatcher.Instance().Enqueue(() =>
				{
					if (clientDisconnected)
					{
						Debug.Log("Client reconnected.");
						clientDisconnected = false;
					}
					foreach (var obj in controller.savedModel)
					{
						if (splitData[0] == obj.name && obj != null)
						{
							Vector3 position = new Vector3(float.Parse(splitData[1]), float.Parse(splitData[2]), float.Parse(splitData[3]));
							obj.transform.position = position;
							Vector3 rotation = new Vector3(float.Parse(splitData[4]), float.Parse(splitData[5]), float.Parse(splitData[6]));
							obj.transform.localEulerAngles = rotation;
							Vector3 scale = new Vector3(float.Parse(splitData[7]), float.Parse(splitData[8]), float.Parse(splitData[9]));
							obj.transform.localScale = scale;
						}
					}
				});
			}
			catch (SocketException ex)
			{
				if (!clientDisconnected)
				{
					Debug.LogWarning($"SocketException: {ex.Message}");
					HandleClientDisconnect();
					clientDisconnected = true; // client斷開
				}
			}
			catch (ObjectDisposedException ex)
			{
				Debug.LogWarning("UdpClient is already closed: " + ex.Message);
				break; 
			}
			catch (Exception ex)
			{
				Debug.LogError($"Unexpected exception: {ex.Message}");
			}
		}
	}
	void SendObjectData(GameObject obj)
	{
		if (obj != null && !clientDisconnected)
		{
			try
			{
				Vector3 position = obj.transform.position;
				Vector3 rotation = obj.transform.localEulerAngles;
				Vector3 scale = obj.transform.localScale;
				string message = obj.name + "," + position.x + "," + position.y + "," + position.z +
					"," + rotation.x + "," + rotation.y + "," + rotation.z +
					"," + scale.x + "," + scale.y + "," + scale.z;
				byte[] data = Encoding.UTF8.GetBytes(message);
				udpClient.Send(data, data.Length, remoteEndPoint);
			}
			catch (SocketException ex)
			{
				if (!clientDisconnected) 
				{
					Debug.LogWarning($"SocketException: {ex.Message}");
					HandleClientDisconnect();
					clientDisconnected = true; 
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"Unexpected exception while sending data: {ex.Message}");
			}
		}
	}
	void HandleClientDisconnect()
	{
		Debug.Log("Client disconnected. Cleaning up resources.");
	}
	void OnApplicationQuit()
	{
		isRunning = false;
		if (udpClient != null)
		{
			udpClient.Close();
			udpClient = null;
		}
		Debug.Log("Application quitting, stopped UDP client.");
	}
}
