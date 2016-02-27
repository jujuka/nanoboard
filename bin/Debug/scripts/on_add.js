/*
	This function is called each time user adds a post through web-interface.
	post - contains string with json of post object in it, where message is base64 encoded
*/
function onAdd(post) {
	$.post('http://127.0.0.1:7543', post);
}