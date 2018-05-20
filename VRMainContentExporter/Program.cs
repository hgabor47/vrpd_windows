/*

 *              
 *        
 * 
 *       Pack commands https://docs.google.com/document/d/1dY-_8iMouxdg1eSR6ZEnjNl0ZFFSJhA4JIjBhzVUi5Y/edit#bookmark=id.ryr5jhoe9t6b
 * 
 * 
 * 
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;

namespace VRMainContentExporter
{
    class Program
    {

        // https://docs.google.com/document/d/1dY-_8iMouxdg1eSR6ZEnjNl0ZFFSJhA4JIjBhzVUi5Y/edit#bookmark=id.ryr5jhoe9t6b
        public static bool TEST_IMAGEBUFFER = false;
        public static bool TEST_WINDOWSLIST = false;
        public static bool TEST_SCREENCAPTURE = false;
        public static bool TEST_INPUTCONTROLLER = false;

        public static string ERR_IMAGEBUFFERLOAD = "ImageBuffer load error wrong IP:PORT ";
        public static string ERR_ANDROIDLOAD = "Android load error wrong IP:PORT ";

        //Testing only one handle but if 0 then all windowhandle will be processed
        public static long[] DEBUG_FIXWINDOWHANDLES = { }; //{ 0x001E048C, 0x002F094E }; //FAR...  0x001204B4 
        public static int DEBUG_FirstNScreenCaptureOnly = 8;
        public static int FPS = 5;
        public static bool DEBUG_InputControllerOnly = false;

        public static string UUID = "e7bdb39f-c2c1-447b-b528-4b9a40757e90"; 
        
        //from default UUID but -id parameter can override
        public static string instanceUUID = UUID;  //ennek a futtatott p;ld'nynak az egyedi aktuális azonosítója
        public static    int port_imagebuffer = 9001;
        public static string ip_imagebuffer = "127.0.0.1";

        public static int port_partner = 9000;        
        //public static string ip_partner = "172.24.21.203";  //if empty then the main ip adress will be used
        //public static string ip_partner = "192.168.42.100";  //if empty then the main ip adress will be used
        //public static string ip_partner = "192.168.1.110";  //if empty then the main ip adress will be used
        public static string ip_partner = "127.0.0.1";  //for testing

        public static bool exit = false;
        //miniShips
        public static ImageBuffer imagebuffer;
        public static InputController inputcontroller;
        public static Androids androids;
        public static ObjectIDGenerator IDGenerator=null;
        
        #region DOS Exit
        static bool exitSystem = false;
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;
        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        private static bool Handler(CtrlType sig)
        {
            Console.WriteLine("Exiting system due to external CTRL-C, or process kill, or shutdown");
            try
            {
                if (imagebuffer!=null)
                    imagebuffer.destroy();
            }
            catch (Exception ) { }
            try { 
                 if (inputcontroller!=null)
                    inputcontroller.destroy();
            }
                catch (Exception ) { }
            Console.WriteLine("Cleanup complete");
            //allow main to run off
            exitSystem = true;

            //shutdown right away so there are no lingering threads
            Environment.Exit(-1);

            return true;
        }
        #endregion



        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">
        /// 
        /// Original args was: e7bdb39f-c2c1-447b-b528-4b9a40757e90 127.0.0.1 c9146853-5b63-4e72-bd03-8234f53edbbf
        /// 
        /// new: empty or
        /// new2: -ib 127.0.0.1 -andro 127.0.0.1 -id c9146853-5b63-4e72-bd03-8234f53edbbf
        /// 
        /// -ib 127.0.0.1        //imagebuffer ip address
        /// -ib 127.0.0.1:9001   //imagebuffer ip address:port
        /// -andro 192.168.42.100        //android network card local ip address 
        /// -andro 192.168.42.100:9001   // with port
        /// -id c9146853-5b63-4e72-bd03-8234f53edbbf   //unique ID for this instance
        /// </param>
        static void Main(string[] args)
        {
            var arg = new argprocess(args);
            if (!arg.isSuccess)
            {
                Environment.Exit(0);
            }

            ip_imagebuffer = arg.Get("-ib").SValue;
            port_imagebuffer = arg.Get("-ib").IValue;
            ip_partner = arg.Get("-andro").SValue;
            port_partner = arg.Get("-andro").IValue;
            instanceUUID = arg.Get("-id").SValue;
            DEBUG_FirstNScreenCaptureOnly = arg.Get("-lc").IValue;
            DEBUG_InputControllerOnly = arg.Get("-ic").BValue;

            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);

            testAssembly();

            IDGenerator = new ObjectIDGenerator();
            if (!DEBUG_InputControllerOnly)
                imagebuffer = new ImageBuffer();
            inputcontroller = new InputController();
            //new WindowsList(1f); //for test only

            androids = new Androids(args);
            //todos
            while (true)
            {
                Thread.Sleep(1500);
                String isStop = Console.ReadLine();
                if (isStop.ToLower().CompareTo("s") == 0)
                {
                    Handler(CtrlType.CTRL_CLOSE_EVENT);
                }
                //bms.outputpack.SetAttribs(BabylonMS.BabylonMS.CONST_Separate);
                //bms.TransferPacket(false);
            };
        }

        static void testAssembly()
        {
            if (TEST_IMAGEBUFFER) {
                Console.WriteLine("TESTing IMAGEBUFFER!");
            }
            if (TEST_WINDOWSLIST) {
                Console.WriteLine("TESTing WINDOWSLIST!");
            }
            if (TEST_SCREENCAPTURE) {
                Console.WriteLine("TESTing SCREENCAPTURE!");
            }
            if (TEST_INPUTCONTROLLER) {
                Console.WriteLine("TESTing INPUTCONTROLLER!");
            }
            if ( DEBUG_FIXWINDOWHANDLES.Length>0)
            {
                Console.WriteLine("TESTing WITH FIX WINDOW HANDLES!");
            }

            if (TEST_INPUTCONTROLLER || TEST_SCREENCAPTURE || TEST_WINDOWSLIST || TEST_IMAGEBUFFER || (DEBUG_FIXWINDOWHANDLES.Length>0))
            {

            }
            else {
                Console.WriteLine("ALL IN PRODUCTION MODE!");
            }
            Console.WriteLine("Process " + DEBUG_FirstNScreenCaptureOnly + " windows content only!");
        }

        public static void terminate()
        {
            Handler(CtrlType.CTRL_SHUTDOWN_EVENT);
            Console.ReadLine();
        }

}

    class ImageBuffer
    {
        public static string ImageBufferUUID = "52f1b4e2-61e2-4ca0-8d21-e70b509e7693";  //This Pod is a SHIP
        public BabylonMS.BabylonMS bms;
        public bool ready=false;
        WindowsList windowslist;
        Process proc_imagebuffer;
        public ImageBuffer()
        {
            if (!Program.TEST_IMAGEBUFFER)
            {
                bms = BabylonMS.BabylonMS.LaunchMiniShip(out proc_imagebuffer, "ImageBuffer.exe", ImageBufferUUID, ImageBufferUUID, Program.instanceUUID); //UUID
            }
            else
            {
                bms = BabylonMS.BabylonMS.LaunchMiniShip(ImageBufferUUID, ImageBufferUUID, Program.instanceUUID); //DEBUG because Manual start //UUID
            }
            try
            {
                bms.ChangeMiniShipToNetwork(Program.ip_imagebuffer, Program.port_imagebuffer); //started file but switchOn Radio
                bms.ServerReadyForTransfer += ReadyForTransfer;
                bms.PrepareGate();
            }
            catch (Exception ) {
                Console.WriteLine(Program.ERR_IMAGEBUFFERLOAD);
                Program.terminate();
            }

            while ((bms.IsReady) || (!ready)) { Thread.Sleep(100); }; //TODO Biztosan nem "!bms.IsReady" kell?
            ready = false;
            bms.ServerReadyForTransfer -= ReadyForTransfer;  //NEED!! tedd vissza a norm'l haszn'lathoz            
        }
        void ReadyForTransfer(BabylonMS.BMSEventSessionParameter session)
        {
            bms.Disengage();
            ready = true;
            windowslist = new WindowsList(2f);
            Console.WriteLine("ImageBuffer started (WindowsList created)");
        }
        public void destroy()
        {
            try
            {
                proc_imagebuffer.Kill();
                windowslist.destroy();
            }
            catch (Exception ) { };
        }
    }

    //Window selection element
    class Selection
    {
        public const int CONST_PACKTYPE_IMAGE = 0;
        public const int CONST_PACKTYPE_CONTROLLER = 1;

        public int type;    //Pack type

        public Int64  hwnd;              //select one window
        public String hwndgroupUUID;    //in one group          NOT USED YET

        public String mouseUUID;        //Select one mouse

    }

    class AndroidDescriptor
    {
        public StreamReader reader;
        public StreamWriter writer;
        public TcpClient client;
        public BabylonMS.BabylonMS bms_imagebuffer;
        public Int64 ID;
        List<Selection> selection;  //if empty than all types of pack will be transmit
        public Semaphore locker;
        public BabylonMS.BMSEventSessionParameter session;
        public int sentPackCounter;

        //public AndroidDescriptor(TcpClient client, StreamReader reader, StreamWriter writer)
        public AndroidDescriptor(BabylonMS.BMSEventSessionParameter session)
        {
            sentPackCounter = 0;
            locker = new Semaphore(1, 1); //for one android
            bool isFirstTime;
            selection = new List<Selection>(); //empty yet so all types is accepted
            this.session= session;
            this.reader = session.reader;
            this.writer = session.writer;
            this.client = session.client;            
            try
            {
                //Console.WriteLine(reader.CurrentEncoding.ToString());
                ID = Program.IDGenerator.GetId(client, out isFirstTime); //The client ID from reader instance unique ID
            }
            catch (Exception ) {
                Console.WriteLine("ID not created bad stream.");
            }
        }

        // az aktuális pack tartalom engedélyezve?
        public bool isEnabled(BabylonMS.BMSEventSessionParameter session)
        {
            BabylonMS.BMSPack pack = session.inputPack;
            if (selection.Count() < 1) { return true; }  //if empty the selection list then accepted (all types)
            long cmd = pack.GetField(0).getValue(0);//CMD

            if (cmd == VRCEShared.CONST_COMMAND_STORE)  //now only one type of IMAGEPACK
            {
                long hwnd = pack.GetFieldByName("HWND").getValue(0);
                String group = pack.GetFieldByName("GROUP").GetUUIDValue(0);
                if (selection.Find(x => ((x.type == Selection.CONST_PACKTYPE_IMAGE) && (x.hwnd == hwnd) && (x.hwndgroupUUID==group)  )) != null)
                {
                    return true;  //found STORE/HWND data
                }
                else {                    
                    if (selection.Find(x => ((x.type == Selection.CONST_PACKTYPE_IMAGE))) == null)
                    { //ebben a t\pusban egy'ltal'n nincs adat akkor elfogadva
                        return true;
                    } 
                }
                return false;
            }
            //check mouse
            if (cmd == VRCEShared.CONST_IC_EVENT)
            {
                String mouseUUID = session.shipUUID;

                if (selection.Find(x => ((x.type == Selection.CONST_PACKTYPE_CONTROLLER) && (x.mouseUUID == mouseUUID))) != null)
                {
                    return true;  //found stored data erre az egérre fel van iratkozva
                }
                else
                {
                    if (selection.Find(x => ((x.type == Selection.CONST_PACKTYPE_CONTROLLER))) == null)
                    { //ebben a t\pusban egy'ltal'n nincs adat akkor úgy vesszük fel van iratkozva rá
                        return true; 
                    }
                }
            }

            return false;  //unknown pack CMD
        }

        // Minden ablakot el kell k-ldeni? vagy csak egyet-egyet
        public List<Selection> allRetrieveHwnd()
        {
            if (selection.Count() < 1) { return null; }  //if empty the selection list then accepted (all types)
            List<Selection> partially = selection.FindAll(x => (x.type == Selection.CONST_PACKTYPE_IMAGE));
            return partially;
            /*
            if (partially.Count() == 0) { return null; }
            Int64[] res = new Int64[partially.Count()];
            int i = 0;
            foreach(var b in partially)
            {
                res[i++] = b.hwnd;                
            }
            return res;
            */
        }
    }

    class Androids
    {
        static string UUIDAndroid = "cf70c42b-93b7-49a9-b8ab-c5afb8d7dd4d";
        public List<AndroidDescriptor> AndroidsList;

        public BabylonMS.BabylonMS tcp;
        //public BabylonMS.BabylonMS req;
        public Androids(string[] args)
        {
            AndroidsList = new List<AndroidDescriptor>();
            try { 
            tcp = BabylonMS.BabylonMS.ShipDocking(Program.ip_partner, Program.port_partner, UUIDAndroid);  //server
            tcp.Connected += NewAndroidConnected;
            tcp.Disconnected += AndroidDisconnected;
            tcp.NewInputFrame += InputFrame_fromAnAndroid;
            tcp.OpenGate(false); //Server
            }
            catch (Exception ) {
                Console.WriteLine(Program.ERR_ANDROIDLOAD);
                Program.terminate();
            }
}
        //void NewAndroidConnected(TcpClient client, StreamReader reader, StreamWriter writer)
        void NewAndroidConnected(BabylonMS.BMSEventSessionParameter session)
        {
            Console.WriteLine("Android Connected");
            AndroidDescriptor thisAndroid = new AndroidDescriptor(session);
            if (!Program.DEBUG_InputControllerOnly)
            {
                thisAndroid.bms_imagebuffer = BabylonMS.BabylonMS.LaunchMiniShip(Program.ip_imagebuffer, Program.port_imagebuffer, ImageBuffer.ImageBufferUUID, ImageBuffer.ImageBufferUUID, Program.instanceUUID); //UUID
                thisAndroid.bms_imagebuffer.IsReady = false;
                thisAndroid.bms_imagebuffer.NewInputFrame += InputFrame_fromImageBuffer;
                thisAndroid.bms_imagebuffer.Disconnected += (ses) =>
                {
                    Console.WriteLine("Disconnect from imagebuffer");
                };
                thisAndroid.bms_imagebuffer.Connected += (ses) =>
                {
                    thisAndroid.bms_imagebuffer.Tag = ses;
                    AndroidsList.Add(thisAndroid);
                };
                thisAndroid.bms_imagebuffer.PrepareGate();//Nonblocking   net Client             
                while ((!thisAndroid.bms_imagebuffer.IsReady)) { Thread.Sleep(100); };
                //although the device connected but need time so device can retrieve all windows
                //Message_To_ImageBuffer_CONST_COMMAND_RETRIEVE(thisAndroid, (BabylonMS.BMSEventSessionParameter)thisAndroid.bms_imagebuffer.Tag);
                Console.WriteLine("Android personal Imagebuffer Connected ");
            } else
            {
                AndroidsList.Add(thisAndroid);
                Console.WriteLine("Inputcontroller only so Android personal Imagebuffer not connected");
            }
        }

        void Message_To_ImageBuffer_CONST_COMMAND_RETRIEVE(AndroidDescriptor thisAndroid, BabylonMS.BMSEventSessionParameter imagebufferSession)
        {
            // after connect send first packet for all content
            imagebufferSession.outputPack.Clear();
            imagebufferSession.outputPack.AddField("CMD", BabylonMS.BabylonMS.CONST_FT_INT8).
                Value(VRCEShared.CONST_COMMAND_RETRIEVE);
            imagebufferSession.outputPack.AddField("REQID", BabylonMS.BabylonMS.CONST_FT_INT64).
                Value(thisAndroid.ID);
            imagebufferSession.outputPack.SetAttribs(BabylonMS.BabylonMS.CONST_AutosendOutputpack); //A felkapcsolódás után az alábbi packot fogja elküldeni vissza

            List<Selection> windows = thisAndroid.allRetrieveHwnd();
            if ((windows !=null) && (windows.Count > 0))
            {
                foreach (var b in windows)
                {
                    imagebufferSession.outputPack.AddField("HWND", BabylonMS.BabylonMS.CONST_FT_INT64).Value(b.hwnd);
                    imagebufferSession.outputPack.AddField("GROUP", BabylonMS.BabylonMS.CONST_FT_UUID).ValueAsUUID(b.hwndgroupUUID);
                }
            }
            //Így megspórolhatunk egy event-et és nem várunk semmire
            //ha nincs HWND akkor mindet keri
            imagebufferSession.TransferPacket(true);  //InputFrame_fromAnAndroid
        }

        void AndroidDisconnected(BabylonMS.BMSEventSessionParameter session)
        {
            Console.WriteLine("**********Android Disconnected!");
        }

        AndroidDescriptor getAndroidDescriptor(StreamReader reader)
        {
            return AndroidsList.Find(x => (x.reader == reader));
        }
        AndroidDescriptor getAndroidDescriptor(Int64 ID)
        {
            return AndroidsList.Find(x => (x.ID == ID));
        }
        AndroidDescriptor getAndroidDescriptor(TcpClient client)
        {
            return AndroidsList.Find(x => (x.client == client));
        }

        //void InputFrame_fromImageBuffer(String partnerUUID, BabylonMS.BMSPack imagebufferPack, StreamReader imagebufferReader, StreamWriter imagebufferWriter)
        void InputFrame_fromImageBuffer(BabylonMS.BMSEventSessionParameter imagebuffersession)
        {
            // The Imgebuffer sent some hwnd image data
            //please send to requester android
            try
            {
                byte cmd = (byte)imagebuffersession.inputPack.GetFieldByName("CMD").getValue(0);
                Int64 ID = (Int64)imagebuffersession.inputPack.GetFieldByName("REQID").getValue(0);
                AndroidDescriptor thisAndroid = getAndroidDescriptor(ID);

                switch (cmd)
                {
                    case VRCEShared.CONST_COMMAND_RETRIEVE:
                        if (imagebuffersession.inputPack.FieldsCount() > 2)
                        {
                            BabylonMS.BMSPack pack = new BabylonMS.BMSPack();
                            pack.AddField("CMD", BabylonMS.BabylonMS.CONST_FT_INT8).Value(VRCEShared.CONST_ANDROIDCOMMAND_RETRIEVE_ALL);
                            pack.AddField("PCKCNT", BabylonMS.BabylonMS.CONST_FT_INT8).Value((byte)thisAndroid.sentPackCounter++);
                            int cnt = (imagebuffersession.inputPack.FieldsCount() - 2) / 3;
                            for (int i = 0; i < cnt; i++)
                            {
                                byte idx = (byte)imagebuffersession.inputPack.GetField((i * 4) + 2).getValue(0);
                                pack.AddField("IDX", BabylonMS.BabylonMS.CONST_FT_INT8).Value(idx);
                                pack.AddField("HWND", BabylonMS.BabylonMS.CONST_FT_INT64).Value(imagebuffersession.inputPack.GetField((i * 4) + 3).getValue(0));
                                pack.AddField("GROUP", BabylonMS.BabylonMS.CONST_FT_UUID).ValueAsUUID(imagebuffersession.inputPack.GetField((i * 4) + 4).GetUUIDValue(0));
                                pack.AddField("IMAGE", BabylonMS.BabylonMS.CONST_FT_BYTE).Value(imagebuffersession.inputPack.GetField((i * 4) + 5).getValue());

                            }
                            BabylonMS.BabylonMS.TransferPacket(thisAndroid.writer, pack, true, thisAndroid.locker);  //transfer images to requester android
                        }
                        break;
                }
            }
            finally { }
        }
        //F R O M   A N D R O I D
        //void InputFrame_fromAnAndroid(String partnerUUID, BabylonMS.BMSPack pack, TcpClient client, StreamReader reader, StreamWriter writer)
        void InputFrame_fromAnAndroid(BabylonMS.BMSEventSessionParameter session)
        {
            AndroidDescriptor thisAndroid = getAndroidDescriptor(session.client);
            
            //Nothing    
            //- direct retrieve images
            //- setup mouse behaviour
            // etc... select requested images 
            byte cmd = (byte)session.inputPack.GetField(0).getValue(0);
            switch (cmd) {
                case VRCEShared.CONST_ANDROIDCOMMAND_SUBSCRIBE_HWND : //subscribe hwnd
                    break;
                case VRCEShared.CONST_ANDROIDCOMMAND_SUBSCRIBE_TYPE : //subscribe type (images,mouse,keyboard....
                    break;
                case VRCEShared.CONST_ANDROIDCOMMAND_RETRIEVE_HWND : //Direct retrieve a buffer element if need independently from refresh
                    {
                        //TODO szerintem nincs kész nem tesztelt...!

                        //request from IMAGEBUFFER
                        byte index = (byte)session.inputPack.GetField(1).getValue(0);
                        //Request imagebuffer on tcp
                        session.outputPack.Clear();
                        session.outputPack.SetAttribs(BabylonMS.BabylonMS.CONST_AutosendOutputpack); //A felkapcsolódás után az alábbi packot fogja elküldeni vissza
                                                                                                     //Így megspórolhatunk egy event-et és nem várunk semmire
                        session.outputPack.AddField("CMD", BabylonMS.BabylonMS.CONST_FT_INT8).Value(VRCEShared.CONST_COMMAND_RETRIEVE_IDX); //TODO 
                        session.outputPack.AddField("IDX", BabylonMS.BabylonMS.CONST_FT_INT8).Value(index);
                        session.TransferPacket(true);
                    }
                    break;
                case VRCEShared.CONST_ANDROIDCOMMAND_RETRIEVE_ALL: //Elküldi a legfrissebb tartalmat minden feliratkozott képről.(feliratkozások minden Androidhoz előzőleg letárolva)
                    {
                        //Message_To_ImageBuffer_CONST_COMMAND_RETRIEVE(thisAndroid,session.);
                        Message_To_ImageBuffer_CONST_COMMAND_RETRIEVE(thisAndroid, (BabylonMS.BMSEventSessionParameter)thisAndroid.bms_imagebuffer.Tag);
                        Console.WriteLine("First retrieve for imagebuffer");
                    }
                    break;
                case VRCEShared.CONST_ANDROIDCOMMAND_FOCUS_WINDOW:  //Rááll egy ablakra
                    //TODO kapcsolt hálózat? egyelőre ez a gép
                    {
                        String groupUUID = session.inputPack.GetFieldByName("GROUP").GetUUIDValue(0);
                        if ( BabylonMS.BabylonMS.compareUUID(Program.instanceUUID,groupUUID) )
                        {
                            //this instance
                            Int64 hwnd = session.inputPack.GetFieldByName("HWND").getValue(0);
                            HandlesList win = HandlesList.getw(hwnd, HandlesList.windows);
                            if (win != null)
                            {
                                win.sc.BringToFront();
                            }
                        }

                        break;
                    }
            }
        }
        //T O  A N D R O I D
        //public void sendNoticeToAndroidsNewContent(BabylonMS.BMSPack inputpack, StreamReader reader, StreamWriter writer)
        public void sendNoticeToAndroidsNewContent(BabylonMS.BMSEventSessionParameter session)
        {
            BabylonMS.BMSPack androidpack;
            foreach (AndroidDescriptor android in AndroidsList)
            {
                try
                {
                    
                    if (android.isEnabled(session))  //adott android mit kaphat meg...
                    {
                        byte cmd = (byte)session.inputPack.GetFieldByName("CMD").getValue(0);                        
                        switch (cmd)
                        {
                            case VRCEShared.CONST_COMMAND_STORE: //->CONST_ANDROIDCOMMAND_CHANGE_HWND
                                //Translate pack
                                androidpack = new BabylonMS.BMSPack();
                                session.inputPack.CopyTo(androidpack);
                                androidpack.GetFieldByName("CMD").clearValue().Value(VRCEShared.CONST_ANDROIDCOMMAND_CHANGE_HWND);  //ClearValue because addarray to next position (2.)
                                androidpack.AddField("PCKCNT", BabylonMS.BabylonMS.CONST_FT_INT8).Value((byte)android.sentPackCounter++);
                                BabylonMS.BabylonMS.TransferPacket(android.writer, androidpack, true,android.locker);
                                break;
                            case VRCEShared.CONST_ANDROIDCOMMAND_LOST_WINDOW:
                                androidpack = session.inputPack;
                                androidpack.AddField("PCKCNT", BabylonMS.BabylonMS.CONST_FT_INT8).Value((byte)android.sentPackCounter++);
                                BabylonMS.BabylonMS.TransferPacket(android.writer, androidpack, true, android.locker);
                                break;
                            case VRCEShared.CONST_IC_EVENT: //->CONST_ANDROIDCOMMAND_IC_EVENT
                                androidpack = new BabylonMS.BMSPack();
                                session.inputPack.CopyTo(androidpack);
                                androidpack.GetFieldByName("CMD").clearValue().Value(VRCEShared.CONST_ANDROIDCOMMAND_IC_EVENT);  //ClearValue because addarray to next position (2.)
                                ScreenCapture focused = ScreenCapture.getFocused();
                                if (focused != null)
                                {
                                    androidpack.AddField("LEFT", BabylonMS.BabylonMS.CONST_FT_INT16).Value((Int16)ScreenCapture.frame[0]);
                                    androidpack.AddField("TOP", BabylonMS.BabylonMS.CONST_FT_INT16).Value((Int16)ScreenCapture.frame[1]);
                                }
                                androidpack.AddField("PCKCNT", BabylonMS.BabylonMS.CONST_FT_INT8).Value((byte)android.sentPackCounter++);
                                BabylonMS.BabylonMS.TransferPacket(android.writer, androidpack, true, android.locker);
                                break;
                        }


                    }
                }
                catch (Exception )
                {
                }
            }
        }

    }


    class WindowsList
    {
        static string UUIDWindowsList = "08440500-6940-4d89-a007-fe598f26e146";
        BabylonMS.BabylonMS bms;
        float FPS;
        Process proc_windowslist;
        public WindowsList(float fps)
        {
            FPS = fps;
            //For testing only one window
            if (Program.DEBUG_FIXWINDOWHANDLES.Length > 0)
            {
                Console.WriteLine("For testing please check the FAR Handle.");
                foreach (var f in Program.DEBUG_FIXWINDOWHANDLES)
                {
                    HandlesList.Add(f, HandlesList.windows);
                }
                return;
            }
            if (!Program.TEST_WINDOWSLIST)
            {
                bms = BabylonMS.BabylonMS.LaunchMiniShip(out proc_windowslist, "WindowsList.exe", UUIDWindowsList, UUIDWindowsList, Program.instanceUUID); //UUID
            }
            else { }
            Console.WriteLine("WindowsList Ship launched.");
            bms.NewInputFrame += NewInputFrame;
            bms.ServerReadyForTransfer += ReadyForTransfer;
            bms.PrepareGate();
        }
        void ReadyForTransfer(BabylonMS.BMSEventSessionParameter session)
        {
            //BabylonMS.Util.setNextProcessorCyclic();
            Console.WriteLine("WindowsList started (WindowsList created)");
            session.outputPack.setMinTimeWithoutStart((uint)(1000 / FPS));
            session.outputPack.SetAttribs(BabylonMS.BabylonMS.CONST_Continous);
            session.TransferPacket(true); //request
        }
        public void destroy()
        {
            try
            {
                proc_windowslist.Kill();                
            }
            catch (Exception ) { };
        }

        void NewInputFrame(BabylonMS.BMSEventSessionParameter session)
        {
            try
            {
                //Console.Write("L ");
                foreach (BabylonMS.BMSField f in session.inputPack.GetFields())
                {
                    Int64 hwnd = f.getValue(0);                    
                    HandlesList.Add(hwnd, HandlesList.windows);                    
                }
                //Int64 buffer = session.inputPack.GetField(0).getValue(0);
                //Console.Write("arrived. ");
            }
            catch (Exception ) {
                Console.WriteLine("Error.");
            }
        }

    }
    
    class HandlesList
    {
        //static int DEBUG_FirstNScreenCaptureOnly = 1;

        public static List<HandlesList> windows = new List<HandlesList>();
        public static IntPtr selected;
        public static IntPtr self; // starting process

        public bool del = false;
        public Int64 handle;
        public string title = "";
        public ScreenCapture sc;

        public HandlesList(Int64 hwnd, string tit, ScreenCapture sc)
        {
            this.sc = sc;
            handle = hwnd;
            title = tit;
        }

        public static void Add(Int64 handle,List<HandlesList> list)
        {
            try
            {
                var element = list.Find(x => (x.handle == handle));
                if (element == null)
                {
                    ScreenCapture scr = null;
                    if ((--Program.DEBUG_FirstNScreenCaptureOnly) > -1)
                    {
                        scr = new ScreenCapture(handle, Program.FPS);
                        Thread.Sleep(100);
                    }
                    list.Add(new HandlesList(handle, "",scr));
                    display_FirstN();
                }
            }
            catch (Exception ) { }
        }

        public static HandlesList getw(Int64 hwnd, List<HandlesList> list)
        {
            try
            {
                return list.Find(x => (x.handle == hwnd));
            }
            catch (Exception ) { return null; }
        }

        public static void Remove(Int64 handle, List<HandlesList> list)
        {
            var element = list.Find(x => (x.handle == handle));
            if (element != null) {
                list.Remove(element);
                Program.DEBUG_FirstNScreenCaptureOnly++;
                display_FirstN();
            }
        }

        private static void display_FirstN()
        {
            Console.WriteLine("Remain " + Program.DEBUG_FirstNScreenCaptureOnly + " capture ability.");
        }

    }


    class ScreenCapture
    {
        static string UUIDScreenCapture = "18c549f7-5254-44e7-8842-7ff7c3ba839f";
        BabylonMS.BabylonMS bms;
        ImageBufferInterface IBIface;
        Int64 Hwnd;
        float FPS;
        BabylonMS.BMSEventSessionParameter screensession;
        static ScreenCapture focused;
        static public Int16[] frame = new Int16[4];
        private bool live;

        public ScreenCapture(Int64 hwnd, float fps) {
            focused = null;
            this.Hwnd = hwnd;
            FPS = fps;
            if (!Program.TEST_SCREENCAPTURE)
            {
                bms = BabylonMS.BabylonMS.LaunchMiniShip("ScreenContentExporter.exe", UUIDScreenCapture + hwnd.ToString(), UUIDScreenCapture, Program.instanceUUID); //UUID
            }
            else
            {
                bms = BabylonMS.BabylonMS.LaunchMiniShip(UUIDScreenCapture , UUIDScreenCapture, Program.instanceUUID);
                //bms = BabylonMS.BabylonMS.LaunchMiniShip("192.168.1.102",9000, UUIDScreenCapture + hwnd.ToString(), UUIDScreenCapture, Program.UUID);
            }
            Console.WriteLine("ScreenCapture Ship launched.");
            bms.NewInputFrame += NewInputFrame;
            bms.ServerReadyForTransfer += ReadyForTransfer;
            bms.Disconnected += Disconnect;
            bms.PrepareGate();//Nonblocking   
        }

        void ReadyForTransfer(BabylonMS.BMSEventSessionParameter session)
        {
            screensession = session;
            BabylonMS.Util.setNextProcessorCyclic();
            IBIface = new ImageBufferInterface();
            Console.WriteLine("ScreenCapture started" + Hwnd.ToString("X"));
            session.outputPack.AddField("CMD", BabylonMS.BabylonMS.CONST_FT_INT8).Value(VRCEShared.CONST_CAPTURE_START);  //hwnd = new IntPtr(Int64.Parse(chd.Attributes["handle"].Value));
            session.outputPack.AddField("HWND", BabylonMS.BabylonMS.CONST_FT_INT64).Value(Hwnd);  //hwnd = new IntPtr(Int64.Parse(chd.Attributes["handle"].Value));
            session.outputPack.AddField("MINTIME", BabylonMS.BabylonMS.CONST_FT_INT32).Value((int)(1000 / FPS));  //MINTIME direkt elküldve
            //session.outputPack.setMinTimeWithoutStart((uint)(1000 / FPS));
            //session.outputPack.SetAttribs(BabylonMS.BabylonMS.CONST_Continous);
            session.TransferPacket(true); //request
        }
        void Disconnect(BabylonMS.BMSEventSessionParameter session) //And reconnect
        {
            Console.WriteLine("ScreenContent disconnect window content not reachable. "+Hwnd.ToString("X"));
            //screen capture class need to remove from handles list and
            session.inputPack.ClearFields();
            session.inputPack.AddField("CMD", BabylonMS.BabylonMS.CONST_FT_INT8).Value(VRCEShared.CONST_ANDROIDCOMMAND_LOST_WINDOW);
            session.inputPack.AddField("HWND", BabylonMS.BabylonMS.CONST_FT_INT64).Value(Hwnd);
            session.inputPack.AddField("GROUP", BabylonMS.BabylonMS.CONST_FT_UUID).ValueAsUUID(Program.instanceUUID); //TODO ezt 't kell gondolni hogy  tényleg ezt kell e küldeni.. de valszeg igen
            Program.androids.sendNoticeToAndroidsNewContent(session);// inputpack,reader,writer);//So the partner will send a request for imagebuffer on TCP
            //IBIface.Disconnect(session);
            HandlesList.Remove(Hwnd, HandlesList.windows);
            if (focused == this)
            {
                focused = null;
            }
        }

        public static ScreenCapture getFocused()
        {
            if (focused != null)
            {
                try
                {
                    focused.live = true;  //ha eltűnt a screencapture példány akkor ez hibára fut.
                } catch (Exception)
                {
                    focused = null;
                    return null;
                }
            }
            return focused;
        }

        public void BringToFront()
        {
            focused = this;
            BabylonMS.BMSPack pack = new BabylonMS.BMSPack();
            pack.Attribs = screensession.outputPack.Attribs;
            pack.setMinTimeWithoutStart(screensession.outputPack.MinTime);            
            pack.AddField("CMD",BabylonMS.BabylonMS.CONST_FT_INT8).Value(VRCEShared.CONST_CAPTURE_FOCUS_WINDOW);
            screensession.TransferPacket(true,pack); //v;dett sempahore lock
        }

        //                      void NewInputFrame(String partnerUUID, BabylonMS.BMSPack inputpack, StreamReader reader, StreamWriter writer) //ScreenCapture
        int CCC1 = 0;
        Semaphore lock2 = new Semaphore(1, 1);
        
        void NewInputFrame(BabylonMS.BMSEventSessionParameter session) //from ScreenCapture
        {
            BabylonMS.BMSField rect = session.inputPack.GetFieldByName("RECT");            
            if (rect != null)
            {
                try
                {
                    if ((focused != null) && (focused == this))
                    {

                        frame[0] = (Int16)rect.getValue(0);
                        frame[1] = (Int16)rect.getValue(1);
                        frame[2] = (Int16)rect.getValue(2);
                        frame[3] = (Int16)rect.getValue(3);
                    }
                }
                catch (Exception) { }
            }
            byte[] buffer = session.inputPack.GetField(0).getValue();

            
            if (++CCC1 % 50 == 0)
            {
                Console.WriteLine("Inputframenum:" + CCC1.ToString());
            }
            {
                {
                    //Store screencapture image to Imagebuffer
                    IBIface.session.outputPack.Clear();
                    IBIface.session.outputPack.AddField("CMD", BabylonMS.BabylonMS.CONST_FT_INT8).
                        Value(VRCEShared.CONST_COMMAND_STORE);
                    IBIface.session.outputPack.AddField("HWND", BabylonMS.BabylonMS.CONST_FT_INT64).
                        Value(Hwnd);
                    IBIface.session.outputPack.AddField("GROUP", BabylonMS.BabylonMS.CONST_FT_UUID).
                        ValueAsUUID(Program.instanceUUID);
                    IBIface.session.outputPack.AddField("IMAGE", BabylonMS.BabylonMS.CONST_FT_BYTE).
                        Value(buffer);
                    try
                    {
                        IBIface.session.TransferPacket(true); //to Imagebuffer
                    }
                    catch (Exception )
                    {

                    }
                    //I Don't check buffered or not
                    // Partner class start Notice with modified content to Partner           
                }
            }
        }
    }

    class ImageBufferInterface
    {
        public const byte CONST_COMMAND_ANDROID_SENDIMAGE = 0;
        //static string UUIDTeszt = "a8c2e1b8-2485-4bbb-8626-539bb766d05f";
        //static string UUIDAndroid = "cf70c42b-93b7-49a9-b8ab-c5afb8d7dd4d";
        public BabylonMS.BabylonMS tcp;
        public BabylonMS.BMSEventSessionParameter session;
        public bool androidReady;
        public ImageBufferInterface()
        {
            androidReady = false;
            tcp = BabylonMS.BabylonMS.LaunchMiniShip(Program.ip_imagebuffer,Program.port_imagebuffer, ImageBuffer.ImageBufferUUID, ImageBuffer.ImageBufferUUID, Program.instanceUUID); //UUID
            //tcp = Program.imagebuffer.bms;
            tcp.IsReady = false;
            tcp.Connected += Connected;
            tcp.NewInputFrame       += NewInputFrame;
            tcp.Disconnected  += Disconnect;
            tcp.Waitbytes += Waitbytes;
            tcp.PrepareGate();//Nonblocking   net Client 
            while ((!tcp.IsReady)) { Thread.Sleep(100); };
            Console.WriteLine("Connected ImageBuffer");
        }
        public void Waitbytes(BabylonMS.BMSEventSessionParameter session)
        {
            //Console.Write("W ");
        }
        public void Connected(BabylonMS.BMSEventSessionParameter session) //And reconnect
        {
            this.session = session;
            //Console.WriteLine("Connected1");            
        }

        public void Disconnect(BabylonMS.BMSEventSessionParameter session) //And reconnect
        {
            Console.WriteLine("Disengage");
            tcp.Disengage();
        }
        
        void NewInputFrame(BabylonMS.BMSEventSessionParameter session)
        {
            //Console.Write("Answer for imagebuffer store command ");         
            
            bool needRefresh = session.inputPack.GetFieldByName("REFRESH").getBoolValue(0);
            if (needRefresh)
            {
                //Console.Write(" and need refresh.");
                if (session.inputPack.GetFieldByName("IMAGE") == null) {
                    //Console.Write(" Without image.");
                }
                //Console.WriteLine();
                Program.androids.sendNoticeToAndroidsNewContent(session);// inputpack,reader,writer);//So the partner will send a request for imagebuffer on TCP
            } else
            {   //no image info in pack
                //Console.WriteLine();
            }
            
        }

    }

    class InputController
    {
        public static string InputControllerUUID = "af675eb4-385e-4fda-a3a1-fdebd8901085";  //This Pod is a SHIP
        public BabylonMS.BabylonMS bms;
        
        
        Process proc_inputcontroller=null;
        public InputController()
        {
            if (!Program.TEST_INPUTCONTROLLER)
            {
                bms = BabylonMS.BabylonMS.LaunchMiniShip(out proc_inputcontroller, "InputController.exe", InputControllerUUID, InputControllerUUID, Program.instanceUUID); //UUID
            }
            else
            {
                bms = BabylonMS.BabylonMS.LaunchMiniShip(InputControllerUUID, InputControllerUUID, Program.instanceUUID); //UUID
            }
            bms.Connected += Connected;
            bms.NewInputFrame += NewInputFrame;
            bms.PrepareGate();
        }

        //int cnt = 0;
        private void NewInputFrame(BabylonMS.BMSEventSessionParameter session)
        {
            Task t2 = Task.Factory.StartNew(() =>
            {
                Program.androids.sendNoticeToAndroidsNewContent(session);

                //Thread.Sleep(500);
                //throw new NotImplementedException();
            });
        }

        private void Connected(BabylonMS.BMSEventSessionParameter session)
        {
            Console.WriteLine("Input Controller Connected" );
            //throw new NotImplementedException();
        }

        public void destroy()
        {
            try
            {
                proc_inputcontroller.Kill();                
            }
            catch (Exception ) { };
        }
    }
}



/*
//args process
string cmd = "";
string par = "";
for (int i=0; i<args.Length; i++)
{
    cmd = args[i];
    par = "";
    if (args.Length > i + 1)
        par = args[i + 1];
    if (par.Length > 0)
    {
        if (cmd.CompareTo("-ib") == 0)
        {
            String[] st = par.Split(':');
            ip_imagebuffer = st[0];
            if (st.Length > 1)
            {
                port_imagebuffer = Int32.Parse(st[1]); 
            }
            i++;
        }
        else {
            if (cmd.CompareTo("-andro") == 0)
            {
                String[] st = par.Split(':');
                ip_partner = st[0];
                if (st.Length > 1)
                {
                    port_partner = Int32.Parse(st[1]);
                }
                i++;
            }
            else {
                if (cmd.CompareTo("-id") == 0)
                {
                    instanceUUID = par;
                    i++;
                }
            }
        }
    }
}
*/


/*
 // Ping 
Task.Factory.StartNew(() =>
{
    while (true)
    {                    
        session.TransferPacket(true);
        Thread.Sleep(3000);
    }
}
);
*/
