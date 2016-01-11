using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics; 

namespace rmApplication
{
	class CommProtocol
	{
		public enum RmAddr : int
		{
			Byte2 = 0,
			Byte4
		};

		public static string pbDutVersion;
		public static string pbDumpData;

		public static Queue<string> pbCommLog = new Queue<string>();

		public static List<string> pbCommLogBuffer = new List<string>();

		public static bool pbLogFlg = false;

		private const int MAX_LOG_DATA = 10000;		//log size

		private enum RmMode : int
		{
			LOG = 0,
			COMMAND
		};

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
		
		private static RmMode CommMode = RmMode.COMMAND;
		
		private static RmAddr SelectByte = RmAddr.Byte4;
		
		private static int MasCnt = 0;
		private static int LastSlvCnt = 0;
		private static bool CommSentFlg = false;
		private static int ContinueCnt = 1;

		private static Queue<List<byte>> TxDataStream = new Queue<List<byte>>();
		private static Queue<List<byte>> RxDataStream = new Queue<List<byte>>();

		private static bool RcvFlg = false;
		private static bool EscFlg = false;
		private static List<byte> RcvFrame = new List<byte>();
		private static Queue<byte> RxBytes = new Queue<byte>();

		private struct DataGridViewParam
		{
			public string Size;
			public string Address;
			public string Type;
			
		}

		private static List<DataGridViewParam> listDGVParam = new List<DataGridViewParam>();

		static Stopwatch sw = new Stopwatch();

		public static void startStopWatch()
		{
			sw.Restart();

		}

		public static void stopStopWatch()
		{
			sw.Stop();

		}


		private static List<string> interpretRxFrameToAnyType(List<byte> rxFrameData)
		{
			int maxIndex = listDGVParam.Count;
			
			List<string> listSize = new List<string>();
			List<string> listValue = new List<string>();
			string retVal;

			for (int i = 0; i < maxIndex; i++)
			{
				listSize.Add(listDGVParam[i].Size);

			}

			List<byte> cntCode = rxFrameData.GetRange(0, 1);
			retVal = BitConverter.ToString(cntCode.ToArray());
			listValue.Add(retVal);
			rxFrameData.RemoveRange(0, 1);

			for (int i = 0; i < listSize.Count; i++)
			{
				int size = int.Parse(listSize[i]);

				if (rxFrameData.Count >= size)
				{
					List<byte> buff = rxFrameData.GetRange(0, size);
					rxFrameData.RemoveRange(0, size);
					buff = BinaryEditor.Swap(buff);
					string hexdata = BinaryEditor.BytesToHexString(buff.ToArray());
					retVal = TypeConvert.FromHexChars(listDGVParam[i].Type, int.Parse(listSize[i]), hexdata);
					listValue.Add(retVal);
					
				}
				else
				{
					//Invalid data
					break;

				}
			}

			return listValue;

		}


		public static List<string> interpretRxFrameToHexChars(List<byte> tmp, List<string> lsize, out bool validflg)
		{
			List<string> listValue = new List<string>();
			validflg = true;
			
			for( int i =0; i < lsize.Count; i++ )
			{
				int size = int.Parse(lsize[i]);

				if (tmp.Count >= size)
				{
					List<byte> buff = tmp.GetRange(0, size);
					tmp.RemoveRange(0, size);
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


		private static void setCommLog(List<byte> byteList, string dir)
		{
			float msec = (float)sw.ElapsedMilliseconds;

			string time = (msec/1000).ToString("000.000");

			string ret = BitConverter.ToString(byteList.ToArray());

			string dat = time + " " + dir + " " + ret;
			
			pbCommLog.Enqueue( dat );

		}


		public static void clear()
		{
			RcvFlg = false;
			EscFlg = false;

			RcvFrame = new List<byte>();

		}


		public static byte[] encode(List<byte> bytes)
		{
			List<byte> frame = new List<byte>();

			setCommLog(bytes, "Tx");

			byte crc = Crc8.Calculate(bytes);
			bytes.Add(crc);

			frame.Add((byte)FrameChar.END);

			for (int tmp_length = 0; tmp_length < bytes.Count; tmp_length++)
			{
				switch (bytes[tmp_length])
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
						frame.Add(bytes[tmp_length]);
						break;

				}
			}

			frame.Add((byte)FrameChar.END);

			return frame.ToArray();
		}


		public static void decode(List<byte> bytes)
		{
			foreach (byte tmp in bytes)
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

								setCommLog(RcvFrame, "Rx");

								RxDataStream.Enqueue(RcvFrame);

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


		public static void clearTxData()
		{
			while(TxDataStream.Count != 0)
			{
				TxDataStream.Dequeue();
			}

		}


		public static void clearRxData()
		{
			while(RxDataStream.Count != 0)
			{
				RxDataStream.Dequeue();
			}

		}


		public static void setLogModeStart()
		{
			List<byte> frame = new List<byte>();

			byte seqCode = ctrlMasCnt();
			seqCode = (byte)(seqCode + RmInstr.StartLog);
			frame.Add(seqCode);

			TxDataStream.Enqueue(frame);

		}


		public static void setLogModeStop()
		{
			List<byte> frame = new List<byte>();

			byte seqCode = ctrlMasCnt();
			seqCode = (byte)(seqCode + RmInstr.StopLog);
			frame.Add(seqCode);

			TxDataStream.Enqueue(frame);
			
		}


		public static void setTiming(string timing)
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


		public static void wirteData(string size, string address, string offset, string writeVal)
		{
			List<byte> frame = new List<byte>();
			List<byte> addData = new List<byte>();

			Int64 dec_address = 0;
			Int64 dec_offset = 0;

			try
			{
				dec_address = Convert.ToInt64(address, 16);
				dec_offset = Convert.ToInt64(offset);
				
			}
			catch (Exception ex)
			{
				return;
				
			}

			Exception ex_text = null;

			address = TypeConvert.ToHexChars(numeralSystem.UDEC, 4, ((dec_address + dec_offset).ToString()), out ex_text);

			writeVal = TypeConvert.FromHexChars(numeralSystem.HEX, int.Parse(size), writeVal);

			if( ex_text != null )
			{
				return;
				
			}

			byte seqCode = ctrlMasCnt();
			seqCode = (byte)(seqCode + RmInstr.Write);
			frame.Add(seqCode);
			
			addData = BinaryEditor.HexStringToBytes(size);
			frame.AddRange(addData);

			if( SelectByte == RmAddr.Byte4 )
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


		public static void setLogData(List<string> listSize, List<string> listType, List<string> listAddress, List<string> listOffset, int maxSize)
		{
			listDGVParam = new List<DataGridViewParam>();
			DataGridViewParam listparam = new DataGridViewParam();
			
			int factor_max;
			int frame_num;
			int last_frame_contents;
			
			if( SelectByte == RmAddr.Byte4 )
			{
				factor_max = 4;
			}
			else
			{
				factor_max = 8;
				
			}

			frame_num = maxSize / factor_max;
			last_frame_contents = maxSize - (frame_num * factor_max);

			if (last_frame_contents != 0)
			{
				frame_num++;
			}
			else if (last_frame_contents == 0)
			{
				last_frame_contents = factor_max;
			}
			
			int data_index = 0;
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
					addData = BinaryEditor.HexStringToBytes(listSize[data_index]);
					frame.AddRange(addData);

					string address = listAddress[data_index];
					string offset = listOffset[data_index];

					Int64 dec_address = 0;
					Int64 dec_offset = 0;

					try
					{
						dec_address = Convert.ToInt64(address, 16);
						dec_offset = Convert.ToInt64(offset);
						
					}
					catch (Exception ex)
					{
						err_flg = true;
						break;
						
					}

					Exception ex_text = null;

					address = TypeConvert.ToHexChars(numeralSystem.UDEC, 4, ((dec_address + dec_offset).ToString()), out ex_text);

					if( ex_text != null )
					{
						err_flg = true;
						break;
						
					}
					else
					{
						
					}

					if( SelectByte == RmAddr.Byte4 )
					{
						
					}
					else
					{
						address = address.Substring(address.Length - 4);
						
					}

					addData = BinaryEditor.HexStringToBytes(address);
					addData = BinaryEditor.Swap(addData);
					frame.AddRange(addData);
					
					listparam.Size = listSize[data_index];
					listparam.Type = listType[data_index];
					listparam.Address = listAddress[data_index];

					listDGVParam.Add(listparam);

					data_index++;

				}
				
				if( err_flg != true )
				{
					TxDataStream.Enqueue(frame);
					
				}
				
			}
			
		}


		public static void readVersion(string password)
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


		public static void readDumpData(string address, string size)
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
			
			Int64 dec_address = 0;
			Int64 dec_offset = 0;
			
			bool err_flg = false;
			
			for (int i = 0; i < frame_num; i++)
			{
				List<byte> frame = new List<byte>();
				List<byte> addData = new List<byte>();

				byte seqCode = ctrlMasCnt();
				seqCode = (byte)(seqCode + RmInstr.ReadDump);
				frame.Add(seqCode);

				dec_offset = payload_max * i;
				
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
					dec_address = Convert.ToInt64(address, 16);
					
				}
				catch (Exception ex)
				{
					err_flg = true;
					break;
					
				}

				Exception ex_text = null;

				address = TypeConvert.ToHexChars(numeralSystem.UDEC, 4, ((dec_address + dec_offset).ToString()), out ex_text);

				if( ex_text != null )
				{
					err_flg = true;
					break;
					
				}
				else
				{
					
				}

				if( SelectByte == RmAddr.Byte4 )
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


		public static int checkLogSequence(List<byte> rxBuff)
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


		private static byte ctrlMasCnt()
		{
			MasCnt = MasCnt + 16;

			if (MasCnt > 255)
			{
				MasCnt = 16;

			}

			return ((byte)MasCnt);
		}


		public static List<byte> mainControl()
		{
			List<byte> rxBuff = null;
			
			if (CommMode == RmMode.COMMAND)
			{
				ContinueCnt = 1;
				
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
						while(RxDataStream.Count != 0)
						{
							rxBuff = RxDataStream.Dequeue();

							List<byte> txBuff = TxDataStream.Peek();

							if ((txBuff[0] & 0xF0) == (rxBuff[0] & 0xF0))
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
										rxBuff.RemoveRange(0, 1);	// delete count code
										pbDutVersion = System.Text.Encoding.ASCII.GetString(rxBuff.ToArray());
										break;
									case (byte)RmInstr.ReadDump:
										rxBuff.RemoveRange(0, 1);	// delete count code
										pbDumpData = BitConverter.ToString(rxBuff.ToArray());
										break;
									default:
										break;

								}

							}

						}
						
						// initial useless buffer
						rxBuff = null;
						
					}

				}
				

			}
			else if (CommMode == RmMode.LOG)
			{
				while (RxDataStream.Count != 0)
				{
					List<byte> tmpBuff = RxDataStream.Dequeue();
					rxBuff = new List<byte>(tmpBuff);

					List<string> listValue = interpretRxFrameToAnyType(tmpBuff);

					if (pbLogFlg == true)
					{
						string rxtext = listValue[0];
						listValue.RemoveAt(0);

						foreach( string tmp in listValue )
						{
							rxtext = rxtext + "," + tmp;
							
						}

						pbCommLogBuffer.Add(rxtext);

						if (pbCommLogBuffer.Count > MAX_LOG_DATA)
						{
							pbCommLogBuffer.RemoveAt(0);

						}

					}
					else
					{
						pbCommLogBuffer = new List<string>();

					}

				}


				if (ContinueCnt > 10 )
				{
					ContinueCnt = 1;

					setLogModeStart();

				}
				else
				{
					ContinueCnt++;

				}
				
			}
			
			return rxBuff;
		}


		public static List<byte> getTxData()
		{
			List<byte> buff = null;

			if ((TxDataStream.Count != 0) &&
				(CommSentFlg == false))
			{
				buff = TxDataStream.Peek();
			}
			
			return buff;
		}



		public static void setTxCondtion( bool flg )
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
						CommMode = RmMode.LOG;
						break;
					case (byte)RmInstr.StopLog:
						CommMode = RmMode.COMMAND;
						break;
					case (byte)RmInstr.SetTiming:
						break;
					case (byte)RmInstr.Write:
						break;
					case (byte)RmInstr.SetAddr:
						CommMode = RmMode.COMMAND;
						break;
					case (byte)RmInstr.ReadInfo:
						CommMode = RmMode.COMMAND;
						break;
					case (byte)RmInstr.ReadDump:
						CommMode = RmMode.COMMAND;
						break;
					default:
						break;

				}

			}
			
			if (CommMode == RmMode.LOG)
			{
				CommSentFlg = false;
				
				TxDataStream.Dequeue();
				
			}
			
		}

		public static void setCommAddress4byte()
		{
			SelectByte = RmAddr.Byte4;
			
		}

		public static void setCommAddress2byte()
		{
			SelectByte = RmAddr.Byte2;
			
		}

		public static RmAddr getCommAddress()
		{
			return(SelectByte);
			
		}


		public static void initial()
		{
			MasCnt = 0;
			LastSlvCnt = 0;
			CommMode = RmMode.COMMAND;
			CommSentFlg = false;
			ContinueCnt = 1;
			
			clearTxData();
			clearRxData();
			
		}


	}
}
