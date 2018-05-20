using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRMainContentExporter
{
    class argprocess
    {
        argelement[] argstable = {
             new argelement("-ib",      "127.0.0.1:9001",                       ARGType.IP,         "ImageBuffer address")
            ,new argelement("-andro",   "192.168.42.100:9000",                  ARGType.IP,         "Android address")
            ,new argelement("-id",      "e7bdb39f-c2c1-447b-b528-4b9a40757e90", ARGType.STR,        "Instance UUID")
            ,new argelement("-lc",      "8",                                    ARGType.NUM,        "Limit Content ammount")
            ,new argelement("-ic",      "false",                                ARGType.BOOL,        "Inputcontroller only")
        };

        public bool isSuccess;
        public argprocess(string[] args)
        {
            //args process
            isSuccess = false;
            string cmd = "";
            string par = "";
            for (int i = 0; i < args.Length; i++)
            {
                cmd = args[i];
                par = "";
                if (args.Length > i + 1)
                    par = args[i + 1];
                if (cmd.CompareTo("/?") == 0)
                {
                    printhelp();
                    return;
                }
                i = process_andModifyArgPosition(i, cmd, par);
            }
            isSuccess = true;
        }

        int process_andModifyArgPosition(int index,string cmd, string par)
        {
            try
            {
                var indexoftable = searchDefaultArgsTableIndex(cmd);
                if (indexoftable < 0) {
                    return index;
                };
                argelement arg = argstable[indexoftable];
                index = arg.process(index, par);
            }
            catch (Exception)
            {

            }
            return index;
        }

        int searchDefaultArgsTableIndex(string cmd)
        {
            for (int i = 0; i < argstable.Length; i++)
            {
                if (argstable[i].name.CompareTo(cmd) == 0)
                {
                    return i;
                }
            }
            return -1; //not
        }
        public argelement Get(string cmd)
        {
            var indexoftable = searchDefaultArgsTableIndex(cmd);
            if (indexoftable < 0) { return null; };
            return argstable[indexoftable];
        }

        private void printhelp()
        {            
            Console.WriteLine("Help:");
            for (int i = 0; i < argstable.Length; i++)
            {
                Console.WriteLine(argstable[i].name+" "+ argstable[i].comment+" default:"+ argstable[i].defaultValue);
            }
        }
    }

    class argelement
    {
        public String name;
        public String defaultValue;
        public ARGType argtype;
        public String comment;
        public String SValue;
        public int    IValue;
        public bool   BValue;

        public argelement(string name, string defaultValue, ARGType argtype,String comment)
        {
            this.name = name;
            this.defaultValue = defaultValue;
            this.SValue = defaultValue;
            this.IValue = 0;
            this.BValue = false;
            this.argtype = argtype;
            this.comment = comment;
            process(0, defaultValue);
        }

        public int process(int index,string par)
        {
            try
            {
                switch (argtype)
                {
                    case ARGType.BOOL:
                        if (par.CompareTo("false")!=0)                        
                            BValue = true;
                        //not increment not used par                    
                        break;
                    case ARGType.IP:
                        String[] st = par.Split(':');
                        SValue = st[0];
                        if (st.Length > 1)
                        {
                            IValue = Int32.Parse(st[1]);
                        }
                        index++; //increment used par
                        break;
                    case ARGType.STR:
                        SValue = par;
                        index++; //increment used par
                        break;
                    case ARGType.NUM:
                        IValue = Int32.Parse(par);
                        index++; //increment used par
                        break;
                }
            }
            catch (Exception)
            {

            }
            return index;
        }


    }
    enum ARGType
    {
        BOOL, //no parameter
        IP , //1 string parameter like 127.0.0.1:9000   
        STR ,  //1 string 
        NUM   //1 number 
    }

}
