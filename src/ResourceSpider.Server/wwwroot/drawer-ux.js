(() => {
    let escHandlerAttached = false;

    function onKeyDown(event) {
        if (event.key !== "Escape") {
            return;
        }

        const mask = document.querySelector(".drawer-mask");
        if (!mask) {
            return;
        }

        mask.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    }

    function syncState() {
        const hasDrawer = !!document.querySelector(".drawer-mask");

        if (hasDrawer) {
            document.body.classList.add("no-scroll");
            if (!escHandlerAttached) {
                document.addEventListener("keydown", onKeyDown);
                escHandlerAttached = true;
            }
            return;
        }

        document.body.classList.remove("no-scroll");
        if (escHandlerAttached) {
            document.removeEventListener("keydown", onKeyDown);
            escHandlerAttached = false;
        }
    }

    function forceClose() {
        document.body.classList.remove("no-scroll");
        if (escHandlerAttached) {
            document.removeEventListener("keydown", onKeyDown);
            escHandlerAttached = false;
        }
    }

    window.drawerUx = {
        syncState,
        forceClose
    };
})();
