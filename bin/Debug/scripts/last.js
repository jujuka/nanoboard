
function loadRootThread(hash) {
  var first = hash;
  var prev = hash;
  var fn = function(){
    $.get('../api/get/' + hash)
      .done(function(p){
        p = JSON.parse(p);
        if (p.replyTo == _categories)
        {
          _depth = 2;
          loadThread(prev, first);
          return;
        }
        prev = p.hash;
        hash = p.replyTo;
        fn();
      });
  };
  fn();
}

function showLast(N){
    $.get('../api/pcount')
      .done(function(cnt){
        cnt = parseInt(cnt);
        $.get('../api/prange/'+Math.max(cnt-N,0)+'-'+N)
          .done(function(arr){
            arr = JSON.parse(arr);
            if (arr.length > 0) {
              $('#thread').empty();
            } else { return; }
            for (var i = arr.length - 1; i >= 0; i--) {
              var p = addPost(arr[i], function(d) { d.appendTo($('#thread')); }, false);
              if (arr[i].hash != _categories && 
                  arr[i].replyTo != _categories && 
                  arr[i].replyTo != _rootpost)
              p.append(
                $('<a>')
                  .attr('href', 'javascript:void(0)')
                  .text('[Thread]')
                  .click(function(){
                    loadRootThread($(this).parent().attr('id'));
                  })
                );
            }
          });
      });
  }
