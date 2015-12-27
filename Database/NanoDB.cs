using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace nboard
{
    class NanoDB
    {
        static Random random = new Random();
        private const string Index = "index.db";
        private const string Data = "data.db";
        private const string HideList = "hide.list";
        //private const string Bookmarks = "bookmarks.list";

        private readonly Dictionary<Hash, NanoPost> _posts;
        private readonly List<NanoPost> _addedPosts;
        private readonly HashSet<Hash> _threads;
        private readonly Dictionary<Hash, List<NanoPost>> _threadPosts;
        private readonly HashSet<NanoPost> _new;
        private readonly HashSet<string> _hideList;
        private readonly HashSet<string> _bookmarks;

        public Hash RootHash { get; private set; }

        public event Action<NanoPost> Updated = delegate(NanoPost obj) {};
        //public event Action<NanoPost> BookmarkAdded = delegate(NanoPost obj) {};

        public NanoDB()
        {
            _posts = new Dictionary<Hash, NanoPost>();
            _addedPosts = new List<NanoPost>();
            _threads = new HashSet<Hash>();
            _new = new HashSet<NanoPost>();
            _threadPosts = new Dictionary<Hash, List<NanoPost>>();
            _hideList = new HashSet<string>();
            _bookmarks = new HashSet<string>();
            var root = new NanoPost(Hash.CreateZero(), NanoPost.RootStub);
            AddPost(root, false);
            RootHash = root.GetHash();
            if (File.Exists(HideList))
            {
                _hideList = new HashSet<string>(File.ReadAllLines(HideList));
            }

            /*if (File.Exists(Bookmarks))
            {
                _bookmarks = new HashSet<string>(File.ReadAllLines(Bookmarks));
            }*/

            //AddToBookmarks(RootHash);
        }

        public Hash[] Threads
        {
            get
            {
                return _threads.ToArray();
            }
        }

        public bool IsHiddenListEmpty()
        {
            return _hideList.Count == 0;
        }

        public bool IsHidden(Hash hash)
        {
            return _hideList.Contains(hash.Value) || _hideList.Contains(hash.Value + "\n"); // TODO: find correct part
        }

        public void Hide(Hash hash)
        {
            if (_hideList.Contains(hash.Value)) return;
            if (hash.Zero) return;
            if (hash.Value == RootHash.Value) return;
            _hideList.Add(hash.Value);
            File.AppendAllText(HideList, hash.Value + "\n");
        }

        /*
        public void AddToBookmarks(Hash hash)
        {
            if (_bookmarks.Contains(hash.Value)) return;
            _bookmarks.Add(hash.Value);
            File.AppendAllText(Bookmarks, hash.Value + "\n");
            BookmarkAdded(Get(hash));
        }

        public void PostRemoveFromBookmarks(Hash hash)
        {
            if (!_bookmarks.Contains(hash.Value))
                return;
            _bookmarks.Remove(hash.Value);
            File.Delete(Bookmarks);
            foreach (var b in _bookmarks)
            {
                File.AppendAllText(Bookmarks, b + "\n");
            }
        }
        */

        public NanoPost[] Bookmarked
        {
            get
            {
                return _bookmarks.Select(b => Get(new Hash(b))).ToArray();
            }
        }

        public NanoPost Get(Hash hash)
        {
            if (!_posts.ContainsKey(hash))
            {
                return null;
            }

            return _posts[hash];
        }

        public int CountAnswers(Hash thread)
        {
            return _threadPosts.ContainsKey(thread) ? _threadPosts[thread].ToArray().ExceptHidden(this).Length : 0;
        }

        public NanoPost[] GetExpandedThreadPosts(Hash thread, int depth = 0, List<NanoPost> list = null)
        {
            if (list == null)
            {
                list = new List<NanoPost>();
            }

            if (depth == 0)
            {
                // clear depth
                foreach (var p in _posts)
                {
                    p.Value.DepthTag = 0;
                }
            }


            if (!_threadPosts.ContainsKey(thread))
            {
                if (_posts.ContainsKey(thread))
                {
                    _posts[thread].DepthTag = depth;
                    list.Add(_posts[thread]);
                    return new NanoPost[] { _posts[thread] };
                }
                else
                {
                    return new NanoPost[0];
                }
            }


            if (depth == 0 && _posts.ContainsKey(thread))
            {
                _posts[thread].DepthTag = depth;
                list.Add(_posts[thread]);
            }

            foreach (var tp in _threadPosts[thread])
            {
                tp.DepthTag = depth + 1;
                list.Add(tp);
                GetExpandedThreadPosts(tp.GetHash(), depth + 1, list);
            }

            return list.Distinct().ToArray();
        }

        public NanoPost[] GetThreadPosts(Hash thread)
        {
            if (!_threadPosts.ContainsKey(thread))
            {
                if (_posts.ContainsKey(thread))
                {
                    return new NanoPost[] { _posts[thread] };
                }

                else
                {
                    return new NanoPost[0];
                }
            }

            var list = new List<NanoPost>();

            if (_posts.ContainsKey(thread))
            {
                list.Add(_posts[thread]);
            }

            list.AddRange(_threadPosts[thread].ToArray().Sorted());
            list.ForEach(p => p.DepthTag = 0);
            return list.ToArray();
        }

        public NanoPost[] GetNewPosts()
        {
            return _new.ToArray();
        }

        public NanoPost[] GetNLastPosts(int count)
        {
            if (count > _addedPosts.Count)
            {
                return _addedPosts.ToArray();
            }

            return _addedPosts.GetRange(_addedPosts.Count - count, count).ToArray();
        }

        public NanoPost[] GetNRandomPosts(int count)
        {
            List<NanoPost> posts = new List<NanoPost>();

            for (int i = 0; i < count; i++)
            {
                posts.Add(_addedPosts[random.Next(_addedPosts.Count-1)]);
            }

            return posts.ToArray();
        }

        public bool AddPost(NanoPost post)
        {
            if (post.Invalid) return false;
            return AddPost(post, true);
        }

        private bool AddPost(NanoPost post, bool isNew)
        {
            if (_posts.ContainsKey(post.GetHash()))
            {
                return false;
            }

            if (isNew)
            {
                _new.Add(post);
            }

            _addedPosts.Add(post);
            _posts[post.GetHash()] = post;
            _threads.Add(post.ReplyTo);

            if (!_threadPosts.ContainsKey(post.ReplyTo))
            {
                _threadPosts[post.ReplyTo] = new List<NanoPost>();
            }

            _threadPosts[post.ReplyTo].Add(post);

            if (post.ReplyTo.Zero || post.ReplyTo.Value == RootHash.Value)
            {
                _threads.Add(post.GetHash());
            }

            if (isNew)
            {
                Updated(post);
            }

            return true;
        }

        public void WriteNewPosts(bool clear = true)
        {
            int offset = 0;

            if (File.Exists(Data))
            {
                FileInfo dataInfo = new FileInfo(Data);
                offset = (int)dataInfo.Length;
            }

            foreach (var p in _new)
            {
                var @string = p.SerializedString();
                FileUtils.AppendAllBytes(Index, Encoding.UTF8.GetBytes(offset.ToString("x8")));
                FileUtils.AppendAllBytes(Index, Encoding.UTF8.GetBytes(@string.Length.ToString("x8")));
                FileUtils.AppendAllBytes(Data, p.SerializedBytes());
                offset += @string.Length;
            }

            if (clear)
            {
                _new.Clear();
            }
        }

        public void ReadPosts()
        {
            if (!File.Exists(Index) || !File.Exists(Data))
                return;

            string indexes = Encoding.UTF8.GetString(File.ReadAllBytes(Index));
            string posts = Encoding.UTF8.GetString(File.ReadAllBytes(Data));

            try
            {

                for (int i = 0; i < indexes.Length / 8; i += 2)
                {
                    string offset = indexes.Substring(i * 8, 8);
                    string length = indexes.Substring(i * 8 + 8, 8);
                    string rawpost = posts.Substring(
                                     int.Parse(offset, System.Globalization.NumberStyles.HexNumber), 
                                     int.Parse(length, System.Globalization.NumberStyles.HexNumber));
                    AddPost(new NanoPost(rawpost), false);
                }
            }

            catch
            {
            }
        }

        public void CropDbFiles(int postsToLeft)
        {
            throw new NotImplementedException();
        }

        public void DeleteDbFiles()
        {
            if (!File.Exists(Index) || !File.Exists(Data)) return;
        
            File.Delete(Index);
            File.Delete(Data);
        }
    }
}