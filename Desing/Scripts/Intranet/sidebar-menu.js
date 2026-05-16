/**
 * Submenus del sidebar Materio - handler propio Intranet.
 *
 * No se usa la clase "menu-toggle" del tema Materio para evitar el error
 *     "Toggable `.menu-item` element not found"
 * que lanza su menu.js cuando recibe un click en una estructura que no
 * coincide con la que espera (sucede tras los ajustes de _Sidebar*.cshtml).
 *
 * En su lugar emitimos clases propias en _SidebarMenuItemGroup.cshtml:
 *     <li class="menu-item js-menu-group"> ...
 *         <a class="menu-link js-menu-group-toggle"> ...
 *         <ul class="menu-sub js-menu-group-sub"> ...
 *
 * y aqui simplemente alternamos la clase "open" en el <li> al hacer click
 * sobre .js-menu-group-toggle. Materio nunca intercepta estos clicks.
 */
(function () {
    'use strict';

    function init() {
        var menu = document.getElementById('layout-menu');
        if (!menu) {
            return;
        }
        menu.addEventListener('click', onMenuClick, true);
    }

    function onMenuClick(e) {
        var toggle = e.target.closest('.js-menu-group-toggle');
        if (!toggle) {
            return;
        }

        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();

        // Localizar el <li> de grupo: preferimos .js-menu-group, pero
        // caemos al <li> mas cercano por robustez.
        var li = toggle.closest('.js-menu-group') || toggle.closest('li');
        if (!li) {
            return;
        }

        var willOpen = !li.classList.contains('open');
        li.classList.toggle('open', willOpen);
        toggle.setAttribute('aria-expanded', willOpen ? 'true' : 'false');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
}());
