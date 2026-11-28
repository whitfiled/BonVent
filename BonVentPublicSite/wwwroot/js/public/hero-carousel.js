(() => {
    // Delegation keeps the controls working when MVC navigation replaces the page content.
    document.addEventListener("click", event => {
        const control = event.target.closest("[data-hero-previous], [data-hero-next]");
        if (!control || control.disabled) return;

        const carousel = control.closest("[data-hero-carousel]");
        if (!carousel) return;
        const slides = Array.from(carousel.querySelectorAll("[data-hero-slide]"));
        if (slides.length < 2) return;

        const current = Math.max(0, slides.findIndex(slide => !slide.hidden));
        const direction = control.hasAttribute("data-hero-next") ? 1 : -1;
        const next = (current + direction + slides.length) % slides.length;
        slides[current].hidden = true;
        slides[next].hidden = false;

        const counter = carousel.querySelector("[data-hero-counter]");
        if (counter) counter.textContent = `${String(next + 1).padStart(2, "0")} / ${String(slides.length).padStart(2, "0")}`;
    });
})();
