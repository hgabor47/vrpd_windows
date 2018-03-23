using System;

namespace VRMainContentExporter
{
    class VRCEShared
    {
        // https://docs.google.com/document/d/1dY-_8iMouxdg1eSR6ZEnjNl0ZFFSJhA4JIjBhzVUi5Y/edit#bookmark=id.ryr5jhoe9t6b

        public const byte CONST_BABYLON_INSTANCE = 100; //used in BabylonMS and here I reserve the No.100 

        public const byte CONST_COMMAND_EXIST = 0;
        public const byte CONST_COMMAND_STORE = 1;
        public const byte CONST_COMMAND_RETRIEVE = 2;
        public const byte CONST_COMMAND_GETBUFFER = 3;
        public const byte CONST_COMMAND_RETRIEVE_IDX = 8;

        public const byte CONST_COMMAND_SCREENSHOT = 11;
        public const byte CONST_CAPTURE_START = 23;
        public const byte CONST_CAPTURE_FOCUS_WINDOW = 27;

        public const byte CONST_ANDROIDCOMMAND_SUBSCRIBE_HWND = 30; //subscribe hwnd
        public const byte CONST_ANDROIDCOMMAND_SUBSCRIBE_TYPE = 33; //subscribe type (images,mouse,keyboard....
        public const byte CONST_ANDROIDCOMMAND_RETRIEVE_HWND = 37; //Direct retrieve a buffer element if need independently from refresh
        public const byte CONST_ANDROIDCOMMAND_RETRIEVE_ALL = 39; //Elküldi a legfrissebb tartalmat minden feliratkozott képről.(feliratkozások minden Androidhoz előzőleg letárolva)
        public const byte CONST_ANDROIDCOMMAND_CHANGE_HWND = 43; //Pontosan egy ablak változott 
        public const byte CONST_ANDROIDCOMMAND_IC_EVENT = 46; //InputController esemény (mouse)
        public const byte CONST_ANDROIDCOMMAND_FOCUS_WINDOW = 47; //Sent to desktop
        public const byte CONST_ANDROIDCOMMAND_LOST_WINDOW = 48; //Screencontent session closed disconnected so window is closed

        public const byte CONST_IC_EVENT = 50;
        public const byte CONST_IC_MODE = 51;


        //Imagebuffer
        public const byte CONST_MODE_BFADD = 2;
        public const byte CONST_MODE_BFFOUND = 6;
        public const byte CONST_MODE_BFMODIFY = 9;

        public const Int32 CONST_MOUSEBUTTON_LEFT = 1048576;  //From Mousebuttons
        public const Int32 CONST_MOUSEBUTTON_MIDDLE = 4194304;
        public const Int32 CONST_MOUSEBUTTON_RIGHT = 2097152;
        public const Int32 CONST_MOUSEBUTTON_XBUTTON1 = 8388608;
        public const Int32 CONST_MOUSEBUTTON_XBUTTON2 = 16777216;
        public const UInt32 CONST_MOUSEBUTTON_MASK = 0x3fffffff;
        public const UInt32 CONST_MOUSEBUTTON_VIRTUAL = 0x80000000; 
        public const UInt32 CONST_MOUSEBUTTON_DOWN = 0x40000000;   // bit=1 = DOWN 

    }
}
