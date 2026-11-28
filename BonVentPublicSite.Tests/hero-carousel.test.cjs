const { test } = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

const source = fs.readFileSync(path.join(__dirname, "../BonVentPublicSite/wwwroot/js/public/hero-carousel.js"), "utf8");

function setup(count) {
    let click;
    const document = { addEventListener: (name, listener) => { if (name === "click") click = listener; } };
    vm.runInNewContext(source, { document });
    const slides = Array.from({ length: count }, (_, index) => ({ hidden: index !== 0 }));
    const counter = { textContent: "" };
    const carousel = {
        querySelectorAll: () => slides,
        querySelector: () => counter
    };
    function press(direction, disabled = false) {
        const control = {
            disabled,
            closest: () => carousel,
            hasAttribute: name => name === "data-hero-next" && direction === "next"
        };
        click({ target: { closest: () => control } });
    }
    return { slides, counter, press };
}

test("next and previous show the right image and wrap at both ends", () => {
    const hero = setup(3);
    hero.press("next");
    assert.deepEqual(hero.slides.map(slide => slide.hidden), [true, false, true]);
    assert.equal(hero.counter.textContent, "02 / 03");
    hero.press("previous");
    hero.press("previous");
    assert.deepEqual(hero.slides.map(slide => slide.hidden), [true, true, false]);
    assert.equal(hero.counter.textContent, "03 / 03");
    hero.press("next");
    assert.deepEqual(hero.slides.map(slide => slide.hidden), [false, true, true]);
});

test("disabled controls and a single image do not change the slide", () => {
    const hero = setup(1);
    hero.press("next", true);
    hero.press("previous");
    assert.deepEqual(hero.slides.map(slide => slide.hidden), [false]);
});
