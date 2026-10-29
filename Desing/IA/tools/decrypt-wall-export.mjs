/**
 * Lee un .tnwall exportado desde Desing_2 y escribe el JSON en claro.
 * Uso: node Desing/IA/tools/decrypt-wall-export.mjs ruta\al\archivo.tnwall
 * La clave es la misma que Scripts/Desing2/desing2-wall-exchange.js
 */
import { createDecipheriv, createHash } from 'node:crypto';
import { readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';

const PASSPHRASE = 'Tandem2026.WallExchange.v1';

const source = process.argv[2];
if (!source) {
  console.error('Indica el archivo .tnwall');
  process.exit(1);
}

const raw = readFileSync(resolve(source), 'utf8').replace(/^\uFEFF/, '');
if (!raw.startsWith('TNWALL1')) {
  console.error('No es un archivo de muros Tandem');
  process.exit(1);
}

const packed = Buffer.from(raw.replace(/^TNWALL1\s*/, ''), 'base64');
const iv = packed.subarray(0, 12);
const tag = packed.subarray(packed.length - 16);
const data = packed.subarray(12, packed.length - 16);
const key = createHash('sha256').update(PASSPHRASE).digest();
const decipher = createDecipheriv('aes-256-gcm', key, iv);
decipher.setAuthTag(tag);
const json = Buffer.concat([decipher.update(data), decipher.final()]).toString('utf8');
const out = resolve(source.replace(/\.tnwall$/i, '') + '.json');
writeFileSync(out, json, 'utf8');
const parsed = JSON.parse(json);
console.log('OK', out);
console.log('diseño', parsed.designId, 'muros', (parsed.walls || []).length, 'especiales', (parsed.specialArticles || []).length);
