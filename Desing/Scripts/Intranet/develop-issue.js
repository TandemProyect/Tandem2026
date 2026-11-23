/* Develop › US y Bugs: pegar capturas (Ctrl+V) en el input de ficheros y borrados lógicos por AJAX. */
(function (window, $) {
    'use strict';

    var i18n = window.developIssueI18n || {};
    var urls = window.developIssueUrls || {};

    function token() {
        return $('input[name="__RequestVerificationToken"]').first().val();
    }

    function renderPreview($input) {
        var $preview = $input.closest('form').find('.js-issue-preview').empty();
        Array.prototype.forEach.call($input[0].files || [], function (file, index) {
            if (!/^image\//.test(file.type)) return;
            var $item = $('<div class="position-relative"></div>');
            var $img = $('<img class="rounded border" style="height:90px" />').attr('src', URL.createObjectURL(file)).attr('alt', file.name);
            var $remove = $('<button type="button" class="btn btn-xs btn-danger position-absolute top-0 end-0"><i class="ri ri-close-line"></i></button>')
                .attr('title', i18n.removeFile || '')
                .on('click', function () { removeFile($input, index); });
            $preview.append($item.append($img, $remove));
        });
    }

    function setFiles($input, files) {
        var dt = new DataTransfer();
        files.forEach(function (f) { dt.items.add(f); });
        $input[0].files = dt.files;
        renderPreview($input);
    }

    function removeFile($input, index) {
        var files = Array.prototype.slice.call($input[0].files || []);
        files.splice(index, 1);
        setFiles($input, files);
    }

    $(document).on('change', '.js-issue-captures', function () { renderPreview($(this)); });

    $(document).on('paste', function (e) {
        var $input = $('.js-issue-captures').first();
        var items = (e.originalEvent.clipboardData || {}).items || [];
        if (!$input.length || typeof DataTransfer === 'undefined') return;
        var pasted = [];
        Array.prototype.forEach.call(items, function (item) {
            if (item.kind === 'file' && /^image\//.test(item.type)) {
                var blob = item.getAsFile();
                var ext = (item.type.split('/')[1] || 'png').replace('jpeg', 'jpg');
                pasted.push(new File([blob], 'captura-' + Date.now() + '-' + pasted.length + '.' + ext, { type: item.type }));
            }
        });
        if (!pasted.length) return;
        e.preventDefault();
        setFiles($input, Array.prototype.slice.call($input[0].files || []).concat(pasted));
    });

    $(document).on('click', '.js-issue-delete', function () {
        var $row = $(this).closest('tr');
        if (!window.confirm(i18n.confirmDelete || '')) return;
        $.post(urls.deleteIssue, { id: $row.data('issue-id'), __RequestVerificationToken: token() }, function (r) {
            if (r && r.ok) {
                var dt = $.fn.dataTable && $.fn.dataTable.isDataTable('#ListDevelopIssues') ? $('#ListDevelopIssues').DataTable() : null;
                if (dt) dt.row($row).remove().draw(false); else $row.remove();
            } else if (r && r.message) {
                window.alert(r.message);
            }
        });
    });

    $(document).on('click', '.js-issue-detail-delete', function () {
        var $card = $(this).closest('.js-issue-detail');
        if (!window.confirm(i18n.confirmDeleteDetail || '')) return;
        $.post(urls.deleteDetail, { id: urls.issueId, detailId: $card.data('detail-id'), __RequestVerificationToken: token() }, function (r) {
            if (r && r.ok) $card.remove();
            else if (r && r.message) window.alert(r.message);
        });
    });
}(window, window.jQuery));
