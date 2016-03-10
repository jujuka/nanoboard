function shortenHash(hash) {
  return hash.substring(0,4) + '..' + hash.substring(28,32);
}

function replaceAll(text, search, replace) {
  return text.split(search).join(replace);
}

function escapeTags(text) {
  return text
    .replace(/>/gim, '&gt;')
    .replace(/</gim, '&lt;');
}

function detectImages(text) {
  var prefix = 'data:image/jpeg;base64,';
  var matches = text.match(/\[img=[A-Za-z0-9+\/=]{4,64512}\]/g);
  if (matches != null) {
    for (var i = 0; i < matches.length; i++) {
      var value = matches[i].toString();
      value = value.substring(5);
      value = value.substring(0, value.length - 1);
      value = '<img src="' + prefix + value + '" />';
      text = replaceAll(text, matches[i], value);
    }
  }
  return text;
}

function detectPlacesCommands(obj) {
  var html = obj.html();
  var incl = ''.includes == undefined ? function(x,y) { return x.contains(y); } : function(x,y) { return x.includes(y); };
  var matches = html.match(/[^"]{1}https?:\/\/[A-Za-z%&\?\-=_\.0-9\/:#]+/g);
  if (matches == null) return;
  $.get('../api/paramget/places')
    .done(function(arr){
      arr = arr.split('\n');
      for (var i = 0; i < matches.length; i++) {
        var value = matches[i].toString();
        value = value.substring(1);
        if (arr.indexOf(value) != -1) {
          html = replaceAll(html, matches[i], matches[i] + ' <i><sup>added</sup></i>');
        }
      }
      obj.html(html);
    });
}

function detectURLs(text) {
  var matches = text.match(/https?:\/\/[A-Za-z%&\?\-=_\.0-9\/:#]+/g);
  var you_re=new RegExp(".*youtube\.com.*")
  if (matches != null) {
    for (var i = 0; i < matches.length; i++) {
      var value = matches[i].toString();
      if (you_re.test(value))
      {
        value ='<a class="vd-vid" href="'+value+'"><span class="glyphicon glyphicon-play" aria-hidden="true"></span>'+value+'</a>'
        text = replaceAll(text, matches[i], value);
      
      }
      else
      {
        value = '<a target=_blank href="'+value+'">'+value+'</a>';
        text = replaceAll(text, matches[i], value);
      }
    }
  }
  return text;
}

function detectThreadLinks(text) {
var matches = text.match(/&gt;&gt;[a-f0-9]{32}/g);
  if (matches != null) {
    for (var i = 0; i < matches.length; i++) {
      var value = matches[i].toString();
      value = value.substring(8, value.length);
      value = '<a href="javascript:void(0);" onclick=_depth=2;loadThread("'+value+'")>&gt;&gt;' + value + '</a>';
      text = replaceAll(text, matches[i], value);
    }
  }
  return text;
}

function applyFormatting(text) {
  text = text.replace(/\[sp(oiler|)\]/gim, '[x]');
  text = text.replace(/\[\/sp(oiler|)\]/gim, '[/x]');
  var tags = 'biusxg';
  for (var x = 0; x < tags.length; x++) {
    var ch = tags.charAt(x);
    text = text
      .replace(new RegExp("\\[" + ch + "\\]", 'gim'), '<' + ch + '>')
      .replace(new RegExp("\\[/" + ch + "\\]", 'gim'), '</' + ch + '>');
  }
  text = detectImages(text);
  text = detectThreadLinks(text);
  text = replaceAll(text, '\n', '<br/>');
  text = replaceAll(text, '  ', '&nbsp; ');
  if (_detectURLs == 'true') text = detectURLs(text);
  return text
    .replace(/<x>/gim, '<sp>')
    .replace(/<\/x>/gim, '</sp>');
}

(function($) {
  $.fn.extend({
    addTemporaryClass: function(className, duration) {
      var elements = this;
      setTimeout(function() {
        elements.removeClass(className);
      }, duration);
      return this.each(function() {
        $(this).addClass(className);
      });
    }
  });
})(jQuery);
