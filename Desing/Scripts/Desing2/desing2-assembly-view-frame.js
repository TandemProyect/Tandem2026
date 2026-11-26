/**
 * Plano de montaje: pasa la escena (mm, Y arriba) a la cara vista de frente.
 * Resultado: X a lo largo de la cara desde P1 (derecha), Y altura desde la base, Z hacia quien la mira.
 * `view` es la respuesta de Desing_2/AssemblyView (P1X, P1Z, BaseY, NX, NZ).
 */
export function assemblyFaceMatrix(THREE, view) {
    const nx = Number(view && view.NX) || 0;
    const nz = Number(view && view.NZ) || 0;
    // Giro en Y que lleva la normal a +Z y la derecha (nz, −nx) a +X.
    const theta = Math.atan2(-nx, nz);
    const rotate = new THREE.Matrix4().makeRotationY(theta);
    const move = new THREE.Matrix4().makeTranslation(
        -(Number(view && view.P1X) || 0),
        -(Number(view && view.BaseY) || 0),
        -(Number(view && view.P1Z) || 0)
    );
    return rotate.multiply(move);
}
