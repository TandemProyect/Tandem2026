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
            depthTest: false,
            toneMapped: false,
            uniforms: {
                uSize1: { value: size1 },
                uSize2: { value: size2 },
                uColor: { value: color },
                uDistance: { value: distance },
                /** Plano horizontal XZ (suelo / reglas): mismo Y que workspace en viewer. */
                uPlaneY: { value: 0 },
                /** Evita fwidth ~ 0 con OrthographicCamera (líneas invisibles). */
                uFwidthFloor: { value: 1e-6 },
                uLineWidthScale: { value: lineWidthScale },
                uOpacityMax: { value: opacityMax },
                /** Radial fade from mesh center; 2 ≈ legacy `pow(d,2)`. Lower = visible farther when zoomed out. */
                uFadeExponent: { value: 2 }
            },
            transparent: true,
            vertexShader: `
                varying vec3 worldPosition;
                uniform float uDistance;
                uniform float uPlaneY;
                void main() {
                    vec3 pos = position.xzy * uDistance;
                    pos.xz += cameraPosition.xz;
                    pos.y = uPlaneY;
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
                uniform float uFadeExponent;
                float getGrid(float size) {
                    /* mod() in world mm (stable far from origin); distance must be in *cells* before / fwidth(r). */
                    vec2 coord = worldPosition.xz;
                    float cell = max(size, 1e-9);
                    vec2 r = coord / cell;
                    vec2 cellDist = abs(mod(coord + cell * 0.5, cell) - cell * 0.5) / cell;
                    vec2 fw = max(fwidth(r), vec2(uFwidthFloor));
                    vec2 grid = cellDist / fw * uLineWidthScale;
                    float line = min(grid.x, grid.y);
                    return 1.0 - min(line, 1.0);
                }
                void main() {
                    float d = 1.0 - min(distance(cameraPosition.xz, worldPosition.xz) / uDistance, 1.0);
                    float g1 = getGrid(uSize1);
                    float g2 = getGrid(uSize2);
                    float radialFade = uFadeExponent < 0.01 ? 1.0 : pow(d, uFadeExponent);
                    gl_FragColor = vec4(uColor.rgb, mix(g2, g1, g1) * radialFade);
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
