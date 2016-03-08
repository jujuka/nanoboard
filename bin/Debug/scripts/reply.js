function addReplyForm(id) {
  var form = $('<div>')
    .addClass('post').addClass('reply-div')
    .insertAfter($('#' + id))
    .css('margin-left', parseInt($('#' + id).css('margin-left')) + _treeOffsetPx + 'px')
    .append($('<div>').addClass('reply')
      .append($('<textarea>').val('[g]' + new Date().toUTCString() + ', client: 2.0[/g]\n'))
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
          sendPostToDb({
            'replyTo': id,
            'message': Base64.encode($(this).parent().find('textarea').val())
          });
          $(this).parent().parent().remove();
        }))
      .append(($('<button>')
        .text('Attach image')
        .addClass('reply-button btn btn-default')
        .click(function() {
        __current_text_input=$(this).parent().children(":first")
        $('#imgmodal').modal()
        })))
    );
    var offset = form.offset();
    offset.top -= 100;
    $('html, body').animate({
      scrollTop: offset.top,
      scrollLeft: offset.left
     });
}