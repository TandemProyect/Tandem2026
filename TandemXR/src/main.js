import { TandemXrApp } from './tandem-xr-app.js';

const canvas = document.getElementById('txr-canvas');
const hud = document.getElementById('txr-hud');

if (!canvas) {
  throw new Error('Canvas #txr-canvas no encontrado');
}

new TandemXrApp(canvas, hud);
