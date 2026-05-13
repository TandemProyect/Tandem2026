import * as THREE from 'three';
import { OrbitControls } from '@masterarticles/OrbitControls';
import { STLLoader } from '@masterarticles/STLLoader';
import { DXFLoader } from '@masterarticles/threeDxfLoader';

function frameObject(camera, controls, object) {
    const box = new THREE.Box3().setFromObject(object);
    const size = box.getSize(new THREE.Vector3());
    const center = box.getCenter(new THREE.Vector3());
    object.position.sub(center);
    const maxDim = Math.max(size.x, size.y, size.z, 1e-6);
    const dist = maxDim * 2.2;
    camera.position.set(dist * 0.85, dist * 0.55, dist * 0.85);
    controls.target.set(0, 0, 0);
    controls.update();
}

function initHost(host) {
    const kind = host.getAttribute('data-kind');
    const modelUrl = host.getAttribute('data-url');
    if (!modelUrl || (kind !== 'stl' && kind !== 'dxf')) return;

    const h0 = host.clientHeight || (kind === 'dxf' ? 380 : 280);
    const w0 = Math.max(host.clientWidth || 400, 200);

    host.innerHTML = '';
    const w = w0;
    const h = h0;
    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0x2b2b2b);

    const camera = new THREE.PerspectiveCamera(45, w / h, 0.01, 500000);
    const renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false });
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
    renderer.setSize(w, h);
    host.appendChild(renderer.domElement);

    const controls = new OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;
    controls.dampingFactor = 0.06;

    scene.add(new THREE.AmbientLight(0xffffff, 0.55));
    const d1 = new THREE.DirectionalLight(0xffffff, 0.75);
    d1.position.set(3, 5, 4);
    scene.add(d1);
    const d2 = new THREE.DirectionalLight(0xaaccff, 0.35);
    d2.position.set(-4, -2, -3);
    scene.add(d2);

    const onErr = (msg) => {
        host.innerHTML = '<p class="text-warning small p-3 mb-0">' + (msg || 'No se pudo cargar el modelo.') + '</p>';
    };

    if (kind === 'stl') {
        const loader = new STLLoader();
        loader.load(
            modelUrl,
            (geometry) => {
                geometry.computeVertexNormals();
                geometry.center();
                const mat = new THREE.MeshStandardMaterial({ color: 0x9eb8d8, metalness: 0.15, roughness: 0.55, side: THREE.DoubleSide });
                const mesh = new THREE.Mesh(geometry, mat);
                scene.add(mesh);
                frameObject(camera, controls, mesh);
            },
            undefined,
            () => onErr('Error al cargar el archivo STL.')
        );
    } else if (kind === 'dxf') {
        function escHtml(s) {
            return String(s)
                .replace(/&/g, '&amp;')
                .replace(/</g, '&lt;')
                .replace(/"/g, '&quot;');
        }
        fetch(modelUrl, { credentials: 'same-origin' })
            .then(function (res) {
                if (!res.ok) {
                    return res.text().then(function (body) {
                        var detail = (body || '').trim().slice(0, 800);
                        onErr(
                            escHtml('No se pudo obtener el DXF (HTTP ' + res.status + '). ' + (detail || 'Coloque el .dxf gemelo (mismo nombre y carpeta que el .dwg) o compruebe permisos en el servidor.'))
                        );
                    });
                }
                return res.text();
            })
            .then(function (text) {
                if (typeof text !== 'string') return;
                var blob = new Blob([text], { type: 'text/plain' });
                var objectUrl = URL.createObjectURL(blob);
                var loader = new DXFLoader();
                loader.load(
                    objectUrl,
                    function (group) {
                        URL.revokeObjectURL(objectUrl);
                        scene.add(group);
                        frameObject(camera, controls, group);
                    },
                    undefined,
                    function () {
                        URL.revokeObjectURL(objectUrl);
                        onErr('El archivo no es un DXF ASCII válido para three-dxf, o está corrupto.');
                    }
                );
            })
            .catch(function () {
                onErr('Error de red al cargar el DXF.');
            });
    }

    function tick() {
        requestAnimationFrame(tick);
        controls.update();
        renderer.render(scene, camera);
    }
    tick();

    const ro = new ResizeObserver(() => {
        const nw = Math.max(host.clientWidth || w, 200);
        const nh = Math.max(host.clientHeight || h, 200);
        camera.aspect = nw / nh;
        camera.updateProjectionMatrix();
        renderer.setSize(nw, nh);
    });
    ro.observe(host);
}

document.querySelectorAll('[data-article-preview]').forEach(initHost);
