(() => {
    let keyHandlerAttached = false;
    let lastTriggerElement = null;

    function getFocusableElements() {
        return Array.from(document.querySelectorAll(
            ".drawer-panel button, .drawer-panel [href], .drawer-panel input, .drawer-panel select, .drawer-panel textarea, .drawer-panel [tabindex]:not([tabindex='-1'])"
        )).filter(element => !element.hasAttribute("disabled") && element.getAttribute("aria-hidden") !== "true");
    }

    function onKeyDown(event) {
        const mask = document.querySelector(".drawer-mask");
        if (!mask) {
            return;
        }

        if (event.key === "Escape") {
            mask.dispatchEvent(new MouseEvent("click", { bubbles: true }));
            return;
        }

        if (event.key !== "Tab") {
            return;
        }

        const focusable = getFocusableElements();
        if (focusable.length === 0) {
            event.preventDefault();
            return;
        }

        const first = focusable[0];
        const last = focusable[focusable.length - 1];
        const active = document.activeElement;

        if (event.shiftKey) {
            if (active === first || !focusable.includes(active)) {
                event.preventDefault();
                last.focus();
            }
            return;
        }

        if (active === last || !focusable.includes(active)) {
            event.preventDefault();
            first.focus();
        }
    }

    function focusFirstField() {
        const firstField = getFocusableElements()[0];
        if (firstField && typeof firstField.focus === "function") {
            firstField.focus();
        }
    }

    function restoreFocus() {
        if (lastTriggerElement && typeof lastTriggerElement.focus === "function") {
            lastTriggerElement.focus();
        }
        lastTriggerElement = null;
    }

    function syncState(triggerId) {
        const hasDrawer = !!document.querySelector(".drawer-mask");

        if (hasDrawer) {
            if (triggerId) {
                lastTriggerElement = document.getElementById(triggerId);
            }

            document.body.classList.add("no-scroll");
            if (!keyHandlerAttached) {
                document.addEventListener("keydown", onKeyDown);
                keyHandlerAttached = true;
            }

            window.requestAnimationFrame(() => {
                focusFirstField();
            });
            return;
        }

        document.body.classList.remove("no-scroll");
        if (keyHandlerAttached) {
            document.removeEventListener("keydown", onKeyDown);
            keyHandlerAttached = false;
        }

        window.requestAnimationFrame(() => {
            restoreFocus();
        });
    }

    function forceClose() {
        document.body.classList.remove("no-scroll");
        if (keyHandlerAttached) {
            document.removeEventListener("keydown", onKeyDown);
            keyHandlerAttached = false;
        }
        restoreFocus();
    }

    window.drawerUx = {
        syncState,
        forceClose
    };
})();
