using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Linq;

namespace nboard
{
    class DumpHandler : IRequestHandler
    {
        private readonly NanoDB _db;

        public DumpHandler(NanoDB db)
        {
            _db = db;
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            Hash hash = new Hash(request.Address.Split('/').Last());
            List<NanoPost> all = new List<NanoPost>(_db.GetExpandedThreadPosts(hash));
            //
            // recursively hide posts
            foreach (NanoPost p in all)
            {
                if (_db.IsHidden(p.GetHash()))
                {
                    var children = _db.GetExpandedThreadPosts(p.GetHash());

                    foreach (var child in children)
                    {
                        _db.Hide(child.GetHash());
                    }
                }
            }

            List<NanoPost> list = new List<NanoPost>(all.Where(p => !_db.IsHidden(p.GetHash())));

            new PngContainerCreator(_db).CreateWithList(list);
            return new NanoHttpResponse(StatusCode.Ok, "\n");
        }
    }
}