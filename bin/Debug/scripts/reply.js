function addReplyForm(id) {
  var form = $('<div>')
    .addClass('post').addClass('reply-div')
    .insertAfter($('#' + id))
    .css('margin-left', parseInt($('#' + id).css('margin-left')) + _treeOffsetPx + 'px')
    .append($('<div>').addClass('reply')
      .append($('<textarea>').val('[g]' + new Date().toUTCString() + ', client: 2.0[/g]\n'))
      .append($('<br>'))
      .append($('<button>')
        .addClass('reply-button')
        .text('Cancel')
        .click(function() {
          $(this).parent().parent().remove();
        }))
      .append($('<button>')
        .text('Send')
        .addClass('reply-button')
        .click(function() {
          sendPostToDb({
            'replyTo': id,
            'message': Base64.encode($(this).parent().find('textarea').val())
          });
          $(this).parent().parent().remove();
        }))
    );
    var offset = form.offset();
    offset.top -= 100;
    $('html, body').animate({
      scrollTop: offset.top,
      scrollLeft: offset.left
     });
}