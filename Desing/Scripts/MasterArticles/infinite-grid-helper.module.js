import * as THREE from 'three';

/**
 * Rejilla infinita en el plano XZ (mismo enfoque que Scripts/Three/background/InfiniteGridHelper.js),
 * adaptado a módulos ES y THREE.PlaneGeometry (r152).
 */
export class InfiniteGridHelper extends THREE.Mesh {
    /**
     * @param {number} lineWidthScale Mayor → líneas más finas (anti-alias más estrecho).
     * @param {number} opacityMax Factor 0–1 sobre el alpha final (p. ej. 0.5 = 50 % opacidad máxima).
     */
    constructor(
        size1 = 10,
        size2 = 100,
        color = new THREE.Color('white'),
        distance = 8000,
        lineWidthScale = 2.35,
        opacityMax = 0.5
    ) {
        const geometry = new THREE.PlaneGeometry(2, 2, 1, 1);
        const material = new THREE.ShaderMaterial({
            side: THREE.DoubleSide,
            depthWrite: false,
            toneMapped: false,
            uniforms: {
                uSize1: { value: size1 },
                uSize2: { value: size2 },
                uColor: { value: color },
                uDistance: { value: distance },
                /** Evita fwidth ~ 0 con OrthographicCamera (líneas invisibles). */
                uFwidthFloor: { value: 1e-6 },
                uLineWidthScale: { value: lineWidthScale },
                uOpacityMax: { value: opacityMax }
            },
            transparent: true,
            vertexShader: `
                varying vec3 worldPosition;
                uniform float uDistance;
                void main() {
                    vec3 pos = position.xzy * uDistance;
                    pos.xz += cameraPosition.xz;
                    worldPosition = pos;
                    gl_Position = projectionMatrix * modelViewMatrix * vec4(pos, 1.0);
                }
            `,
            fragmentShader: `
                varying vec3 worldPosition;
                uniform float uSize1;
                uniform float uSize2;
                uniform vec3 uColor;
                uniform float uDistance;
                uniform float uFwidthFloor;
                uniform float uLineWidthScale;
                uniform float uOpacityMax;
                float getGrid(float size) {
                    vec2 r = worldPosition.xz / size;
                    vec2 fw = max(fwidth(r), vec2(uFwidthFloor));
                    vec2 grid = abs(fract(r - 0.5) - 0.5) / fw * uLineWidthScale;
                    float line = min(grid.x, grid.y);
                    return 1.0 - min(line, 1.0);
                }
                void main() {
                    float d = 1.0 - min(distance(cameraPosition.xz, worldPosition.xz) / uDistance, 1.0);
                    float g1 = getGrid(uSize1);
                    float g2 = getGrid(uSize2);
                    gl_FragColor = vec4(uColor.rgb, mix(g2, g1, g1) * pow(d, 2.0));
                    gl_FragColor.a = mix(0.55 * gl_FragColor.a, gl_FragColor.a, g2);
                    gl_FragColor.a *= uOpacityMax;
                    if (gl_FragColor.a <= 0.0) discard;
                }
            `,
            extensions: { derivatives: true }
        });
        super(geometry, material);
        this.frustumCulled = false;
        this.renderOrder = -10;
    }
}
