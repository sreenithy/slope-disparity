using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpiscanUtil
{
    public class SyncBoard
    {
        private SerialPort _port = null;
        public int Timeout
        {
            set
            {
                _port.ReadTimeout = value;
                _port.WriteTimeout = value;
            }
            get => _port.ReadTimeout;
        }

        public static string[] AvailablePortNames
        {
            get { return SerialPort.GetPortNames(); }
        }

        //public SyncBoard()
        //{
        //    _port = new SerialPort("COM3", 57600, Parity.None, 8, StopBits.One);

        //    _port.Open();

        //    // フロー制御はしません。
        //    _port.DtrEnable = false;
        //    _port.RtsEnable = false;
        //}
        public SyncBoard(string portname)
        {
            _port = new SerialPort(portname, 57600, Parity.None, 8, StopBits.One);
        }
        public void Open()
        {
            _port.Open();
            System.Threading.Thread.Sleep(250);
        }
        public void Close()
        {
            if (_port.IsOpen)
            {
                _port.Close();
                _port.Dispose();
                System.Threading.Thread.Sleep(250);
            }
        }
        public void TogglePower()
        {
            try
            {
                _port.Write(new char[] { 'p' }, 0, 1);

                //usleep(10000000);
                System.Threading.Thread.Sleep(10000);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ToggleProjectorMode()
        {
            try
            {
                _port.Write(new char[] { 'm' }, 0, 1);
                //usleep(12000000);
                System.Threading.Thread.Sleep(12000);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool QueryIsSyncBoardConnected()
        {
            _port.Write(new char[] { 'o' }, 0, 1);
            char[] data = new char[1024];

            try
            {
                _port.Read(data, 0, 1);
            }
            catch (Exception)
            {
                _port.Close();
                System.Threading.Thread.Sleep(250);
                return false;
            }

            if (data == null) return false;
            else return true;
        }
        public bool QueryIsProjectorRunning()
        {
            if (QueryIsSyncBoardConnected() == false)
            {
                Console.WriteLine("The sync board is not responding!");
                return false;
            }


            _port.Write(new char[] { 'o' }, 0, 1);

            char[] data = new char[1024];

            _port.Read(data, 0, 1);

            if (data == null)
            {
                Console.WriteLine("The sync board stopped responding!");
                return false;
            }

            if (data[0] == 'y')
                return true;
            else
            {
                return false;
            }
        }

        public static string FindSyncboard()
        {
            // Check com port connectivity
            var portName = "NOT_FOUND";
            foreach (var port in SyncBoard.AvailablePortNames)
            {
                Console.WriteLine(port);
                var sb = new SyncBoard(port);
                sb.Timeout = 1000;
                sb.Open();
                if (!sb.QueryIsSyncBoardConnected()) continue;

                portName = port;
                sb.Close();
            }
            if (portName == "NOT_FOUND")
                Debug.WriteLine("Sync board not found");

            return portName;
        }
    }
}
