using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
public class UDPMarkerSender : MonoBehaviour {
    string remoteHost = "192.168.10.3";
    int remotePort = 22222;
    IPAddress localAddress;
    int localPort = 2002;
    //IPEndPoint localEP;
    UdpClient udpClient;

	// Use this for initialization
	void Start () {
        udpClient = new UdpClient();
        //udpClient.Send()
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("tesudp");
        Dictionary<int, Transform> transforms = MarkerPosition.markerTransforms;

        foreach (KeyValuePair<int, Transform> kvp in transforms)
        {
            string msg = kvp.Key.ToString() + " " + kvp.Value.transform.position.x + " " + kvp.Value.transform.position.y + " " + kvp.Value.transform.position.z;
            byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(msg);

            udpClient.Send(sendBytes, sendBytes.Length, remoteHost, remotePort);

        }
        //string json = JsonConvert.SerializeObject(transforms);
    }
}
