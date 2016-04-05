function addReplyForm(id) {
  var form = $('<div>')
    .addClass('post').addClass('reply-div')
    .insertAfter($('#' + id))
    .css('margin-left', parseInt($('#' + id).css('margin-left')) + _treeOffsetPx + 'px')
    .append($('<div>').addClass('reply')
      .append($('<textarea>').val('[g]' + new Date().toUTCString() + ', client: 3.0[/g]\n'))
      .append($('<br>'))
      .append($('<button>')
        .addClass('reply-button btn btn-danger ')
        .text('Cancel')
        .click(function() {
          $(this).parent().parent().remove();
        }))
      .append($('<button>')
        .text('Send')
        .addClass('reply-button btn btn-primary')
        .click(function() {
          var pst = Base64.encode(id + $(this).parent().find('textarea').val());
          var waitPowModal = $('<div>');
          waitPowModal.addClass('pow_modal');
          waitPowModal.html('<b>Wait for POW</b><br/>usually less than a minute...');
          $('body').append(waitPowModal);
          var form = $(this).parent().parent();
          form.hide();
          $.post('../pow', pst)
            .done(function(token){
              $.get('../captcha/' + token)
                .done(function(dataUri){
                  waitPowModal.remove();
                  var captchaModal = $('<div>');
                  captchaModal.addClass('captcha_modal');
                  captchaModal.append('<img class="captcha_image" src="' + dataUri + '"><br/>');
                  captchaModal.append('<textarea class="captcha_answer"></textarea><br/>');
                  var captchaBtn = $('<button>');
                  captchaBtn
                    .text('Send')
                    .addClass('reply-button btn btn-primary')
                    .click(function(){
                      var answer = $('.captcha_answer').val();
                      $.post('../solve/' + token, Base64.encode(answer))
                        .done(function(postStr){
                          form.remove();
                          mockSendPostToDb(JSON.parse(postStr));
                          captchaModal.remove();
                        })
                        .fail(function(){
                          captchaModal.remove();
                          pushNotification('Wrong captcha answer, please try again', 5000);
                          form.show();
                        });
                    });
                  captchaModal.append(captchaBtn);
                  captchaModal.append($('<button>').text('Cancel').addClass('reply-button btn btn-danger').click(function(){
                    captchaModal.remove();
                    form.show();
                    $.post('../solve/' + token, Base64.encode("~~~~~"));
                  }));
                  $('body').append(captchaModal);
                  $('.captcha_answer').focus();
                });
            });
        })
        /*.click(function() {
          sendPostToDb({
            'replyTo': id,
            'message': Base64.encode($(this).parent().find('textarea').val())
          });
          $(this).parent().parent().remove();
        })*/)
      .append(($('<button>')
        .text('Attach image')
        .addClass('reply-button btn btn-default')
        .click(function() {
        __current_text_input=$(this).parent().children(":first")
        $('#imgmodal').modal()
        $('#scale').click()
        })))
    );
    var offset = form.offset();
    offset.top -= 100;
    $('html, body').animate({
      scrollTop: offset.top,
      scrollLeft: offset.left
     });
}