using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace RenderTrainerLib
{
    public class PythonProcess
    {
        static int _port = 32000;

        public string Id;

        public string Command;

        public string Arguments;
        public int Port;

        dynamic _msgpackrpc;

        dynamic _client;

        public PythonProcess(string id)
        {
            Id = id;
            Command = ConfigurationManager.AppSettings[id + ".Command"];
            Arguments = ConfigurationManager.AppSettings[id + ".Arguments"];
            string port = ConfigurationManager.AppSettings[id + ".Port"];

            if (port == null || port == "auto")
            {
                _port++;
                Port = _port;
            }
            else
                Port = int.Parse(port);

            if (Command == null)
                throw new Exception("Please configure app.config with " + id + ".Command, .Arguments and .Port");

            if (Arguments == null)
                Arguments = "";

            Command = Environment.ExpandEnvironmentVariables(Command);
            Arguments = Environment.ExpandEnvironmentVariables(Arguments);
        }

        public void EnsureCreated()
        {
            Tools.KillPythonProcess(Id);

            Tools.StartProcess(Command, Arguments.Replace("#PORT#", Port.ToString()), Id);
        }

        public void Kill()
        {
            Tools.KillPythonProcess(Id);
        }

        public void Connect()
        {
            using (Py.GIL())
            {
                _msgpackrpc = Py.Import("msgpackrpc");
                _client = _msgpackrpc.Client(_msgpackrpc.Address("localhost", Port),timeout:99999);
            }
        }

        public void Call(string function)
        {
            using (Py.GIL())
            {
                 _client.call(function);
            }
        }

        public void Call(string function, object p1)
        {
            using (Py.GIL())
            {
                _client.call(function, p1);
            }
        }
        public void Call(string function, object p1, object p2)
        {
            using (Py.GIL())
            {
                _client.call(function, p1, p2);
            }
        }
        public void Call(string function, object p1, object p2, object p3)
        {
            using (Py.GIL())
            {
                _client.call(function, p1, p2, p3);
            }
        }
        public void Call(string function, object p1, object p2, object p3, object p4)
        {
            using (Py.GIL())
            {
                _client.call(function, p1, p2, p3, p4);
            }
        }
        public void Call(string function, object p1, object p2, object p3, object p4, object p5)
        {
            using (Py.GIL())
            {
                _client.call(function, p1, p2, p3, p4, p5);
            }
        }
        public void Call(string function, object p1, object p2, object p3, object p4, object p5, object p6)
        {
            using (Py.GIL())
            {
                _client.call(function, p1, p2, p3, p4, p5, p6);
            }
        }
    }
}
