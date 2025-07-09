window.launchFireworks = () => {
    const launch = () => {
        const canvas = document.createElement("canvas");
        canvas.id = "fireworks-canvas-" + Date.now();
        canvas.style.position = "fixed";
        canvas.style.top = 0;
        canvas.style.left = 0;
        canvas.style.width = "100vw";
        canvas.style.height = "100vh";
        canvas.style.pointerEvents = "none";
        canvas.style.zIndex = 9999;
        document.body.appendChild(canvas);

        const ctx = canvas.getContext("2d");
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;

        let particles = [];
        for (let i = 0; i < 120; i++) {
            particles.push({
                x: canvas.width / 2,
                y: canvas.height / 2,
                angle: Math.random() * 2 * Math.PI,
                speed: Math.random() * 1.8 + 0.5, // 💡 Très lent
                radius: Math.random() * 3 + 1,
                color: `hsl(${Math.random() * 360}, 100%, 60%)`,
                life: 100 // 💡 Vie plus longue
            });
        }

        function animate() {
            ctx.fillStyle = "rgba(0, 0, 0, 0.05)";
            ctx.fillRect(0, 0, canvas.width, canvas.height);

            particles.forEach(p => {
                ctx.beginPath();
                ctx.arc(p.x, p.y, p.radius, 0, 2 * Math.PI);
                ctx.fillStyle = p.color;
                ctx.fill();
                p.x += Math.cos(p.angle) * p.speed;
                p.y += Math.sin(p.angle) * p.speed;
                p.life--;
            });

            particles = particles.filter(p => p.life > 0);
            if (particles.length > 0) {
                requestAnimationFrame(animate);
            } else {
                document.body.removeChild(canvas);
            }
        }

        animate();
    };

    // 🎆 Double explosion avec un écart plus large
    launch();
    setTimeout(launch, 1500);
};
