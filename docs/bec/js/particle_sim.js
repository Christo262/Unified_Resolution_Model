import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';


let scene, camera, renderer;
let container;
let particles, trap;
let positions, velocities, geometry;
let spins = []; 

const trapRadius = 5;
let particleCount = 300;
let coherence = 0;
let expansionMode = false;

let trapType = 'sphere';
let dotnet;
let lastRMS = null;

function getEllipsoidScales() {
    return {
        x: trapRadius,
        y: trapRadius * 1.5,
        z: trapRadius * 0.75
    };
}

function resizeRenderer() {
    const w = container.clientWidth, h = container.clientHeight;
    camera.aspect = w / h;
    camera.updateProjectionMatrix();
    renderer.setSize(w, h);
}

function createTrapShape(type, size = trapRadius) {
    if (trap) scene.remove(trap);
    let geom;

    if (type === 'ellipsoid') {
        geom = new THREE.SphereGeometry(1, 32, 32);
        const s = getEllipsoidScales();
        geom.scale(s.x, s.y, s.z);
    } else if (type === 'box') {
        geom = new THREE.BoxGeometry(size * 2, size * 2, size * 2);
    } else {
        geom = new THREE.SphereGeometry(size, 32, 32);
    }

    trap = new THREE.Mesh(
        geom,
        new THREE.MeshBasicMaterial({
            color: 0x8888ff,
            wireframe: true,
            opacity: 0.2,
            transparent: true
        })
    );
    scene.add(trap);
}

let gasMass = 1.443e-25; // default Rb87
let gasCriticalTemp = 170e-9;
let gasScatteringLength = 100; // in units of Bohr radius (a₀)

export function setGas(type) {
    switch (type) {
        case 'Rb87':
            gasMass = 1.443e-25;
            gasCriticalTemp = 170e-9;
            gasScatteringLength = 100;
            break;
        case 'Na23':
            gasMass = 3.817e-26;
            gasCriticalTemp = 1e-6;
            gasScatteringLength = 52;
            break;
        case 'Li7':
            gasMass = 1.165e-26;
            gasCriticalTemp = 1.4e-6;
            gasScatteringLength = -27;
            break;
        default:
            gasMass = 1.443e-25;
            gasCriticalTemp = 170e-9;
            gasScatteringLength = 100;
    }
}

export function getGasMass() {
    return gasMass;
}

function createParticles(count, radius) {
    if (particles) {
        scene.remove(particles);
        geometry.dispose();
        particles.material.dispose();
    }
    spins = [];
    geometry = new THREE.BufferGeometry();
    positions = new Float32Array(count * 3);
    const colors = new Float32Array(count * 3);
    velocities = [];

    for (let i = 0; i < count; i++) {
        const idx = i * 3;
        let x = 0, y = 0, z = 0;

        if (trapType === 'box') {
            x = (Math.random() * 2 - 1) * radius;
            y = (Math.random() * 2 - 1) * radius;
            z = (Math.random() * 2 - 1) * radius;
        } else if (trapType === 'ellipsoid') {
            let inside = false;
            while (!inside) {
                const s = getEllipsoidScales();
                const px = (Math.random() * 2 - 1) * s.x;
                const py = (Math.random() * 2 - 1) * s.y;
                const pz = (Math.random() * 2 - 1) * s.z;
                const norm = (px / s.x) ** 2 + (py / s.y) ** 2 + (pz / s.z) ** 2;
                if (norm <= 1) { x = px; y = py; z = pz; inside = true; }
            }
        } else {
            const u = Math.random();
            const theta = Math.acos(2 * Math.random() - 1);
            const phi = 2 * Math.PI * Math.random();
            const r = radius * Math.pow(u, 1 / 3);
            x = r * Math.sin(theta) * Math.cos(phi);
            y = r * Math.sin(theta) * Math.sin(phi);
            z = r * Math.cos(theta);
        }

        positions[idx] = x;
        positions[idx + 1] = y;
        positions[idx + 2] = z;
        const velScale = 1 / (coherence + 0.01);
        velocities.push(new THREE.Vector3(
            (Math.random() - 0.5) * velScale,
            (Math.random() - 0.5) * velScale,
            (Math.random() - 0.5) * velScale
        ));

        const spin = new THREE.Vector3(
            Math.random() - 0.5,
            Math.random() - 0.5,
            Math.random() - 0.5
        ).normalize();
        spins.push(spin);

        const speed = velocities[i].length();
        const temp = getTemperatureKelvin();
        const inCondensate = temp < gasCriticalTemp && speed < 1e-3;
        const color = new THREE.Color(inCondensate ? 0x00ffff : 0xff0000);

        colors[idx] = color.r;
        colors[idx + 1] = color.g;
        colors[idx + 2] = color.b;

    }

    geometry.setAttribute('position', new THREE.BufferAttribute(positions, 3));
    geometry.setAttribute('color', new THREE.BufferAttribute(colors, 3));

    particles = new THREE.Points(
        geometry,
        new THREE.PointsMaterial({ vertexColors: true, size: 0.1 })
    );

    scene.add(particles);
}

let controls;
function animate() {
    requestAnimationFrame(animate);
    const k = 0.1, dt = 0.016;

    for (let i = 0; i < particleCount; i++) {
        const idx = i * 3;
        const pos = new THREE.Vector3(positions[idx], positions[idx + 1], positions[idx + 2]);
        const vel = velocities[i];
        function estimateLocalDensity(position) {
            let count = 0;
            for (let j = 0; j < particleCount; j++) {
                const jdx = j * 3;
                const dx = positions[jdx] - position.x;
                const dy = positions[jdx + 1] - position.y;
                const dz = positions[jdx + 2] - position.z;
                const distSq = dx * dx + dy * dy + dz * dz;
                if (distSq < 1.0) count++; 
            }
            return count / particleCount;
        }
        const a0 = 5.29e-11; 
        const hbar = 1.055e-34;
        const a_s = gasScatteringLength * a0;
        const g = (4 * Math.PI * hbar * hbar * a_s / gasMass) / 1e-6;

        const density = estimateLocalDensity(pos);
        const interactionForce = pos.clone().normalize().multiplyScalar(g * density);
        vel.add(interactionForce.multiplyScalar(dt));

        if (!expansionMode) {
            const force = pos.clone().multiplyScalar(-k * coherence);
            vel.add(force.multiplyScalar(dt));
        }

        vel.multiplyScalar(1 - 0.1 * coherence * dt);
        pos.addScaledVector(vel, dt);

        let outside = false;
        if (!expansionMode) {
            if (trapType === 'ellipsoid') {
                const s = getEllipsoidScales();
                outside = (pos.x / s.x) ** 2 + (pos.y / s.y) ** 2 + (pos.z / s.z) ** 2 > 1;
            } else if (trapType === 'box') {
                outside = Math.abs(pos.x) > trapRadius || Math.abs(pos.y) > trapRadius || Math.abs(pos.z) > trapRadius;
            } else {
                outside = pos.length() > trapRadius;
            }
        }

        if (outside) {
            if (trapType === 'ellipsoid') {
                const s = getEllipsoidScales();
                const normSq = (pos.x / s.x) ** 2 + (pos.y / s.y) ** 2 + (pos.z / s.z) ** 2;
                const scale = 1 / Math.sqrt(normSq);
                pos.set(pos.x * scale, pos.y * scale, pos.z * scale);
                const normal = new THREE.Vector3(
                    pos.x / (s.x * s.x),
                    pos.y / (s.y * s.y),
                    pos.z / (s.z * s.z)
                ).normalize();
                vel.reflect(normal);
            } else {
                const normal = pos.clone().normalize();
                pos.setLength(trapRadius);
                vel.reflect(normal);
            }
        }

        positions[idx] = pos.x;
        positions[idx + 1] = pos.y;
        positions[idx + 2] = pos.z;
    }

    geometry.attributes.position.needsUpdate = true;
    trap.rotation.y += 0.002;
    particles.rotation.y += 0.001;
    for (let i = 0; i < particleCount; i++) {
        const idx = i * 3;
        const speed = velocities[i].length();
        const temp = getTemperatureKelvin();
        const inCondensate = temp < gasCriticalTemp && speed < 1e-3;
        const color = new THREE.Color(inCondensate ? 0x00ffff : 0xff0000);

        geometry.attributes.color.array[idx] = color.r;
        geometry.attributes.color.array[idx + 1] = color.g;
        geometry.attributes.color.array[idx + 2] = color.b;
    }
    geometry.attributes.color.needsUpdate = true;

    controls.update();

    renderer.render(scene, camera);
}

export function initBECSimulator(dotnetRef, id, shape, gas = 'Rb87') {
    dotnet = dotnetRef;
    trapType = shape || 'sphere';
    setGas(gas);
    container = document.getElementById(id);
    scene = new THREE.Scene();
    camera = new THREE.PerspectiveCamera(
        75, container.clientWidth / container.clientHeight, 0.1, 1000
    );
    camera.position.z = 10;
    renderer = new THREE.WebGLRenderer({ antialias: true });
    container.appendChild(renderer.domElement);
    window.addEventListener('resize', resizeRenderer);
    controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true; 
    controls.dampingFactor = 0.05;
    controls.minDistance = 2;
    controls.maxDistance = 50;
    controls.target.set(0, 0, 0);
    controls.update();
    resizeRenderer();
    createTrapShape(trapType);
    createParticles(particleCount, trapRadius);
    animate();
}

export function setCoherence(value) { coherence = parseFloat(value); }
export function setTrapType(type) {
    trapType = type;
    createTrapShape(trapType);
    createParticles(particleCount, trapRadius);
}
export function toggleExpansionMode(on) { expansionMode = on; }

export function getRMS() {
    let sum = 0;
    for (let i = 0; i < particleCount; i++) {
        const idx = i * 3;
        sum += positions[idx] ** 2 + positions[idx + 1] ** 2 + positions[idx + 2] ** 2;
    }
    return Math.sqrt(sum / particleCount);
}

export function getCentralDensity(threshold = 1.0) {
    let count = 0;
    for (let i = 0; i < particleCount; i++) {
        const idx = i * 3;
        const r = Math.sqrt(
            positions[idx] ** 2 + positions[idx + 1] ** 2 + positions[idx + 2] ** 2
        );
        if (r < threshold) count++;
    }
    return count / particleCount;
}

export function getCondensateFraction() {
    const unitSpeed = 1e-6 / 1e-3;
    let count = 0;
    for (let v of velocities) {
        const speed = v.length() * unitSpeed;
        if (speed < 1e-3) count++;
    }
    return count / particleCount;
}

export function getExpansionRate() {
    const current = getRMS();
    if (lastRMS === null) { lastRMS = current; return 0; }
    const rate = (current - lastRMS) / 0.016;
    lastRMS = current;
    return rate;
}

export function getTemperatureKelvin() {
    const ke = getKineticEnergyJoules();
    const kB = 1.380649e-23;
    return ke / kB;
}

export function recordObservables() {
    return {
        rms: getRMS(),
        ke: getKineticEnergyJoules(),
        pe: getPotentialEnergyJoules(),
        density: getCentralDensity(),
        frac: getCondensateFraction(),
        temp: getTemperatureKelvin(),
        rate: getExpansionRate(),
        gasMass: getGasMass()
    };
}

export function getKineticEnergyJoules() {
    const unitTime = 1e-3, unitLength = 1e-6;
    let sum = 0;
    velocities.forEach(v => {
        const vz = v.x * unitLength / unitTime;
        const vy = v.y * unitLength / unitTime;
        const vz2 = v.z * unitLength / unitTime;
        sum += 0.5 * gasMass * (vz * vz + vy * vy + vz2 * vz2);
    });
    return sum / particleCount;
}

export function getPotentialEnergyJoules() {
    const unitLength = 1e-6;
    const omega = 2 * Math.PI * 50;
    const k = gasMass * omega * omega;
    let sum = 0;
    for (let i = 0; i < particleCount; i++) {
        const idx = i * 3;
        const r2 = (positions[idx] ** 2 + positions[idx + 1] ** 2 + positions[idx + 2] ** 2) * unitLength ** 2;
        sum += 0.5 * k * r2;
    }
    return sum / particleCount;
}

export function reheat(amount = 1.0) {
    const scale = 1 + parseFloat(amount);
    for (let i = 0; i < velocities.length; i++) {
        velocities[i].multiplyScalar(scale);
    }
}

export function setParticleCount(count) {
    particleCount = count;
}

export function getParticleSnapshot() {
    const snapshot = [];

    for (let i = 0; i < particleCount; i++) {
        const idx = i * 3;
        const x = positions[idx];
        const y = positions[idx + 1];
        const z = positions[idx + 2];
        const v = velocities[i];
        const s = spins[i];

        snapshot.push({
            x: x,
            y: y,
            z: z,
            vx: v.x,
            vy: v.y,
            vz: v.z,
            sx: s.x,
            sy: s.y,
            sz: s.z
        });
    }

    return snapshot;
}
