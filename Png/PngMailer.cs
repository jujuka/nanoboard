using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace nboard
{
    class PngMailer
    {
        public void ReadInbox(NanoDB to)
        {
            if (Directory.Exists(Strings.Download))
            {
                string[] files = Directory.GetFiles(Strings.Download);

                foreach (string f in files)
                {
                    string pathToPng = f;
                    byte[] packed = null;

                    try
                    {
                        packed = new PngStegoUtil().ReadHiddenBytesFromPng(pathToPng);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e.ToString());
                    }

                    NanoPost[] posts = null;

                    try
                    {   
                        posts = NanoPostPackUtil.Unpack(packed);
                    }
                    catch (Exception e)
                    {
                        Logger.Log(f);
                        Logger.Log(e.ToString());
                    }

                    bool any = false;

                    if (posts != null)
                    {
                        foreach (var p in posts)
                        {
                            any |= to.AddPost(p);
                        }
                    }

                    if (any)
                    {
                        NotificationHandler.Instance.AddNotification("Извлечены новые сообщения.");
                    }
                }

                foreach (string f in files)
                {
                    File.Delete(f);
                }
            }
        }

        public void FillOutbox(NanoDB from)
        {
            if (!Directory.Exists(Strings.Upload))
            {
                Directory.CreateDirectory(Strings.Upload);
            }

            new PngContainerCreator().SaveToPngContainer(from);
            from.WritePosts(false);
        }
    }
}