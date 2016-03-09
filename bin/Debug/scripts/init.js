function updatePlacesBar() {
  $.get('../api/paramget/places')
      .done(function(v){
        v = v.split('\n');
        $('#placesd').empty();
        $('#placesd').append('<b>Places (to post PNG containers to):</b><br/>')
        for (var i = 0; i < v.length; i++) {
          if (v[i].length > 0 && v[i][0]=='#') continue;
          $('#placesd').append('• <a target=_blank href="'+v[i]+'">'+v[i]+'</a><br/>')
        }
        $('#placesd').append('You can edit this list on <a href=params.html>[Settings]</a> page.');
      });
}

var postCount = 0;

function notifyAboutPostCount() {
  $.get('../api/count')
    .done(function(data){
      data = parseInt(data);
      if (data != postCount) {
        if (postCount != 0) {
          var countStr = (data - postCount).toString();
          pushNotification(countStr + ' post' + numSuffix(countStr) + ' added.', _post_count_notification_time);
        }
        postCount = data;
        $('#statusd').html('<a href=javascript:void(0)>Posts (including deleted): '+postCount+'</a>');
      }
    })
    .fail(function(){
      pushNotification('Connection to server lost.', 900);
    });
}

var _location = '';

$(function() {
  var collectionRun = false;
  var creationRun = false;

  $('#png-collect').click(function(){
    $.get('../api/png-collect')
      .done(function(){collectionRun = true;});
    $('#png-collect').hide();
    pushNotification('PNG collection started.');
  });
  $('#png-create').click(function(){
    pushNotification('PNG creation started.');
    $('#png-create').hide();
    $.get('../api/png-create')
      .done(function(){
        creationRun = true;
      });
  });

  setInterval(function() {
    if (creationRun)
    $.get('../api/png-create-avail')
      .done(function(){
        if (!creationRun) return;
        $('#png-create').show();
        pushNotification('PNG creation finished (check your "upload" folder).');
        $('#png-create').show();
        creationRun = false;
      })
      .fail(function(){$('#png-create').hide();});
    if (collectionRun)
    $.get('../api/png-collect-avail')
      .done(function(){
        if (!collectionRun) return;
        $('#png-collect').show();
        pushNotification('PNG collection finished.');
        collectionRun = false;
      })
      .fail(function(){$('#png-collect').hide();});
  }, 300);

  reloadParams();

  setInterval(function() {
    var newLocation = window.location.href.toString();
    if (newLocation != _location) {
      _location = newLocation;
      if (_location.endsWith('#') || _location.endsWith('html')) {
        _depth = 0;
        loadThread(_categories);
      } else if (_location.includes('#thread')) {
        _depth = 2;
        loadThread(_location.split('#thread')[1]);
      } else if (_location.includes('#category')) {
        _depth = 1;
        loadThread(_location.split('#category')[1]);
      } else if (_location.includes('#last')) {
        showLast(parseInt(_location.split('#last')[1]));
      } else {
        // do nothing intentionally
      }
    }
  }, 100);

  setInterval(function(){
    updatePlacesBar();
  }, 2000);

  setInterval(function(){
    retranslate();    
  }, 300000);
  setInterval(function(){
    checkVersion();    
  }, 60000);
  checkVersion();
  setInterval(function(){ notifyAboutPostCount(); }, 1000);
});