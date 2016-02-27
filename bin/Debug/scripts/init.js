var postCount = 0;

function notifyAboutPostCount() {
  $.get('../api/count')
    .done(function(data){
      data = parseInt(data);
      if (data != postCount) {
        if (postCount != 0) {
          pushNotification((data - postCount) + ' post(s) added.', _post_count_notification_time);
        }
        postCount = data;
        $('#statusd').html('<a href=javascript:void(0)>Posts (including deleted): '+postCount+'</a>');
      }
    })
    .fail(function(){
      pushNotification('Connection to server lost.', 900);
    });
}

$(function() {
  $('#searcha').click(function(){showSearch();});
  $('#last10a').click(function(){showLast(10);});
  $('#last50a').click(function(){showLast(50);});
  $('#last100a').click(function(){showLast(100);});
  $('#last500a').click(function(){showLast(500);});
  $('#maina').click(function(){_depth = 0;loadThread(_categories);});
  reloadParams();
  setTimeout(function(){
    loadThread(_categories);    
  }, 500);
  setInterval(function(){ notifyAboutPostCount(); }, 1000);
});