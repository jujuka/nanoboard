var transportUri = 'http://127.0.0.1:7543';
var _transportMsgLimit = 250000;
var _base64scale = 1 + 1/3.0;
var _maxPostSize = 65536*_base64scale + 32 + 32 + 'hashreplyTomessage{},,,"""""":::   \n\n\n'.length + 100;

function httpPost(uri, data, callback) {
  var x = new XMLHttpRequest();
  x.onreadystatechange = callback;
  x.open('POST', uri);
  x.send(data);
}

function startRecursivelySend(hash, callback) {
$.get("../rawpost/"+hash).done(function(data){
    post=convertPost(data);
    recursivelySendParentsArray(post,undefined,callback);
    });
}

function convertPost(post) {
    //Get post as str, return as json
    var conv="";
    conv=$.base64('encode', post["message"]);
    post["message"]=conv;
    return post;
}
function recursivelySendParentsArray(post, arr, callback) {
  if (arr == undefined) arr = [ post ];
  $.get('../rawpost/' + post.replyTo)
    .done(function(data){
      data = convertPost(data);
      var str = JSON.stringify(arr);
      if (str.length >= _transportMsgLimit - _maxPostSize) {
        console.log('sending to transport: ' + str);
        httpPost(transportUri, str, callback);
        arr = [];
      }
      arr.push(data);
      recursivelySendParentsArray(data, arr, callback);
    })
    .fail(function() {  // 404 - means we've reached the top
      var str = JSON.stringify(arr);
      console.log('sending to transport: ' + str);
      httpPost(transportUri, str, callback);
    });
}

/*
	This function is called each time user adds a post through web-interface.
	post - contains string with json of post object in it, where message is base64 encoded
*/
function onAdd(hash,callback) {
  startRecursivelySend(hash,callback);
}