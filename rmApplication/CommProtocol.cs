using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics; 

namespace rmApplication
{
	public class CommProtocol
	{
		public struct RxDataParam
		{
			public string Time;
			public List<byte> Data;

		}

		public struct SetLogParam
		{
			public string Size;
			public string Address;
		}
		
		public class Components
		{
			public enum RmMode : int
			{
				LOG = 0,
				COMMAND
			};

			public enum RmAddr : int
			{
				Byte2 = 0,
				Byte4
			};


			public RmMode CommMode { set; get; }
			public RmAddr SelectByte { set; get; }
			
			public Queue<string> CommLog { set; get; }
			public Queue<RxDataParam> ReceiveStream { set; get; }
			
			public string DutVersion { set; get; }
			public string DumpData { set; get; }

			public Components()
			{
				CommMode = RmMode.COMMAND;
				SelectByte = RmAddr.Byte2;
				
				CommLog = new Queue<string>();
				ReceiveStream = new Queue<RxDataParam>();
				
				DutVersion = "";
				DumpData = null;
				
			}

			public Components( Components data )
			{
				CommMode = data.CommMode;
				SelectByte = data.SelectByte;
				
				CommLog = data.CommLog;
				ReceiveStream = data.ReceiveStream;
				
				DutVersion = data.DutVersion;
				DumpData = data.DumpData;
				
			}

		}
		
		public Components myComponents;
		
		private enum RmInstr : byte
		{
			StartLog = 1,
			StopLog = 2,
			SetTiming = 3,
			Write = 4,
			SetAddr = 5,
			ReadInfo = 6,
			ReadDump = 7
		};

		private enum FrameChar : byte
		{
			END =		0xC0,
			ESC =		0xDB,
			ESC_END =	0xDC,
			ESC_ESC =	0xDD
		};
		
		
		private int MasCnt = 0;
		private int LastSlvCnt = 0;
		private bool CommSentFlg = false;

		private Queue<List<byte>> TxDataStream = new Queue<List<byte>>();

		private bool RcvFlg = false;
		private bool EscFlg = false;
		private List<byte> RcvFrame = new List<byte>();
		private Queue<byte> RxBytes = new Queue<byte>();

		private Stopwatch sw = new Stopwatch();

		public CommProtocol()
		{
			myComponents = new Components();
			
		}


		public void startStopWatch()
		{
			sw.Restart();

		}

		public void stopStopWatch()
		{
			sw.Stop();

		}


		public List<string> interpretRxFrameToHexChars(List<byte> tmp, List<string> listSize, out bool validflg)
		{
			List<string> listValue = new List<string>();
			validflg = true;
			
			foreach( var size in listSize )
			{
				int intSize = int.Parse(size);

				if (tmp.Count >= intSize)
				{
					List<byte> buff = tmp.GetRange(0, intSize);
					tmp.RemoveRange(0, intSize);
					buff = BinaryEditor.Swap(buff);
					listValue.Add(BinaryEditor.BytesToHexString(buff.ToArray()));
				}
				else
				{
					//Invalid data
					validflg = false;
					
				}
				
			}
			
			return listValue;
		}


		private string setCommLog(List<byte> byteList, string dir)
		{
			float msec = (float)sw.ElapsedMilliseconds;

			string time = (msec/1000).ToString("000.000");

			string byteStrings = BitConverter.ToString(byteList.ToArray());

			string dat = time + " " + dir + " " + byteStrings;
			
			myComponents.CommLog.Enqueue( dat );
			
			return time;

		}


		public void clear()
		{
			RcvFlg = false;
			EscFlg = false;

			RcvFrame = new List<byte>();

		}


		public byte[] encode(List<byte> bytes)
		{
			List<byte> frame = new List<byte>();

			setCommLog(bytes, "Tx");

			byte crc = Crc8.Calculate(bytes);
			bytes.Add(crc);

			frame.Add((byte)FrameChar.END);

			for (int i = 0; i < bytes.Count; i++)
			{
				switch (bytes[i])
				{
					case (byte)FrameChar.END:
						frame.Add((byte)FrameChar.ESC);
						frame.Add((byte)FrameChar.ESC_END);
						break;
					case (byte)FrameChar.ESC:
						frame.Add((byte)FrameChar.ESC);
						frame.Add((byte)FrameChar.ESC_ESC);
						break;

					default:
						frame.Add(bytes[i]);
						break;

				}
			}

			frame.Add((byte)FrameChar.END);

			return frame.ToArray();
		}


		public void decode(byte[] bytes)
		{
			foreach (var tmp in bytes)
			{
				RxBytes.Enqueue(tmp);

			}

			while( RxBytes.Count != 0 )
			{
				byte tmp = RxBytes.Dequeue();

				if (tmp == (byte)FrameChar.END)
				{
					// Start SLIP Frame
					if (RcvFlg == false)
					{
						RcvFlg = true;
						RcvFrame = new List<byte>();

					}
					else
					{
						if( RcvFrame.Count == 0 )
						{
							// receive order FRAMECHAR.END,FRAMECHAR.END
							// Last FRAMECHAR.END might be Start of Frame
							RcvFlg = true;
							
						}
						else
						{
							byte crc = Crc8.Calculate(RcvFrame);
							
							if( ( crc == 0x00 ) &&
								( RcvFrame.Count >= 2 ) )
							{
								// End SLIP Frame

								// delete useless crc data size
								RcvFrame.RemoveRange((RcvFrame.Count - 1), 1);

								string time = setCommLog(RcvFrame, "Rx");

								RxDataParam rxData = new RxDataParam();

								rxData.Time = time;
								rxData.Data = new List<byte>(RcvFrame);

								myComponents.ReceiveStream.Enqueue(rxData);
				
								RcvFrame = new List<byte>();

								RcvFlg = false;

							}
							else
							{
								RcvFlg = true;
								RcvFrame = new List<byte>();
								
							}
							
						}
						
					}

				}
				else
				{
					if (EscFlg == true)
					{
						EscFlg = false;

						switch (tmp)
						{
							case (byte)FrameChar.ESC_END:
								RcvFrame.Add((byte)FrameChar.END);
								break;
							case (byte)FrameChar.ESC_ESC:
								RcvFrame.Add((byte)FrameChar.ESC);
								break;

						}

					}
					else if (tmp == (byte)FrameChar.ESC)
					{
						EscFlg = true;
					}
					else
					{
						if (RcvFlg == true)
						{
							RcvFrame.Add(tmp);
						}

					}

				}

			}

		}


		public void clearTxData()
		{
			while(TxDataStream.Count != 0)
			{
				TxDataStream.Dequeue();
			}

		}


		public void clearRxData()
		{
			while(myComponents.ReceiveStream.Count != 0)
			{
				myComponents.ReceiveStream.Dequeue();
			}

		}


		public void setLogModeStart()
		{
			List<byte> frame = new List<byte>();

			byte seqCode = ctrlMasCnt();
			seqCode = (byte)(seqCode + RmInstr.StartLog);
			frame.Add(seqCode);

			TxDataStream.Enqueue(frame);

		}


		public void setLogModeStop()
		{
			List<byte> frame = new List<byte>();

			byte seqCode = ctrlMasCnt();
			seqCode = (byte)(seqCode + RmInstr.StopLog);
			frame.Add(seqCode);

			TxDataStream.Enqueue(frame);
			
		}


		public void setTiming(string timing)
		{
			List<byte> frame = new List<byte>();
			List<byte> addData = new List<byte>();

			byte seqCode = ctrlMasCnt();
			seqCode = (byte)(seqCode + RmInstr.SetTiming);
			frame.Add(seqCode);

			addData = BinaryEditor.HexStringToBytes(timing);

			if (addData.Count == 1)
			{
				addData.Add((byte)0);

			}

			addData = BinaryEditor.Swap(addData);
			frame.AddRange(addData);

			TxDataStream.Enqueue(frame);

		}


		public void wirteData(string size, string address, string writeVal)
		{
			List<byte> frame = new List<byte>();
			List<byte> addData = new List<byte>();

			byte seqCode = ctrlMasCnt();
			seqCode = (byte)(seqCode + RmInstr.Write);
			frame.Add(seqCode);
			
			addData = BinaryEditor.HexStringToBytes(size);
			frame.AddRange(addData);

			if( myComponents.SelectByte == Components.RmAddr.Byte4 )
			{

			}
			else
			{
				address = address.Substring(address.Length - 4);
				
			}

			addData = BinaryEditor.HexStringToBytes(address);
			addData = BinaryEditor.Swap(addData);
			frame.AddRange(addData);
			addData = BinaryEditor.HexStringToBytes(writeVal);
			addData = BinaryEditor.Swap(addData);
			frame.AddRange(addData);
			
			TxDataStream.Enqueue(frame);
			
		}


		public void setLogData(List<SetLogParam> argParam)
		{
			int factor_max;
			int frame_num;
			int last_frame_contents;
			
			if( myComponents.SelectByte == Components.RmAddr.Byte4 )
			{
				factor_max = 4;
			}
			else
			{
				factor_max = 8;
				
			}

			frame_num = argParam.Count / factor_max;
			last_frame_contents = argParam.Count - (frame_num * factor_max);

			if (last_frame_contents != 0)
			{
				frame_num++;
			}
			else if (last_frame_contents == 0)
			{
				last_frame_contents = factor_max;
			}

			int dataIndex = 0;
			int frame_contents = 0;

			for (int i = 0; i < frame_num; i++)
			{
				List<byte> frame = new List<byte>();
				List<byte> addData = new List<byte>();

				byte seqCode = ctrlMasCnt();
				seqCode = (byte)(seqCode + RmInstr.SetAddr);
				frame.Add(seqCode);
				
				byte flgCode = 0x00;

				frame_contents = factor_max;
				
				if( i == 0 )
				{
					flgCode |= 0x10;
					
				}
				
				if( i == (frame_num-1) )
				{
					flgCode |= 0x20;
					
					frame_contents = last_frame_contents;
					
				}
				
				flgCode |= (byte)i;
				
				frame.Add(flgCode);
				
				bool err_flg = false;
				
				for (int j = 0; j < frame_contents; j++)
				{
					addData = BinaryEditor.HexStringToBytes(argParam[dataIndex].Size);
					frame.AddRange(addData);

					string address = argParam[dataIndex].Address;

					if( myComponents.SelectByte == Components.RmAddr.Byte4 )
					{
						
					}
					else
					{
						address = address.Substring(address.Length - 4);
						
					}

					addData = BinaryEditor.HexStringToBytes(address);
					addData = BinaryEditor.Swap(addData);
					frame.AddRange(addData);
					
					dataIndex++;

				}
				
				if( err_flg != true )
				{
					TxDataStream.Enqueue(frame);
					
				}
				
			}
			
		}


		public void readVersion(string password)
		{
			List<byte> frame = new List<byte>();
			List<byte> addData = new List<byte>();

			byte seqCode = ctrlMasCnt();
			seqCode = (byte)(seqCode + RmInstr.ReadInfo);
			frame.Add(seqCode);

			addData = BinaryEditor.HexStringToBytes(password);
			addData = BinaryEditor.Swap(addData);
			frame.AddRange(addData);

			TxDataStream.Enqueue(frame);

		}


		public void readDumpData(string address, string size)
		{
			int payload_max = 128;
			int frame_num;
			int last_frame_contents;
			
			int total_size = int.Parse(size);
			int payload_size;
			
			frame_num = total_size / payload_max;
			last_frame_contents = total_size - (frame_num * payload_max);

			if (last_frame_contents != 0)
			{
				frame_num++;
			}
			else if (last_frame_contents == 0)
			{
				last_frame_contents = payload_max;
			}
			
			Int64 intAddress = 0;
			Int64 intOffset = 0;
			
			bool err_flg = false;
			
			for (int i = 0; i < frame_num; i++)
			{
				List<byte> frame = new List<byte>();
				List<byte> addData = new List<byte>();

				byte seqCode = ctrlMasCnt();
				seqCode = (byte)(seqCode + RmInstr.ReadDump);
				frame.Add(seqCode);

				intOffset = payload_max * i;
				
				if( i == (frame_num-1) )
				{
					payload_size = last_frame_contents;
					
				}
				else
				{
					payload_size = payload_max;
					
				}

				try
				{
					intAddress = Convert.ToInt64(address, 16);
					
				}
				catch (Exception ex)
				{
					err_flg = true;
					break;
					
				}

				var value = Convert.ToInt64(intAddress + intOffset);
				var bytes = BitConverter.GetBytes(value);

				var list = bytes.ToList();
				list.RemoveRange(4, (list.Count - 4));
				bytes = list.ToArray();
				Array.Reverse(bytes, 0, bytes.Length);
				string byteStrings = BitConverter.ToString(bytes);

				address = byteStrings.Replace("-", "");

				if( myComponents.SelectByte == Components.RmAddr.Byte4 )
				{

				}
				else
				{
					address = address.Substring(address.Length - 4);
					
				}
				
				
				addData = BinaryEditor.HexStringToBytes(address);
				addData = BinaryEditor.Swap(addData);
				frame.AddRange(addData);
				byte bdata = (byte)payload_size;
				frame.Add(bdata);
				
				if( err_flg != true )
				{
					TxDataStream.Enqueue(frame);
					
				}
				
			}
			
		}


		public int checkLogSequence(List<byte> rxBuff)
		{
			int lostcnt = 0;

			lostcnt = 0;
			int slvcnt = rxBuff[0] & 0x0F;

			if (slvcnt == 0)
			{

			}
			else
			{
				lostcnt = slvcnt - LastSlvCnt;
				LastSlvCnt = slvcnt;

				if (LastSlvCnt == 15)
				{
					LastSlvCnt = 1;

				}

			}

			return lostcnt;
		}


		private byte ctrlMasCnt()
		{
			MasCnt = MasCnt + 16;

			if (MasCnt > 255)
			{
				MasCnt = 16;

			}

			return ((byte)MasCnt);
		}


		public bool mainControl()
		{
			bool rcvFlg = false;
			
			if( myComponents.ReceiveStream.Count != 0 )
			{
				rcvFlg = true;
				
			}
			
			if (myComponents.CommMode == Components.RmMode.COMMAND)
			{
				if( CommSentFlg == false )
				{
					
				}
				else
				{
					if (TxDataStream.Count == 0)
					{
						
					}
					else
					{
						while(myComponents.ReceiveStream.Count != 0)
						{
							RxDataParam rxStream = myComponents.ReceiveStream.Dequeue();

							List<byte> txBuff = TxDataStream.Peek();

							if ((txBuff[0] & 0xF0) == (rxStream.Data[0] & 0xF0))
							{
								CommSentFlg = false;

								TxDataStream.Dequeue();

								byte req = (byte)(txBuff[0] & 0x0F);

								switch (req)
								{
									case (byte)RmInstr.StartLog:
										break;
									case (byte)RmInstr.StopLog:
										break;
									case (byte)RmInstr.SetTiming:
										break;
									case (byte)RmInstr.Write:
										break;
									case (byte)RmInstr.SetAddr:
										break;
									case (byte)RmInstr.ReadInfo:
										rxStream.Data.RemoveRange(0, 1);	// delete count code
										myComponents.DutVersion = System.Text.Encoding.ASCII.GetString(rxStream.Data.ToArray());
										break;
									case (byte)RmInstr.ReadDump:
										rxStream.Data.RemoveRange(0, 1);	// delete count code
										myComponents.DumpData = BitConverter.ToString(rxStream.Data.ToArray());
										break;
									default:
										break;

								}

							}

						}

					}

				}

			}
			else if (myComponents.CommMode == Components.RmMode.LOG)
			{
				
				
			}

			return rcvFlg;
			
		}


		public List<byte> getTxData()
		{
			List<byte> buff = null;

			if ((TxDataStream.Count != 0) &&
				(CommSentFlg == false))
			{
				buff = TxDataStream.Peek();
			}
			
			return buff;
		}



		public void setTxCondtion( bool flg )
		{
			CommSentFlg = flg;
				
			if( flg == false )
			{
				
			}
			else
			{
				List<byte> txBuff = TxDataStream.Peek();

				byte req = (byte)(txBuff[0] & 0x0F);

				switch (req)
				{
					case (byte)RmInstr.StartLog:
						myComponents.CommMode = Components.RmMode.LOG;
						break;
					case (byte)RmInstr.StopLog:
						myComponents.CommMode = Components.RmMode.COMMAND;
						break;
					case (byte)RmInstr.SetTiming:
						break;
					case (byte)RmInstr.Write:
						break;
					case (byte)RmInstr.SetAddr:
						myComponents.CommMode = Components.RmMode.COMMAND;
						break;
					case (byte)RmInstr.ReadInfo:
						myComponents.CommMode = Components.RmMode.COMMAND;
						break;
					case (byte)RmInstr.ReadDump:
						myComponents.CommMode = Components.RmMode.COMMAND;
						break;
					default:
						break;

				}

			}
			
			if (myComponents.CommMode == Components.RmMode.LOG)
			{
				CommSentFlg = false;
				
				TxDataStream.Dequeue();
				
			}
			
		}

		public bool isLogMode()
		{
			bool ret = false;
			
			if( myComponents.CommMode == Components.RmMode.LOG )
			{
				ret = true;
				
			}
			
			return ret;
		}

		public void initial()
		{
			MasCnt = 0;
			LastSlvCnt = 0;
			myComponents.CommMode = Components.RmMode.COMMAND;
			CommSentFlg = false;
			clear ();
			
			clearTxData();
			clearRxData();
			
		}


	}
}
