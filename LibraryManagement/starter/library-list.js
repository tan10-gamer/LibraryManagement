/*
 * library-list.js: turn any list marked with data-list into a searchable, sortable List.js list.
 *
 * Markup:
 *
 *   <div data-list="title,author">                     <- class names List.js should search/sort by
 *       <input class="search" />                        <- List.js filters as you type
 *       <button class="sort" data-sort="title">Sort</button>
 *       <ul class="list">
 *           <li><span class="title">...</span> <span class="author">...</span></li>
 *       </ul>
 *   </div>
 *
 * Blazor's enhanced navigation swaps pages without a full reload, so DOMContentLoaded only fires
 * once. Running initLists again on "enhancedload" sets up the list on every page you navigate to.
 * This script must be loaded after blazor.web.js (it uses the Blazor object).
 */
function initLists() {
    document.querySelectorAll('[data-list]').forEach(el => {
        new List(el, { valueNames: el.dataset.list.split(',') });
    });
}
document.addEventListener('DOMContentLoaded', initLists);
Blazor.addEventListener('enhancedload', initLists);
