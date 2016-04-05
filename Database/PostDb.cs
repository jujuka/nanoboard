using Newtonsoft.Json;
using NDB;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;

namespace NDB
{
    /*
        Class that operates on posts database.
    */
    public class PostDb
    {
        private readonly string _index = "index-3.json";  // name of index file
        private const string DiffFile = "diff-3.list";    // name of file that keeps changes to the index
        private readonly string DeletedStub = "post was deleted".ToB64();   // this message returned when someone asks for deleted post
        private const string DataPrefix = "";   // will be prepended to a data chunk name
        private const string DataSuffix = ".db3"; // data chunk extension
        private string _data = "0.db3";  // initial data chunk filename
        private int _dataIndex = 0; // initial data chunk name index
        private int _dataSize = 0; // size of current data chunk will be here
        private const int DataLimit = 1024 * 1024 * 1024; // 1GB allowed to be stored inside one chunk before creating new one
        private const int CacheLimit = 1000; // how many posts to keep in memory (reduce disk read operations)

        Dictionary<string, DbPostRef> _refs; // index entries by hash
        Dictionary<string, List<DbPostRef>> _rrefs; // replies by hash
        HashSet<string> _deleted; // hashes entries marked as deleted
        HashSet<string> _free; // hashes of entries marked as deleted space of which is not used now (empty space)
        List<string> _ordered; // just all hashed from index.json in the same order as in file

        Dictionary<string,Post> _cache; // cached posts by hash

        object _lock = new object();

        public PostDb()
        {
            /* 
                Determining which data chunk file to use for new posts:
                for example check 0.db, 1.db, 2.db etc while file with size less than Limit
                will be found or stop at not existing yet file name like 3.db to use it for new posts.
            */
            for (int i = 0; i < int.MaxValue; i++)
            {
                _data = DataPrefix + i + DataSuffix;
                _dataIndex = i;

                if (!File.Exists(DataPrefix + i + DataSuffix))
                {
                    break;
                }
                else
                {
                    long length = new System.IO.FileInfo(_data).Length;
                    _dataSize = (int)length;

                    if (length > DataLimit)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            _refs = new Dictionary<string, DbPostRef>();
            _rrefs = new Dictionary<string, List<DbPostRef>>();
            _deleted = new HashSet<string>();
            _free = new HashSet<string>();
            _cache = new Dictionary<string, Post>();
            _ordered = new List<string>();
            ReadRefs();
            
            /*
                Hardcoded root post, categories post and example category post - adding them to DB below
            */
            var initialPostsStr = @"[{'hash':'bdd4b5fc1b3a933367bc6830fef72a35','message':'W2Jd0JrQkNCi0JXQk9Ce0KDQmNCYWy9iXQrQp9GC0L7QsdGLINGB0L7Qt9C00LDRgtGMINC90L7QstGD0Y4g0LrQsNGC0LXQs9C+0YDQuNGOLCDQvtGC0LLQtdGC0YzRgtC1INC90LAg0Y3RgtC+INGB0L7QvtCx0YnQtdC90LjQtS4K0J7RgtCy0LXRgtGM0YLQtSDQvdCwINC+0LTQvdGDINC40Lcg0LrQsNGC0LXQs9C+0YDQuNC5LCDRh9GC0L7QsdGLINGB0L7Qt9C00LDRgtGMINGC0LDQvCDRgtGA0LXQtC4=','replyTo':'f682830a470200d738d32c69e6c2b8a4'},{'hash':'cd94a3d60f2f521806abebcd3dc3f549','message':'W2Jd0JHRgNC10LQv0KDQsNC30L3QvtC1Wy9iXQ==','replyTo':'bdd4b5fc1b3a933367bc6830fef72a35'},{'hash':'cd94a3d60f2f521806abebcd3dc3f549','message':'W2Jd0JHRgNC10LQv0KDQsNC30L3QvtC1Wy9iXQ==','replyTo':'bdd4b5fc1b3a933367bc6830fef72a35'},{'hash':'bdd4b5fc1b3a933367bc6830fef72a35','message':'W2Jd0JrQkNCi0JXQk9Ce0KDQmNCYWy9iXQrQp9GC0L7QsdGLINGB0L7Qt9C00LDRgtGMINC90L7QstGD0Y4g0LrQsNGC0LXQs9C+0YDQuNGOLCDQvtGC0LLQtdGC0YzRgtC1INC90LAg0Y3RgtC+INGB0L7QvtCx0YnQtdC90LjQtS4K0J7RgtCy0LXRgtGM0YLQtSDQvdCwINC+0LTQvdGDINC40Lcg0LrQsNGC0LXQs9C+0YDQuNC5LCDRh9GC0L7QsdGLINGB0L7Qt9C00LDRgtGMINGC0LDQvCDRgtGA0LXQtC4=','replyTo':'f682830a470200d738d32c69e6c2b8a4'},{'hash':'f682830a470200d738d32c69e6c2b8a4','message':'e1dlbGNvbWUgdG8gTmFub2JvYXJkfQ==','replyTo':'00000000000000000000000000000000'}]";
            var initialPosts = JsonConvert.DeserializeObject<Post[]>(initialPostsStr);

            foreach (var p in initialPosts)
            {
                PutPost(p, true, true);
            }
        }

        public Post GetNthPost(int n)
        {
            if (n >= _ordered.Count) return null;
            return GetPost(_ordered[n]);
        }

        /*
            Returns recursive replies count for specified post (except deleted)
        */
        public int GetThreadSize(string hash)
        {
            if (!_rrefs.ContainsKey(hash)) return 0;
            int count = 0;
            var stack = new Stack<List<DbPostRef>>();
            stack.Push(_rrefs[hash]);

            while (stack.Count > 0)
            {
                var elem = stack.Pop();
                count += elem.ToArray().Where(r => !r.deleted).Count();

                foreach (var reply in elem.ToArray())
                {
                    if (_rrefs.ContainsKey(reply.hash))
                        stack.Push(_rrefs[reply.hash]);
                }
            }

            return count;
        }

        public int GetPostCount()
        {
            return _ordered.Count;
        }

        /*
            Increases dataSize counter and checks if it still meets the Limit,
            if not - change filename of current data chunk.
        */
        private void IncreaseCheckDataSize(int length)
        {
            _dataSize += length;

            if (_dataSize > DataLimit)
            {
                _dataIndex += 1;
                _data = DataPrefix + _dataIndex + DataSuffix;
                _dataSize = 0;
            }
        }

        /*
            Add new index entry to the instance collections.
        */
        private void AddDbRef(DbPostRef r)
        {
            _refs[r.hash] = r;
            // ensure there is a list for replies:
            if (!_rrefs.ContainsKey(r.replyTo))
                _rrefs[r.replyTo] = new List<DbPostRef>();
            _rrefs[r.replyTo].Add(r); // add as reply to it's parent
            if (r.deleted)
                _deleted.Add(r.hash);
            // if this offset-length (file area) is not used yet, mark it as free
            if (r.deleted && r.length > 0)
                _free.Add(r.hash);
            _ordered.Add(r.hash);
        }

        // reading diff
        /*
            This method is called while reading diff file
            to update index entries collection to the most recent state.
        */
		private void UpdateDbRef(DbPostRef r)
		{
			bool isNew = !_refs.ContainsKey(r.hash);
			_refs[r.hash] = r;
			if (!r.deleted && _deleted.Contains(r.hash)) {
				_deleted.Remove(r.hash);
			}
			if (!_rrefs.ContainsKey(r.replyTo))
				_rrefs[r.replyTo] = new List<DbPostRef>();
            if (isNew) 
                _rrefs[r.replyTo].Add(r);
			if (r.deleted)
				_deleted.Add(r.hash);
			if (r.deleted && r.length > 0)
				_free.Add(r.hash);
            if (r.deleted && r.length == 0)
                _free.Remove(r.hash);
			if (isNew)
				_ordered.Add(r.hash);
		}

        /*
            Reads index file and diff file, updates index file.
        */
        private void ReadRefs()
        {
			if (File.Exists (_index)) 
			{
				var indexString = File.ReadAllText(_index);
				var refs = JsonConvert.DeserializeObject<Index> (indexString).indexes;

				foreach (var r in refs) 
				{
					AddDbRef (r);
				}
			}

            if (File.Exists(DiffFile))
            {
                var diffs = File.ReadAllLines(DiffFile);

                foreach (var diff in diffs)
                {
                    var r = JsonConvert.DeserializeObject<DbPostRef>(diff);
                    UpdateDbRef(r);
                }

                File.WriteAllText(DiffFile, "");
            }

			Flush();
        }

        #region INanoDb implementation

        [Obsolete]
        public string[] GetAllHashes()
        {
            return _ordered.Where(k => !_deleted.Contains(k)).ToArray();
        }

        /*
            Returns count of not deleted posts
        */
        public int GetPresentCount()
        {
            return _ordered.Where(k => !_deleted.Contains(k)).ToArray().Length;
        }

        /*
            Returns slice of not deleted posts
        */
        public Post[] RangePresent(int skip, int count)
        {
            return _ordered.Where(k => !_deleted.Contains(k)).Skip(skip).Take(count).Select(h => GetPost(h)).ToArray();
        }
        
        /*
            Adds post back to DB after deletion.
        */
        private bool ReputPost(Post p)
        {
            var r = _refs[p.hash];
            var bytes = Encoding.UTF8.GetBytes(p.message.FromB64());
            r.length = bytes.Length;
            r.deleted = false;
            _deleted.Remove(r.hash);

            // if original file space not occupied yet - use it
            if (_free.Contains(r.hash))
            {
                _free.Remove(r.hash);
                FileUtil.Write(r.file, bytes, r.offset);
				File.AppendAllText(DiffFile, JsonConvert.SerializeObject(r) + "\n");
                return true;
            }

            // else try to find best empty area in some file
            else if (_free.Any())
            {
                DbPostRef best = null;
                int min = int.MaxValue;
                var freeArr = _free.ToArray();

                for (int i = 0; i < _free.Count; i++)
                {
                    var fr = _refs[freeArr[i]];

                    if (fr.length <= r.length)
                    {
                        int diff = r.length - fr.length;

                        if (diff < min)
                        {
                            min = diff;
                            best = fr;
                        }
                    }
                }

                if (best != null)
                {
                    best.length = 0;
                    _free.Remove(best.hash);
                    r.offset = best.offset;
                    FileUtil.Write(best.file, bytes, r.offset);
                    r.file = best.file;
                    best.file = null;
                    File.AppendAllText(DiffFile, JsonConvert.SerializeObject(best) + "\n");
					File.AppendAllText(DiffFile, JsonConvert.SerializeObject(r) + "\n");
                    return true;
                }
            }
            
            // no apropriate space was found - extend current data chunk
            r.offset = FileUtil.Append(_data, bytes);
            r.file = _data;
            IncreaseCheckDataSize(r.length);
			File.AppendAllText(DiffFile, JsonConvert.SerializeObject(r) + "\n");
            return true;
        }

        public bool PutPost(Post p, bool allowReput = false, bool bypassValidation = false)
        {
            if (!bypassValidation && !PostsValidator.Validate(p)) // do not add posts that fail validation
                return false;

            lock (_lock)
            {
                if (_refs.ContainsKey(p.hash) && !_deleted.Contains(p.hash)) // do not add existing not deleted posts
                return false;

                // if posts was deleted, add it back if allowed in allowReput param
                if (allowReput && _deleted.Contains(p.hash))
                {
                    return ReputPost(p);
                }

                // start creating new index entry
                var r = new DbPostRef();
                r.hash = p.hash;
                r.replyTo = p.replyto;
                var bytes = Encoding.UTF8.GetBytes(p.message.FromB64());
                r.length = bytes.Length;

                // try to find some empty space in db file chunks
                if (_free.Any())
                {
                    DbPostRef best = null;
                    int min = int.MaxValue;
                    var freeArr = _free.ToArray();

                    for (int i = 0; i < _free.Count; i++)
                    {
                        var fr = _refs[freeArr[i]];

                        if (fr.length <= r.length)
                        {
                            int diff = r.length - fr.length;

                            if (diff < min)
                            {
                                min = diff;
                                best = fr;
                            }
                        }
                    }

                    // found some useful place - write post's message into it,
                    // also update diff file immediately
                    if (best != null)
                    {
                        best.length = 0;
                        _free.Remove(best.hash);
                        r.offset = best.offset;
                        FileUtil.Write(best.file, bytes, r.offset);
                        r.file = best.file;
                        best.file = null;
                        AddDbRef(r);
                        File.AppendAllText(DiffFile, JsonConvert.SerializeObject(best) + "\n");
                        File.AppendAllText(DiffFile, JsonConvert.SerializeObject(r) + "\n");
                        return true;
                    }
                }

                // no appropriate space was found - extend current data chunk file
                r.offset = FileUtil.Append(_data, bytes);
                r.file = _data;
                IncreaseCheckDataSize(r.length);
                AddDbRef(r);
                File.AppendAllText(DiffFile, JsonConvert.SerializeObject(r) + "\n");
            }

            return true;
        }

        /*
            Mark post as deleted, erase it's message bytes (write zeros to the file),
            update diff file
        */
        public bool DeletePost(string hash)
        {
            if (!_refs.ContainsKey(hash) || _deleted.Contains(hash))
                return false;
            if (_cache.ContainsKey(hash))
                _cache.Remove(hash);
            _refs[hash].deleted = true;
            _deleted.Add(hash);
            _free.Add(hash);
            FileUtil.Write(_data, new byte[_refs[hash].length], _refs[hash].offset);
            File.AppendAllText(DiffFile, JsonConvert.SerializeObject(_refs[hash]) + "\n");
            return true;
        }

        /*
            Get post from DB by hash.
            Returns null if not found.
            This method does caching of recently requested posts for faster response.
        */
        public Post GetPost(string hash)
        {
            if (_cache.ContainsKey(hash))
                return _cache[hash];
            if (!_refs.ContainsKey(hash))
                return null;
            var r = _refs[hash];

            if (r.deleted)
            {
                var p1 = new Post(r.replyTo, DeletedStub);
                p1.hash = r.hash;
                return p1;
            }

            var chunk = FileUtil.Read(_data, r.offset, r.length);
            var p = new Post();
            p.hash = hash;
            p.replyto = r.replyTo;
            p.message = Encoding.UTF8.GetString(chunk).ToB64();

            // clear half of cache if cache limit was met
            if (_cache.Keys.Count > CacheLimit)
            {
                while (_cache.Keys.Count > CacheLimit - CacheLimit/10)
                {
                    _cache.Remove(_cache.Keys.First());
                }
            }

            _cache[hash] = p;
            return p;
        }

        public Post[] GetReplies(string hash)
        {
            if (!_rrefs.ContainsKey(hash))
                return new Post[0];
            var res = new Post[_rrefs[hash].Count];
            var rrefs = _rrefs[hash].ToArray();

            for (int i = 0; i < rrefs.Length; i++)
            {
                var rref = rrefs[i];
                res[i] = GetPost(rref.hash);
            }

            return res;
        }

        /*
            Rewrite index.json. Called once at start, during runtime all changes go
            to the diff file - new line for each change.
        */
        private void Flush()
        {
            var index = new Index();
            index.indexes = _refs.Values.ToArray();
            var json = JsonConvert.SerializeObject(index, Formatting.None);
            File.WriteAllText(_index, json);
        }
        #endregion
    }
}
