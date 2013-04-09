﻿(function ($, document) {

	// The base query options
	var query = {

		SortBy: "SortName",
		SortOrder: "Ascending",
		IncludeItemTypes: "BoxSet",
		Recursive: true,
		Fields: "PrimaryImageAspectRatio,ItemCounts"
	};


	function reloadItems(page) {

		Dashboard.showLoadingMsg();

		ApiClient.getItems(Dashboard.getCurrentUserId(), query).done(function (result) {

			$('#items', page).html(LibraryBrowser.getBoxsetPosterViewHtml({

				items: result.Items,
				useAverageAspectRatio: true

			}));

			Dashboard.hideLoadingMsg();
		});
	}

	$(document).on('pageinit', "#boxsetsPage", function () {

		var page = this;

		$('.radioSortBy', this).on('click', function () {
			query.SortBy = this.getAttribute('data-sortby');
			reloadItems(page);
		});

		$('.radioSortOrder', this).on('click', function () {
			query.SortOrder = this.getAttribute('data-sortorder');
			reloadItems(page);
		});

		$('.chkStandardFilter', this).on('change', function () {

			var filterName = this.getAttribute('data-filter');
			var filters = query.Filters || "";

			filters = (',' + filters).replace(',' + filterName, '').substring(1);

			if (this.checked) {
				filters = filters ? (filters + ',' + filterName) : filterName;
			}

			query.Filters = filters;

			reloadItems(page);
		});

	}).on('pagebeforeshow', "#boxsetsPage", function () {

	    reloadItems(this);

	}).on('pageshow', "#boxsetsPage", function () {

			// Reset form values using the last used query
			$('.radioSortBy', this).each(function () {

				this.checked = query.SortBy == this.getAttribute('data-sortby');

			}).checkboxradio('refresh');

			$('.radioSortOrder', this).each(function () {

				this.checked = query.SortOrder == this.getAttribute('data-sortorder');

			}).checkboxradio('refresh');

			$('.chkStandardFilter', this).each(function () {

				var filters = "," + (query.Filters || "");
				var filterName = this.getAttribute('data-filter');

				this.checked = filters.indexOf(',' + filterName) != -1;

			}).checkboxradio('refresh');
		});

})(jQuery, document);