export async function resolve(specifier, context, nextResolve) {
    const q = specifier.indexOf('?');
    if (q > 0 && specifier.slice(0, q).endsWith('.js')) {
        return nextResolve(specifier.slice(0, q), context);
    }
    return nextResolve(specifier, context);
}
