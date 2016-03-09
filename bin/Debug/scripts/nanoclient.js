function numSuffix(numStr) {
  if (numStr.endsWith('11')) return 's';
  if (numStr.endsWith('1')) return '';
  return 's';
}

function addPost(post, appendFunc, hasShowButton, short) {
  var locationBackup = window.location.href.toString();
  if (_depth > 1) hasShowButton = false;
  if (short == undefined) short = true;
  var d = $(document.createElement('div'));
  d
    .addClass('post')
    .attr('id', post.hash);
  if (_depth != 0)
    d.append('<gr>#' + (short&&_depth!=1?shortenHash(post.hash):post.hash) + '&nbsp;</gr>');
  if (_depth != 0) {
    $('<a>')
      .attr('href', '#' + post.replyTo)
      .click(function() {
        $('#' + post.replyTo)
          .addTemporaryClass('high', 1000);
        setTimeout(function(){
          console.log('assigning location');
          _location = locationBackup;
          location.assign(locationBackup);
        }, 200);
      })
      .appendTo(d)
      .html('^' + shortenHash(post.replyTo));
  }
  d.append('&nbsp;');
  d
    .append($('<a>')
      .attr('href', 'javascript:void(0)')
      .html('<span class="glyphicon glyphicon-pencil" aria-hidden="true"></span><span class="btn-title">&thinsp;Reply</span>')
      .click(function() {
        addReplyForm(post.hash);
        d.next().find('textarea').focus();
      }));
  if (hasShowButton) {
    d.append('&nbsp;');
    var showLink = 
      $('<a>')
        .attr('href', (_depth==0?'#category':'#thread') + post.hash)
        .text('[Show]')
        .click(function() {
          //_depth += 1;
          //loadThread(post.hash);
        });
    d.append(showLink);
    $.get('../api/threadsize/' + post.hash)
      .done(function(size){
        if (size == '0')
          showLink.html('<span class="glyphicon glyphicon-envelope not-avail" aria-hidden="true"></span><span class="btn-title not-avail">&thinsp;0</span>');
        else
          showLink.html('<span class="glyphicon glyphicon-envelope" aria-hidden="true"></span><span class="btn-title">&thinsp;'+size+' – Show</span>');
      });
  }
  d.append('&nbsp;');
  d
    .append($('<a>')
      .attr('href', 'javascript:void(0)')
      .html('<span class="glyphicon glyphicon-trash" aria-hidden="true"></span><span class="btn-title">Delete</span>')
      .attr('title', 'Click to delete post forever.')
      .click(function() {
        if (post.hash == _categories) {
          pushNotification("Cannot delete root post.");
          return;
        }
        var undo = false;
        d.append(
          $('<button>')
            .text('Undo')
            .click(function(){
              undo = true;
              $(this).remove();
            })
            .append($('<span>').html('&nbsp;')
              .css({ background: 'red', height: '5px', marginLeft: '5px'})
              .animate({width: '100px'},50)
              .animate({width: '0px'},_post_delete_timeout)));
        setTimeout(function(){
          if (undo) return;
          deletePostFromDb(post.hash);
          d.remove();
          pushNotification('A post was deleted forever.');
        }, _post_delete_timeout);
      }));
  appendFunc(d);
  $('<div>')
    .addClass('post-inner')
    .html(applyFormatting(escapeTags(Base64.decode(post.message))))
    .appendTo(d);
  d.find('img').click(function(){
    $(this).toggleClass('full');
  });
  if (_showTimestamps == 'false') {
	if (d.find('g').length != 0)
		d.find('br').first().remove();
    d.find('g').css('display','none');
  }
  return d;
}

function loadReplies(hash, offset, highlight) {
  $.get('../api/replies/' + hash)
    .done(function(arr){
      arr = JSON.parse(arr);
      if (arr.length == 0) return;
      for (var i = arr.length-1; i >= 0; i--) {
        var deleted = Base64.decode(arr[i].message) == _postWasDeletedMarker;
        if (_showDeleted == 'false' && deleted) continue;
        var p = addPost(arr[i], function(d) { d.insertAfter($('#'+hash)); }, false)
          .css('margin-left', offset * _treeOffsetPx + 'px');
        if (deleted) p.css({ opacity: _deletedOpacity });
        loadReplies(arr[i].hash, offset + 1, highlight);
        if (highlight == arr[i].hash) {
          p.addTemporaryClass('high', 8000);
        }
      }
      vid_show()
    });
}

function loadThread(hash, highlight) {
  thisPosts = [];
  $.get('../api/replies/' + hash)
    .done(function(arr){
      arr = JSON.parse(arr);
      if (arr.length > 0) {
        $('#thread').empty();
      } else { 
        _depth -= 1; 
        pushNotification('This thread/category is empty.');
        return; 
      }
      $.get('../api/get/' + hash)
        .done(function(post){
          post = JSON.parse(post);
          if (_depth > 0) {
            $('#thread').append(
              $('<a>')
                .attr('href', (post.replyTo != _categories) ? ('#category' + post.replyTo) : ('#'))
                .html('<b><span class="glyphicon glyphicon-arrow-up" aria-hidden="true"></span>Up</b>')
                .click(function(){
                  //_depth -= 1;
                  //loadThread(post.replyTo);
                }));
          }
          $('#thread').append('&nbsp;');
          $('#thread').append(
            $('<a>')
              .attr('href','javascript:void(0)')
              .html('<span class="glyphicon glyphicon-refresh" aria-hidden="true"></span>Refresh')
              .click(function(){
                reloadParams();
                setTimeout(function(){
                  loadThread(hash);                  
                }, 500);
              }));
          addPost(post, function(d){ d.appendTo($('#thread')); }, false, false);
          if (_depth == 1) arr.reverse();
          for (var i = 0; i < arr.length; i++) {
            var deleted = Base64.decode(arr[i].message) == _postWasDeletedMarker;
            if (_showDeleted == 'false' && deleted) continue;
            var p = addPost(arr[i], function(d) { d.appendTo($('#thread')); }, true)
              .css('margin-left',  _treeOffsetPx + 'px');
            if (deleted) p.css({ opacity: _deletedOpacity});
            if (highlight == arr[i].hash) {
              p.addTemporaryClass('high', 8000);
            }
            if (_depth > 1) {
              loadReplies(arr[i].hash, 2, highlight);
            }
          }
          vid_show()
        });
    });
}