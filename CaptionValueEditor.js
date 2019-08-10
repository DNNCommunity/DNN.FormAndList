(function ($) {
    $.fn.CaptionValueEditor = function (options) {

        var settings = {
            editLinkText: 'edit',
            editImageUrl: '~/images/edit.png',
            captionHeaderText: 'Caption',
            valueHeaderText: 'Value',
            addButtonText: 'add',
            addImageUrl: '~/images/add.gif',
            removeButtonText: 'remove',
            removeImageUrl: '~/images/delete.gif',
            saveButtonText: 'save',
            cancelButtonText: 'cancel',
            title: 'Edit',
            dialogClass: 'dnnFormPopup dnnClear',
            modal: true,
            resizable: true,
            width: 420,
            height: 360
        };

        if (options) {
            $.extend(settings, options);
        }

        var input = this;
        var dialog;

        input.parent().append('<a id="editCaptions" style="cursor: pointer;" title="' + settings.editLinkText + '"><img src="' + settings.editImageUrl + '" style="margin: 5px 0 0 5px;" alt="' + settings.editLinkText + '" /></a>');
        input.parent().append('<div id="cveditor" class="dnnDialog"><div id="popupCaptions"></div></div>');

        // in tabelle wandeln
        $("#editCaptions").click(function () {
            dialog = $("#cveditor").dialog(settings);
            $("#popupCaptions").text("");
            $("#popupCaptions").append('<div class="dnnGrid"><table id="inputTbl" width="100%" cellspacing="0" cellpadding="2" style="border-collapse:collapse;"><thead></thead>'
                + '<tbody></tbody></table></div>'
                + '<ul class="dnnActions dnnClear"><li><a id="parse" class="dnnPrimaryAction" style="cursor: pointer">' + settings.saveButtonText + '</a></li>'
                + '<li><a id="cancelEdit" class="dnnSecondaryAction" style="cursor: pointer">' + settings.cancelButtonText + '</a></li></ul>');

            $("#inputTbl > thead").append('<tr class="dnnGridHeader"><th></th><th>' + settings.captionHeaderText + '</th><th>' + settings.valueHeaderText + '</th></tr>');
            $("#inputTbl > tbody").remove();
            $("#inputTbl").append("<tbody></tbody>");

            if (input.val().indexOf(';') != -1 || $.trim(input.val()).length > 0) {
                var splits = input.val().split(";");

                for (var i = 0; i < splits.length; i++) {
                    var newLine = "";
                    if (splits[i].indexOf('|') != -1) {
                        var kvp = splits[i].split('|');
                        newLine = getRow(kvp[0], kvp[1], false);
                    }
                    else {
                        if ($.trim(splits[i]).length > 0) {
                            newLine = getRow("", splits[i], false);
                        }
                    }

                    $("#inputTbl > tbody:last").append(newLine);
                }
            }
            $("#inputTbl > tbody:last").append(getRow("", "", true));
//            $("#inputTbl tr:odd").addClass("dnnGridAltItem");
//            $("#inputTbl tr:even").addClass("dnnGridItem");
            $("#popupCaptions").show();
        });

        // tabelle auslesen
        $("#cveditor").on('click', '#parse', function () {
            var array = new Array();

            $("#inputTbl > tbody tr").each(function () {
                var value = $(this).find(':text');

                var first = $.trim(value.first().val());
                var last = $.trim(value.last().val());

                if (first.length > 0 && last.length > 0) {
                    if (first == last) {
                        array.push(first);
                    } else {
                        array.push(first + "|" + last);
                    }
                } else if (last.length > 0) {
                    array.push(last);
                }
            });
            input.val(array.join(';'));
            $("#popupCaptions").hide();
            dialog.dialog("close");
        });
        $("#cveditor").on('click', '#cancelEdit', function () {
            $("#popupCaptions").hide();
            dialog.dialog("close");
        });

        // zeile entfernen
        $("#cveditor").on('click', 'a.rem', function () {
            $(this).parent().parent().remove();
        });

        // neue zeile
        $("#cveditor").on('click', 'a.add', function () {
            var values = $(this).parent().siblings().find(':text');
            var first = $.trim(values.first().val());
            var last = $.trim(values.last().val());

            if (first.length > 0 || last.length > 0) {
                $(this).hide();
                $(this).siblings(".rem").show();
                $("#inputTbl > tbody:last").append(getRow("", "", true));
            }
        });

        function getRow(key, value, isAdd) {
            var row = "<tr>";

            if (isAdd) {
                row += "<td><a class='rem' style='cursor: pointer; display:none;' title='" + settings.removeButtonText + "'><img src='" + settings.removeImageUrl + "' alt='" + settings.removeButtonText + "' /></a>"
				+ "<a class='add' style='cursor: pointer;' title='" + settings.addButtonText + "'><img src='" + settings.addImageUrl + "' alt='" + settings.addButtonText + "'/></a></td>";
            } else {
                row += "<td><a class='rem' style='cursor: pointer' title='" + settings.removeButtonText + "'><img src='" + settings.removeImageUrl + "' alt='" + settings.removeButtonText + "' /></a></td>";
            }

            row += '<td><input type="text" value="' + key + '" style="width:100%;margin:-3px;border:2px inset #eee" /></td>'
				+ '<td><input type="text" value="' + value + '" style="width:100%;margin:-3px;border:2px inset #eee" /></td></tr>';

            return row;
        }
    };
})(jQuery);

			
