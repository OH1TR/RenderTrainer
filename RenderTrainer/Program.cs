using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using RenderTrainerLib;

namespace RenderTrainer
{
    class Program
    {
        static void Main(string[] args)
        {
            string workDir = ConfigurationManager.AppSettings["WorkDir"];
            string tensorBoardLogs = Path.Combine(workDir, "Logs");
            List<string> index = new List<string>();

            Directory.SetCurrentDirectory("..\\..\\..\\PythonScripts");

            if (!Directory.Exists(tensorBoardLogs))
                Directory.CreateDirectory(tensorBoardLogs);

            PythonProcess blender = new PythonProcess("Blender");
            blender.EnsureCreated();
            blender.Connect();

            PythonProcess Tf = new PythonProcess("Tf");
            Tf.EnsureCreated();
            Tf.Connect();


            string dir = Directory.GetFiles(workDir, "Checkpoint_*").Where(i=>!i.Contains("epoch")).OrderByDescending(i => i).FirstOrDefault();
            if (dir != null)
            {
                Tf.Call("load", dir);
                //Tf.Call("dump", "c:\\temp\\b.txt");
            }
            else
                Tf.Call("build_model");

            

            Tf.Call("enable_tensorboard", tensorBoardLogs);

            while (true)
            {
                string indexFile = Path.Combine(workDir, "index.txt");

                for (int i = 0; i < 1000; i++)
                {
                    string filePath = Path.Combine(workDir, "image" + i + ".jpg");
                    blender.Call("renderImage", filePath, i % 6);
                    index.Add((i % 6) + "|" + filePath);
                    File.WriteAllLines(indexFile, index.ToArray());
                }
                return;

                File.WriteAllLines(indexFile, index.ToArray());

                Tf.Call("loadDataset", indexFile);
                
                Tf.Call("fit");
                //Tf.Call("evaluate");
                Tf.Call("save", Path.Combine(workDir, "Checkpoint_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")));
                //Tf.Call("dump", "c:\\temp\\a.txt");
            }
        }
    }
}
