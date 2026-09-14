// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function confirmDelete(event, formElement) {
    const message = formElement.dataset.deleteMessage || 'Are you sure you want to delete this record?';
    if (!window.confirm(message)) {
        event.preventDefault();
    }
}

// Phone bottom bar: when the tabs scroll sideways, keep the current page's tab in view.
(() => {
    const nav = document.querySelector('.sidebar-nav');
    const activeTab = nav?.querySelector('.nav-item.active');
    if (!activeTab || !window.matchMedia('(max-width: 575.98px)').matches || nav.scrollWidth <= nav.clientWidth) return;

    activeTab.scrollIntoView({ block: 'nearest', inline: 'nearest' });
})();
