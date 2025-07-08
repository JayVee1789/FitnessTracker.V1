window.triggerFireworks = () => {
    const fw = document.getElementById("fireworks");
    if (!fw) return;

    fw.style.display = "flex";
    setTimeout(() => fw.style.display = "none", 1000);
}
