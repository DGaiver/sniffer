using System;
using System.Net;
using System.IO;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UdpSocket_Class;

namespace RoboSniffer
{
	


	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
    public partial class MainWindow : Window
	{
        private UdpSocketCl localSocket;
		private event EventHandler<UdpSocketReciveEventArgs> Recived_Data;

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.localSocket != null)
			{
				this.localSocket.StopRecive();
				this.localSocket = null;
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			// определяет поле IPHostEntry, которое есть искомый адрес сетевого интерфейса.
			IPHostEntry HEntry = Dns.GetHostEntry((Dns.GetHostName()));
			if (HEntry.AddressList.Length>0)
			{
				foreach (IPAddress ip in HEntry.AddressList)
				{
					interfaceList.Items.Add(ip.ToString()); // куда хотим, туда и сохраняем
				}
			}
			Recived_Data+= OnDataRecivedEvent;
		}


		//открывает raw сокет без учёта конкретного интерфейса
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (localSocket==null)
			{
				try
				{
					if (interfaceList.Text!="")
					{
                    (sender as Button).Content = "Close Socket";
					localSocket = new UdpSocketCl();
					IPAddress LocalIP = IPAddress.Parse(interfaceList.Text);
					IPAddress ipDst = IPAddress.Any;
					errbox.Text = localSocket.Set_Net(ipDst, 5670, 5670);
					localSocket.Recived_Socket_Data += DataRecived;
					localSocket.StartRecive();
					}
					else
					{
						errbox.Text = "Выберите сетевой интерфейс!";
					}
					
				}
				catch(Exception err) { errbox.Text = err.Message; }
			}
			else
			{
				try
				{
                    (sender as Button).Content = "Open Socket";
					errbox.Text = localSocket.StopRecive();
			    	this.localSocket = null;
				}
				catch (Exception err) { errbox.Text = err.Message; }
				
			}
		}

		private void DataRecived(object sender, UdpSocketReciveEventArgs dataRrr)
		{
			
			try
			{
				Dispatcher.Invoke(new Action(() =>
				{
					Recived_Data(this, new UdpSocketReciveEventArgs(dataRrr.Data,dataRrr.length));
					
				}));
			}
			catch (Exception ee)
			{ }
		}

		
		private void OnDataRecivedEvent(object sender, UdpSocketReciveEventArgs arrayRecived)
		{
			//LogBlock.Text += "Recived \r\n";
			IPHeader recHeader = new IPHeader(arrayRecived.Data, arrayRecived.length);
			byte[] recData = new byte[4096];
			//LogBlock.Text += $"IP: {recHeader.SourceAddress.ToString()} Version {recHeader.Version} " +
			//	$"DST ADDR {recHeader.DestinationAddress.ToString()} Total length {recHeader.TotalLength}" + 
			//    $"                                   " +
			//    $"                     \r\n";
			TextBox tempBox = new TextBox();
			IPAddress tmpAdr = new IPAddress(new byte[4] { 0xac, 0x1f, 0x16, 0x18 });
			if (tmpAdr==recHeader.SourceAddress)
			{
                  tempBox = tB1;
			}
			else
			{
				  tempBox = tB;
			}
			
			switch (recHeader.ProtocolType)
			{
				case Protocol.TCP:
				//	LogBlock.Text += $"TCP Packet Recived! \r\n";
					break;

				case Protocol.UDP:
			//		LogBlock.Text += $"UDP Packet Recived! \r\n";
					UDPHeader udpHeader = new UDPHeader(recHeader.Data, (int)recHeader.MessageLength);
					//tempBox.Text += $"{recHeader.SourceAddress.ToString()} SCR port: {udpHeader.SourcePort} DST port: {udpHeader.DestinationPort}  \r\n";
					byte[] rDt = new byte[Int32.Parse(udpHeader.Length)-8];
					Array.Copy(udpHeader.Data, rDt, Int32.Parse(udpHeader.Length)-8);
					//tempBox.Text += $"Data: {BitConverter.ToString(rDt)} \r\n";
					if (LportBox.Text!="" && LportBox.Text==udpHeader.SourcePort )
					{
						tempBox.Text += $"Data: {BitConverter.ToString(rDt)} \r\n";
					}
					break;

				case Protocol.Unknown:
					break;
			}
		}

		private void interfaceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		// данная кнопка открывает сокет, привязывая его к определённому интерфейсу
		private void OpenButt_Copy_Click(object sender, RoutedEventArgs e)
		{
			if (localSocket == null)
			{
				try
				{
					if (interfaceList.Text != "")
					{
						(sender as Button).Content = "Close Socket";
						localSocket = new UdpSocketCl();
						IPAddress LocalIP = IPAddress.Parse(interfaceList.Text);
						IPAddress ipDst = IPAddress.Any;
						errbox.Text = localSocket.Set_Net(LocalIP, ipDst, 5670, 5670);
						localSocket.Recived_Socket_Data += DataRecived;
						localSocket.StartRecive();
					}
					else
					{
						errbox.Text = "Выберите сетевой интерфейс!";
					}

				}
				catch (Exception err) { errbox.Text = err.Message; }
			}
			else
			{
				try
				{
					(sender as Button).Content = "Open Socket";
					errbox.Text = localSocket.StopRecive();
					this.localSocket = null;
				}
				catch (Exception err) { errbox.Text = err.Message; }

			}
		}


	}


	public class IPHeader
{
	//IP Header fields
	private byte byVersionAndHeaderLength;   //Eight bits for version and header length
	private byte byDifferentiatedServices;    //Eight bits for differentiated services (TOS)
	private ushort usTotalLength;              //Sixteen bits for total length of the datagram (header + message)
	private ushort usIdentification;           //Sixteen bits for identification
	private ushort usFlagsAndOffset;           //Eight bits for flags and fragmentation offset
	private byte byTTL;                      //Eight bits for TTL (Time To Live)
	private byte byProtocol;                 //Eight bits for the underlying protocol
	private short sChecksum;                  //Sixteen bits containing the checksum of the header
											  //(checksum can be negative so taken as short)
	private uint uiSourceIPAddress;          //Thirty two bit source IP Address
	private uint uiDestinationIPAddress;     //Thirty two bit destination IP Address
											 //End IP Header fields

	private byte byHeaderLength;             //Header length
	private byte[] byIPData = new byte[4096];  //Data carried by the datagram


	public IPHeader(byte[] byBuffer, int nReceived)
	{

		try
		{
			//Create MemoryStream out of the received bytes
			MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);
			//Next we create a BinaryReader out of the MemoryStream
			BinaryReader binaryReader = new BinaryReader(memoryStream);

			//The first eight bits of the IP header contain the version and
			//header length so we read them
			byVersionAndHeaderLength = binaryReader.ReadByte();

			//The next eight bits contain the Differentiated services
			byDifferentiatedServices = binaryReader.ReadByte();

			//Next eight bits hold the total length of the datagram
			usTotalLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

			//Next sixteen have the identification bytes
			usIdentification = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

			//Next sixteen bits contain the flags and fragmentation offset
			usFlagsAndOffset = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

			//Next eight bits have the TTL value
			byTTL = binaryReader.ReadByte();

			//Next eight represnts the protocol encapsulated in the datagram
			byProtocol = binaryReader.ReadByte();

			//Next sixteen bits contain the checksum of the header
			sChecksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

			//Next thirty two bits have the source IP address
			uiSourceIPAddress = (uint)(binaryReader.ReadInt32());

			//Next thirty two hold the destination IP address
			uiDestinationIPAddress = (uint)(binaryReader.ReadInt32());

			//Now we calculate the header length

			byHeaderLength = byVersionAndHeaderLength;
			//The last four bits of the version and header length field contain the
			//header length, we perform some simple binary airthmatic operations to
			//extract them
			byHeaderLength <<= 4;
			byHeaderLength >>= 4;
			//Multiply by four to get the exact header length
			byHeaderLength *= 4;

			//Copy the data carried by the data gram into another array so that
			//according to the protocol being carried in the IP datagram
			Array.Copy(byBuffer,
					   byHeaderLength,  //start copying from the end of the header
					   byIPData, 0,
					   usTotalLength - byHeaderLength);
		}
		catch (Exception ex)
		{
			
		}
	}

	public string Version
	{
		get
		{
			//Calculate the IP version

			//The four bits of the IP header contain the IP version
			if ((byVersionAndHeaderLength >> 4) == 4)
			{
				return "IP v4";
			}
			else if ((byVersionAndHeaderLength >> 4) == 6)
			{
				return "IP v6";
			}
			else
			{
				return "Unknown";
			}
		}
	}

	public string HeaderLength
	{
		get
		{
			return byHeaderLength.ToString();
		}
	}

	public ushort MessageLength
	{
		get
		{
			//MessageLength = Total length of the datagram - Header length
			return (ushort)(usTotalLength - byHeaderLength);
		}
	}

	public string DifferentiatedServices
	{
		get
		{
			//Returns the differentiated services in hexadecimal format
			return string.Format("0x{0:x2} ({1})", byDifferentiatedServices,
				byDifferentiatedServices);
		}
	}

	public string Flags
	{
		get
		{
			//The first three bits of the flags and fragmentation field 
			//represent the flags (which indicate whether the data is 
			//fragmented or not)
			int nFlags = usFlagsAndOffset >> 13;
			if (nFlags == 2)
			{
				return "Don't fragment";
			}
			else if (nFlags == 1)
			{
				return "More fragments to come";
			}
			else
			{
				return nFlags.ToString();
			}
		}
	}

	public string FragmentationOffset
	{
		get
		{
			//The last thirteen bits of the flags and fragmentation field 
			//contain the fragmentation offset
			int nOffset = usFlagsAndOffset << 3;
			nOffset >>= 3;

			return nOffset.ToString();
		}
	}

	public string TTL
	{
		get
		{
			return byTTL.ToString();
		}
	}

	public Protocol ProtocolType
	{
		get
		{
			//The protocol field represents the protocol in the data portion
			//of the datagram
			if (byProtocol == 6)        //A value of six represents the TCP protocol
			{
				return Protocol.TCP;
			}
			else if (byProtocol == 17)  //Seventeen for UDP
			{
				return Protocol.UDP;
			}
			else
			{
				return Protocol.Unknown;
			}
		}
	}

	public string Checksum
	{
		get
		{
			//Returns the checksum in hexadecimal format
			return string.Format("0x{0:x2}", sChecksum);
		}
	}

	public IPAddress SourceAddress
	{
		get
		{
			return new IPAddress(uiSourceIPAddress);
		}
	}

	public IPAddress DestinationAddress
	{
		get
		{
			return new IPAddress(uiDestinationIPAddress);
		}
	}

	public string TotalLength
	{
		get
		{
			return usTotalLength.ToString();
		}
	}

	public string Identification
	{
		get
		{
			return usIdentification.ToString();
		}
	}

	public byte[] Data
	{
		get
		{
			return byIPData;
		}
	}
}


	public enum Protocol
	{
		TCP = 6,
		UDP = 17,
		Unknown = -1
	};

	public class UDPHeader
	{
		//UDP header fields
		private ushort usSourcePort;            //Sixteen bits for the source port number        
		private ushort usDestinationPort;       //Sixteen bits for the destination port number
		private ushort usLength;                //Length of the UDP header
		private short sChecksum;                //Sixteen bits for the checksum
												//(checksum can be negative so taken as short)              
												//End UDP header fields

		private byte[] byUDPData = new byte[4096];  //Data carried by the UDP packet

		public UDPHeader(byte[] byBuffer, int nReceived)
		{
			MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);
			BinaryReader binaryReader = new BinaryReader(memoryStream);

			//The first sixteen bits contain the source port
			usSourcePort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

			//The next sixteen bits contain the destination port
			usDestinationPort = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

			//The next sixteen bits contain the length of the UDP packet
			usLength = (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

			//The next sixteen bits contain the checksum
			sChecksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

			//Copy the data carried by the UDP packet into the data buffer
			Array.Copy(byBuffer,
					   8,               //The UDP header is of 8 bytes so we start copying after it
					   byUDPData,
					   0,
					   nReceived - 8);
		}

		public string SourcePort
		{
			get
			{
				return usSourcePort.ToString();
			}
		}

		public string DestinationPort
		{
			get
			{
				return usDestinationPort.ToString();
			}
		}

		public string Length
		{
			get
			{
				return usLength.ToString();
			}
		}

		public string Checksum
		{
			get
			{
				//Return the checksum in hexadecimal format
				return string.Format("0x{0:x2}", sChecksum);
			}
		}

		public byte[] Data
		{
			get
			{
				return byUDPData;
			}
		}
	}






}

