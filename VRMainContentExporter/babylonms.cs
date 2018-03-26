/* !!!!!!Need to compile with "allow unsafe" code in project build options!!!
 * 
 * Any others:
 * The ship has uniq identifier UUID
 * 
 * command line parameters
 * 
 * 0. used pipename
 * 1. Called UUID , this need to be same than own ship UUID
 * 
 * if only 1 parameter then calledUUID = executable file name
 * 
 * Suggest choice pipename exactly like UUID 
 *
 * If no parameter then write help and exit
 
     https://docs.google.com/document/d/1AaLC5kUbeizOW730QE3NK-NNb6GpgpdfWzwgjiq35eQ/edit#bookmark=id.tytknolw0e4s
     
     PACK

        00-10 HDR        
        11-14 LENGTH                                //Exclude Hdr                                   0..3
        15    VERSION                                                                           //  4
        16    COMPRESSFORMAT                        //0=CFNone, 1=CFZip                             5
        17    ATTRIBS                               //bits  0. 0=once 1=continous                   6..7                                 
            0.  0= normal 1 = Separate Request (Exit, undock)
            1.  0=once, 1= Continuous 
            2.  0= Normal 1=Testmode (need send information to partner)
            3.  1 = send UUID
            4.  AutoSendOuputpack if connected
        
        19-22    MINTIME               (4byte in ms)                                                8..11                
                                                    //if the transfer packet ready lower than mintime then wait for mintime
                                                    //mintime counter zero when this packet received or after last packet sended
                                                    //default  = 0 = off
        
        23    FIELDSCOUNT                           2byte                                          //  12..13
        
        25....                                      IntegerArray
            FIELDTYPE(1)    CONST_FT_INT32               
            COUNT(1)        (1byte)                  1..255
            FIELDDATA       (4*count)

            FIELDTYPE(1)    CONST_FT_DOUBLE            
            COUNT(1)        (1byte)                  1..255
            FIELDDATA       (8*COUNT)

            FIELDTYPE(1)    CONST_FT_BYTE            
            COUNT(4)        (4byte)
            FIELDDATA       (COUNT)
            
Special field types

            FIELDTYPE       CONST_FT_NAME                    //Fields names if requested
            FIELDDATA       (CONST_FIELDNAME_LEN*FIELDSCOUNT)        (10*)
            ...

            FIELDTYPE       CONST_FT_UUID                    //TODO Not implemented  own UUID
            FIELDDATA       (16byte)
        

 */



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BabylonMS
{

    public class BabylonMS
    {

        #region Varibles, Consts
        [DllImport("Shell32.dll")]
        public static extern int ShellExecuteA(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirecotry, int nShowCmd);

        public const byte CONST_VERSION = 1;
        public const string CONST_MARKER = "Prelimutens";
        public const byte CONST_FIELDNAME_LEN = 10;

        public const UInt16 CONST_Separate = 1;//0.bit
        public const UInt16 CONST_Continous = 2;//1.bit
        public const UInt16 CONST_Test = 4;//2.bit
        public const UInt16 CONST_AutosendOutputpack = 16;
        public const UInt16 CONST_HasFieldNames = 32;  //if last field is NAME type and has field's names

        public const int CONST_StreamLiveTimeMS = 30000; // namedpipe after 2 mins shutdown
        public const uint CONST_PingCarbage = 0xffaa8811; //zaj, szemét, trash, noise for testing or live for channel this noise not occours packet loss
        public const bool CONST_StreamLiveEnable = false; //stream PING ?

        public const byte CONST_CF_None = 0;
        public const byte CONST_CF_Zip = 1;

        public const byte CONST_FT_INT8 = 0;  //1  C# bitorder
        public const byte CONST_FT_INT16 = 1;  //2  C# bitorder
        public const byte CONST_FT_INT32 = 2;  //4  C# bitorder
        public const byte CONST_FT_INT64 = 3;  //8  C# bitorder
        public const byte CONST_FT_FLOAT = 4; //4
        public const byte CONST_FT_DOUBLE = 5; //8
        public const byte CONST_FT_BYTE = 6; //x byte
        public const byte CONST_FT_UUID = 7; //x byte
        public const byte CONST_FT_NAME = 8; //x byte
        public static byte CONST_FT_COUNT = CONST_FT_NAME + 1;  //max fields count
        public static byte CONST_FT_END = CONST_FT_COUNT;  //max fields count

        public const int CONST_STARTSHIP_SUCCESS = 0;
        public const int CONST_STARTSHIP_ERROR = -1;
        public const int CONST_STARTSHIP_NOTFOUND = -2;
        public const int CONST_STARTSHIP_FOUNDPIPE = -3;
        public const int CONST_STARTSHIP_UNEXPEXTEDUUID = -4;

        private const int CONST_SHIPDOCKINGEXIT_SUCCESS = 0;
        private const int CONST_SHIPDOCKINGEXIT_FEWPARAMS = 1;
        private const int CONST_SHIPDOCKINGEXIT_ERROR = 2;

        public const bool DEBUG_WriteConsole = false;
        public const bool DEBUG_WithoutStartShip = false;

        public delegate void BMSEventHandler(BMSEventSessionParameter session);
        public int WaitBytesMS = 10;


        Semaphore lock6 = new Semaphore(1, 1);
        public event BMSEventHandler Waitbytes;
        public virtual void OnWaitbytes(BMSEventSessionParameter session)
        {
            lock6.WaitOne();
            BMSEventHandler handler = Waitbytes;
            if (handler != null)
            {
                try
                {
                    handler(session);
                } catch (Exception )
                {

                }
            }
            if (CONST_StreamLiveEnable)
            {
                if ((session != null) && (session.writer != null) && (session.IsNeedPing(CONST_StreamLiveTimeMS)))
                {
                    session.writelock.WaitOne(); //ne másik írás közben
                    session.writer.Write(CONST_PingCarbage); /// this is a garbage but filtered in "readinput.."
                    session.writelock.Release();
                    session.Ping();
                }
            }
            lock6.Release();
        }
        
        //public event BMSEventHandler ClientConnected;
        public event BMSEventHandler Connected;
        protected virtual void OnClientConnected(BMSEventSessionParameter session)
        {
            try
            {
                session.writer.WriteLine(ShipUUID);  //client first things send own (ship) UUID with stringline
                session.writer.Flush();
            }
            catch (Exception ) {
                if (DEBUG_WriteConsole) Console.WriteLine("ErrWriteLine");
            }
            BMSEventHandler handler = Connected;
            if (handler != null)
            {
                try { 
                    handler(session);
                }
                catch (Exception )
                {

                }
            }
        }
        public event BMSEventHandler ServerWaitConnection;
        protected virtual void OnServerWaitConnection(BMSEventSessionParameter session)
        {
            BMSEventHandler handler = ServerWaitConnection;
            if (handler != null)
            {
                try { 
                handler(session);
                } catch (Exception ) { }
            }
        }
        //public event BMSEventHandler ServerConnected;
        protected virtual void OnServerConnected(BMSEventSessionParameter session)
        {
            BMSEventHandler handler = Connected;
            if (handler != null)
            {
                try { 
                handler(session); //transferpacket not accepted in Connected event!
                }
                catch (Exception ) { }

            }
        }
        public event BMSEventHandler Disconnected;
        protected virtual void OnDisconnected(BMSEventSessionParameter session)
        {
            BMSEventHandler handler = Disconnected;
            if (handler != null)
            {
                try { 
                handler(session);
                }
                catch (Exception ) { }

            }
        }
        //public delegate void NewInputFrameEventHandler(String shipUUID, BMSPack pack, StreamReader reader, StreamWriter writer);
        //public event NewInputFrameEventHandler NewInputFrame;
        public event BMSEventHandler NewInputFrame;
        protected virtual void OnNewInputFrame(BMSEventSessionParameter session)
        {
            if (DEBUG_WriteConsole) Console.WriteLine("Packet received.");
            BMSEventHandler handler = NewInputFrame;
            if (handler != null)
            {
                try { 
                handler(session);
                }
                catch (Exception ) { }

            }
        }

        /*
        //public delegate void NewInputNetworkFrameEventHandler(String shipUUID, BMSPack pack, TcpClient client, StreamReader reader, StreamWriter writer);
        //public event NewInputNetworkFrameEventHandler NewInputNetworkFrame;
        public event BMSEventHandler NewInputNetworkFrame;
        protected virtual void OnNewInputNetworkFrame(BMSEventSessionParameter session)
        {
            if (DEBUG_WriteConsole) Console.WriteLine("Packet received.");
            BMSEventHandler handler = NewInputNetworkFrame;
            if (handler != null)
            {
                try { 
                handler(session);
                }
                catch (Exception handlerexcept) { }

            }
        }
        */

        private bool UnexectedShipDocked = false;
        public bool IsReady = false;
        public delegate void ServerReadyEventHandler(BMSEventSessionParameter session);
        public event ServerReadyEventHandler ServerReadyForTransfer;
        protected virtual String OnServerReadyForTransfer(BMSEventSessionParameter session)
        {
            string shipUUID = "";
            UnexectedShipDocked = false;
            try {
                shipUUID = session.reader.ReadLine();
                session.reader.DiscardBufferedData();
                if (this.ShipUUID.CompareTo(shipUUID) != 0)
                {
                    // A ship azonos\t=ja nem egyezik meg az elv'rt azonos\t=val, szétkapcsolás
                    UnexectedShipDocked = true;
                    if (networked)
                    {
                        NET.client.Close();
                    }
                    else
                    {
                        server.Close();
                        serverWriter.Close();
                    }
                    if (DEBUG_WriteConsole) Console.WriteLine("Unexpected Ship will separate!");
                } else
                {
                    if (DEBUG_WriteConsole) Console.WriteLine("Packet ready to transfer.");
                    session.writer.WriteLine(StationUUID);  //Connect publish my ID
                    session.writer.Flush();
                }

            }
            catch (Exception )
            {
                if (DEBUG_WriteConsole) Console.WriteLine("ErrReadLine");
            }
            if (!UnexectedShipDocked) // ha minden rendben akkor a user is dolgozhat...
            {
                IsReady = true;
                if (session.outputPack.IsAutosendOutputpack())        //TODO nem az inputpack???
                    session.writer.Write(session.outputPack.getPack(false));  //TODO nem az inputpack???
                ServerReadyEventHandler handler = ServerReadyForTransfer;
                if (handler != null)
                {
                    try { 
                    handler(session);
                    }
                    catch (Exception ) { }

                }
            }
            return shipUUID;
        }

        public object Tag;

        //StreamReader reader;
        //StreamWriter writer;
        //public BMSPack inputpack;
        //public BMSPack outputpack;
        string PipeName;
        string ShipUUID;  //app UUID
        string StationUUID;  //station UUID

        /// <summary>
        /// TCP IP Address for server or client
        /// </summary>
        string IP = "";
        /// <summary>
        /// Port number for network connection server or client side
        /// </summary>
        int PORT = 0;
        /// <summary>
        /// Socket communication over IP for this instance
        /// </summary>
        bool networked = false;
        Network NET;

        #endregion

        public BabylonMS(string pipeName, string shipUUID, string stationUUID)
        {
            this.StationUUID = stationUUID;
            this.ShipUUID = shipUUID;
            //inputpack = new BMSPack();
            //outputpack = new BMSPack();
            this.PipeName = pipeName;
        }

        public static byte[] toUUID128(String UUID)
        {
            byte[] FromHexString(string hexString)
            {
                byte[] bytes = new byte[hexString.Length / 2];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }

                return bytes; // returns: "Hello world" for "48656C6C6F20776F726C64"
            }
            byte[] uuid = FromHexString(UUID.Replace("-", ""));
            return uuid;
        }
        public static String UUID128ToString(byte[] UUID)
        {
            string result = BitConverter.ToString(UUID).Replace("-", "");
            return result;
        }
        public static String UUID128ToString(byte[] UUID,int sfrom)
        {
            byte[] ui = new byte[16];
            Buffer.BlockCopy(UUID, sfrom, ui, 0, 16);
            string result = BitConverter.ToString(ui).Replace("-", "");
            return result;
        }
        public static bool compareUUID(string UUID1, string UUID2)
        {
            try
            {
                byte[] a = toUUID128(UUID1);
                byte[] b = toUUID128(UUID2);
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] != b[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ) {
                return false;
            }
        }

        //args[0] = pipename
        /// <summary>
        /// ShipDocking Namedpipe (client) solution and after need OpenGate.
        /// </summary>
        /// <param name="shipUUID"></param>
        /// <param name="args">0 = pipename</param>
        /// <returns></returns>
        public static BabylonMS ShipDocking(string shipUUID, string[] args)
        {
            BabylonMS bms = null;
            Console.WriteLine(shipUUID);
            Console.WriteLine("---");
            if (args.Length < 1)
            {
                Environment.Exit(CONST_SHIPDOCKINGEXIT_FEWPARAMS);
                return bms;
            }
            bms = new BabylonMS(args[0], shipUUID, null); //stationUUID will fill with connection
            if (bms == null)
            {
                Environment.Exit(CONST_SHIPDOCKINGEXIT_ERROR);
            }
            return bms;
        }

        /// <summary>
        /// ShipDocking NET (server) solution and after need OpenGate.
        /// </summary>
        /// <param name="ip">Connect to </param>
        /// <param name="port">with port</param>
        /// <param name="shipUUID">Own UUID what can reject from miniship if not accepted</param>
        /// <returns>BabylonMS</returns>
        public static BabylonMS ShipDocking(string ip, int port, string shipUUID)
        {
            BabylonMS bms = null;
            Console.WriteLine(shipUUID);
            Console.WriteLine("---");
            bms = new BabylonMS(null, shipUUID, null); //stationUUID will fill with connection
            bms.networked = true;
            bms.IP = ip;
            bms.PORT = port;
            if (bms == null)
            {
                Environment.Exit(CONST_SHIPDOCKINGEXIT_ERROR);
            }
            return bms;
        }
        public static int OpenWithStartInfo(out Process process, string filename, string pipename)
        {
            process = null;
            string startpath = Directory.GetCurrentDirectory();
            String[] listOfPipes = System.IO.Directory.GetFiles(@"\\.\pipe\");
            string found = Array.Find(listOfPipes, element => element.EndsWith(pipename));
            if (found != null)
            {
                return CONST_STARTSHIP_FOUNDPIPE;
            }
            string path = startpath + '\\';
            process = new Process();
            process.StartInfo.Arguments = pipename;  //argu> pipename shipUUID stationUUID
            process.StartInfo.WorkingDirectory = startpath;
            if (File.Exists(path + filename))
            {
                process.StartInfo.FileName = path + filename;
            } else
            {
                if (File.Exists(path + pipename))
                {
                    process.StartInfo.FileName = path + pipename;
                } else
                {
                    //TODO From network
                    return CONST_STARTSHIP_NOTFOUND;
                }
            }
            process.StartInfo.UseShellExecute = false;
            process.EnableRaisingEvents = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;//TODO ???? SURE <<????
            //process.Exited += OnProcessExited;
            bool res = process.Start();
            if (res)
            {
                return CONST_STARTSHIP_SUCCESS;
            } else
            {
                return CONST_STARTSHIP_ERROR;
            }
        }
        /*
        private static void OnProcessExited(object sender, EventArgs e)
        {
            Console.WriteLine("ProcessExited");
        }
        */

        public static int OpenWithStartInfo(string filename, string pipename)
        {
            Process process;
            return OpenWithStartInfo(out process, filename, pipename);
        }
        /// <summary>
        /// Start APP and connect to started APP with access his process. After need PrepareGate
        /// </summary>
        /// <param name="process">APP process</param>
        /// <param name="appname">APP name (....exe)</param>
        /// <param name="pipename">Used PipeName usually right the calledUUID </param>
        /// <param name="calledUUID">APP UUID</param>
        /// <param name="callerUUID">This UUID</param>
        /// <returns>BabylonMS</returns>
        public static BabylonMS LaunchMiniShip(out Process process, string appname, string pipename, string calledUUID, string callerUUID)
        {
            if (DEBUG_WithoutStartShip ||
                (OpenWithStartInfo(out process, appname, pipename) == CONST_STARTSHIP_SUCCESS))
            {
                BabylonMS bms = new BabylonMS(pipename, calledUUID, callerUUID);
                bms.networked = false;
                return bms;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Start APP and connect to started APP. After need PrepareGate
        /// </summary>
        /// <param name="appname">APP name (....exe)</param>
        /// <param name="pipename">Used PipeName usually right the calledUUID </param>
        /// <param name="calledUUID">APP UUID</param>
        /// <param name="callerUUID">This UUID</param>
        /// <returns>BabylonMS</returns>
        public static BabylonMS LaunchMiniShip(string appname, string pipename, string calledUUID, string callerUUID)
        {
            if (DEBUG_WithoutStartShip ||
                (OpenWithStartInfo(appname, pipename) == CONST_STARTSHIP_SUCCESS))
            {
                BabylonMS bms = new BabylonMS(pipename, calledUUID, callerUUID);
                bms.networked = false;
                return bms;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Connect to started APP. After need PrepareGate
        /// </summary>
        /// <param name="pipename">Used PipeName usually right the calledUUID </param>
        /// <param name="calledUUID">APP UUID</param>
        /// <param name="callerUUID">This UUID</param>
        /// <returns>BabylonMS</returns>
        public static BabylonMS LaunchMiniShip(string pipename, string calledUUID, string callerUUID)
        {
            BabylonMS bms = new BabylonMS(pipename, calledUUID, callerUUID);
            bms.networked = false;
            return bms;
        }
        /// <summary>
        /// Prepare to connect as a client to an early started NET APP. After need PrepareGate.
        /// </summary>
        /// <param name="ip">NET address for APP to connect</param>
        /// <param name="port">Port number</param>
        /// <param name="pipename">Used PipeName usually right the calledUUID </param>
        /// <param name="calledUUID">APP UUID</param>
        /// <param name="callerUUID">This UUID</param>
        /// <returns>BabylonMS</returns>
        public static BabylonMS LaunchMiniShip(string ip, int port, string pipename, string calledUUID, string callerUUID)
        {
            BabylonMS bms = new BabylonMS(pipename, calledUUID, callerUUID);
            bms.networked = true;
            bms.IP = ip;
            bms.PORT = port;
            return bms;
        }
        /// <summary>
        /// After LaunchMiniShip (and maybe start APP) prepare to connect annother method (NET) instead of NamedPipe        
        /// </summary>
        /// <param name="ip">APP Network Address</param>
        /// <param name="port">Port number</param>
        public void ChangeMiniShipToNetwork(string ip, int port)
        {
            networked = true;
            IP = ip;
            PORT = port;
        }
        //return true if succesfull
        Semaphore transfer = new Semaphore(1, 1);
        public bool TransferPacket(StreamWriter writer, BMSPack outputpack, bool hasFieldNames)
        {
            //transfer.WaitOne();
            bool b = TransferPacket(writer, outputpack, hasFieldNames, transfer);
            //transfer.Release();
            return b;
        }
        //ThreadUnsafe need semaphore maybe static Semaphore transferStatic = new Semaphore(1, 1);
        // Unsafe because the caller define the sensitive points
        static public bool TransferPacket(StreamWriter writer, BMSPack pack, bool hasFieldNames, Semaphore sema)
        {
            if (sema != null) sema.WaitOne();
            MemoryStream mem = pack.getPack(hasFieldNames);
            if (DEBUG_WriteConsole) Console.Write("Transfer Packet");
            try
            {
                mem.Position = 0;
                mem.WriteTo(writer.BaseStream);
                writer.BaseStream.Flush();
                if (DEBUG_WriteConsole) Console.Write(" created");
                /*
                if (client == null)
                {
                    //server.WaitForPipeDrain();
                }
                else
                {
                    //client.WaitForPipeDrain();
                }
                */
                if (DEBUG_WriteConsole) Console.Write(",transferred.");
                return true;
            }
            catch (Exception e)
            {
                if (DEBUG_WriteConsole) Console.WriteLine(" but critical " + e.Message);
                return false;
            }
            finally
            {
                if (sema != null) sema.Release();
            }
        }

        NamedPipeClientStream client;
        NamedPipeClientStream clientWriter;
        Semaphore newframe = new Semaphore(1, 1);
        bool exit = false;
        /// <summary>
        /// OpenGate after ShipDocking. NET (server) or NamedPipe (client)
        /// </summary>
        /// <param name="blocking">
        /// <para>if NETShipDocking and True then blocking</para>
        /// <para>the Namedpipe not use this parameter</para>
        /// </param>
        public void OpenGate(bool blocking)
        {
            if (networked)
            {
                OpenGateInternal(blocking);
            }
            else
            {
                if (blocking)
                {
                    OpenGateInternal(blocking);
                }
                else
                {
                    Task t2 = Task.Factory.StartNew(() =>
                    {
                        OpenGateInternal(blocking);
                    });
                }
            }
        }
        /// <summary>
        /// <para>OpenGate after ShipDocking. NET (server) or NamedPipe (client) </para>
        /// <para>(Blocking if NET ShipDocking</para>
        /// <para>Otherwise no effect (like namedpipe solution)</para>
        /// </summary>
        public void OpenGate() { OpenGate(true); }


        private void OpenGateInternal(bool blocking)
        {
            if (MODE != 0) return;
            MODE = MODE_OpenGate;
            if (DEBUG_WriteConsole) Console.Write("Open gate, ");
            exit = false;
            while (!exit)
            {
                if (networked)
                {
                    NET = new Network(Network.CONST_NetworkType_Server, IP, PORT); //more instance
                    if (NET != null)
                    {
                        //   bool conn = false;
                        NET.Disconnected += (client, stream, arg) =>
                        {
                            newframe.WaitOne();
                            OnDisconnected(null);
                            newframe.Release();
                        };
                        NET.Connected += (client, stream) =>
                        {
                            ///THREAD----------------------------------------------------------------------------------------------
                            Task.Factory.StartNew(new Action<object>((currentclient) =>
                            {
                                TcpClient curClient = (TcpClient)currentclient;
                                bool exit = false;

                                BMSEventSessionParameter session = new BMSEventSessionParameter(this);
                                session.client = curClient;
                                session.reader = new StreamReader(curClient.GetStream()); 
                                session.writer = new StreamWriter(curClient.GetStream());

                                OnClientConnected(session);
                                //String stationUUID = session.reader.ReadLine();
                                //session.shipUUID = stationUUID; //Todo ?? de lehet hog ykellene egy külön?
                                session.shipUUID = session.reader.ReadLine();

                                if (DEBUG_WriteConsole) Console.WriteLine(" and object docked " + session.shipUUID);
                                while (!exit)
                                {
                                    if (readInputToInputPack(session))
                                    {
                                        //UnCompress(session.inputPack);
                                        exit = session.inputPack.IsSeparate();
                                        newframe.WaitOne();                                        
                                        OnNewInputFrame(session);
                                        //OnNewInputNetworkFrame(session);
                                        newframe.Release();
                                        session.inputPack.WaitMinTime(Waitbytes);
                                        //client.WaitForPipeDrain();
                                        if (!session.inputPack.IsOnce())
                                        {
                                            //TODO continuous on new thread   
                                            ///THREAD----------------------------------------------------------------------------------------------                                          
                                            Task t2 = Task.Factory.StartNew(new Action<object>((currentclient2) =>
                                            {
                                                TcpClient curClient2 = (TcpClient)currentclient2;
                                                bool run = true;
                                                while (run)   //A readinputpack-ot lejjebb ciklus intézi ha jön de mivel isOnce false ezért folyamatosan küldök anyagot az eredeti inputpack-al
                                                {
                                                    if (DEBUG_WriteConsole) Console.WriteLine("Begin1");
                                                    readbufferFill.WaitOne();
                                                    newframe.WaitOne();
                                                    OnNewInputFrame(session);
                                                    //OnNewInputNetworkFrame(session);
                                                    newframe.Release();
                                                    //client.WaitForPipeDrain();
                                                    run = !session.inputPack.IsOnce() && !session.inputPack.IsSeparate();
                                                    readbufferFill.Release();
                                                    if (DEBUG_WriteConsole) Console.WriteLine("End1");
                                                    session.inputPack.WaitMinTime(Waitbytes);
                                                }
                                            }), curClient);
                                            while (!exit && !t2.IsCompleted)
                                            {
                                                if (DEBUG_WriteConsole) Console.WriteLine("Async read");
                                                if (readInputToInputPack(session))
                                                {
                                                    if (DEBUG_WriteConsole) Console.WriteLine("Async read success");
                                                    //UnCompress(session.inputPack);
                                                    exit = session.inputPack.IsSeparate();
                                                    if (t2.IsCompleted)
                                                    { //ha épp kilép az IsOnce ciklusból akkor az első bejött packetet itt kell feldolgozni
                                                      //aztán megy a főciklusba
                                                        OnNewInputFrame(session);
                                                        //OnNewInputNetworkFrame(session);
                                                    }
                                                    session.inputPack.WaitMinTime(Waitbytes);
                                                }
                                                else
                                                {
                                                    exit = true;
                                                }
                                            }
                                        }
                                        else
                                        {

                                        }
                                    }
                                    else
                                    {
                                        exit = true;
                                    }
                                }
                                client.Close();
                            }), client);
                        };

                        while (blocking)  //TODO Nincs ;rtelmezve a kil;p;s hiszen a kliens nem l;ptetheti ki mert nem is ő léptette be
                        {
                            Thread.Sleep(500);
                        }
                        exit = true;
                    };
                }
                else
                {
                    try
                    {   //http://suriyanto.blogspot.hu/2007/12/idle-timeout-for-named-pipe-binding.html
                        BMSEventSessionParameter session = new BMSEventSessionParameter(this);
                        client = new NamedPipeClientStream(".", PipeName + 'W', PipeDirection.InOut);    
                        session.reader = new StreamReader(client);
                        clientWriter = new NamedPipeClientStream(".", PipeName + 'R', PipeDirection.InOut);
                        session.writer = new StreamWriter(clientWriter);
                        if (!client.IsConnected)
                        {
                            client.Connect(60000);
                        }
                        if (!clientWriter.IsConnected)
                        {
                            clientWriter.Connect(60000);
                        }
                        OnClientConnected(session);
                        if (client.IsConnected)
                        {
                            String stationUUID = session.reader.ReadLine();
                            session.shipUUID = stationUUID; //Todo ?? de lehet hog ykellene egy külön?
                            if (DEBUG_WriteConsole) Console.WriteLine(" and object docked " + stationUUID);
                            while (!exit)
                            {
                                if (readInputToInputPack(session))
                                {
                                    //UnCompress(session.inputPack);
                                    exit = session.inputPack.IsSeparate();
                                    OnNewInputFrame(session);
                                    session.inputPack.WaitMinTime(Waitbytes);
                                    //client.WaitForPipeDrain();
                                    if (!session.inputPack.IsOnce())
                                    {
                                        //TODO continuous on new thread     
                                        ///THREAD----------------------------------------------------------------------------------------------                                      
                                        Task t2 = Task.Factory.StartNew(() =>
                                        {                                            
                                            bool run = true;
                                            while (run) //a readInputpack-ot a későbbi ciklus vézgi nem kell mivel folyamatosan küldöm az anyagokat
                                            {
                                                //Console.WriteLine("Begin1");
                                                readbufferFill.WaitOne();
                                                OnNewInputFrame(session);
                                                //client.WaitForPipeDrain();
                                                run = !session.inputPack.IsOnce() && !session.inputPack.IsSeparate();
                                                readbufferFill.Release();
                                                //Console.WriteLine("End1");
                                                session.inputPack.WaitMinTime(Waitbytes);

                                            }                                            
                                            Console.WriteLine("Exit from inner thread circle.");
                                        });
                                        while (!exit && !t2.IsCompleted)
                                        {
                                            if (DEBUG_WriteConsole) Console.WriteLine("Async read");
                                            if (readInputToInputPack(session))
                                            {
                                                if (DEBUG_WriteConsole) Console.WriteLine("Async read success");
                                                //UnCompress(session.inputPack);
                                                exit = session.inputPack.IsSeparate();
                                                if (t2.IsCompleted)
                                                { //ha épp kilép az IsOnce ciklusból akkor az első bejött packetet itt kell feldolgozni
                                                    //aztán megy a főciklusba
                                                    OnNewInputFrame(session);
                                                }

                                                //session.inputPack.WaitMinTime(Waitbytes);
                                            }
                                            else
                                            {
                                                exit = true;
                                            }
                                        }
                                        Console.WriteLine("t2.iscompleted");
                                    }
                                    else
                                    {

                                    }
                                }
                                else
                                {
                                    exit = true;
                                }
                            }
                        }

                    }
                    catch (Exception )
                    {

                    }
                    client.Close();
                    clientWriter.Close();
                }
            }

        }



        NamedPipeServerStream server;
        NamedPipeServerStream serverWriter;
        public NamedPipeServerStream getWriterServer()
        {
            return serverWriter;
        }
        public NamedPipeServerStream getReaderServer()
        {
            return server;
        }

        public void Disengage()
        {
            if (MODE == 0) return;
            try
            {
                if (networked)
                {
                    if (MODE == MODE_PrepareGate)
                    {
                        NET.client.Close();
                    } else
                    {
                        NET.server.Stop();
                    }
                } else
                {
                    if (MODE == MODE_PrepareGate)
                    {
                        server.Close();
                        serverWriter.Close();
                    }
                    else
                    {
                        client.Close();
                        clientWriter.Close();
                    }
                }
                MODE = 0;
            }
            catch (Exception ) { }
        }

        private const byte MODE_PrepareGate = 1;
        private const byte MODE_OpenGate = 2;
        private int MODE = 0;
        Semaphore sema3 = new Semaphore(1, 1);
        /// <summary>
        /// After LaunchMiniShip. Not blocking. <para>If NET then Client mode. </para>
        /// <para>If NamedPipe then Server mode (only 1 client can connect).</para>
        /// <para>Pipe: ServerWaitConnection,ServerReadyForTransfer,Connected,NewInputFrame,Disconnected, Waitbytes </para>
        /// <para>NET: ServerReadyForTransfer,Connected,NewInputFrame,Disconnected, Waitbytes</para>
        /// </summary>
        public void PrepareGate()
        {
            if (MODE != 0) return; //only once in one instance
            MODE = MODE_PrepareGate;
///THREAD----------------------------------------------------------------------------------------------
            Task.Factory.StartNew(() =>
            {
                bool shipSeparated;
                exit = false;
                while (!exit)
                {
                    if (networked)
                    {                        
                        NET = new Network(Network.CONST_NetworkType_Client, IP, PORT);
                        if (NET != null)
                        {
                            BMSEventSessionParameter session = new BMSEventSessionParameter(this);
                            shipSeparated = false;
                            bool conn = false;
                            NET.Disconnected += (o, stream, arg) =>
                            {
                                OnDisconnected(session);
                                shipSeparated = true;
                                NET = null;
                            };
                            NET.Connected += (o, stream) =>
                            {
                                session.reader = new StreamReader(stream);
                                session.writer = new StreamWriter(stream);
                                OnServerConnected(session);
                                conn = true;
                            };
                            while ((!conn) && (!shipSeparated)) //WaitforConnection
                            {
                                Thread.Sleep(50);
                            }
                            if (!shipSeparated)
                            {
                                String UUID = OnServerReadyForTransfer(session);   //reader.ReadLine();
                                session.shipUUID = UUID;
                                if (!UnexectedShipDocked)
                                {
                                    while ((!shipSeparated) && (NET.client.Connected))
                                    {
                                        try
                                        {
                                            if (readInputToInputPack(session))
                                            {
                                                //UnCompress(session.inputPack);
                                                sema3.WaitOne();
                                                OnNewInputFrame(session);
                                                sema3.Release();
                                                session.inputPack.WaitMinTime(Waitbytes);
                                            }
                                            else
                                            {
                                                shipSeparated = true;
                                            }
                                        }
                                        catch (Exception )
                                        {
                                            shipSeparated = true;
                                            IsReady = false;
                                        }
                                    }
                                }
                            }
                            exit = true;
                            IsReady = false;
                            shipSeparated = true;
                            //NET.Stop();
                        }
                    }
                    else
                    {
                        try
                        {
                            BMSEventSessionParameter session = new BMSEventSessionParameter(this);
                            shipSeparated = false;
                            server = new NamedPipeServerStream(PipeName + 'R', PipeDirection.InOut, 1, PipeTransmissionMode.Byte);                            
                            session.reader = new StreamReader(server);
                            serverWriter = new NamedPipeServerStream(PipeName + 'W', PipeDirection.InOut, 1, PipeTransmissionMode.Byte);
                            session.writer = new StreamWriter(serverWriter);
                            OnServerWaitConnection(session);
                            server.WaitForConnection();
                            serverWriter.WaitForConnection();
                            String UUID = OnServerReadyForTransfer(session);              //readline
                            OnServerConnected(session);  //for compatibility only with NET configuration UUID already usable
                            session.shipUUID = UUID;
                            if (!UnexectedShipDocked)
                            {
                                while (!shipSeparated)
                                {
                                    if (readInputToInputPack(session))
                                    {
                                        //UnCompress(session.inputPack);
                                        OnNewInputFrame(session);
                                        session.inputPack.WaitMinTime(Waitbytes);
                                        //server.WaitForPipeDrain();
                                    }
                                    else
                                    {
                                        shipSeparated = true;
                                    }
                                }
                            }
                            OnDisconnected(session);
                            server.Close();
                            serverWriter.Close();
                        }
                        catch (Exception ) { }
                    }
                }
            });
            if (DEBUG_WriteConsole) Console.WriteLine("Gate prepared " + PipeName);
        }


        public static bool compareMarker(byte[] buffer, int index)
        {
            for (int i = 0; i < CONST_MARKER.Length; i++)
            {
                if (buffer[i + index] != CONST_MARKER[i]) return false;
            }
            return true;
        }

        byte[] UnCompress(byte[] buffer)
        {

            return new byte[0];
        }


        Semaphore readbufferFill = new Semaphore(1, 1);
        // return= true if success and false if not
        //bool readInputToInputPack(BMSPack inputpack, StreamReader reader)
        bool readInputToInputPack(BMSEventSessionParameter session)
        {
            BMSPack inputpack = session.inputPack;
            //StreamReader reader = session.reader;
            try
            {
                byte[] buffer = new byte[512];
                if (!Fill(session.reader.BaseStream, buffer, CONST_MARKER.Length,session)) return false;
                if (DEBUG_WriteConsole) Console.WriteLine("Begin2-Waitone");
                readbufferFill.WaitOne();
                if (DEBUG_WriteConsole) Console.WriteLine("Begin2-ReadInput");
                //header.WaitOne();
                //OnNewInputHeaderDetected(new EventArgs());            
                while (!compareMarker(buffer, 0))
                {
                    Buffer.BlockCopy(buffer, 1, buffer, 0, CONST_MARKER.Length - 1);
                    buffer[CONST_MARKER.Length - 1] = (byte)session.reader.BaseStream.ReadByte();
                }
                //!!!!!!!!!!!found marker
                if (!Fill(session.reader.BaseStream, buffer, BMSPack.HEADERBYTENUM,session)) return false;

                if (inputpack != null) //TODO Biztos ???
                {
                    inputpack.Clear();
                } else
                {
                    inputpack = new BMSPack();
                }
                int length = BitConverter.ToInt32(buffer, 0);
                inputpack.Version = buffer[4];
                inputpack.CompressFormat = buffer[5];
                inputpack.Attribs = BitConverter.ToUInt16(buffer, 6);
                inputpack.setMinTime(BitConverter.ToUInt32(buffer, 8));
                int fieldscount = BitConverter.ToUInt16(buffer, 12);

                length = length - (BMSPack.HEADERBYTENUM - 4);
                if (buffer.Length < length) buffer = new byte[length];
                if (!Fill(session.reader.BaseStream, buffer, length,session)) return false;

                //minden adat itt van 
                
                if (inputpack.CompressFormat == BabylonMS.CONST_CF_Zip)
                {
                    //TODO uncompress to new buffer NOT TESTED
                    ////BUFFER [0] elemétől a végéig tömörített adat
                    buffer = unzipper.unzip(buffer);
                    ///mostm'r a végéig uncompressed
                }

                int c = 0;
                byte Type;
                //length new 

                float FLOAT;
                int INTEGER;
                Int64 INT64;
                double DOUBLE;
                BMSField field = null;
                for (int i = 0; i < fieldscount; i++)
                {
                    Type = buffer[c];
                    switch (Type)
                    {
                        case CONST_FT_INT8:
                            length = (byte)buffer[++c];
                            c++;
                            field = inputpack.AddField("", Type);
                            if (field != null)
                            {
                                for (int k = 0; k < length; k++)
                                {
                                    INTEGER = buffer[c];
                                    c += 1;
                                    field.Value((byte)INTEGER);
                                }
                            }
                            break;
                        case CONST_FT_INT16:
                            length = (byte)buffer[++c];
                            c++;
                            field = inputpack.AddField("", Type);
                            if (field != null)
                            {
                                for (int k = 0; k < length; k++)
                                {
                                    INTEGER = BitConverter.ToInt16(buffer, c);
                                    c += 2;
                                    field.Value((UInt16)INTEGER);
                                }
                            }
                            break;
                        case CONST_FT_INT32:
                            length = (byte)buffer[++c];
                            c++;
                            field = inputpack.AddField("", Type);
                            if (field != null)
                            {
                                for (int k = 0; k < length; k++)
                                {
                                    INTEGER = BitConverter.ToInt32(buffer, c);
                                    c += 4;
                                    field.Value(INTEGER);
                                }
                            }
                            break;
                        case CONST_FT_INT64:
                            length = (byte)buffer[++c];
                            c++;
                            field = inputpack.AddField("", Type);
                            if (field != null)
                            {
                                for (int k = 0; k < length; k++)
                                {
                                    INT64 = BitConverter.ToInt64(buffer, c);
                                    c += 8;
                                    field.Value(INT64);
                                }
                            }
                            break;
                        case CONST_FT_FLOAT:
                            length = (byte)buffer[++c];
                            c++;
                            field = inputpack.AddField("", Type);
                            if (field != null)
                            {
                                for (int k = 0; k < length; k++)
                                {
                                    FLOAT = BitConverter.ToSingle(buffer, c);
                                    c += 4;
                                    field.Value(FLOAT);
                                }
                            }
                            break;
                        case CONST_FT_DOUBLE:
                            length = (byte)buffer[++c];
                            c++;
                            field = inputpack.AddField("", Type);
                            if (field != null)
                            {
                                for (int k = 0; k < length; k++)
                                {
                                    DOUBLE = BitConverter.ToDouble(buffer, c);
                                    c += 8;
                                    field.Value(DOUBLE);
                                }
                            }
                            break;
                        case CONST_FT_UUID:
                            length = (byte)buffer[++c];
                            c++;
                            field = inputpack.AddField("", Type);
                            if (field != null)
                            {
                                for (int k = 0; k < length; k++)
                                {
                                    String s = BabylonMS.UUID128ToString(buffer,c);
                                    //field.GetStream().Write(buffer, c, 16);
                                    c += 16;
                                    field.ValueAsUUID(s);
                                }
                            }
                            break;
                        case CONST_FT_BYTE:
                            length = BitConverter.ToInt32(buffer, ++c); //data length
                            c += 4;
                            field = inputpack.AddField("", Type);
                            if (field != null)
                            {
                                field.GetStream().Write(buffer, c, length);
                            }
                            c += length;
                            break;
                        case CONST_FT_NAME:
                            length = fieldscount - 1;
                            c += 2;
                            for (int k = 0; k < length; k++)
                            {
                                String s = Encoding.ASCII.GetString(buffer, c, CONST_FIELDNAME_LEN);
                                inputpack.GetField(k).SetId(s);
                                c += CONST_FIELDNAME_LEN;
                            }
                            break;
                    }
                }
                readbufferFill.Release();
                if (DEBUG_WriteConsole) Console.WriteLine("End2");
                return true;
            } catch (Exception )
            {
                return false;
            }
        }

        public static bool FillSTA(Stream source, byte[] destination, int count)
        {
            int bytesRead, offset = 0;
            while (count > 0 &&
                  (bytesRead = source.Read(destination, offset, count)) > 0)
            {
                offset += bytesRead;
                count -= bytesRead;
            }
            return count == 0;
        }

        public bool Fill(Stream source, byte[] destination, int count, BMSEventSessionParameter session)
        {
            int bytesRead, offset = 0;
            while (count > 0 &&
                (bytesRead = sourceRead(source,destination, offset, count,session)) > 0)             
                //(bytesRead = source.Read(destination, offset, count)) > 0)
            {
                offset += bytesRead;
                count -= bytesRead;
            }
            return count == 0;
        }

        public int sourceRead(Stream source, byte[] destination, int offset, int count,BMSEventSessionParameter session)
        {
            ThreadClass2 param = new ThreadClass2(source, destination, offset, count, 0);
            ///THREAD----------------------------------------------------------------------------------------------
            Task.Factory.StartNew(new Action<object>((p) =>
            {
                ThreadClass2 p2 = (ThreadClass2)p;
                try
                {
                    
                    p2.bytesread = p2.source.Read(p2.destination, p2.offset, p2.count);
                }
                catch (Exception ) { }
                p2.exit = true;
            }), param);
            while (!param.exit)
            {
                Thread.Sleep(WaitBytesMS);
                OnWaitbytes(session);
            }
            return param.bytesread;
        }

    }

    class ThreadClass2
    {
        public Stream source;
        public byte[] destination;
        public int offset;
        public int count;
        public int bytesread;
        public bool exit;

        public ThreadClass2(Stream source, byte[] destination, int offset, int count, int bytesread)
        {
            exit = false;
            this.source = source;
            this.destination = destination;
            this.offset = offset;
            this.count = count;
            this.bytesread = bytesread;
        }
    }



    public class MiniShip
    {
        Socket client;
        string ShipUUID;
        public MiniShip(Socket client, string ShipUUID)
        {
            this.client = client;
            this.ShipUUID = ShipUUID;
        }
    }

    [Serializable]
    public class BMSField
    {
        string Id;
        private byte Type;        //Type of CONST_FT_...
        private MemoryStream stream = null;

        public BMSField(string Id, byte Type)
        {
            SetId(Id);
            this.Type = Math.Min(Type, (byte)(BabylonMS.CONST_FT_COUNT - 1));
            stream = new MemoryStream();
        }
        public MemoryStream GetStream() { return stream; }

        public int AtomLen()
        {
            int atomlen = 4;
            switch (Type)
            {
                case BabylonMS.CONST_FT_INT8: atomlen = 1; break;
                case BabylonMS.CONST_FT_INT16: atomlen = 2; break;
                case BabylonMS.CONST_FT_INT32: atomlen = 4; break;
                case BabylonMS.CONST_FT_INT64: atomlen = 8; break;
                case BabylonMS.CONST_FT_DOUBLE: atomlen = 8; break;
                case BabylonMS.CONST_FT_FLOAT: atomlen = 4; break;
                case BabylonMS.CONST_FT_BYTE: atomlen = 1; break;
                case BabylonMS.CONST_FT_UUID: atomlen = 16; break;
            }
            return atomlen;
        }


        /// <summary>
        /// Get field array element count
        /// </summary>
        /// <returns></returns>
        public int GetLengthInType()
        {
            return (int)(stream.Length / AtomLen());
        }
        public int Length()
        {
            return GetLengthInType();
        }


        public byte GetTypeOfField() { return Type; }
        public byte[] GetId()
        {
            byte[] a = Encoding.ASCII.GetBytes(Id);
            return a;
        }

        public void SetId(string Id)
        {
            if (Id.Length > BabylonMS.CONST_FIELDNAME_LEN)
            {
                this.Id = Id.Substring(0, BabylonMS.CONST_FIELDNAME_LEN);
            }
            else this.Id = Id + new String('\0', BabylonMS.CONST_FIELDNAME_LEN - Id.Length);
        }
        public bool MatchId(string anotherID)
        {
            byte[] a = Encoding.ASCII.GetBytes(anotherID);
            byte[] b = GetId();
            int la;
            for (la = 0; (la < a.Length) && (a[la] != 0); la++) { };
            int lb;
            for (lb = 0; (lb < b.Length) && (b[lb] != 0); lb++) { };
            if (la != lb) { return false; };
            for (int i = 0; i < la; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        public int Value(float value)
        {
            if (Type == BabylonMS.CONST_FT_FLOAT)
            {
                stream.Write(BitConverter.GetBytes(value), 0, 4);
                return 0;
            }
            else
                return -1;
        }
        public int Value(double value)
        {
            if (Type == BabylonMS.CONST_FT_DOUBLE)
            {
                stream.Write(BitConverter.GetBytes(value), 0, 8);
                return 0;
            }
            else
                return -1;
        }
        public int Value(byte value)
        {
            if (Type == BabylonMS.CONST_FT_INT8)
            {
                stream.Write(BitConverter.GetBytes(value), 0, 1);
                return 0;
            }
            else
                return -1;
        }
        public int Value(bool value)
        {
            if (Type == BabylonMS.CONST_FT_INT8)
            {
                if (value)
                {
                    stream.Write(BitConverter.GetBytes(1), 0, 1);
                }
                else
                {
                    stream.Write(BitConverter.GetBytes(0), 0, 1);
                }
                return 0;
            }
            else
                return -1;
        }
        public int Value(Int16 value)
        {
            return Value((UInt16)(value));
        }
        public int Value(UInt16 value)
        {
            if (Type == BabylonMS.CONST_FT_INT16)
            {
                stream.Write(BitConverter.GetBytes(value), 0, 2);
                return 0;
            }
            else
                return -1;
        }
        public int Value(int value)
        {
            if (Type == BabylonMS.CONST_FT_INT32)
            {
                stream.Write(BitConverter.GetBytes(value), 0, 4);
                return 0;
            }
            else
                return -1; 
        }
        public int Value(Int64 value)
        {
            return Value((UInt64)value);
        }
        public int Value(UInt64 value)
        {
            if (Type == BabylonMS.CONST_FT_INT64)
            {
                stream.Write(BitConverter.GetBytes(value), 0, 8);
                return 0;
            }
            else
                return -1;
        }
        public int Value(string stringvalue)
        {
            byte[] value = Encoding.Unicode.GetBytes(stringvalue);
            if (Type == BabylonMS.CONST_FT_BYTE)
            {
                stream.Write(value, 0, value.Length);
                return 0;
            }
            else
                return -1;
        }
        // append value to inner stream
        public int Value(byte[] value)
        {
            if (Type == BabylonMS.CONST_FT_BYTE)
            {
                stream.Write(value, 0, value.Length);
                return 0;
            }
            else
                return -1;
        }
        public int ValueAsUUID(String UUID)
        {
            if (Type == BabylonMS.CONST_FT_UUID)
            {
                byte[] uuid = BabylonMS.toUUID128(UUID);
                stream.Write(uuid, 0, uuid.Length);
                return 0;
            }
            else
                return -1;
        }

        byte[] buffer;
        public Int64 getValue(byte indexOfArray)
        {
            Int64[] ret = new Int64[1];

            switch (Type) {
                case BabylonMS.CONST_FT_INT64:
                    stream.Position = 8 * indexOfArray;
                    buffer = new byte[8];
                    stream.Read(buffer, 0, 8);
                    Buffer.BlockCopy(buffer, 0, ret, 0, 8);
                    return ret[0];
                    break;
                case BabylonMS.CONST_FT_INT8:
                    stream.Position = indexOfArray;
                    return stream.ReadByte();
                    break;
                case BabylonMS.CONST_FT_INT16:
                    stream.Position = 2 * indexOfArray;
                    buffer = new byte[2];
                    stream.Read(buffer, 0, 2);
                    Buffer.BlockCopy(buffer, 0, ret, 0, 2);
                    return ret[0];
                    break;
                case BabylonMS.CONST_FT_INT32:
                    stream.Position = 4 * indexOfArray;
                    buffer = new byte[4];
                    stream.Read(buffer, 0, 4);
                    Buffer.BlockCopy(buffer, 0, ret, 0, 4);
                    return ret[0];
                    break;
            }
            return 0;
        }
        public double getFloatValue(byte indexOfArray)
        {
            double[] ret = new double[1];
            switch (Type)
            {
                case BabylonMS.CONST_FT_DOUBLE:
                    stream.Position = 8 * indexOfArray;
                    buffer = new byte[8];
                    stream.Read(buffer, 0, 8);
                    Buffer.BlockCopy(buffer, 0, ret, 0, 8);
                    return ret[0];
                    break;
                case BabylonMS.CONST_FT_FLOAT:
                    stream.Position = 4 * indexOfArray;
                    buffer = new byte[4];
                    stream.Read(buffer, 0, 4);
                    Buffer.BlockCopy(buffer, 0, ret, 0, 4);
                    return ret[0];
                    break;
            }
            return 0;
        }

        public String GetUUIDValue(byte indexOfArray)
        {
            if (Type == BabylonMS.CONST_FT_UUID)
            {
                stream.Position = 16 * indexOfArray;
                buffer = new byte[16];
                stream.Read(buffer, 0, 16);
                return BabylonMS.UUID128ToString(buffer);
            }
            return "";
        }
        public byte[] GetUUIDValueAsBytes()
        {
            if (Type == BabylonMS.CONST_FT_UUID)
            {
                return stream.ToArray();
            }
            return null;
        }

        public bool getBoolValue(byte indexOfArray)
        {
            if (Type == BabylonMS.CONST_FT_INT8)
            {
                stream.Position = indexOfArray;
                if (stream.ReadByte() == 0)
                    return false;
                else
                    return true;
            }
            return false;
        }

        public byte[] getValue()
        {
            if (Type == BabylonMS.CONST_FT_BYTE) {
                return stream.ToArray();
            }
            return null;
        }
        public String GetString()
        {
            if (Type == BabylonMS.CONST_FT_BYTE)
            {
                return System.Text.Encoding.Unicode.GetString(stream.ToArray());
            }
            return null;
        }
        public String GetString(int idx,int elemLength)
        {
            if (Type == BabylonMS.CONST_FT_BYTE)
            {
                String s = System.Text.Encoding.Unicode.GetString(stream.ToArray());
                return s.Substring(idx*elemLength,elemLength);
            }
            return null;
        }


        public BMSField clearValue()
        {
            Dispose();
            stream = new MemoryStream();
            return this;
        }
        public void Dispose()
        {
            stream.Close();
            stream.Dispose();
        }

    }

    //sdfsdfa
    public class BMSPack
    {
        public const byte HEADERBYTENUM = 14; //without marker (11) with full length =25

        public string Hdr;   /*Prelimutens*/
        public byte Version;
        public byte CompressFormat;
        public UInt16 Attribs; //Continue, once
        public UInt32 MinTime; //minimal transfer time in ms defa = 0        
        List<BMSField> Fields;
        public StreamReader reader;
        public StreamWriter writer;

        public UInt64 StartMinTime;
        Stopwatch stopWatch;

        public BMSPack() {
            Clear();
            stopWatch = new Stopwatch();
        }

        public void Clear()
        {
            Hdr = BabylonMS.CONST_MARKER;
            Version = BabylonMS.CONST_VERSION;
            CompressFormat = 0;
            Attribs = 0;
            if (Fields != null)
            {
                foreach (var f in Fields)
                {
                    f.Dispose();
                }
            }
            Fields = new List<BMSField>();
        }
        public void ClearFields()
        {
            Fields = new List<BMSField>();
        }

        public void setMinTimeWithoutStart(UInt32 time)
        {
            MinTime = time;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">in milliseconds</param>
        public void setMinTime(UInt32 time)
        {
            MinTime = time;
            if (stopWatch.IsRunning)
                stopWatch.Restart();
            else
                stopWatch.Start();
        }
        public bool IsReachedMinTime()
        {
            if (MinTime == 0) return true; 
            if (stopWatch.IsRunning)
            {
                return stopWatch.ElapsedMilliseconds > MinTime;
            } else { return true; }
        }
        public void WaitMinTime(BabylonMS.BMSEventHandler handler)
        {
            if (IsReachedMinTime()) {
                setMinTime(MinTime);
                return;
            }
            if (stopWatch.IsRunning)
            {
                while ((MinTime - stopWatch.ElapsedMilliseconds) > 0)
                {
                    Thread.Sleep(10);
                    if (handler != null)
                    {
                        handler(null);
                    }
                }
                //Thread.Sleep((int)(MinTime - stopWatch.ElapsedMilliseconds));
                stopWatch.Restart();
            }
        }

        public void SetAttribs(UInt16 attr)
        {
            Attribs |= attr;
        }
        public bool IsOnce()
        {
            return (Attribs & BabylonMS.CONST_Continous) == 0;
        }
        public bool HasFieldNames()
        {
            return (Attribs & BabylonMS.CONST_HasFieldNames) != 0;
        }
        public bool IsAutosendOutputpack()
        {
            return (Attribs & BabylonMS.CONST_AutosendOutputpack) != 0;  //TODO eredetileg == de sztem nem volt jo.. majd kider-l
        }
        public bool IsSeparate()
        {
            return (Attribs & BabylonMS.CONST_Separate) != 0;
        }
        //return index of field
        public BMSField AddField(string Id, byte Type) {
            if (Fields.Count >= 255) { return null; }
            BMSField bms = new BMSField(Id, Type);
            Fields.Add(bms);
            return bms;
        }
        public List<BMSField> GetFields()
        {
            return Fields;
        }
        public BMSField GetField(int index)
        {
            return Fields[index];
        }
        public int FieldsCount()
        {
            //if (HasFieldNames()) {
            //    return Fields.Count() - 1; //the last field entry is NAMES   
            //} else
                return Fields.Count();
        }

        public BMSField GetFieldByName(string id)
        {
            return Fields.Find(x => (x.MatchId(id)));
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public byte[] getPackToByteArray(bool hasFieldNames) {
            return ReadFully(getPack(hasFieldNames));
        }

        public MemoryStream getPack(bool hasFieldNames)
        {
            int Length = 0;                 /*Exclude Hdr and exclude Length*/
            byte fiType;
            MemoryStream mem = new MemoryStream();
            try
            {
                if (hasFieldNames) {
                    SetAttribs(BabylonMS.CONST_HasFieldNames);
                };
                mem.Write(Encoding.ASCII.GetBytes(Hdr), 0, Hdr.Length);   //0-10
                mem.Write(BitConverter.GetBytes(Length), 0, 4);         //11-14
                mem.WriteByte(Version);                                 //15
                mem.WriteByte(CompressFormat);                          //16
                mem.Write(BitConverter.GetBytes(Attribs), 0, 2);          //17. 2byte
                mem.Write(BitConverter.GetBytes(MinTime), 0, 4);          //19. 4byte
                if (hasFieldNames)
                {
                    mem.Write(BitConverter.GetBytes((ushort)(Fields.Count + 1)), 0, 2); //23,24
                    //mem.WriteByte((byte)(Fields.Count+1));                      //23.
                } else {
                    mem.Write(BitConverter.GetBytes((ushort)Fields.Count), 0, 2); //23,24
                    //mem.WriteByte((byte)(Fields.Count));                      //23.
                }
                Length += (HEADERBYTENUM - 4);            //without length  (5)   
                int len;
                foreach (BMSField fi in Fields)
                {
                    fiType = fi.GetTypeOfField();
                    mem.WriteByte(fiType);
                    Length++;
                    switch (fiType)
                    {
                        case BabylonMS.CONST_FT_INT8:
                            len = fi.GetLengthInType();
                            mem.WriteByte((byte)len);
                            fi.GetStream().WriteTo(mem);
                            Length += (len) + 1; //count
                            break;
                        case BabylonMS.CONST_FT_INT16:
                            len = fi.GetLengthInType();
                            mem.WriteByte((byte)len);
                            fi.GetStream().WriteTo(mem);
                            Length += (len * 2) + 1; //count
                            break;
                        case BabylonMS.CONST_FT_INT32:
                        case BabylonMS.CONST_FT_FLOAT:
                            len = fi.GetLengthInType();
                            mem.WriteByte((byte)len);
                            fi.GetStream().WriteTo(mem);
                            Length += (len * 4) + 1; //count
                            break;
                        case BabylonMS.CONST_FT_INT64:
                        case BabylonMS.CONST_FT_DOUBLE:
                            len = fi.GetLengthInType();
                            mem.WriteByte((byte)len);
                            fi.GetStream().WriteTo(mem);
                            Length += (len * 8) + 1; //count
                            break;
                        case BabylonMS.CONST_FT_UUID:
                            len = fi.GetLengthInType();
                            mem.WriteByte((byte)len);
                            fi.GetStream().WriteTo(mem);
                            Length += (len * 16) + 1; //count
                            break;
                        case BabylonMS.CONST_FT_BYTE:
                            len = (int)fi.GetStream().Length;
                            mem.Write(BitConverter.GetBytes(len), 0, 4); //length = X....
                            fi.GetStream().WriteTo(mem);
                            Length += len + 4; //count(4)
                            break;
                    }
                }
                if (hasFieldNames)
                {
                    mem.WriteByte(BabylonMS.CONST_FT_NAME);
                    Length++;
                    len = Fields.Count();
                    mem.WriteByte((byte)len);
                    Length++;
                    foreach (BMSField fi in Fields)
                    {
                        mem.Write(fi.GetId(), 0, BabylonMS.CONST_FIELDNAME_LEN);
                        Length += BabylonMS.CONST_FIELDNAME_LEN;
                    }
                }
                if (CompressFormat == BabylonMS.CONST_CF_Zip)
                {
                    int sfrom = HEADERBYTENUM + BabylonMS.CONST_MARKER.Length;
                    MemoryStream zmem = new zipper(mem,sfrom).GetZip(false);
                    //byte[] newmem= new byte[sfrom];
                    //mem.Read(newmem, 0, sfrom);
                    //MemoryStream mem2 = new MemoryStream(newmem);                    
                    mem.SetLength(sfrom);
                    mem.Position = sfrom;
                    zmem.CopyTo(mem);
                }


                mem.Position = 11;
                mem.Write(BitConverter.GetBytes(Length), 0, 4);         //11-14
            }
            catch (Exception ) { }
            return mem;
        }

        public void CopyTo(BMSPack destination)
        {
            CopyTo(destination, 0);
        }

        public void CopyTo(BMSPack destination, int FromFieldIndex)
        {
            CopyTo(destination, FromFieldIndex, int.MaxValue);
        }
        public void CopyTo(BMSPack destination, int FromFieldIndex, int ToFieldIndex)
        {
            destination.Clear();
            destination.Hdr = this.Hdr;
            destination.Version = this.Version;
            destination.CompressFormat = this.CompressFormat;
            destination.Attribs = this.Attribs;
            destination.MinTime = this.MinTime;
            destination.StartMinTime = this.StartMinTime;
            if (this.Fields != null)
            {
                BMSField f2;
                int i = 0;
                foreach (var f in this.Fields)
                {
                    if ((i >= FromFieldIndex) && (i <= ToFieldIndex))
                    {
                        f2 = Util.DeepCopy(f);
                        destination.Fields.Add(f2);
                    }
                    i++;
                }
            }
        }


        //compare fields type
        public bool AcceptedEnergyPattern(byte[] typePattern) {

            bool res = true;
            int c = typePattern.Length;
            int i = 0; int k = 0;
            while (c > 0)
            {
                i = i % Fields.Count;
                if (Fields[i].GetTypeOfField() != typePattern[i + k])
                    res = false;
                c--; i++;
                if ((typePattern.Length <= i + k) && (typePattern[i + k] == BabylonMS.CONST_FT_END))
                {
                    if (res == true)
                        return true;
                    res = true;
                    k++; c--;
                }
            }
            if (res == true)
                return true;
            throw new Exception();
        }
    }


    public class Util
    {
        static int usedProcessor = 0;
        static int cpu = 0x0001;
        public static int ProcessorCount()
        {
            return Environment.ProcessorCount;
        }
        /// <summary>
        /// Give me a number like 1,2,3,4, or 0x000f mean use all cpu in 4 core
        /// </summary>
        /// <returns></returns>
        public static int getFreeProcessor()
        {
            if (usedProcessor < ProcessorCount())
            {
                int c = cpu;
                cpu = cpu << 1;
                usedProcessor++;
                return c;
            } else
            {
                return 0x000f;
            }
        }
        /// <summary>
        /// Processor affinity if there is free like 1,2,3,4,ALL
        /// </summary>
        public static void setNextProcessor()
        {
            Process.GetCurrentProcess().ProcessorAffinity = (System.IntPtr)getFreeProcessor();
        }
        /// <summary>
        /// Cyclic processor affinity like 1,2,3,4,1,2,3,4,1,2....
        /// </summary>
        public static void setNextProcessorCyclic()
        {
            int p = getFreeProcessor();
            if (p == 0x000f)
            {
                cpu = 0x0002;
                p = 1;
            }
            Process.GetCurrentProcess().ProcessorAffinity = (System.IntPtr)p;
        }
        public static void setPriorityUp()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
        }
        public static void setPriorityDown()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.BelowNormal;
        }

        public static T DeepCopy<T>(T other)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, other);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }
    }


    public class Network
    {
        public const int CONST_NetworkType_Server = 1;
        public const int CONST_NetworkType_Client = 2;
        /// <summary>
        /// Summarized timeout for wait for connection
        /// </summary>
        public const int CONST_CONNECT_TIMEOUT = 5000; //in ms
        /// <summary>
        /// Sleep connection thread for X ms and try again
        /// </summary>
        public const int CONST_CONNECT_FREQ = 500; //in ms 

        public int NetworkType = 0;

        public TcpClient client = null;
        public TcpListener server = null;
        bool exit = false;
        string ip;

        public delegate void ReceivedEventHandler(object o, NetworkStream stream, EventArgs e);
        public delegate void ConnectEventHandler(TcpClient client, NetworkStream stream);
        public event ConnectEventHandler Connected;
        protected virtual void OnConnected(TcpClient client, NetworkStream stream)
        {
            ConnectEventHandler handler = Connected;
            if (handler != null)
            {
                handler(client, stream);
            }
        }
        public event ReceivedEventHandler Received;
        protected virtual void OnReceived(NetworkStream stream, EventArgs e)
        {
            ReceivedEventHandler handler = Received;
            if (handler != null)
            {
                handler(this, stream, e);
            }
        }
        public event ReceivedEventHandler Disconnected;
        protected virtual void OnDisconnected(NetworkStream stream, EventArgs e)
        {
            ReceivedEventHandler handler = Disconnected;
            if (handler != null)
            {
                handler(this, stream, e);
            }
        }


        public Network(int networkType, string ip, int port)
        {
            switch (networkType)
            {
                case CONST_NetworkType_Client:
                    Client(ip, port);
                    break;
                case CONST_NetworkType_Server:
                    Server(ip, port);
                    break;
            }
        }
        public void Stop()
        {
            OnDisconnected(null, new EventArgs());
            switch (NetworkType)
            {
                case CONST_NetworkType_Client:
                    client.Close();
                    break;
                case CONST_NetworkType_Server:
                    server.Stop();
                    exit = true;
                    break;
            }
        }

        Task servertask;
        public Network Server(string ip, int port)
        {

            NetworkType = CONST_NetworkType_Server;
            if (ip.CompareTo("") == 0) {
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                server = new TcpListener(ipAddress, port);//192.168.42.100 vagy 192.168.43.100
            } else {
                server = new TcpListener(IPAddress.Parse(ip), port);//192.168.42.100 vagy 192.168.43.100
            }
            this.ip = ((IPEndPoint)server.LocalEndpoint).Address.ToString();
            server.Start();
            ///THREAD----------------------------------------------------------------------------------------------
            servertask = Task.Factory.StartNew(() =>
            {
                try
                {
                    while (!exit)
                    {
                        TcpClient client = server.AcceptTcpClient();
                        NetworkStream clientstream = client.GetStream();
                        OnConnected(client, clientstream);
                        //Task.Factory.StartNew(() => ServerCore(client));
                    }
                } catch (Exception )
                {

                }
            });
            return this;
        }
        /*
        private void ServerCore(TcpClient client)
        {
            try
            {
                NetworkStream clientstream = client.GetStream();
                OnConnected(client, clientstream, new EventArgs());
                while (!exit)
                {
                    if (clientstream.DataAvailable)
                    {
                        OnReceived(clientstream, new EventArgs());
                    }
                    Thread.Sleep(10);
                }
            }
            finally
            {
                client.Close();
            }
        }
        */
        //Client

        public Network Client(string ip, int port)
        {
            NetworkType = CONST_NetworkType_Client;
            client = new TcpClient();
            ///THREAD----------------------------------------------------------------------------------------------
            Task.Factory.StartNew(() =>
            {
                int WaitConnection = CONST_CONNECT_TIMEOUT / CONST_CONNECT_FREQ;
                while ((!client.Connected) && (--WaitConnection > 0))
                {
                    try
                    {
                        client.Connect(ip, port);
                    }
                    catch (Exception ) { }
                    Thread.Sleep(CONST_CONNECT_FREQ);
                }
                if (client.Connected)
                { //not timeout then
                    ClientCore(client);
                }
                Console.WriteLine("Client not connected/disconnect.");
                Stop();
            });
            return this;
        }

        public NetworkStream clientstream;
        private void ClientCore(TcpClient client)
        {
            clientstream = client.GetStream();

            OnConnected(client, clientstream);
            while ((!exit) && (client.Connected))
            {
                if (clientstream.DataAvailable)
                {
                    OnReceived(clientstream, new EventArgs());
                }
                Thread.Sleep(10);
            }
        }

        public void Send2Client(TcpClient client, BMSPack pack)
        {
            byte[] b = pack.getPackToByteArray(false);
            client.GetStream().Write(b, 0, b.Length);
        }
        public void Send2Server(BMSPack pack)
        {
            Send2Server(server, pack);
        }
        public void Send2Server(TcpListener server, BMSPack pack)
        {
            byte[] b = pack.getPackToByteArray(false);
            server.Server.Send(b);
        }
        public void Send2Server(TcpListener server, string value)
        {
            server.Server.Send(BitConverter.GetBytes((int)value.Length));
            byte[] b = Encoding.ASCII.GetBytes(value);
            server.Server.Send(b);
        }

        private MemoryStream readPack(NetworkStream clientstream)
        {
            byte[] buffer = new byte[512];
            if (!BabylonMS.FillSTA(clientstream, buffer, BabylonMS.CONST_MARKER.Length)) return null;
            if (BabylonMS.DEBUG_WriteConsole) Console.WriteLine("Begin3");
            while (!BabylonMS.compareMarker(buffer, 0))
            {
                Buffer.BlockCopy(buffer, 1, buffer, 0, BabylonMS.CONST_MARKER.Length - 1);
                buffer[BabylonMS.CONST_MARKER.Length - 1] = (byte)clientstream.ReadByte();
            }
            //!!!!!!!!!!!found marker
            try
            {
                MemoryStream mem = new MemoryStream();
                mem.Write(Encoding.ASCII.GetBytes(BabylonMS.CONST_MARKER), 0, BabylonMS.CONST_MARKER.Length);
                if (!BabylonMS.FillSTA(clientstream, buffer, BMSPack.HEADERBYTENUM)) return null;
                mem.Write(buffer, 0, BMSPack.HEADERBYTENUM);

                int length = BitConverter.ToInt32(buffer, 0);
                int fieldscount = BitConverter.ToInt16(buffer, 0);// buffer[12];
                length = length - (BMSPack.HEADERBYTENUM - 4);
                if (buffer.Length < length) buffer = new byte[length];
                if (!BabylonMS.FillSTA(clientstream, buffer, length)) return null;
                return mem;
            }
            catch (Exception )
            {
                return null;
            }
        }

        public String readString(NetworkStream s)
        {
            byte[] buffer = new byte[4];
            s.Read(buffer, 0, 4);
            int len = BitConverter.ToInt32(buffer, 0);
            buffer = new byte[len];
            s.Read(buffer, 0, len);
            string st = Encoding.ASCII.GetString(buffer);
            return st;
        }

    }
/*
    public class InstanceInformation
    {
        public const byte CONST_BABYLON_INSTANCE = 100; //This pack catch for babylon instance information
        String InstanceUUID;
        public InstanceInformation()
        {

        }

        public static void SendInstanceInfo(String ID)
        {
            BMSPack outputpack = new BMSPack();
            outputpack.AddField("CMD", BabylonMS.CONST_FT_INT8).Value(CONST_BABYLON_INSTANCE);
            outputpack.AddField("ID", BabylonMS.CONST_FT_UUID);

        }

    }
    */
    public class BMSEventSessionParameter
    {
        public BMSPack inputPack = null;
        public BMSPack outputPack = null;
        public TcpClient client = null;
        public StreamReader reader = null;
        public StreamWriter writer = null;
        public String shipUUID = null;
        public object bms=null;
        public DateTime pingtime;

        public Semaphore writelock = new Semaphore(1, 1);

        public BMSEventSessionParameter(object bms)
        {
            this.bms = bms;
            Ping();
            inputPack = new BMSPack();
            outputPack = new BMSPack();
        }

        public void Ping()
        {
            pingtime = DateTime.Now;
        }
        public bool IsNeedPing(int afterms) {
            long dateticks = afterms + (pingtime.Ticks / 10000);            
            if (dateticks < (DateTime.Now.Ticks / 10000))
                return true;
            return false;
        }

        public bool TransferPacket(bool hasFieldNames)
        {
            return TransferPacket(hasFieldNames, outputPack);
        }
        public bool TransferPacket(bool hasFieldNames,BMSPack newpack)
        {
            if (writelock != null) writelock.WaitOne();
            MemoryStream mem = newpack.getPack(hasFieldNames);
            try
            {
                mem.Position = 0;
                mem.WriteTo(writer.BaseStream);
                //writer.Flush();
                return true;
            }
            catch (Exception )
            {
                //Console.Write("9");
                return false;
            }
            finally
            {
                if (writelock != null) writelock.Release();
            }
        }

    }

    /// <summary>
    /// Usage : MemoryStream f1 = new BabylonMS.zipper(f).GetZip(false);
    /// 
    /// FileStream f = new FileStream(@"C:\a\file.txt", FileMode.Open);
    /// MemoryStream f1 = new BabylonMS.zipper(f).GetZip(false);
    /// FileStream f2 = new FileStream(@"C:\a\file.zip", FileMode.Create);
    /// f1.WriteTo(f2);
    /// f1.Dispose();
    /// f1.Close();
    /// f2.Close();
    /// 
    /// Where f an uncompressed stream and f1 the compressed stream
    /// </summary>
    class zipper
    {
        MemoryStream ZIPStream;
        ZipArchive archive;

        public zipper(Stream mem)
        {
            createZIP();
            AddZIP(mem, "data.bin");
            archive.Dispose();
        }
        public zipper(MemoryStream mem, int from)
        {
            byte[] b = mem.ToArray();
            MemoryStream mem2 = new MemoryStream(b, from, b.Length - from);

            createZIP();
            AddZIP(mem2, "data.bin");
            archive.Dispose();
            mem2.Dispose();
        }

        public MemoryStream createZIP(Stream mem, String appname)
        {
            ZIPStream = new MemoryStream();
            using (archive = new ZipArchive(ZIPStream, ZipArchiveMode.Create))
            {
                ZipArchiveEntry file1 = archive.CreateEntry((appname));
                using (var entryStream = file1.Open())
                {
                    using (var sw = new StreamWriter(entryStream))
                    {                        
                        mem.CopyTo(sw.BaseStream); 
                    }
                }     
            }
            return null; //TODO not null.. .so not working
        }

        public void createZIP()
        {
            ZIPStream = new MemoryStream();
            archive = new ZipArchive(ZIPStream, ZipArchiveMode.Create, true);
        }

        //file1;
        void AddZIP(Stream mem, String appname)
        {
            ZipArchiveEntry file1 = archive.CreateEntry(/*Path.GetFileNameWithoutExtension*/(appname));
            using (var entryStream = file1.Open())
            {
                using (var sw = new StreamWriter(entryStream))
                {
                    mem.Position = 0;
                    mem.CopyTo(sw.BaseStream);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="needLengthHeader">before stream 4 bytes length integer for stream length</param>
        /// <returns></returns>
        public MemoryStream GetZip(bool needLengthHeader)
        {
            int len = (int)ZIPStream.Length;
            MemoryStream mem;
            if (needLengthHeader)
            {
                mem = new MemoryStream(len + 4);
                Byte[] b = BitConverter.GetBytes(len);  //A küldött adatcsomag hossza van a fejlécben azaz a fejléc hossza nincs (4 bájt)
                mem.Write(b, 0, 4);
            } else
            {
                mem = new MemoryStream(len);
            }            
            ZIPStream.Seek(0, SeekOrigin.Begin);
            ZIPStream.WriteTo(mem);
            ZIPStream.Flush();
            ZIPStream.Close();
            //CloseZIP();
            return mem;
        }
    }


    /// <summary>
    /// Usage: MemoryStream af1 = BabylonMS.unzipper.unzip(af);
    /// 
    /// FileStream af = new FileStream(@"C:\a\file.zip", FileMode.Open);
    ///     MemoryStream af1 = BabylonMS.unzipper.unzip(af);
    ///     FileStream af2 = new FileStream(@"C:\a\ufile.txt", FileMode.Create);
    ///     af1.WriteTo(af2);
    ///     af1.Dispose();
    ///     af1.Close();
    ///     af2.Close();
    /// </summary>
    class unzipper
    {
        static public byte[] unzip(byte[] mem)
        {
            return unzip(new MemoryStream(mem)).ToArray();
        }

        static public MemoryStream unzip(Stream mem)
        {
            unzipper z = new unzipper();
            z.createZIP(mem);
            MemoryStream res = z.RetrieveZIP("data.bin");            
            z.Dispose();
            return res;
        }

        Stream ZIPStream;
        ZipArchive archive;
        public void createZIP(Stream mem)
        {
            ZIPStream = mem;
            archive = new ZipArchive(ZIPStream, ZipArchiveMode.Read);
        }

        //file1;
        private MemoryStream RetrieveZIP(String appname)
        {            
            ZipArchiveEntry file1 = archive.GetEntry(appname);            
            using (var entryStream = file1.Open())
            {
                MemoryStream mem = new MemoryStream();
                entryStream.CopyTo(mem);
                return mem;
            }            
        }
        public void Dispose()
        {
            archive.Dispose();
        }
    }

}





