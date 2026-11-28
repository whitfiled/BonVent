(() => {
    const main = document.getElementById("page-content");
    const notice = document.getElementById("availability-notice");
    if (!main || !window.fetch || !window.history.pushState) return;

    let request;
    let socket;
    let retryTimer;
    let retryDelay = 1000;

    async function navigate(url, options = {}) {
        if (request) request.abort();
        request = new AbortController();
        const controller = request;
        main.setAttribute("aria-busy", "true");

        try {
            const response = await fetch(url, {
                signal: controller.signal,
                headers: { "X-Requested-With": "BonVentNavigation" },
                credentials: "same-origin"
            });
            const destination = new URL(response.url, location.href);
            if (!response.ok || destination.origin !== location.origin ||
                !response.headers.get("content-type")?.includes("text/html")) {
                location.assign(url);
                return;
            }

            const page = new DOMParser().parseFromString(await response.text(), "text/html");
            const nextMain = page.getElementById("page-content");
            if (!nextMain) {
                location.assign(url);
                return;
            }
            if (controller.signal.aborted) return;

            document.querySelectorAll("head link[data-page-style]").forEach(link => link.remove());
            page.querySelectorAll("head link[data-page-style]").forEach(link => document.head.append(link.cloneNode(true)));
            main.replaceChildren(...nextMain.childNodes);
            document.title = page.title;
            notice.hidden = true;
            const target = new URL(url, location.href);
            if (response.redirected) target.pathname = destination.pathname, target.search = destination.search;
            if (!options.historyTraversal) history.pushState({}, "", target);
            if (target.hash) document.getElementById(decodeURIComponent(target.hash.slice(1)))?.scrollIntoView();
            else window.scrollTo(0, 0);
            main.focus({ preventScroll: true });
        } catch (error) {
            if (error.name !== "AbortError") location.assign(url);
        } finally {
            if (request === controller) {
                main.removeAttribute("aria-busy");
                request = null;
            }
        }
    }

    document.addEventListener("click", event => {
        if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
        const link = event.target.closest("a[href]");
        if (!link || link.hasAttribute("download") || (link.target && link.target !== "_self")) return;
        const url = new URL(link.href, location.href);
        if (url.origin !== location.origin || !/^https?:$/.test(url.protocol)) return;
        if (url.pathname === location.pathname && url.search === location.search && url.hash) return;
        event.preventDefault();
        navigate(url.href);
    });

    document.addEventListener("submit", event => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || form.method.toLowerCase() !== "get" || form.target && form.target !== "_self") return;
        const url = new URL(form.action, location.href);
        if (url.origin !== location.origin) return;
        event.preventDefault();
        const data = new FormData(form);
        if (event.submitter?.name) data.append(event.submitter.name, event.submitter.value);
        url.search = new URLSearchParams(data).toString();
        navigate(url.href);
    });

    window.addEventListener("popstate", () => navigate(location.href, { historyTraversal: true }));

    function connectAvailability() {
        if (!("WebSocket" in window)) return;
        if (socket && (socket.readyState === WebSocket.OPEN || socket.readyState === WebSocket.CONNECTING)) return;
        clearTimeout(retryTimer);
        const endpoint = document.querySelector("script[data-availability-url]")?.dataset.availabilityUrl;
        if (!endpoint) return;
        const url = new URL(endpoint, location.href);
        url.protocol = location.protocol === "https:" ? "wss:" : "ws:";
        socket = new WebSocket(url);
        socket.addEventListener("open", () => { retryDelay = 1000; });
        socket.addEventListener("message", event => {
            if (event.data === "availability-changed" && main.querySelector(".room-search-form, .reservation-form")) {
                notice.hidden = false;
            }
        });
        socket.addEventListener("close", () => {
            if (document.visibilityState === "hidden") return;
            retryTimer = setTimeout(connectAvailability, retryDelay);
            retryDelay = Math.min(retryDelay * 2, 30000);
        });
    }

    document.addEventListener("visibilitychange", () => {
        if (document.visibilityState === "visible" && (!socket || socket.readyState === WebSocket.CLOSED)) connectAvailability();
    });
    connectAvailability();
})();
