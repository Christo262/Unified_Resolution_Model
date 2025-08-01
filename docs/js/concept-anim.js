export function initUrmAnim(id) {
    if (typeof THREE === 'undefined') {
        alert("Error loading THREE.js. Please check your internet connection or CDN availability.");
    }
    else {
        var container = document.getElementById(id);
        const width = container.clientWidth;
        const height = container.clientHeight;
        console.log(width);
        console.log(height);
        const scene = new THREE.Scene();
        const camera = new THREE.PerspectiveCamera(75, width / height, 0.1, 1000);
        const renderer = new THREE.WebGLRenderer({ antialias: true });

        renderer.setSize(width, height);
        container.appendChild(renderer.domElement);

        const resolutionSlider = document.getElementById('resolutionSlider');
        camera.position.set(0, 0, 8);

        const nodeMaterial = new THREE.MeshBasicMaterial({ color: 0x00ffcc, transparent: true, opacity: 1 });
        const edgeMaterial = new THREE.LineBasicMaterial({ color: 0xffffff, transparent: true, opacity: 1 });

        const nodes = [
            new THREE.Vector3(-2, 0, 0),
            new THREE.Vector3(2, 0, 0),
            new THREE.Vector3(0, 2, 0),
            new THREE.Vector3(0, -2, 0), // extra node
            new THREE.Vector3(-2, -2, 0) // extra node
        ];

        const nodeMeshes = nodes.map(pos => {
            const geometry = new THREE.SphereGeometry(0.2, 16, 16);
            const mesh = new THREE.Mesh(geometry, nodeMaterial);
            mesh.position.copy(pos);
            scene.add(mesh);
            return mesh;
        });

        const edges = [
            [nodes[0], nodes[1]],
            [nodes[1], nodes[2]],
            [nodes[2], nodes[0]],
            [nodes[0], nodes[3]],
            [nodes[3], nodes[4]],
            [nodes[4], nodes[0]]
        ];

        const edgeLines = edges.map(([a, b]) => {
            const geometry = new THREE.BufferGeometry().setFromPoints([a.clone(), b.clone()]);
            const line = new THREE.Line(geometry, edgeMaterial);
            scene.add(line);
            return line;
        });

        const strings = [];
        const stringOrigin = nodes[0].clone();

        for (let i = 0; i < 60; i++) {
            const curve = new THREE.CatmullRomCurve3([
                new THREE.Vector3(),
                new THREE.Vector3(),
                new THREE.Vector3(),
                new THREE.Vector3()
            ], true);

            const geometry = new THREE.BufferGeometry().setFromPoints(curve.getPoints(20));
            const color = new THREE.Color(`hsl(${Math.random() * 360}, 100%, 60%)`);
            const material = new THREE.LineBasicMaterial({ color, transparent: true, opacity: 0 });

            const loop = new THREE.Line(geometry, material);
            scene.add(loop);
            strings.push({ loop, curve });
        }

        function animate() {
            requestAnimationFrame(animate);
            const ρ = parseInt(resolutionSlider.value);
            const t = performance.now() * 0.001;

            const fadeOut = Math.max(0, 1 - (ρ / 100));
            nodeMaterial.opacity = fadeOut;
            edgeMaterial.opacity = fadeOut;
            nodeMeshes.forEach(mesh => mesh.visible = fadeOut > 0);
            edgeLines.forEach(line => line.visible = fadeOut > 0);

            const emergenceFactor = Math.min(Math.max((ρ - 40) / 40, 0), 1);

            strings.forEach(({ loop, curve }) => {
                const points = [];
                const r = 0.05 + 0.1 * emergenceFactor;
                const cx = stringOrigin.x + (Math.random() - 0.5) * 0.25;
                const cy = stringOrigin.y + (Math.random() - 0.5) * 0.25;
                const cz = stringOrigin.z + (Math.random() - 0.5) * 0.25;

                for (let i = 0; i < 4; i++) {
                    const angle = Math.random() * Math.PI * 2;
                    points.push(new THREE.Vector3(
                        cx + r * Math.cos(angle + t * 0.01),
                        cy + r * Math.sin(angle + t * 0.01),
                        cz + r * 0.2 * Math.sin(angle * 2 + t * 0.01)
                    ));
                }

                curve.points = points;
                loop.geometry.setFromPoints(curve.getPoints(20));
                loop.material.opacity = ρ > 40 ? (ρ >= 80 ? 1 : emergenceFactor * 0.8) : 0;
            });

            const target = stringOrigin;
            camera.position.set(target.x, target.y, Math.max(1.0, 8 - (ρ / 10)));
            camera.lookAt(target);

            renderer.render(scene, camera);
        }

        animate();

        const resizeObserver = new ResizeObserver(() => {
            const newWidth = container.clientWidth;
            const newHeight = container.clientHeight;
            renderer.setSize(newWidth, newHeight);
            camera.aspect = newWidth / newHeight;
            camera.updateProjectionMatrix();
        });

        resizeObserver.observe(container);
    }
}

window.initOverviewAnim = function (id) {
    initUrmAnim(id);
}