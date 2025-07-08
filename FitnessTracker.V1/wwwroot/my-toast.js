function showToast(message) {
    const toast = document.createElement("div");
    toast.textContent = message;
    toast.style.position = "fixed";
    toast.style.bottom = "80px";
    toast.style.left = "50%";
    toast.style.transform = "translateX(-50%)";
    toast.style.backgroundColor = "#333";
    toast.style.color = "white";
    toast.style.padding = "10px 20px";
    toast.style.borderRadius = "10px";
    toast.style.zIndex = "9999";
    toast.style.opacity = "0.95";

    document.body.appendChild(toast);
    setTimeout(() => {
        toast.remove();
    }, 2500);
}
