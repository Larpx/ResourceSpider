window.runtimeLogScroller = {
    scrollToBottom: function (elementId) {
        const element = document.getElementById(elementId);
        if (!element) {
            return;
        }

        element.scrollTop = element.scrollHeight;
    }
};
