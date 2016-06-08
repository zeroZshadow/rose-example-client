using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;

public class Parameters
{
#if UNITY_WEBGL && !UNITY_EDITOR
	[DllImport("__Internal")]
  public static extern string Get(string name);
#else
  public static string Get(string name) {
    return "";
  }
#endif 
}