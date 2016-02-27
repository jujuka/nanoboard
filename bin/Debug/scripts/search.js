
  function showSearch() {
    $('#thread').empty();
    $('#thread').append(
      $('<input>')
        .addClass('searchfield'));
    $('.searchfield').focus();
    $('#thread').append(
      $('<button>').text('Search')
        .click(function() {
          $('#searchresult').empty();
          $('#searchresult').append('<img style="border: 0px" src="../images/spinner.gif">');
          var search = Base64.encode($('.searchfield').val());
          $.post('../api/search', search)
            .done(function(arr){
              $('#searchresult').empty();
              arr = JSON.parse(arr);
              if (arr.length == 0) {
                $('#searchresult').append('No results<br/>');
                return;
              } else { 
                $('#searchresult')
                  .append('Results: ' + arr.length + 
                          (arr.length >= 500 ? '(limit reached)' : '') + '<br/>');
              }
              for (var i = arr.length - 1; i >= 0; i--) {
                var p = addPost(arr[i], function(d) {
                  d.appendTo($('#searchresult')); 
                }, false);
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
              search = Base64.decode(search);
              $('.post-inner').each(function() { $(this).html(
                  replaceAll($(this).html().toString(), search, '<span class="word-search">' + search + '</span>')
                )});
              $('img').click(function(){
                $(this).toggleClass('full');
              });
            });
        }));
      $('#thread').append('<hr>');
      $('#thread').append('<div id="searchresult"></div>');
  }