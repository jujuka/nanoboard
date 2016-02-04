using Newtonsoft.Json;
using NDB;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;

namespace NDB
{
    class PostDb
    {
        private readonly string _index = "index.json";
		private const string DiffFile = "diff.list";
        private const string DeletedStub = "post was deleted";
        private const string DataPrefix = "";
        private const string DataSuffix = ".db";
        private string _data = "0.db";
        private int _dataIndex = 0;
        private int _dataSize = 0;
        private const int DataLimit = 1024 * 1024 * 1024; // 1GB per chunk
        private const int CacheLimit = 1000;

        Dictionary<string, DbPostRef> _refs;
        Dictionary<string, List<DbPostRef>> _rrefs;
        HashSet<string> _deleted;
        HashSet<string> _free;
        List<string> _ordered;

        Dictionary<string,Post> _cache;

        public PostDb()
        {
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
        }

        public Post GetNthPost(int n)
        {
            if (n >= _ordered.Count) return null;
            return GetPost(_ordered[n]);
        }

        public int GetPostCount()
        {
            return _ordered.Count;
        }

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

        private void AddDbRef(DbPostRef r)
        {
            _refs[r.hash] = r;
            if (!_rrefs.ContainsKey(r.replyTo))
                _rrefs[r.replyTo] = new List<DbPostRef>();
            _rrefs[r.replyTo].Add(r);
            if (r.deleted)
                _deleted.Add(r.hash);
            if (r.deleted && r.length > 0)
                _free.Add(r.hash);
            _ordered.Add(r.hash);
        }

        // reading diff
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

        public int GetPresentCount()
        {
            return _ordered.Where(k => !_deleted.Contains(k)).ToArray().Length;
        }

        public Post[] RangePresent(int skip, int count)
        {
            return _ordered.Where(k => !_deleted.Contains(k)).Skip(skip).Take(count).Select(h => GetPost(h)).ToArray();
        }

        private bool ReputPost(Post p)
        {
            var r = _refs[p.hash];
            var bytes = Encoding.UTF8.GetBytes(p.message);
            r.length = bytes.Length;
            r.deleted = false;
            _deleted.Remove(r.hash);

            if (_free.Contains(r.hash))
            {
                _free.Remove(r.hash);
                FileUtil.Write(r.file, bytes, r.offset);
				File.AppendAllText(DiffFile, JsonConvert.SerializeObject(r) + "\n");
                return true;
            }

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

            r.offset = FileUtil.Append(_data, bytes);
            r.file = _data;
            IncreaseCheckDataSize(r.length);
			File.AppendAllText(DiffFile, JsonConvert.SerializeObject(r) + "\n");
            return true;
        }

        public bool PutPost(Post p)
        {
            if (!PostsValidator.Validate(p))
                return false;
            if (_refs.ContainsKey(p.hash) && !_deleted.Contains(p.hash))
                return false;

            if (_deleted.Contains(p.hash))
            {
                return ReputPost(p);
            }

            var r = new DbPostRef();
            r.hash = p.hash;
            r.replyTo = p.replyto;
            var bytes = Encoding.UTF8.GetBytes(p.message);
            r.length = bytes.Length;

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

            r.offset = FileUtil.Append(_data, bytes);
            r.file = _data;
            IncreaseCheckDataSize(r.length);
            AddDbRef(r);
			File.AppendAllText(DiffFile, JsonConvert.SerializeObject(r) + "\n");
            return true;
        }

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

        // cached
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
            p.message = Encoding.UTF8.GetString(chunk);

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