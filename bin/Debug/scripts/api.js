function sendPostToDb(post) {
  $.post('../api/add/' + post.replyTo, post.message)
    .done(function(response){
      addPost(JSON.parse(response), function(d){ d.insertAfter($('#'+post.replyTo)); }, false)
        .addTemporaryClass('high', 1000)
        .css('margin-left', parseInt($('#'+post.replyTo).css('margin-left'))+10+'px');
      pushNotification('Post was successfully added.');
      onAdd(post);
    })
    .fail(function() {
      pushNotification('Failed to add post (exists or too big).');
    });
}

function deletePostFromDb(hash) {
  $.post('../api/delete/' + hash);
}