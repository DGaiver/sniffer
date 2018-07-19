using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace UdpSocket_Class
{
	class UdpSocketCl
	{
		private Socket s;
		private Socket rawS;
		private Thread socketRecive;
		private IPEndPoint iDst;
		public int buffSize = 1500;
		private bool StopThr;
		public event EventHandler<UdpSocketReciveEventArgs> Recived_Socket_Data;
		private byte[] byteData = new byte[4096];
		private bool bConCap = false;



		public string Set_Net(IPAddress IpDst, int PortDst, int ListenPort)
		{
			try
			{
				s = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
				IPAddress locIP = IPAddress.Any;
				iDst = new IPEndPoint(IpDst, PortDst);
				IPEndPoint iEPLoc = new IPEndPoint(locIP, ListenPort);
				s.Bind(iEPLoc);
				return "Socket Net Set and Start";
			
			}
			catch (Exception e ) { return e.Message; }
		}

		public string Set_Net(IPAddress IpLoc, IPAddress IpDst, int PortDst, int ListenPort)
		{
			try
			{
				bConCap = true;
				s = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
				s.Bind(new IPEndPoint(IpLoc,0));

				//как раз данной строкой задаётся параметр сокету какой адрес ему использовать, то есть с какого интерфейса он будет посылать/слушать сообщения
				s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

				byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
				byte[] byOut = new byte[4] { 1, 0, 0, 0 };

				s.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);

				s.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnRecivedDataAsync), null);
				
				return "Socket Net Set and Start";

			}
			catch (Exception e) { return e.Message; }
		}

		private void OnRecivedDataAsync(IAsyncResult ar)
		{
			try
			{
              int nRec = s.EndReceive(ar);
	  		  ParseData(byteData, nRec);
				
			  if (bConCap)
			  {
				byteData = new byte[4096];
				s.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None, new AsyncCallback(OnRecivedDataAsync), null);
			  }
				//return "Recived Data";

			}
			catch (Exception e) { }
		}

		private void ParseData(byte[] byteData, int nReceived)
		{

			Recived_Socket_Data(this, new UdpSocketReciveEventArgs(byteData, nReceived));
		}

		public void Send_Message(byte[] sendingData)
		{
			try
			{
				s.SendTo(sendingData, iDst);
			}
			catch { }
		}

		public void StartRecive()
		{
			StopThr = false;
			socketRecive = new Thread(ReciveMess);
			socketRecive.Start();
		}

		public string StopRecive()
		{
			try
			{
               StopThr = true;
			   s.Close(0);
				return "Socket Stop";
			}
			catch (Exception e) { return e.Message; }
			
		}

		public void CloseSocket()
		{
			s.Close(0);
			
		}

		private void ReciveMess()
		{
			try
			{
				while (true)
				{
					if (StopThr)
					{
						break;
					}
					else
					{
						Byte[] recbuffbytes = new Byte[buffSize];
						IPEndPoint tmpEp = new IPEndPoint(IPAddress.Any, 0);
						EndPoint tmpE_p = (tmpEp);
						int CountRecBytes = s.ReceiveFrom(recbuffbytes, ref tmpE_p);
						Array.Resize(ref recbuffbytes, CountRecBytes);
						if (CountRecBytes != 0)
						{
						//	Recived_Socket_Data(this, new UdpSocketReciveEventArgs(recbuffbytes));
						}
					}
					
				}
			}
			catch { }
		}



	}

	public class UdpSocketReciveEventArgs
	{
		private byte[] _data;
		private int _length;

		public UdpSocketReciveEventArgs(byte[] data, int length)
		{
			_data = data;
			_length = length;
		}

		public byte[] Data
		{
			get
			{
				return _data;
			}
		}

		public int length
		{
			get
			{
				return _length;
			}
		}
	}

}




