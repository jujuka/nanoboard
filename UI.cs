using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace nboard
{
    class UI
    {
        Form _form;
        FlowLayoutPanel _panel;
        NanoDB _db;
        bool lockFocus = false;

        Form _updates;
        FlowLayoutPanel _updatesPanel;

        Form _bookmarks;
        FlowLayoutPanel _bookmarksPanel;

        public UI(Form form, NanoDB db)
        {
            var aggregator = new Aggregator();
            _db = db;
            _form = form;
            _form.Text = "Threads";

            _bookmarks = new Form();
            _bookmarks.Text = "Bookmarks";
            _bookmarks.BackColor = Color.FromArgb(0xbb, 0xbb, 0xbb);
            _bookmarks.Width = Screen.AllScreens[0].Bounds.Width / 4;
            _bookmarks.Height = Screen.AllScreens[0].Bounds.Height / 2;
            _bookmarks.Left = Screen.AllScreens[0].Bounds.Width - _bookmarks.Width - 15;
            _bookmarks.StartPosition = FormStartPosition.Manual;

            var bookmarks = db.Bookmarked;

            foreach (var b in bookmarks)
            {
                AddBookmark(b);
            }

            db.BookmarkAdded += AddBookmark;

            _updates = new Form();
            _updates.Text = "Updates";
            _updates.BackColor = Color.FromArgb(0xbb, 0xbb, 0xbb);
            _updates.Width = Screen.AllScreens[0].Bounds.Width/4;
            _updates.Height = Screen.AllScreens[0].Bounds.Height/2;
            _updates.Left = 15;
            _updates.StartPosition = FormStartPosition.Manual;



            _bookmarks.Controls.Add(_bookmarksPanel);
            _bookmarks.Show();

            _updatesPanel = new FlowLayoutPanel();
            _updatesPanel.AutoSize = false;
            _updatesPanel.AutoScroll = true;
            _updatesPanel.Dock = DockStyle.Fill;
            _updatesPanel.FlowDirection = FlowDirection.TopDown;
            _updatesPanel.WrapContents = false;
            _updatesPanel.MouseEnter += (sender,args) => { if (!lockFocus) _panel.Focus(); };
            _updates.Controls.Add(_updatesPanel);
            _updates.Show();

            _form.BackColor = Color.FromArgb(0xee,0xee,0xee);
            _form.ForeColor = Color.FromArgb(0x33,0x33,0x33);
            _form.Width = Screen.AllScreens[0].Bounds.Width/2;
            _form.Height = Screen.AllScreens[0].Bounds.Height/2;
            var menu = new ContextMenu();
            bool menuOn = false;
            menu.MenuItems.Add(new MenuItem("Back", (sender, args) => {
                menuOn = false;
                Update(_db.GetThreadPosts(_back));
            }));
            menu.MenuItems.Add(new MenuItem("Create PNG", (sender, args) => {
                menuOn = false;
                new PngMailer().FillOutbox(db);
            }));

            var mi = new MenuItem("Search nanoposts", (sender, args) => {
                menuOn = false;
                if (aggregator.InProgress == 0)
                {
                    _form.Text = "Threads. Starting search...";
                    aggregator.Aggregate();
                }
            });
            aggregator.ProgressChanged += () => {
                if (aggregator.InProgress > 0)
                {
                    _form.Text = "Threads. Searching (" + aggregator.InProgress + ")";
                    mi.Text = "Searching (" + aggregator.InProgress + ")";
                }

                else
                {
                    _form.Text = "Threads";
                    mi.Text = "Search nanoposts";
                    new PngMailer().ReadInbox(db);
                }
            };
            menu.MenuItems.Add(mi);
            /*menu.MenuItems.Add(new MenuItem("Search settings", (sender, args) => {
                menuOn = false;
            }));*/
            menu.MenuItems.Add(new MenuItem("Quit", (sender, args) => {
                menuOn = false;
                _form.Close();
                Application.Exit();
            }));

            _panel = new FlowLayoutPanel();
            _form.Controls.Add(_panel);
            _form.StartPosition = FormStartPosition.CenterScreen;
            _panel.AutoSize = false;
            _panel.AutoScroll = true;
            _panel.Dock = DockStyle.Fill;
            _panel.FlowDirection = FlowDirection.TopDown;
            _panel.WrapContents = false;
            _panel.MouseEnter += (sender,args) => { if (!lockFocus) _panel.Focus(); };

            _panel.MouseClick += (object sender, MouseEventArgs e) => {
                if (_back != null)
                {
                    menuOn = !menuOn;
                    if (menuOn)
                        menu.Show(_panel, e.Location);
                }
            };

            _db.Updated += (NanoPost post) => {
                var label = new Label();
                label.Text = post.Message + " (" + _db.CountAnswers(post.GetHash()) + ")";
                label.Font = FontProvider.Get();
                label.BackColor = Color.FromArgb(0xdd, 0xdd, 0xdd);
                label.ForeColor = Color.FromArgb(0x33, 0x33, 0x33);
                label.AutoSize = true;
                label.BorderStyle = BorderStyle.None;
                label.Margin = new Padding(6, 3, 6, 3);
                label.Padding = new Padding(6, 6, 8, 2);
                label.MouseClick += (object sender, MouseEventArgs e) => {
                    Update(_db.GetThreadPosts(post.ReplyTo));
                    _updatesPanel.Controls.Remove(label);
                };
                _updatesPanel.Invoke(new Action(()=>{_updatesPanel.Controls.Add(label);}));
                //_updatesPanel.Controls.Add(label);
            };
        }

        private void AddBookmark(NanoPost post)
        {
            if (post == null)
                return;
            if (_bookmarksPanel == null)
            {
                _bookmarksPanel = new FlowLayoutPanel();
                _bookmarksPanel.AutoSize = false;
                _bookmarksPanel.AutoScroll = true;
                _bookmarksPanel.Dock = DockStyle.Fill;
                _bookmarksPanel.FlowDirection = FlowDirection.TopDown;
                _bookmarksPanel.WrapContents = false;
                _bookmarksPanel.MouseEnter += (sender,args) => { if (!lockFocus) _bookmarksPanel.Focus(); };
            }

            var label = new Label();
            label.Text = post.Message;
            label.Font = FontProvider.Get();
            label.BackColor = Color.FromArgb(0xdd, 0xdd, 0xdd);
            label.ForeColor = Color.FromArgb(0x33, 0x33, 0x33);
            label.AutoSize = true;
            label.BorderStyle = BorderStyle.None;
            label.Margin = new Padding(6, 3, 6, 3);
            label.Padding = new Padding(6, 6, 8, 2);
            label.MouseClick += (object sender, MouseEventArgs e) => {
                Update(_db.GetThreadPosts(post.GetHash()));
            };
            label.MouseDoubleClick += (object sender, MouseEventArgs e) => {
                Update(_db.GetThreadPosts(post.GetHash()));
                _bookmarksPanel.Controls.Remove(label);
                _db.PostRemoveFromBookmarks(post.GetHash());
            };
            if (_bookmarksPanel.InvokeRequired)
                _bookmarksPanel.Invoke(new Action(()=>_bookmarksPanel.Controls.Add(label)));
            else
                _bookmarksPanel.Controls.Add(label);
        }

        private void ShowReply(NanoDB db, NanoPost to, Control origin)
        {
            var tb = new TextBox();
            tb.Multiline = true;
            tb.Width = (int)(_form.Width / 2f);
            tb.Height = (int)(_form.Height / 3f);
            tb.Text = ">" + to.Message.Replace("\n", "\n>") + "\n";
            origin.Parent.Controls.Add(tb);
            var ok = new Button();
            var cancel = new Button();
            ok.Text = "Send";
            cancel.Text = "Cancel";
            lockFocus = true;
            var i = origin.Parent.Controls.GetChildIndex(origin);
            var panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.WrapContents = false;
            panel.Height = (int)(ok.Height * 1.5f);
            panel.Controls.Add(ok);
            panel.Controls.Add(cancel);
            origin.Parent.Controls.Add(panel);
            origin.Parent.Controls.SetChildIndex(tb, i+1);
            origin.Parent.Controls.SetChildIndex(panel, i+2);
            ok.Click += (object sender, EventArgs e) => {
                lockFocus = false;
                panel.Parent.Controls.Remove(panel);
                tb.Parent.Controls.Remove(tb);
                NanoPost newPost = new NanoPost(to.GetHash(), tb.Text);
                _db.AddPost(newPost);
                //var label = AddPost(newPost, new NanoPost[]{to});
                //_panel.Controls.Add(label);
                //_form.Invalidate(true);
                Update(_db.GetThreadPosts(newPost.ReplyTo));
            };
            cancel.Click += (object sender, EventArgs e) => {
                lockFocus = false;
                panel.Parent.Controls.Remove(panel);
                tb.Parent.Controls.Remove(tb);
            };
            _form.Invalidate(true);
            tb.Focus();
        }

        private Hash _back;

        private Label AddPost(NanoPost post, NanoPost[] posts)
        {
            var label = new Label();
            label.Text = post.Message + " (" + _db.CountAnswers(post.GetHash()) + ")";
            label.Font = FontProvider.Get();
            label.BackColor = Color.FromArgb(0xdd, 0xdd, 0xdd);
            label.ForeColor = Color.FromArgb(0x33, 0x33, 0x33);
            label.AutoSize = true;
            label.BorderStyle = BorderStyle.None;

            label.Margin = new Padding(6, 3, 6, 3);
            label.Padding = new Padding(6, 6, 8, 2);
            var menu = new ContextMenu();
            label.MouseClick += (object sender, MouseEventArgs e) => {
                menu.Show(label, e.Location);
            };
            menu.MenuItems.Add(new MenuItem("Show Thread", (sender, args) => {
                Update(_db.GetThreadPosts(post.GetHash()));
            }));
            _back = posts[0].ReplyTo;
            menu.MenuItems.Add(new MenuItem("Back", (sender, args) => {
                Update(_db.GetThreadPosts(posts[0].ReplyTo));
            }));
            menu.MenuItems.Add(new MenuItem("Hide", (sender, args) => {
                _db.Hide(post.GetHash());
                Update(_db.GetThreadPosts(posts[0].GetHash()));
            }));
            menu.MenuItems.Add(new MenuItem("Add To Bookmarks", (sender, args) => {
                _db.AddToBookmarks(post.GetHash());
            }));
            menu.MenuItems.Add(new MenuItem("Send Reply", (sender, args) => {
                ShowReply(_db, post, label);
            }));
            /*label.DoubleClick += (object sender, EventArgs e) => {
                Update(_db.GetThreadPosts(post.GetHash()));
            };*/
            label.ContextMenu = menu;
            label.MouseEnter += (sender,args) => { if (!lockFocus) _panel.Focus(); };
            return label;
        }

        public void Update(NanoPost[] posts)
        {
            posts = posts.ExceptHidden(_db);
            _panel.Controls.Clear();
            _panel.SuspendLayout();
            bool first = true;

            foreach (var p in posts)
            {
                var label = AddPost(p, posts);
                _panel.Controls.Add(label);

                if (first)
                {
                    first = false;
                    label.BackColor = Color.Orange;
                }
            }

            _panel.ResumeLayout(true);
            _form.Invalidate(true);
        }
    }
}