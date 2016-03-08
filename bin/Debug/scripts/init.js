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
  //$('#searcha').click(function(){showSearch();});
  /*$('#last10').click(function(){showLast(10);});
  $('#last50').click(function(){showLast(50);});
  $('#last100').click(function(){showLast(100);});
  $('#last500').click(function(){showLast(500);});*/
  //$('#maina').click(function(){_depth = 0;loadThread(_categories);});
  $('#png-collect').click(function(){$.get('../api/png-collect')});
  $('#png-create').click(function(){$.get('../api/png-create')});
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
      } else if (_location.endsWith("#search")) {
        showSearch();
      } else {
        // do nothing intentionally
      }
    }
  }, 100);
  /*setTimeout(function(){
    loadThread(_categories);    
  }, 500);*/
  setInterval(function(){
    retranslate();    
  }, 300000);
  setInterval(function(){
    checkVersion();    
  }, 60000);
  checkVersion();
  setInterval(function(){ notifyAboutPostCount(); }, 1000);
});