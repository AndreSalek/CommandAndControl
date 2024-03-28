// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
(document).ready(() => {
    document.getElementById('btnSwitch').addEventListener('click', () => {
        if (document.documentElement.getAttribute('data-bs-theme') == 'dark') {
            document.documentElement.setAttribute('data-bs-theme', 'light');
        }
        else {
            document.documentElement.setAttribute('data-bs-theme', 'dark');
        }
    })
});