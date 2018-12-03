// listen for messages received from content scripts
chrome.runtime.onMessage.addListener(
  function(request, sender, sendResponse) {
	// initiate AJAX request to get related article's address via .NET Related Articles Web API
	$.ajax({
		url: "http://localhost:50506/api/article/?address=" + request.url,
		method: "GET",
		success: function(response) {
			// successful response received, return reponse (URL of related article) to content script
			sendResponse({result: response});
		},
		error: function(xhr, ajaxOptions, thrownError) {
			// unsuccessful response received
			if (xhr.status == 404) {
				// 404 Not Found means no such article, or no related article found
				sendResponse({result: "Not found"});
			} else {
				// error happened while connecting to the API, or the API encountered an internal error
				// log the error for debugging purposes
				console.log("Error happened during AJAX call: " + xhr.status + ": " + thrownError);
				sendResponse({result: "Not found"});
			}
		}
	});
	  
	return true; // async sendResponse will be sent
});