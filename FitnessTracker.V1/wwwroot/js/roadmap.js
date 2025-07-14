window.getStepPosition = (id) => {
    const el = document.getElementById(id);
    if (el) {
        const rect = el.getBoundingClientRect();
        return {
            top: rect.top,
            left: rect.left,
            width: rect.width,
            height: rect.height
        };
    }
    return { top: 0, left: 0, width: 0, height: 0 };
};
