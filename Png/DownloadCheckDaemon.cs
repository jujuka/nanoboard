using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace nboard
{
    class DownloadCheckDaemon
    {
        bool stopped = false;
        object lockObject = new object();

        public DownloadCheckDaemon(NanoDB db)
        {
            var mailer = new PngMailer();
            ThreadPool.QueueUserWorkItem(state => {
                while (!stopped)
                { 
                    Thread.Sleep(5000);
                    try{
                    mailer.ReadInbox(db);
                    }catch{}
                }
            });
            ThreadPool.QueueUserWorkItem(state => {
                while (!stopped)
                {
                    Thread.Sleep(1000);
                    GC.Collect();
                }
            });
        }

        public void Stop()
        {
            lock (lockObject)
            {
                stopped = true;
            }
        }
    }
    
}