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
                    catch (Exception)
                    {
                        // invalid container error:
                        //Logger.LogError(e.ToString());
                    }

                    NanoPost[] posts = null;

                    try
                    {   
                        posts = NanoPostPackUtil.Unpack(packed);

                        foreach (var p in posts)
                        {
                            if (to.Get(p.GetHash()) != null)
                            {
                                // reset known posts so they won't be deleted from spammer's container
                                // in case if spammer included some existing posts in his container
                                to.Get(p.GetHash()).ContainerTag = null;
                            }
                        }
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
                        to.WriteNewPosts(false);
                    }
                }

                try
                {
                    foreach (string f in files)
                    {
                        File.Delete(f);
                    }
                }
                catch
                {
                }
            }
        }

        public void FillOutbox(NanoDB from)
        {
            if (!Directory.Exists(Strings.Upload))
            {
                Directory.CreateDirectory(Strings.Upload);
            }

            new PngContainerCreatorNew().SaveToPngContainer(from);
            from.WriteNewPosts(false);
        }
    }
}