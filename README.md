How to run tools:
  Windows:
    In command line just write tool name without *.exe extension
  Linux/OS X:
    Install mono, in command line write: mono toolname.exe

Tool: 
  Aggregator.exe
Description:
    Takes urls from places.txt (example: http://board.com/threads/2123.html)
    For each url downloads page, searches for png files and downloads them 
  to 'download' folder.
    If you create proxy.txt with proxy url in it: http://127.0.0.1:port
    it will use proxy.

Tool:
  nbpack.exe
Description:
    Run nbpack.exe without arguments to see what is does.
    In short: it is for getting posts from 'download' folder (with
  containers) and to create container. Both actions require running nanodb.

Tool:
  nanodb.exe
Description:
    Server that gives access to nanoposts database via /api endpoint, also
  redirects to /pages/index.html with web interface to the categories/threads
  (more info - later).

Typical workflow would be:
1) run nanodb.exe to start viewing threads in browser
2) when you need to get fresh posts, run prepared script that invokes 
   Aggregator and then 
   nbpack -a http://127.0.0.1:7346 nano
3) when you need to create container, call 
   npback -g http://127.0.0.1:7346 nano and container will appear in 'upload'
4) Don't forget to keep your places.txt updated.

Helper scripts already there:
run - runs nanodb server
search - calls Aggregator and nbpack in -a mode
prepare - calls nbpack in -g mode
