// after the page is loaded, add popovers for headings of articles
addPopoversForArticles();

$("#ajanlok").on('DOMNodeInserted', function(){
	// each time a new article DOM element was added (because of scrolling down)
	// we must add popovers for the new articles' headings (already processed articles will be skipped)
	addPopoversForArticles();
});

function addPopoversForArticles() {
	// iterate through Index articles
	$("#ajanlok .rovatajanlo").each(function() {
		
		if ($(this).attr("class").indexOf("vezeto") >= 0) {
			// contains vezeto class
			var titleNode = $(this).find("h1 a").first();
		} else {
			var titleNode = $(this).find("h1 .cim").first();
		}
		
		// check if we already added a popover for this article
		var dataToggleAttribute = titleNode.attr('data-toggle');
		if (typeof dataToggleAttribute !== typeof undefined && dataToggleAttribute !== false) {
			// this article already has the popover attribute, so we skip processing it
			return;
		}
		
		// this article doesn't have a popover yet, let's add necessary attributes
		titleNode.attr("data-toggle", "popover");		
		titleNode.attr("data-container", "body");
		titleNode.attr("data-placement", "top");
		// popover will have "Please wait" title and "..." content while AJAX call is in progress
		titleNode.attr("title", "Please wait");
		titleNode.attr("data-content", "...");		
		
		// enable popover
		titleNode.popover();
		
		titleNode.on("mouseenter", function() {
			// on mouse enter event, show popover
			var popover = $(this);
			popover.popover('show');

			// get address of news article from title node (add https if it's missing)
			var senderAddress = popover.attr("href");
			if (!senderAddress.startsWith("https:")) {
				senderAddress = "https:" + senderAddress;
			}
			
			// send message to background script with the URL of the news article
			chrome.runtime.sendMessage({url: senderAddress}, function(response) {
				var popoverId = popover.attr("aria-describedby");
				var popoverContent = $('#' + popoverId + " .popover-content");
				var popoverHeader = $('#' + popoverId + " .popover-title");
				
				// background script returned Not found result (no such article, or no related article)
				if (response.result == "Not found")
				{
					// set popover title and content accordingly
					popoverHeader.text("No result");
					popoverContent.text("No related news article found.");
				}
				else
				{
					// background script successfully returned related article's URL
					popoverHeader.text("Related news article found");
					
					// get root domain of returned URL
					var rootDomain = getRootDomainFromUrl(response.result);
					rootDomain = rootDomain.replace("www.", "");
					
					// show related article's link in popover
					popoverContent.html('<a class="related-link" href="' + response.result + '" target="_blank">Read related article on ' + rootDomain + '.</a>');
				}
				
				// after 4 seconds, automatically hide the popover
				setTimeout(function () {
					popover.popover('hide');
				}, 4000);
			});
		  }
		);		
	});
}

// "https://www.hirado.hu/belfold/..." becomes "www.hirado.hu"
function getRootDomainFromUrl(data) {
	var a = document.createElement('a');
	a.href = data;
	return a.hostname;
}