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
    class ImageBase64ConvertHandler : IRequestHandler
    {
        private static string GeneratePage() { 
            return @"
 <!DOCTYPE html>
<html>
<head>
<style>
#result {
  word-break: break-all;
  max-height: 50px;
  max-width: 400px;
  width: 400px;
  height: 50px;
  overflow: auto;
}

img {
  width: auto;
  height: auto;
}

.hide {
  display: none;
}

button {
  border: .5px solid #888;
  border-radius: 4px;
  background-color: #eee;
  color: black;
}

button:hover {
  background-color: #f4f4f4;
}

button:active {
  color: black;
  background-color: #ddd;
}


body {
  font-family: Tahoma, sans-serif;
  font-size: 70%;
  padding: 20px;
}

.range1 {
  width: 300px;
}
</style>
<script>"+ HtmlStringExtensions.JQueryMinJs +@"</script>
<script>
$(function(){
/// sharpen image:
/// USAGE:
///    sharpen(context, width, height, mixFactor)
///  mixFactor: [0.0, 1.0]
function sharpen(ctx, w, h, mix) {

  var weights = [0, -1, 0, -1, 5, -1, 0, -1, 0],
    katet = Math.round(Math.sqrt(weights.length)),
    half = (katet * 0.5) | 0,
    dstData = ctx.createImageData(w, h),
    dstBuff = dstData.data,
    srcBuff = ctx.getImageData(0, 0, w, h).data,
    y = h;

  while (y--) {

    x = w;

    while (x--) {

      var sy = y,
        sx = x,
        dstOff = (y * w + x) * 4,
        r = 0,
        g = 0,
        b = 0,
        a = 0;

      for (var cy = 0; cy < katet; cy++) {
        for (var cx = 0; cx < katet; cx++) {

          var scy = sy + cy - half;
          var scx = sx + cx - half;

          if (scy >= 0 && scy < h && scx >= 0 && scx < w) {

            var srcOff = (scy * w + scx) * 4;
            var wt = weights[cy * katet + cx];

            r += srcBuff[srcOff] * wt;
            g += srcBuff[srcOff + 1] * wt;
            b += srcBuff[srcOff + 2] * wt;
            a += srcBuff[srcOff + 3] * wt;
          }
        }
      }

      dstBuff[dstOff] = r * mix + srcBuff[dstOff] * (1 - mix);
      dstBuff[dstOff + 1] = g * mix + srcBuff[dstOff + 1] * (1 - mix);
      dstBuff[dstOff + 2] = b * mix + srcBuff[dstOff + 2] * (1 - mix)
      dstBuff[dstOff + 3] = srcBuff[dstOff + 3];
    }
  }

  ctx.putImageData(dstData, 0, 0);
}

var _loader;

function updateImage(loader) {
  _loader = loader;
  var file = _loader.files[0];
  var reader = new FileReader();
  reader.onloadend = function() {
    var res = reader.result;
    if ($('#imgtype').val().startsWith('No compression')) {
      $('#result').text(res.toString());
        $('#info').html('Length (base64): ' + res.length +
          '<br>Max allowed: 64512');
        if (res.length > 64512) {
          $('.output').find('img').attr('src', 'error');
          $('#info').css('color','red');
        } else {
          $('#result').text('[img='+res.substring(res.indexOf(',')+1)+']');
          $('.output').find('img').attr('src', res);
          $('#info').css('color','black');
        }
      return;
    }
    var canvas = document.createElement('canvas');
    var ctx = canvas.getContext('2d');
    img = new Image();
    img.onload = function() {
      canvas.width = img.width;
      canvas.height = img.height;
      ctx.drawImage(img, 0, 0, img.width, img.height, 0, 0, img.width, img.height);
      sharpen(ctx, img.width, img.height, $('#sharpness').val() / 100.0);
      var scale = 1.0 / ($('#scale').val() / 100.0);
      var shr = new Image();
      shr.onload = function() {
        canvas.width = img.width / scale;
        canvas.height = img.height / scale;
        ctx.drawImage(
          shr, 0, 0, img.width, img.height, 0, 0,
          img.width / scale, img.height / scale);
        var imgType = $('#imgtype').val()=='JPEG'?'image/jpeg':'image/webp';
        var dataURL = canvas.toDataURL(imgType, $('#quality').val() / 100.0);
        $('#info').html('Length (base64): ' + dataURL.length +
          '<br>Max allowed: 64512<br>'+Math.floor(img.width/scale)+'x'+Math.floor(img.height/scale)+'px');
        if (dataURL.length > 64512) {
          $('#info').css('color','red');
          $('#result').text('error');
          $('.output').find('img').attr('src', 'error');
        } else {
          $('#info').css('color','black');
          $('#result').text('[img='+dataURL.substring(dataURL.indexOf(',')+1)+']');
          $('.output').find('img').attr('src', dataURL);
        }
      };
      var shrd = canvas.toDataURL();
      shr.src = shrd;
    }
    img.src = res;
  }
  reader.readAsDataURL(file);
}
/*
$('#update').click(function() {
  updateImage(_loader);
});
*/
$('#imgtype').change(function() {
  updateImage(_loader);
});


$('#sharpness').change(function() {
  updateImage(_loader);
});
$('#quality').change(function() {
  updateImage(_loader);
});
$('#scale').change(function() {
  updateImage(_loader);
});

$('#show').click(function() {
  $('#imgres').toggleClass('hide');
  $('#result').toggleClass('hide');
  $('#resulthelp').toggleClass('hide');
});

$('#inputFileToLoad').change(function() {
  updateImage(this)
});
});
</script>
</head>
<body>
<h1>Image2Base64</h1>
<form class='input-group' id='img2b64'>
  <input id='inputFileToLoad' type='file' />
</form>
<br/>
<div class='range1'>
  Scale:
  <br>
  <input id='scale' type='range' min=1 max=100 />
  <br>Quality:
  <br>
  <input id='quality' type='range' min=1 max=100 />
  <br> Sharpness:
  <br>
  <input id='sharpness' type='range' min=0 max=100 />
  <br>Type:<br/>  
  <select id='imgtype'>
    <option>JPEG</option>
    <option>WebP (Chrome only)</option>
    <option>No compression (for zipJPEGs)</option>
  </select><br>
</div><br>
<!-- <button id='update'>Apply settings</button> -->


<div id='info'></div>
<br/>
<button id='show'>Switch Base64/Image</button>

<div class='output'>
  <img id='imgres'>
</div>
<div class='hide' id='resulthelp'>Triple click, Ctrl/CMD+C:</div>
<div class='hide' id='result'></div>
</body>
</html>
"; 
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            return new NanoHttpResponse(StatusCode.Ok, GeneratePage());
        }
    }
}