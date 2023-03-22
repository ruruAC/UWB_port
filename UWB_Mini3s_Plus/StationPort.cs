using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWB_Mini3s_Plus
{
    using System.Windows;
    using System.IO.Ports;
    using System.Threading;
    /// <summary>
    /// UWP基站接口
    /// </summary>
    public class StationPort : IDisposable
    {
        public StationPort(string comport)
        {
            this.ComPort = comport;
            serialPort = new SerialPort(comport);
            thread = new Thread(threadWhile);
            thread.Start();
        }

        private void dataRec(string data)
        {
            var d = DataParser.Parse(data);
            DataReceived?.Invoke(this, new DataReceivedEventArgs() { Data = d });
        }
        public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
        public class DataReceivedEventArgs : EventArgs
        {
            public UWB_Data Data { get; set; }
        }
        /// <summary>
        /// 收到数据后触发
        /// </summary>
        public event DataReceivedEventHandler DataReceived;

        private void error(Exception exception)
        {

        }

        private void threadWhile()
        {
            while (!isdisposed)
            {
                if (IsConnected)
                {

                    try
                    {
                        var line = serialPort.ReadLine();
                        dataRec(line);
                    }
                    catch (Exception ex)
                    {
                        error(ex);
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }
        private Thread thread;
        private readonly SerialPort serialPort;
        /// <summary>
        /// 串口号
        /// </summary>
        public string ComPort { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected { get; private set; } = false;

        public void Open()
        {
            try
            {
                serialPort.Open();
                IsConnected = true;
            }
            catch (Exception ex)
            {
                error(ex);
            }

        }

        public void Close()
        {
            IsConnected = false;
            serialPort.Close();
        }
        private bool isdisposed = false;
        public void Dispose()
        {
            isdisposed = true;
            try
            {

                serialPort?.DiscardInBuffer();
                serialPort?.DiscardOutBuffer();
                this.Close();
            }
            catch (Exception)
            {

            }
            serialPort.Dispose();

        }
    }
}
